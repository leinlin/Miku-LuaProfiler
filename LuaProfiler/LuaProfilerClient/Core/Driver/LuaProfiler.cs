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
using RefDict = System.Collections.Generic.Dictionary<string, System.Collections.Generic.HashSet<string>>;

namespace MikuLuaProfiler
{
    public static class LuaProfiler
    {
        #region member
        private static IntPtr _mainL = IntPtr.Zero;
        private static readonly Stack<Sample> beginSampleMemoryStack = new Stack<Sample>();
        private static int m_currentFrame = 0;
        private static Dictionary<MethodBase, string> m_methodNameDict = new Dictionary<MethodBase, string>(4096);
        public static int mainThreadId = -100;
        const long MaxB = 1024;
        const long MaxK = MaxB * 1024;
        const long MaxM = MaxK * 1024;
        const long MaxG = MaxM * 1024;
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
                if (value != IntPtr.Zero)
                {
                    LuaLib.lua_openLib(value);
                }
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
        public static int GetLuaMemorySize()
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

            return (int)result;
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
            if (!IsMainThread)
            {
                return;
            }

            var setting = LuaDeepProfilerSetting.Instance;
            if (setting == null) return;

            int frameCount = HookLuaSetup.frameCount;

            if (m_currentFrame != frameCount)
            {
                PopAllSampleWhenLateUpdate();
                m_currentFrame = frameCount;
            }
            long memoryCount = LuaLib.GetLuaMemory(luaState);
            Sample sample = Sample.Create(getcurrentTime, (int)memoryCount, name);
            beginSampleMemoryStack.Push(sample);
        }
        public static void PopAllSampleWhenLateUpdate()
        {
            while(beginSampleMemoryStack.Count > 0)
            {
                var item = beginSampleMemoryStack.Pop();
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
            if (!IsMainThread)
            {
                return;
            }

            var setting = LuaDeepProfilerSetting.Instance;
            if (setting == null) return;

            if (beginSampleMemoryStack.Count <= 0)
            {
                return;
            }
            long nowMemoryCount = LuaLib.GetLuaMemory(luaState);
            long nowMonoCount = GC.GetTotalMemory(false);
            Sample sample = beginSampleMemoryStack.Pop();

            sample.costTime = (int)(getcurrentTime - sample.currentTime);
            var monoGC = nowMonoCount - sample.currentMonoMemory;
            var luaGC = nowMemoryCount - sample.currentLuaMemory;
            sample.costLuaGC = (int)(luaGC > 0 ? luaGC : 0);
            sample.costMonoGC = (int)(monoGC > 0 ? monoGC : 0);

            if (!sample.CheckSampleValid())
            {
                sample.Restore();
                return;
            }
            sample.fahter = beginSampleMemoryStack.Count > 0 ? beginSampleMemoryStack.Peek() : null;
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

        #region ref
        private static Dictionary<byte, RefDict> m_refDict = new Dictionary<byte, RefDict>(4);

        public static void AddRef(string refName, string refAddr, byte type)
        {
            RefDict refDict;
            if (!m_refDict.TryGetValue(type, out refDict))
            {
                refDict = new RefDict(2048);
                m_refDict.Add(type, refDict);
            }

            HashSet<string> addrList;
            if (!refDict.TryGetValue(refName, out addrList))
            {
                addrList = new HashSet<string>();
                refDict.Add(refName, addrList);
            }
            if (!addrList.Contains(refAddr))
            {
                addrList.Add(refAddr);
            }
            SendAddRef(refName, refAddr, type);
        }
        public static void SendAddRef(string funName, string funAddr, byte type)
        {
            LuaRefInfo refInfo = LuaRefInfo.Create(1, funName, funAddr, type);
            NetWorkClient.SendMessage(refInfo);
        }
        public static void RemoveRef(string refName, string refAddr, byte type)
        {
            if (string.IsNullOrEmpty(refName)) return;
            RefDict refDict;

            if (!m_refDict.TryGetValue(type, out refDict))
            {
                return;
            }

            HashSet<string> addrList;
            if (!refDict.TryGetValue(refName, out addrList))
            {
                return;
            }
            if (!addrList.Contains(refAddr))
            {
                return;
            }
            addrList.Remove(refAddr);
            if (addrList.Count == 0)
            {
                refDict.Remove(refName);
            }
            SendRemoveRef(refName, refAddr, type);
        }
        public static void SendRemoveRef(string funName, string funAddr, byte type)
        {
            LuaRefInfo refInfo = LuaRefInfo.Create(0, funName, funAddr, type);
            NetWorkClient.SendMessage(refInfo);
        }
        public static void SendAllRef()
        {
            foreach (var dictItem in m_refDict)
            {
                foreach (var hashList in dictItem.Value)
                {
                    foreach (var item in hashList.Value)
                    {
                        SendAddRef(hashList.Key, item, dictItem.Key);
                    }
                }
            }
        }
        #endregion

    }
}
#endif

