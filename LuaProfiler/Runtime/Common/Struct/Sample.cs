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
using System.Text;
using UnityEngine;

namespace MikuLuaProfiler
{
    public enum LuaTypes
    {
        LUA_TNONE = -1,
        LUA_TNIL = 0,
        LUA_TNUMBER = 3,
        LUA_TSTRING = 4,
        LUA_TBOOLEAN = 1,
        LUA_TTABLE = 5,
        LUA_TFUNCTION = 6,
        LUA_TUSERDATA = 7,
        LUA_TTHREAD = 8,
        LUA_TLIGHTUSERDATA = 2
    }

    public abstract class NetBase
    {
        public abstract void Restore();
    }

    public class LuaDiffInfo : NetBase
    {
        #region field
        public Dictionary<LuaTypes, HashSet<string>> addRef = new Dictionary<LuaTypes, HashSet<string>>();
        public Dictionary<string, List<string>> addDetail = new Dictionary<string, List<string>>();
        public Dictionary<LuaTypes, HashSet<string>> rmRef = new Dictionary<LuaTypes, HashSet<string>>();
        public Dictionary<string, List<string>> rmDetail = new Dictionary<string, List<string>>();
        public Dictionary<LuaTypes, HashSet<string>> nullRef = new Dictionary<LuaTypes, HashSet<string>>();
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

        #region api
        public void PushAddRef(string addKey, int addType)
        {
            HashSet<string> list;
            LuaTypes t = (LuaTypes)addType;
            if (!addRef.TryGetValue(t, out list))
            {
                list = new HashSet<string>();
                addRef.Add(t, list);
            }
            if (!list.Contains(addKey))
            {
                list.Add(addKey);
            }
        }

        public void PushAddDetail(string addKey, string value)
        {
            List<string> list;
            if (!addDetail.TryGetValue(addKey, out list))
            {
                list = new List<string>();
                addDetail[addKey] = list;
            }
            list.Add(value);
        }

        public void PushRmRef(string addKey, int addType)
        {
            HashSet<string> list;
            LuaTypes t = (LuaTypes)addType;
            if (!rmRef.TryGetValue(t, out list))
            {
                list = new HashSet<string>();
                rmRef.Add(t, list);
            }
            if (!list.Contains(addKey))
            {
                list.Add(addKey);
            }
        }

        public void PushRmDetail(string key, string value)
        {
            List<string> list;
            if (!rmDetail.TryGetValue(key, out list))
            {
                list = new List<string>();
                rmDetail[key] = list;
            }
            list.Add(value);
        }

        public void PushNullRef(string addKey, int addType)
        {
            HashSet<string> list;
            LuaTypes t = (LuaTypes)addType;
            if (!nullRef.TryGetValue(t, out list))
            {
                list = new HashSet<string>();
                nullRef.Add(t, list);
            }
            if (!list.Contains(addKey))
            {
                list.Add(addKey);
            }
        }

        public void PushNullDetail(string addKey, string value)
        {
            List<string> list;
            if (!nullDetail.TryGetValue(addKey, out list))
            {
                list = new List<string>();
                nullDetail[addKey] = list;
            }
            list.Add(value);
        }
        #endregion
    }

    public class SampleData
    {
        public static int frameCount;
        public static float fps;
        public static uint pss;
        public static float power;
    }

}
#endif

