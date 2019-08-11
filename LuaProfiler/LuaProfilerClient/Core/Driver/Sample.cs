/*
               #########                       
              ############                     
              #############                    
             ##  ###########                   
            ###  ###### #####                  
            ### #######   ####                 
           ###  ########## ####                
          ####  ########### ####               
         ####   ###########  #####             
        #####   ### ########   #####           
       #####   ###   ########   ######         
      ######   ###  ###########   ######       
     ######   #### ##############  ######      
    #######  #####################  ######     
    #######  ######################  ######    
   #######  ###### #################  ######   
   #######  ###### ###### #########   ######   
   #######    ##  ######   ######     ######   
   #######        ######    #####     #####    
    ######        #####     #####     ####     
     #####        ####      #####     ###      
      #####       ###        ###      #        
        ###       ###        ###               
         ##       ###        ###               
__________#_______####_______####______________
                我们的未来没有BUG                
* ==============================================================================
* Filename: Sample
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
    public abstract class NetBase
    {
        public abstract void Restore();
    }

    public class LuaRefInfo : NetBase
    {
        #region field
        public byte cmd; //1添加、0移除
        public string name;
        public string addr;
        public byte type; //1 function 2 table
        #endregion

        #region pool
        private static ObjectPool<LuaRefInfo> m_pool = new ObjectPool<LuaRefInfo>(32);
        public static LuaRefInfo Create(byte cmd, string name, string addr, byte type)
        {
            LuaRefInfo r = m_pool.GetObject();
            r.cmd = cmd;
            r.name = name;
            r.addr = addr;
            r.type = type;
            return r;
        }

        public override void Restore()
        {
            m_pool.Store(this);
        }
        #endregion
    }

    public class LuaDiffInfo : NetBase
    {
        #region field
        public Dictionary<string, LuaTypes> addRef = new Dictionary<string, LuaTypes>();
        public Dictionary<string, List<string>> addDetail = new Dictionary<string, List<string>>();
        public Dictionary<string, LuaTypes> rmRef = new Dictionary<string, LuaTypes>();
        public Dictionary<string, List<string>> rmDetail = new Dictionary<string, List<string>>();
        public Dictionary<string, LuaTypes> nullRef = new Dictionary<string, LuaTypes>();
        public Dictionary<string, List<string>> nullDetail = new Dictionary<string, List<string>>();
        #endregion

        #region pool
        private static ObjectPool<LuaDiffInfo> m_pool = new ObjectPool<LuaDiffInfo>(32);
        public static LuaDiffInfo Create()
        {
            LuaDiffInfo r = m_pool.GetObject();
            r.addRef.Clear();
            r.addDetail.Clear();
            r.rmRef.Clear();
            r.rmDetail.Clear();
            r.nullRef.Clear();
            return r;
        }
        public override void Restore()
        {
            m_pool.Store(this);
        }
        #endregion
    }

    public class Sample : NetBase
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
        public MList<Sample> childs = new MList<Sample>(16);
        public string captureUrl = null;

        public bool needShow = false;

        public long selfLuaGC
        {
            get
            {
                long result = costLuaGC;
                for (int i = 0, imax = childs.Count; i < imax; i++)
                {
                    var item = childs[i];
                    result -= item.costLuaGC;
                }
                return result;
            }
        }

        public long selfMonoGC
        {
            get
            {
                long result = costMonoGC;
                for (int i = 0, imax = childs.Count; i < imax; i++)
                {
                    var item = childs[i];
                    result -= item.costMonoGC;
                }

                return result;
            }
        }
 
        public bool CheckSampleValid()
        {
            bool result = false;
            do
            {
                if (needShow)
                {
                    result = true;
                    break;
                }
                var setting = LuaDeepProfilerSetting.Instance;
                if (setting != null && !setting.discardInvalid)
                {
                    result = true;
                    break;
                }

                if (costLuaGC != 0)
                {
                    result = true;
                    break;
                }

                if (costMonoGC != 0)
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
                    var childList = value.childs;
                    for (int i = 0,imax = childList.Count;i<imax;i++)
                    {
                        var item = childList[i];
                        if ((object)(item.name) == (object)(name))
                        {
                            needAdd = false;
                            item.AddSample(this);
                            break;
                        }
                    }
                    if (needAdd)
                    {
                        childList.Add(this);
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

        public override void Restore()
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

