/*
* ==============================================================================
* Filename: LuaExport
* Created:  2018/7/13 14:29:22
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/

#if UNITY_EDITOR || USE_LUA_PROFILER
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MikuLuaProfiler
{
    public class Sample
    {
        public long currentLuaMemory;
        public long currentMonoMemory;
        public long currentTime;

        public int calls;
        public int frameCount;
        public long costLuaGC;
        public long costMonoGC;
        public string name;
        public long costTime;
        public Sample _father;
        public List<Sample> childs = new List<Sample>(32);
        public string captureUrl = null;

        #region property
        public Sample fahter
        {
            set
            {
                if (value != null)
                {
                    bool needAdd = true;
                    foreach (var item in value.childs)
                    {
                        if (item.name == name)
                        {
                            needAdd = false;
                            item.AddSample(this);
                            break;
                        }
                    }
                    if (needAdd)
                    {
                        value.childs.Add(this);
                        _father = value;
                    }
                }
                else
                {
                    _father = null;
                }
            }
            get
            {
                return _father;
            }
        }
        #endregion

        #region pool
        private static string capturePath = "";
        private static ObjectPool<Sample> samplePool = new ObjectPool<Sample>(250);
        public static Sample Create(long time, long memory, string name)
        {
            Sample s = samplePool.GetObject();

            s.calls = 1;
            s.currentTime = time;
            s.currentLuaMemory = memory;
            s.currentMonoMemory = GC.GetTotalMemory(false);
            s.frameCount = LuaProfiler.m_frameCount;
            s.costLuaGC = 0;
            s.costMonoGC = 0;
            s.name = name;
            s.costTime = 0;
            s._father = null;
            s.childs.Clear();
            s.captureUrl = null;

            return s;
        }

        public bool CheckSampleValid()
        {
            bool result = false;

            do
            {
                if (costLuaGC > 0)
                {
                    result = true;
                    break;
                }

                if (costMonoGC > 0)
                {
                    result = true;
                    break;
                }

                if (costTime > 100000)
                {
                    result = true;
                    break;
                }

            } while (false);


            return result;
        }

        public void Restore()
        {
            lock (this)
            {
                for (int i = 0, imax = childs.Count; i < imax; i++)
                {
                    childs[i].Restore();
                }
                childs.Clear();
                samplePool.Store(this);
            }
        }
        #endregion

        #region method
        public void AddSample(Sample s)
        {
            calls += s.calls;
            costLuaGC += s.costLuaGC;
            costMonoGC += s.costMonoGC;
            costTime += s.costTime;
            for (int i = s.childs.Count - 1; i >= 0; i--)
            {
                var item = s.childs[i];
                item.fahter = this;
                if (item.fahter != s)
                {
                    s.childs.RemoveAt(i);
                }
            }
        }
        public static string Capture()
        {
            if (string.IsNullOrEmpty(capturePath)) capturePath = "capture" + DateTime.Now.Ticks.ToString();

            Directory.CreateDirectory(capturePath);

            string result = capturePath + "/" + UnityEngine.Time.frameCount.ToString() + ".png";
#if UNITY_2017_1_OR_NEWER
            ScreenCapture.CaptureScreenshot(result, 0);
#else
            Application.CaptureScreenshot(result, 0);
#endif
            return result;
        }

        public Sample Clone()
        {
            Sample s = new Sample();

            s.calls = calls;
            s.frameCount = frameCount;
            s.costLuaGC = costLuaGC;
            s.name = name;
            s.costTime = costTime;

            int childCount = childs.Count;
            for (int i = 0; i < childCount; i++)
            {
                Sample child = childs[i].Clone();
                child.fahter = s;
            }
            s.captureUrl = captureUrl;
            s.currentLuaMemory = currentLuaMemory;

            return s;
        }
        #endregion

    }

}
#endif

