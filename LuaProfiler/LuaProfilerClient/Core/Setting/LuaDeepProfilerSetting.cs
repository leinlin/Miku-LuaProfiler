/*
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
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using UnityEngine;

    public class LuaDeepProfilerSetting
    {
        public static LuaDeepProfilerSetting Instance
        {
            get
            {
                if (LuaDeepProfilerSetting.instance == null)
                {
                    LuaDeepProfilerSetting.instance = LuaDeepProfilerSetting.Load();
                }
                return LuaDeepProfilerSetting.instance;
            }
        }

        public bool isDeepMonoProfiler
        {
            get
            {
                return this.m_isDeepMonoProfiler;
            }
            set
            {
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
                return this.m_isDeepLuaProfiler;
            }
            set
            {
                if (this.m_isDeepLuaProfiler != value)
                {
                    this.m_isDeepLuaProfiler = value;
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
            StackTrace stackTrace = new StackTrace(true);
            StackFrame frame = stackTrace.GetFrame(0);
            string text = frame.GetFileName();
#if UNITY_EDITOR_WIN
            text = text.Replace("Core\\Setting\\LuaDeepProfilerSetting.cs", "Resources\\LuaDeepProfilerSettings.bytes");
#else
            text = text.Replace("Core/Setting/LuaDeepProfilerSetting.cs", "Resources/LuaDeepProfilerSettings.bytes");
#endif
            if (!Directory.Exists(Path.GetDirectoryName(text)))
            {
                Directory.CreateDirectory(text);
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
            binaryWriter.Close();
        }

        // Token: 0x060000C9 RID: 201 RVA: 0x00006674 File Offset: 0x00004A74
        public static LuaDeepProfilerSetting Load()
        {
            LuaDeepProfilerSetting luaDeepProfilerSetting = new LuaDeepProfilerSetting();
            TextAsset textAsset = Resources.Load<TextAsset>("LuaDeepProfilerSettings");
            if (textAsset != null)
            {
                MemoryStream memoryStream = new MemoryStream(textAsset.bytes);
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
                    binaryReader.Close();
                }
                catch
                {
                    memoryStream.Dispose();
                    File.Delete("LuaDeepProfilerSettings");
                    return LuaDeepProfilerSetting.Load();
                }
            }
            else
            {
                luaDeepProfilerSetting.Save();
            }
            return luaDeepProfilerSetting;
        }

        // Token: 0x040000AF RID: 175
        public const string SettingsAssetName = "LuaDeepProfilerSettings";

        // Token: 0x040000B0 RID: 176
        private static LuaDeepProfilerSetting instance;

        // Token: 0x040000B1 RID: 177
        private bool m_isDeepMonoProfiler = false;

        // Token: 0x040000B2 RID: 178
        private bool m_isDeepLuaProfiler = false;

        // Token: 0x040000B3 RID: 179
        private int m_captureLuaGC = 51200;

        // Token: 0x040000B4 RID: 180
        private bool m_isNeedCapture = false;

        // Token: 0x040000B5 RID: 181
        private string m_assMd5 = "";

        // Token: 0x040000B6 RID: 182
        private bool m_isInited = false;

        // Token: 0x040000B9 RID: 185
        private int m_captureMonoGC = 51200;

        // Token: 0x040000BA RID: 186
        private int m_captureFrameRate = 30;

        // Token: 0x040000BB RID: 187
        private string m_ip = "127.0.0.1";

        // Token: 0x040000BC RID: 188
        private int m_port = 2333;
    }
}
#endif