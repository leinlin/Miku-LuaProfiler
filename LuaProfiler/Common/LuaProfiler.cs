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
using System.Runtime.Serialization.Formatters.Binary;

namespace MikuLuaProfiler
{
    public class LuaProfiler
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

        [Serializable]
        public class Sample
        {
            public float currentTime;
            public long realCurrentLuaMemory;
            public string name;
            public long currentLuaMemory;
            public float costTime;
            public long costGC;
            public Sample _father;
            public List<Sample> childs = new List<Sample>(256);
            public string _fullName = null;

            #region property
            public string fullName
            {
                get
                {
                    if (_father == null) return name;

                    if (_fullName == null)
                    {
                        Dictionary<string, string> childDict;
                        if (!m_fullNamePool.TryGetValue(_father.fullName, out childDict))
                        {
                            childDict = new Dictionary<string, string>();
                            m_fullNamePool.Add(_father.fullName, childDict);
                        }

                        if (!childDict.TryGetValue(name, out _fullName))
                        {
                            string value = name;
                            var f = _father;
                            while (f != null)
                            {
                                value = f.name + value;
                                f = f.fahter;
                            }
                            _fullName = value;
                            childDict[name] = _fullName;
                        }

                        return _fullName;
                    }
                    else
                    {
                        return _fullName;
                    }
                }
            }
            public Sample fahter
            {
                set
                {
                    _father = value;
                    if (_father != null)
                    {
                        _father.childs.Add(this);
                    }
                }
                get
                {
                    return _father;
                }
            }
            #endregion

            #region pool
            private static Dictionary<string, Dictionary<string, string>> m_fullNamePool = new Dictionary<string, Dictionary<string, string>>();
            private static ObjectPool<Sample> samplePool = new ObjectPool<Sample>(250);
            public static Sample Create(float time, long memory, string name)
            {
                Sample s = samplePool.GetObject();
                s.currentTime = time;
                s.currentLuaMemory = memory;
                s.realCurrentLuaMemory = memory;
                s.costGC = 0;
                s.name = name;
                s.costTime = 0;
                s.childs.Clear();
                s._father = null;
                s._fullName = null;

                return s;
            }

            public void Restore()
            {
                for (int i = 0, imax = childs.Count; i < imax; i++)
                {
                    childs[i].Restore();
                }
                samplePool.Store(this);
            }
            #endregion

        }


        //开始采样时候的lua内存情况，因为中间有可能会有二次采样，所以要丢到一个盏中
        public static readonly List<Sample> beginSampleMemoryStack = new List<Sample>();

        private static Action<Sample> m_SampleEndAction;

        private static bool isDeep
        {
            get
            {
#if UNITY_EDITOR
                return ProfilerDriver.deepProfiling;
#else
            return false;
#endif
            }
        }
        public static void SetSampleEnd(Action<Sample> action)
        {
            m_SampleEndAction = action;
        }
        private static int m_currentFrame = 0;
        public static void BeginSample(IntPtr luaState, string name)
        {
            if (m_currentFrame != Time.frameCount)
            {
                PopAllSampleWhenLateUpdate();
                m_currentFrame = Time.frameCount;
            }

#if DEBUG
            long memoryCount = LuaLib.GetLuaMemory(luaState);
            Sample sample = Sample.Create(Time.realtimeSinceStartup, memoryCount, name);

            beginSampleMemoryStack.Add(sample);
#endif
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
#if DEBUG
            if (beginSampleMemoryStack.Count <= 0)
            {
                return;
            }
            int count = beginSampleMemoryStack.Count;
            Sample sample = beginSampleMemoryStack[beginSampleMemoryStack.Count - 1];
            long oldMemoryCount = sample.currentLuaMemory;
            beginSampleMemoryStack.RemoveAt(count - 1);
            long nowMemoryCount = LuaLib.GetLuaMemory(luaState);
            sample.fahter = count > 1 ? beginSampleMemoryStack[count - 2] : null;

            if (!isDeep)
            {
                long delta = nowMemoryCount - oldMemoryCount;

                long tmpDelta = delta;
                for (int i = 0, imax = beginSampleMemoryStack.Count; i < imax; i++)
                {
                    Sample s = beginSampleMemoryStack[i];
                    s.currentLuaMemory += tmpDelta;
                    beginSampleMemoryStack[i] = s;
                }
            }

            sample.costTime = Time.realtimeSinceStartup - sample.currentTime;
            var gc = nowMemoryCount - sample.realCurrentLuaMemory;
            sample.costGC = gc > 0 ? gc : 0;

            if (m_SampleEndAction != null && beginSampleMemoryStack.Count == 0)
            {
                m_SampleEndAction(sample);
            }

            if (sample.fahter == null)
            {
                sample.Restore();
            }
#endif
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

