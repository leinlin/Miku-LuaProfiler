/*
* ==============================================================================
* Filename: LuaDeepProfilerSetting
* Created:  2018/7/13 14:29:22
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/
#if UNITY_EDITOR
namespace MikuLuaProfiler
{
    using System.IO;
    using UnityEditor;
    using UnityEngine;
    using System.Text;
    using System;

    public class LuaDeepProfilerSetting
    {
        public const string SettingsAssetName = "LuaDeepProfilerSettings.config";
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

        private bool m_isDeepProfiler = false;
        public bool isDeepProfiler
        {
            get
            {
                return m_isDeepProfiler;
            }
            set
            {
                if (m_isDeepProfiler == value) return;
                m_isDeepProfiler = value;
                if (value && LuaDeepProfilerSetting.Instance.isRecord)
                {
                    GameViewUtility.ChangeGameViewSize(480, 270);
                }
                Save();
            }
        }

        private int m_captureLuaGC = 50 * 1024;
        public int captureLuaGC
        {
            get
            {
                return m_captureLuaGC;
            }
            set
            {
                if (m_captureLuaGC == value) return;
                m_captureLuaGC = value;
                Save();
            }
        }

        private bool m_profilerMono = true;
        public bool profilerMono
        {
            get
            {
                return m_profilerMono;
            }
            set
            {
                if (m_profilerMono == value) return;
                m_profilerMono = value;
                Save();
            }
        }

        private bool m_isRecord = false;
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

        private bool m_isNeedRecord = true;
        public bool isNeedRecord
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

        private string m_assMd5 = "";
        public string assMd5
        {
            get
            {
                return m_assMd5;
            }
            set
            {
                if (m_assMd5 == value) return;
                m_assMd5 = value;
                Save();
            }
        }

        private bool m_isInited = false;
        public bool isInited
        {
            get
            {
                return m_isInited;
            }

            set
            {
                if (m_isInited == value) return;
                m_isInited = value;
                Save();
            }
        }

        private string m_luaDir = "";
        public string luaDir
        {
            get
            {
                return m_luaDir;
            }
            set
            {
                if (m_luaDir == value) return;
                m_luaDir = value;
                Save();
            }
        }

        private string m_luaIDE = "";
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

        private int m_captureMonoGC = 50 * 1024;
        public int captureMonoGC
        {
            get
            {
                return m_captureMonoGC;
            }
            set
            {
                if (m_captureMonoGC == value) return;
                m_captureMonoGC = value;
                Save();
            }
        }

        private int m_captureFrameRate = 30;
        public int captureFrameRate
        {
            get
            {
                return m_captureFrameRate;
            }
            set
            {
                if (m_captureFrameRate == value) return;
                m_captureFrameRate = value;
                Save();
            }
        }

        public void Save()
        {
            FileStream fs = new FileStream(SettingsAssetName, FileMode.Create);
            BinaryWriter b = new BinaryWriter(fs);

            b.Write(m_isDeepProfiler);
            b.Write(m_captureLuaGC);
            b.Write(m_profilerMono);
            b.Write(false);
            b.Write(m_isRecord);
            b.Write(m_isNeedRecord);

            byte[] datas = Encoding.UTF8.GetBytes(m_assMd5);
            b.Write(datas.Length);
            b.Write(datas);

            b.Write(m_isInited);

            datas = Encoding.UTF8.GetBytes(m_luaDir);
            b.Write(datas.Length);
            b.Write(datas);

            datas = Encoding.UTF8.GetBytes(m_luaIDE);
            b.Write(datas.Length);
            b.Write(datas);

            b.Write(m_captureMonoGC);
            b.Write(m_captureFrameRate);

            b.Close();
        }

        public static LuaDeepProfilerSetting Load()
        {
            LuaDeepProfilerSetting result = new LuaDeepProfilerSetting(); 

            if (File.Exists(SettingsAssetName))
            {
                FileStream fs = new FileStream(SettingsAssetName, FileMode.OpenOrCreate);
                try
                {
                    BinaryReader b = new BinaryReader(fs);

                    result.m_isDeepProfiler = b.ReadBoolean();
                    result.m_captureLuaGC = b.ReadInt32();
                    result.m_profilerMono = b.ReadBoolean();
                    b.ReadBoolean();
                    result.m_isRecord = b.ReadBoolean();
                    result.m_isNeedRecord = b.ReadBoolean();

                    int len = b.ReadInt32();
                    result.m_assMd5 = Encoding.UTF8.GetString(b.ReadBytes(len));
                    result.m_isInited = b.ReadBoolean();

                    len = b.ReadInt32();
                    result.m_luaDir = Encoding.UTF8.GetString(b.ReadBytes(len));

                    len = b.ReadInt32();
                    result.m_luaIDE = Encoding.UTF8.GetString(b.ReadBytes(len));

                    result.m_captureMonoGC = b.ReadInt32();
                    result.m_captureFrameRate = b.ReadInt32();

                    b.Close();
                }
                catch
                {
                    fs.Dispose();
                    File.Delete(SettingsAssetName);
                    return Load();
                }
            }
            else
            {
                result.Save();
            }

            return result;
        }

        //[MenuItem("LuaProfiler/ExportFiles", priority = 10)]
        public static void EditSettings()
        {
            string path = EditorUtility.OpenFolderPanel("请选择Lua脚本存放文件夹", "", "*");
#if UNITY_EDITOR_WIN
            path = path.Replace("/", "\\");
#endif
            string rootProfilerDirPath = path + "Profiler";
            DirectoryInfo dir = new DirectoryInfo(path);
            FileInfo[] files = dir.GetFiles("*.lua", SearchOption.AllDirectories);
            int count = files.Length;
            int process = 0;

            //实例化一个计时器
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

            foreach (FileInfo item in files)
            {
                process++;

                EditorUtility.DisplayProgressBar("profiler lua", item.FullName, (float)process / count);
                string allCode = File.ReadAllText(item.FullName);
                watch.Start();
                allCode = Parse.InsertSample(allCode, "Template");
                watch.Stop();
                string profilerPath = item.FullName.Replace(path, rootProfilerDirPath);
                string profilerDirPath = profilerPath.Replace(item.Name, "");
                if (!Directory.Exists(profilerDirPath))
                {
                    Directory.CreateDirectory(profilerDirPath);
                }
                File.WriteAllText(profilerPath, allCode);
            }
            Debug.LogFormat("cost time: {0} ms", watch.ElapsedMilliseconds);

            EditorUtility.ClearProgressBar();
        }
    }
}
#endif