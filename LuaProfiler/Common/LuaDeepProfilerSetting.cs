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
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    public class LuaDeepProfilerSetting : ScriptableObject
    {
        public const string SettingsAssetName = "LuaDeepProfilerSettings";
        private static LuaDeepProfilerSetting instance;
        public static LuaDeepProfilerSetting Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = AssetDatabase.LoadAssetAtPath<LuaDeepProfilerSetting>("Assets/" + SettingsAssetName + ".asset");
                    if (instance == null)
                    {
                        Debug.Log("Lua Profiler: cannot find integration settings, creating default settings");
                        instance = CreateInstance<LuaDeepProfilerSetting>();
                        instance.name = "Lua Profiler Integration Settings";
#if UNITY_EDITOR
                        AssetDatabase.CreateAsset(instance, "Assets/" + SettingsAssetName + ".asset");
#endif
                    }
                }
                return instance;
            }
        }
        [SerializeField]
        private bool m_isDeepProfiler = false;
        public bool isDeepProfiler
        {
            get
            {
                return m_isDeepProfiler;
            }
            set
            {
                m_isDeepProfiler = value;
                if (value && LuaDeepProfilerSetting.Instance.isRecord)
                {
                    GameViewUtility.ChangeGameViewSize(480, 270);
                }
            }
        }

        [SerializeField]
        private int m_captureGC = 50 * 1024;
        public int captureGC
        {
            get
            {
                return m_captureGC;
            }
            set
            {
                if (m_captureGC == value) return;
                m_captureGC = value;
                EditorUtility.SetDirty(this);
            }
        }

        [SerializeField]
        private bool m_isRecord = true;
        public bool isRecord
        {
            get
            {
                return m_isRecord;
            }
            set
            {
                m_isRecord = value;
            }
        }

        [SerializeField]
        private bool m_isNeedRecord = true;
        public bool isNeedRecord
        {
            get
            {
                return m_isNeedRecord;
            }
            set
            {
                m_isNeedRecord = value;
            }
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
                allCode = MikuLuaProfiler.Parse.InsertSample(allCode, "Template");
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
            Selection.activeObject = Instance;
#if UNITY_2018_2_OR_NEWER
            EditorApplication.ExecuteMenuItem("Window/General/Inspector");
#else
            EditorApplication.ExecuteMenuItem("Window/Inspector");
#endif
        }
    }
}
#endif