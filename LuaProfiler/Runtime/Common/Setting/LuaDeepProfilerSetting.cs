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
* Filename: LuaDeepProfilerSetting.cs
* Created:  2018/7/13 14:29:22
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/

using System;
using System.IO;

#if UNITY_EDITOR_WIN || USE_LUA_PROFILER
namespace MikuLuaProfiler
{
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable]
    public class LuaDeepProfilerSetting
    {
        #region instance
        private static LuaDeepProfilerSetting instance;
        public static LuaDeepProfilerSetting Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Load();
                }
                return instance;
            }
        }

        private const string CONFIG_NAME = "MIKU_LUAPROFILER";
        
        #endregion

        public bool isDeepLuaProfiler
        {
            get
            {
                return m_isDeepLuaProfiler;
            }
            set
            {
                if (m_isDeepLuaProfiler != value)
                {
                    m_isDeepLuaProfiler = value;
                    Save();
                }
            }
        }

        public bool isLocal
        {
            get
            {
#if UNITY_5_6_OR_NEWER && UNITY_EDITOR_WIN
                return m_isLocal;
#else
                return false;
#endif
            }
            set
            {
                if (m_isLocal != value)
                {
                    m_isLocal = value;
                    Save();
                }
            }
        }

        public bool isCleanMode
        {
            get
            {
                return m_isCleanMode;
            }
            set
            {
                if (this.m_isCleanMode != value)
                {
                    this.m_isCleanMode = value;
                    Save();
                }
            }
        }
        

        public bool discardInvalid
        {
            get
            {
                return m_discardInvalid;
            }
            set
            {
                if (m_discardInvalid != value)
                {
                    m_discardInvalid = value;
                    Save();
                }
            }
        }

        public int captureFrameRate
        {
            get
            {
                return this.m_captureFrameRate;
            }
            set
            {
                if (this.m_captureFrameRate != value)
                {
                    this.m_captureFrameRate = value;
                    Save();
                }
            }
        }

        public string ip
        {
            get
            {
                return this.m_ip;
            }
            set
            {
                if (!(this.m_ip == value))
                {
                    this.m_ip = value;
                    Save();
                }
            }
        }

        public int port
        {
            get
            {
                return this.m_port;
            }
            set
            {
                if (this.m_port != value)
                {
                    this.m_port = value;
                    Save();
                }
            }
        }

        public bool isRecord
        {
            get
            {
                return m_isRecord;
            }
            set
            {
                if (m_isRecord == value) return;
                m_isRecord = value;
                Save();
            }
        }

        public bool isStartRecord
        {
            get
            {
                return m_isNeedRecord;
            }
            set
            {
                if (m_isNeedRecord == value) return;
                m_isNeedRecord = value;
                Save();
            }
        }

        public bool isFrameRecord
        {
            get
            {
                return m_isFrameRecord;
            }
            set
            {
                if (m_isFrameRecord == value) return;
                m_isFrameRecord = value;
                Save();
            }
        }

        public List<string> luaDir
        {
            get
            {
                return m_luaDir;
            }
        }

        public void AddLuaDir(string path)
        {
            if (!m_luaDir.Contains(path))
            {
                m_luaDir.Add(path);
                Save();
            }
        }

        public string luaIDE
        {
            get
            {
                return m_luaIDE;
            }
            set
            {
                if (m_luaIDE == value) return;
                m_luaIDE = value;
               Save();
            }
        }

        public const string SettingsAssetName = "LuaDeepProfilerSettings";
        public bool m_isNeedRecord = false;
        public bool m_isDeepLuaProfiler = false;
        public bool m_isCleanMode = false;
        public bool m_discardInvalid = false;
        public bool m_isFrameRecord = false;
        public int m_captureFrameRate = 30;
        public bool m_isLocal = true;
        public string m_ip = "127.0.0.1";
        public int m_port = 2333;

        private bool m_isRecord = false;
        [SerializeField]
        private List<string> m_luaDir = new List<string>();
        [SerializeField]
        private string m_luaIDE = "";
        
#if UNITY_EDITOR
        public void Save()
        {
            string v = JsonUtility.ToJson(this);
            PlayerPrefs.SetString(CONFIG_NAME, v);
        }
#else
        public void Save()
        {}
#endif
        public static LuaDeepProfilerSetting Load()
        {
            LuaDeepProfilerSetting result = null;
            var ta = PlayerPrefs.GetString(CONFIG_NAME);
            if (string.IsNullOrEmpty(ta))
            {
                result = new LuaDeepProfilerSetting();
#if UNITY_EDITOR
                result.Save();
#endif
            }
            else
            {
                string json = ta;

                try
                {
                    result = JsonUtility.FromJson<LuaDeepProfilerSetting>(json);
                }
                catch
                {
                    result = new LuaDeepProfilerSetting();
#if UNITY_EDITOR
                    result.Save();
#endif
                }
            }
            return result;
        }

    }
}
#endif