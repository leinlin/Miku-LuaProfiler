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
        public int currentLuaMemory;
        public int currentMonoMemory;
        public long currentTime;

        public int calls;
        public int frameCount;
        public float fps;
        public int pss;
        public float power;

        public int costLuaGC;
        public int costMonoGC;
        public string name;
        public int costTime;
        public Sample _father;
        public List<Sample> childs = new List<Sample>(16);
        public string captureUrl = null;

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
        private static ObjectPool<Sample> samplePool = new ObjectPool<Sample>(4096);
        public static Sample Create(long time, int memory, string name)
        {
            Sample s = samplePool.GetObject();

            s.calls = 1;
            s.currentTime = time;
            s.currentLuaMemory = memory;
            s.currentMonoMemory = (int)GC.GetTotalMemory(false);
            s.frameCount = HookLuaSetup.frameCount;
            s.fps = HookLuaSetup.fps;
            s.pss = HookLuaSetup.pss;
            s.power = HookLuaSetup.power;
            s.costLuaGC = 0;
            s.costMonoGC = 0;
            s.name = name;
            s.costTime = 0;
            s._father = null;
            s.childs.Clear();
            s.captureUrl = null;

            return s;
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

            string result = capturePath + "/" + Time.frameCount.ToString() + ".png";
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
            s.fps = fps;
            s.pss = pss;
            s.power = power;
            s.costMonoGC = costMonoGC;
            s.costLuaGC = costLuaGC;
            s.name = name;
            s.costTime = costTime;

            int childCount = childs.Count;
            for (int i = 0; i < childCount; i++)
            {
                Sample child = childs[i].Clone();
                child.fahter = s;
            }

            s.currentLuaMemory = currentLuaMemory;
            s.currentMonoMemory = currentMonoMemory;
            s.currentTime = currentTime;
            s.captureUrl = captureUrl;
            return s;
        }
        #endregion

    }

}
#endif

