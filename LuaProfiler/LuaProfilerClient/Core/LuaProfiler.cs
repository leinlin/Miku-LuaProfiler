/*
* ==============================================================================
* Filename: LuaExport
* Created:  2018/7/13 14:29:22
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditorInternal;
using System.Reflection;
using System.Reflection.Emit;
#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif

namespace MikuLuaProfiler
{
    public static class LuaProfiler
    {
        private static IntPtr _mainL = IntPtr.Zero;
        public static IntPtr mainL
        {
            get
            {
                return _mainL;
            }
            set
            {
                _mainL = value;
            }
        }

        public static string GetLuaMemory()
        {
            long result = 0;
            if (mainL != IntPtr.Zero)
            {
                try
                {
                    result = LuaLib.GetLuaMemory(mainL);
                }
                catch { }
            }

            return GetMemoryString(result);
        }

        //开始采样时候的lua内存情况，因为中间有可能会有二次采样，所以要丢到一个盏中
        public static readonly List<Sample> beginSampleMemoryStack = new List<Sample>();

        private static Action<Sample> m_SampleEndAction;
        public static void SetSampleEnd(Action<Sample> action)
        {
            m_SampleEndAction = action;
        }
        private static int m_currentFrame = 0;
        private static Dictionary<MethodBase, string> m_methodNameDict = new Dictionary<MethodBase, string>(4096);
        public static string GetMethodLineString(MethodBase m)
        {
            string methodName = "";
            if (!m_methodNameDict.TryGetValue(m, out methodName))
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
                System.Diagnostics.StackFrame sf = st.GetFrame(1);
                if (sf != null)
                {
                    string fileName = sf.GetFileName();
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        fileName = fileName.Replace(Environment.CurrentDirectory, "").Replace("\\", "/");
                        fileName = fileName.Substring(1, fileName.Length - 1);
                        methodName = string.Format("{0},line:{1}&[c#]:{2}::{3}",
                            fileName, sf.GetFileLineNumber(), m.ReflectedType.FullName, m.Name);
                    }
                }
                if(string.IsNullOrEmpty(methodName))
                {
                    methodName = string.Format("{0}::{1}", m.ReflectedType.FullName, m.Name);
                }
                m_methodNameDict.Add(m, methodName);
            }

            return methodName;
        }


        static int mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;

 
        // If called in the non main thread, will return false;
        public static bool IsMainThread
        {
            get
            {
                return System.Threading.Thread.CurrentThread.ManagedThreadId == mainThreadId;
            }
        }
        public static void BeginSampleCSharp(string name)
        {
            BeginSample(_mainL, name);
        }
        public static void EndSampleCSharp()
        {
            EndSample(_mainL);
        }
        public static int m_frameCount = 0;
        public static long getcurrentTime
        {
            get
            {
                return DateTime.UtcNow.Ticks;
            }
        }
        public static void BeginSample(IntPtr luaState, string name)
        {
            if (!IsMainThread) return;
            m_frameCount = HookLuaUtil.frameCount;

            if (m_currentFrame != m_frameCount)
            {
                PopAllSampleWhenLateUpdate();
                m_currentFrame = m_frameCount;
            }
            long memoryCount = LuaLib.GetLuaMemory(luaState);
            Sample sample = Sample.Create(getcurrentTime, memoryCount, name);
            beginSampleMemoryStack.Add(sample);
        }
        public static void PopAllSampleWhenLateUpdate()
        {
            for (int i = 0, imax = beginSampleMemoryStack.Count; i < imax; i++)
            {
                var item = beginSampleMemoryStack[i];
                if (item.fahter == null)
                {
                    item.Restore();
                }
            }
            beginSampleMemoryStack.Clear();
        }

        public static void EndSample(IntPtr luaState)
        {
            if (!IsMainThread) return;
            if (beginSampleMemoryStack.Count <= 0)
            {
                return;
            }
            long nowMemoryCount = LuaLib.GetLuaMemory(luaState);
            long nowMonoCount = GC.GetTotalMemory(false);
            Sample sample = beginSampleMemoryStack[beginSampleMemoryStack.Count - 1];
            beginSampleMemoryStack.RemoveAt(beginSampleMemoryStack.Count - 1);

            sample.costTime = getcurrentTime - sample.currentTime;
            var monoGC = nowMonoCount - sample.currentMonoMemory;
            var luaGC = nowMemoryCount - sample.currentLuaMemory;
            sample.costLuaGC = luaGC > 0 ? luaGC : 0;
            sample.costMonoGC = monoGC > 0 ? monoGC : 0;

            if (!sample.CheckSampleValid())
            {
                sample.Restore();
                return;
            }
            sample.fahter = beginSampleMemoryStack.Count > 0 ? beginSampleMemoryStack[beginSampleMemoryStack.Count - 1] : null;

            if (m_SampleEndAction != null && beginSampleMemoryStack.Count == 0)
            {
                if (LuaDeepProfilerSetting.Instance.isRecord && LuaDeepProfilerSetting.Instance.isNeedRecord)
                {
                    //迟钝了
                    if (sample.costTime >= (1 / 30.0f) * 10000000)
                    {
                        sample.captureUrl = Sample.Capture();
                    }
                    else if (sample.costLuaGC > LuaDeepProfilerSetting.Instance.captureLuaGC)
                    {
                        sample.captureUrl = Sample.Capture();
                    }
                    else
                    {
                        sample.captureUrl = null;
                    }
                }
                m_SampleEndAction(sample);
            }

            //释放掉被累加的Sample
            if (beginSampleMemoryStack.Count != 0 && sample.fahter == null)
            {
                sample.Restore();
            }
        }

        const long MaxB = 1024;
        const long MaxK = MaxB * 1024;
        const long MaxM = MaxK * 1024;
        const long MaxG = MaxM * 1024;

        public static string GetMemoryString(long value, string unit = "B")
        {
            string result = null;
            if (value < MaxB)
            {
                result = string.Format("{0}{1}", value, unit);
            }
            else if (value < MaxK)
            {
                result = string.Format("{0:N2}K{1}", (float)value / MaxB, unit);
            }
            else if (value < MaxM)
            {
                result = string.Format("{0:N2}M{1}", (float)value / MaxK, unit);
            }
            else if (value < MaxG)
            {
                result = string.Format("{0:N2}G{1}", (float)value / MaxM, unit);
            }
            return result;
        }
    }
}
#endif

