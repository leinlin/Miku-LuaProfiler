/*
* ==============================================================================
* Filename: LuaDeepProfilerSetting
* Created:  2018/7/13 14:29:22
* Author:   エル・プサイ・コングルゥ
* Purpose:  
* ==============================================================================
*/
#if UNITY_EDITOR
namespace MikuLuaProfiler
{
    using System.Collections.Generic;
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
            }
        }

        //[MenuItem("Tools/LuaProfilerSetting", priority = 10)]
        public static void EditSettings()
        {
            //string text = System.IO.File.ReadAllText("Lua/TemplateCommon.lua");
            //text = MikuLuaProfiler.Parse.InsertSample(text, "Template");
            //System.IO.File.WriteAllText("TemplateCommon.lua", text);

            Selection.activeObject = Instance;
#if UNITY_2018_1_OR_NEWER
            EditorApplication.ExecuteMenuItem("Window/General/Inspector");
#else
            EditorApplication.ExecuteMenuItem("Window/Inspector");
#endif
        }
    }
}
#endif