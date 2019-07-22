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

#if UNITY_EDITOR || USE_LUA_PROFILER
namespace MikuLuaProfiler
{
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using UnityEngine;
    public class LuaDeepProfilerSetting
    {
        public static LuaDeepProfilerSetting MakeInstance()
        {
            instance = LuaDeepProfilerSetting.Load();
            return instance;
        }
        public static LuaDeepProfilerSetting Instance
        {
            get
            {
#if UNITY_EDITOR
                if (instance == null)
                {
                    instance = MakeInstance();
                }
#endif
                return instance;
            }
        }

        public bool isDeepMonoProfiler
        {
            get
            {
#if UNITY_EDITOR
                return LuaDeepProfilerAssetSetting.Instance.isDeepMonoProfiler;
#else
                return this.m_isDeepMonoProfiler;
#endif
            }
            set
            {
#if UNITY_EDITOR
                LuaDeepProfilerAssetSetting.Instance.isDeepMonoProfiler = value;
#endif
                if (this.m_isDeepMonoProfiler != value)
                {
                    this.m_isDeepMonoProfiler = value;
                    this.Save();
                }
            }
        }

        public bool isDeepLuaProfiler
        {
            get
            {
#if UNITY_EDITOR
                return LuaDeepProfilerAssetSetting.Instance.isDeepLuaProfiler;
#else
                return this.m_isDeepLuaProfiler;
#endif
            }
            set
            {
#if UNITY_EDITOR
                LuaDeepProfilerAssetSetting.Instance.isDeepLuaProfiler = value;
#endif
                if (this.m_isDeepLuaProfiler != value)
                {
                    this.m_isDeepLuaProfiler = value;
                    this.Save();
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
                    this.Save();
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
                    this.Save();
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
                    this.Save();
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
                    this.Save();
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
                    this.Save();
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
                    this.Save();
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
                    this.Save();
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
                    this.Save();
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
                    this.Save();
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
                    this.Save();
                }
            }
        }

        public void Save()
        {
#if UNITY_EDITOR
            string text = "Assets/Resources/LuaDeepProfilerSettings.bytes";

            if (!Directory.Exists("Assets/Resources"))
            {
                Directory.CreateDirectory("Assets/Resources");
            }

            FileStream output = new FileStream(text, FileMode.Create);
            BinaryWriter binaryWriter = new BinaryWriter(output);
            binaryWriter.Write(this.m_isDeepMonoProfiler);
            binaryWriter.Write(this.m_isDeepLuaProfiler);
            binaryWriter.Write(this.m_captureLuaGC);
            binaryWriter.Write(this.m_isInited);
            binaryWriter.Write(this.m_isNeedCapture);
            binaryWriter.Write(this.m_captureMonoGC);
            binaryWriter.Write(this.m_captureFrameRate);
            byte[] bytes = Encoding.UTF8.GetBytes(this.m_assMd5);
            binaryWriter.Write(bytes.Length);
            binaryWriter.Write(bytes);

            bytes = Encoding.UTF8.GetBytes(this.m_ip);
            binaryWriter.Write(bytes.Length);
            binaryWriter.Write(bytes);
            binaryWriter.Write(this.m_port);
            binaryWriter.Write(m_discardInvalid);
            binaryWriter.Write(m_isCleanMode);
            output.Flush();
            binaryWriter.Close();
#endif
        }

        // Token: 0x060000C9 RID: 201 RVA: 0x00006674 File Offset: 0x00004A74
        public static LuaDeepProfilerSetting Load()
        {
            LuaDeepProfilerSetting luaDeepProfilerSetting = new LuaDeepProfilerSetting();
            byte[] datas = null;
#if UNITY_EDITOR
            string text = "Assets/Resources/LuaDeepProfilerSettings.bytes";
            if (!File.Exists(text))
            {
                luaDeepProfilerSetting.Save();
            }
            datas = File.ReadAllBytes(text);
#else
            TextAsset textAsset = null;
            string path = Application.persistentDataPath + "/LuaDeepProfilerSettings.bytes";
            if (File.Exists(path))
            {
                datas = File.ReadAllBytes(path);
            }
            else
            {
                textAsset = Resources.Load<TextAsset>("LuaDeepProfilerSettings");
                datas = textAsset != null ? textAsset.bytes : null;
            }
#endif

            if (datas != null)
            {
                MemoryStream memoryStream = new MemoryStream(datas);
                try
                {
                    BinaryReader binaryReader = new BinaryReader(memoryStream);
                    luaDeepProfilerSetting.m_isDeepMonoProfiler = binaryReader.ReadBoolean();
                    luaDeepProfilerSetting.m_isDeepLuaProfiler = binaryReader.ReadBoolean();
                    luaDeepProfilerSetting.m_captureLuaGC = binaryReader.ReadInt32();
                    luaDeepProfilerSetting.m_isInited = binaryReader.ReadBoolean();
                    luaDeepProfilerSetting.m_isNeedCapture = binaryReader.ReadBoolean();
                    luaDeepProfilerSetting.m_captureMonoGC = binaryReader.ReadInt32();
                    luaDeepProfilerSetting.m_captureFrameRate = binaryReader.ReadInt32();
                    int count = binaryReader.ReadInt32();
                    luaDeepProfilerSetting.m_assMd5 = Encoding.UTF8.GetString(binaryReader.ReadBytes(count));
                    count = binaryReader.ReadInt32();
                    luaDeepProfilerSetting.m_ip = Encoding.UTF8.GetString(binaryReader.ReadBytes(count));
                    luaDeepProfilerSetting.m_port = binaryReader.ReadInt32();
                    luaDeepProfilerSetting.m_discardInvalid = binaryReader.ReadBoolean();
                    luaDeepProfilerSetting.m_isCleanMode = binaryReader.ReadBoolean();
                    binaryReader.Close();
                }
                catch
                {
#if UNITY_EDITOR
                    memoryStream.Dispose();
                    File.Delete(text);
                    return LuaDeepProfilerSetting.Load();
#endif
                }
            }
            else
            {
                luaDeepProfilerSetting.Save();
            }
#if !UNITY_EDITOR
            if (!File.Exists(path))
            {
                File.WriteAllBytes(path, datas);
            }
            if (textAsset != null)
            {
                Resources.UnloadAsset(textAsset);
            }
#endif
            return luaDeepProfilerSetting;
        }

        public const string SettingsAssetName = "LuaDeepProfilerSettings";
        private static LuaDeepProfilerSetting instance;
        private bool m_isDeepMonoProfiler = false;
        private bool m_isDeepLuaProfiler = false;
        private bool m_isCleanMode = false;
        private int m_captureLuaGC = 51200;
        private bool m_isNeedCapture = false;
        private bool m_discardInvalid = true;
        private string m_assMd5 = "";
        private bool m_isInited = false;
        private int m_captureMonoGC = 51200;
        private int m_captureFrameRate = 30;
        private string m_ip = "127.0.0.1";
        private int m_port = 2333;
    }
}
#endif