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

#if UNITY_EDITOR_WIN || USE_LUA_PROFILER
namespace MikuLuaProfiler
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public class LuaDeepProfilerSetting : ScriptableObject
    {
        #region instance
        private static LuaDeepProfilerSetting instance;
        public static LuaDeepProfilerSetting Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = AssetDatabase.LoadAssetAtPath<LuaDeepProfilerSetting>("Assets/LuaDeepProfilerSetting.asset");
                    if (instance == null)
                    {
                        UnityEngine.Debug.Log("Lua Profiler: cannot find integration settings, creating default settings");
                        instance = CreateInstance<LuaDeepProfilerSetting>();
                        instance.name = "Lua Profiler LuaDeepProfiler Setting";
#if UNITY_EDITOR
                        AssetDatabase.CreateAsset(instance, "Assets/LuaDeepProfilerSetting.asset");
#endif
                    }
                }
                return instance;
            }
        }
        #endregion

        public bool isDeepMonoProfiler
        {
            get
            {
                return LuaDeepProfilerAssetSetting.Instance.isDeepMonoProfiler;
            }
            set
            {
                LuaDeepProfilerAssetSetting.Instance.isDeepMonoProfiler = value;
            }
        }

        public bool isDeepLuaProfiler
        {
            get
            {
                return LuaDeepProfilerAssetSetting.Instance.isDeepLuaProfiler;
            }
            set
            {
                LuaDeepProfilerAssetSetting.Instance.isDeepLuaProfiler = value;
            }
        }
        public bool isLocal
        {
            get
            {
                return LuaDeepProfilerAssetSetting.Instance.isLocal;
            }
            set
            {
                LuaDeepProfilerAssetSetting.Instance.isLocal = value;
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
                    EditorUtility.SetDirty(this);
                }
            }
        }

        public int captureLuaGC
        {
            get
            {
                return this.m_captureLuaGC;
            }
            set
            {
                if (this.m_captureLuaGC != value)
                {
                    this.m_captureLuaGC = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }

        public bool isNeedCapture
        {
            get
            {
                return this.m_isNeedCapture;
            }
            set
            {
                if (this.m_isNeedCapture != value)
                {
                    this.m_isNeedCapture = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }

        public string assMd5
        {
            get
            {
                return this.m_assMd5;
            }
            set
            {
                if (!(this.m_assMd5 == value))
                {
                    this.m_assMd5 = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }

        public bool isInited
        {
            get
            {
                return this.m_isInited;
            }
            set
            {
                if (this.m_isInited != value)
                {
                    this.m_isInited = value;
                    EditorUtility.SetDirty(this);
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
                    EditorUtility.SetDirty(this);
                }
            }
        }

        public int captureMonoGC
        {
            get
            {
                return this.m_captureMonoGC;
            }
            set
            {
                if (this.m_captureMonoGC != value)
                {
                    this.m_captureMonoGC = value;
                    EditorUtility.SetDirty(this);
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
                    EditorUtility.SetDirty(this);
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
                    EditorUtility.SetDirty(this);
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
                    EditorUtility.SetDirty(this);
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
                EditorUtility.SetDirty(this);
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
                EditorUtility.SetDirty(this);
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
                EditorUtility.SetDirty(this);
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
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
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
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
            }
        }

        public const string SettingsAssetName = "LuaDeepProfilerSettings";
        public bool m_isDeepMonoProfiler = false;
        public bool m_isDeepLuaProfiler = false;
        public bool m_isCleanMode = false;
        public int m_captureLuaGC = 51200;
        public bool m_isNeedCapture = false;
        public bool m_discardInvalid = true;
        public string m_assMd5 = "";
        public bool m_isInited = false;
        public int m_captureMonoGC = 51200;
        public int m_captureFrameRate = 30;
        public string m_ip = "127.0.0.1";
        public int m_port = 2333;

        private bool m_isRecord = false;
        [SerializeField]
        private List<string> m_luaDir = new List<string>();
        [SerializeField]
        private string m_luaIDE = "";
        private bool m_isNeedRecord = false;
        private bool m_isFrameRecord = true;
    }
}
#endif