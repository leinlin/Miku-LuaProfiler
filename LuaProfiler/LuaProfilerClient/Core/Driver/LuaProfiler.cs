/*
* ==============================================================================
* Filename: LuaExport
* Created:  2018/7/13 14:29:22
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/

#if UNITY_EDITOR  || USE_LUA_PROFILER
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MikuLuaProfiler
{
    public static class LuaProfiler
    {
        #region member
        private static IntPtr _mainL = IntPtr.Zero;
        private static readonly List<Sample> beginSampleMemoryStack = new List<Sample>();
        private static int m_currentFrame = 0;
        private static Dictionary<MethodBase, string> m_methodNameDict = new Dictionary<MethodBase, string>(4096);
        private static int mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
        const long MaxB = 1024;
        const long MaxK = MaxB * 1024;
        const long MaxM = MaxK * 1024;
        const long MaxG = MaxM * 1024;
        public static int m_frameCount = 0;
        #endregion

        #region property
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
        public static bool IsMainThread
        {
            get
            {
                return System.Threading.Thread.CurrentThread.ManagedThreadId == mainThreadId;
            }
        }
        #endregion

        #region api
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
        #endregion

        #region sample
        public static void BeginSampleCSharp(string name)
        {
            BeginSample(_mainL, name);
        }
        public static void EndSampleCSharp()
        {
            EndSample(_mainL);
        }
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
            var setting = LuaDeepProfilerSetting.Instance;
            if (setting == null) return;

            m_frameCount = HookLuaSetup.frameCount;

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
                    NetWorkClient.SendMessage(item);
                    //item.Restore();
                }
            }
            beginSampleMemoryStack.Clear();
        }
        public static void EndSample(IntPtr luaState)
        {
            if (!IsMainThread) return;
            var setting = LuaDeepProfilerSetting.Instance;
            if (setting == null) return;

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
            //UnityEngine.Debug.Log(sample.name);
            if (beginSampleMemoryStack.Count == 0)
            {
                if (setting != null && setting.isNeedCapture)
                {
                    //迟钝了
                    if (sample.costTime >= (1 / (float)(setting.captureFrameRate)) * 10000000)
                    {
                        sample.captureUrl = Sample.Capture();
                    }
                    else if (sample.costLuaGC > setting.captureLuaGC)
                    {
                        sample.captureUrl = Sample.Capture();
                    }
                    else if (sample.costMonoGC > setting.captureMonoGC)
                    {
                        sample.captureUrl = Sample.Capture();
                    }
                    else
                    {
                        sample.captureUrl = null;
                    }
                }
                NetWorkClient.SendMessage(sample);
            }
            //释放掉被累加的Sample
            if (beginSampleMemoryStack.Count != 0 && sample.fahter == null)
            {
                sample.Restore();
            }
        }
        #endregion
    }
}
#endif

