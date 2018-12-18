#define XLUA
/*
* ==============================================================================
* Filename: StartUp
* Created:  2018/7/2 11:36:16
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/

using UnityEditor;
using UnityEngine;
using System.IO;


namespace MikuLuaProfiler
{

    [InitializeOnLoad]
    public static class StartUp
    {
        static StartUp()
        {
            if (LuaDeepProfilerSetting.Instance.isDeepMonoProfiler)
            {
                InjectMethods.InjectAllMethods();
            }
#if XLUA || TOLUA || SLUA
            if (LuaDeepProfilerSetting.Instance.isInited) return;
#endif
            string[] paths = Directory.GetFiles(Application.dataPath, "*.dll", SearchOption.AllDirectories);
            foreach (var item in paths)
            {
                string fileName = Path.GetFileName(item);
                if (fileName == "slua.dll")
                {
                    AppendMacro("#define SLUA");
                }

                if (fileName == "xlua.dll")
                {
                    AppendMacro("#define XLUA");
                    break;
                }

                if (fileName == "tolua.dll")
                {
                    AppendMacro("#define TOLUA");
                    break;
                }
            }

            LuaDeepProfilerSetting.Instance.isInited = true;
        }

        private static void AppendMacro(string macro)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
            System.Diagnostics.StackFrame sf = st.GetFrame(0);
            string path = sf.GetFileName();
            string selfPath = path;

#if UNITY_EDITOR_WIN
            path = path.Replace("Editor\\StartUp.cs", "Core\\LuaHookSetup.cs");
#else
            path = path.Replace("Editor/StartUp.cs", "Core/LuaHookSetup.cs");
#endif
            AppendText(macro, selfPath);
            AppendText(macro, path);
        }

        private static void AppendText(string macro, string path)
        {
            string text = File.ReadAllText(path);
            string text2 = new StringReader(text).ReadLine();
            if (text2.Contains("#define"))
            {
                text = text.Substring(text2.Length, text.Length - text2.Length);
            }
            else
            {
                macro += "\r\n";
            }
            text = text.Insert(0, macro);
            File.WriteAllText(path, text);
        }
    }

}