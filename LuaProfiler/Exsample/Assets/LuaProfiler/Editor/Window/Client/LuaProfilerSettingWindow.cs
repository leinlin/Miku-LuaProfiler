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
* Filename: LuaProfilerWindow
* Created:  2018/7/13 14:29:22
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/

#if !UNITY_5_6_OR_NEWER
/*
* ==============================================================================
* Filename: LuaProfilerWindow
* Created:  2018/7/13 14:29:22
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/
using UnityEngine;
using UnityEditor;

namespace MikuLuaProfiler
{
    public class LuaProfilerSettingWindow : EditorWindow
    {
        void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            DoToolbar();
            EditorGUILayout.EndVertical();
        }

        void DoToolbar()
        {
            var setting = LuaDeepProfilerSetting.Instance;

#region profiler settting
            GUILayout.Label("profiler setting");
            GUILayout.BeginVertical("Box");

            EditorGUILayout.BeginHorizontal();
            bool flag = GUILayout.Toggle(setting.isDeepLuaProfiler, "Deep Profiler Lua");
            if (flag != setting.isDeepLuaProfiler)
            {
                setting.isDeepLuaProfiler = flag;
                if (!flag)
                {
                    setting.isCleanMode = false;
                }
                EditorApplication.isPlaying = false;
            }

            flag = GUILayout.Toggle(setting.isDeepMonoProfiler, "Deep Profiler Mono");
            if (flag != setting.isDeepMonoProfiler)
            {
                setting.isDeepMonoProfiler = flag;
                EditorApplication.isPlaying = false;
                InjectMethods.Recompile();
            }

            flag = GUILayout.Toggle(setting.discardInvalid, "Discard Invalid");
            if (flag != setting.discardInvalid)
            {
                setting.discardInvalid = flag;
            }

            flag = GUILayout.Toggle(setting.isCleanMode, "PreCompile Lua(Use InjectLua.exe)");
            if (flag != setting.isCleanMode)
            {
                setting.isCleanMode = flag;
                if (setting.isCleanMode)
                {
                    setting.isDeepLuaProfiler = true;
                }
                EditorApplication.isPlaying = false;
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);
            if (GUILayout.Button("ReCompile", GUILayout.Height(50)))
            {
                InjectMethods.Recompile();
            }
            GUILayout.Space(5);

            GUILayout.EndVertical();
#endregion

#region socket
            GUILayout.Space(10);
            GUILayout.Label("connet");

            GUILayout.BeginVertical("Box");
            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("ip:", GUILayout.Height(30), GUILayout.Width(35));
            setting.ip = EditorGUILayout.TextField(setting.ip, GUILayout.Height(16), GUILayout.Width(100));

            GUILayout.Label("port:", GUILayout.Height(30), GUILayout.Width(35));
            setting.port = EditorGUILayout.IntField(setting.port, GUILayout.Height(16), GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();
            /*
            if (GUILayout.Button("Connent", GUILayout.Height(50)))
            {
                NetWorkClient.ConnectServer(setting.ip, setting.port);
            }
            GUILayout.Space(5);*/

            GUILayout.EndVertical();
#endregion

#region capture
            /*
            GUILayout.Space(10);

            GUILayout.Label("capture setting");
            GUILayout.BeginVertical("Box");

            flag = GUILayout.Toggle(LuaDeepProfilerSetting.Instance.isNeedCapture, "NeedCapture");
            if (flag != LuaDeepProfilerSetting.Instance.isNeedCapture)
            {
                LuaDeepProfilerSetting.Instance.isNeedCapture = flag;
                if (flag)
                {
                    GameViewUtility.ChangeGameViewSize(480, 270);
                }
            }

            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("lua gc", GUILayout.Height(30), GUILayout.Width(45));
            LuaDeepProfilerSetting.Instance.captureLuaGC
                = EditorGUILayout.IntField(LuaDeepProfilerSetting.Instance.captureLuaGC, GUILayout.Height(16), GUILayout.Width(50));
            LuaDeepProfilerSetting.Instance.captureLuaGC = Mathf.Max(0, LuaDeepProfilerSetting.Instance.captureLuaGC);
            GUILayout.Label("B", GUILayout.Height(30), GUILayout.Width(20));

            GUILayout.Label("mono gc", GUILayout.Height(30), GUILayout.Width(55));
            LuaDeepProfilerSetting.Instance.captureMonoGC
                = EditorGUILayout.IntField(LuaDeepProfilerSetting.Instance.captureMonoGC, GUILayout.Height(16), GUILayout.Width(50));
            LuaDeepProfilerSetting.Instance.captureMonoGC = Mathf.Max(0, LuaDeepProfilerSetting.Instance.captureMonoGC);
            GUILayout.Label("B", GUILayout.Height(30), GUILayout.Width(20));

            GUILayout.Label("capture ", GUILayout.Height(30), GUILayout.Width(50));
            LuaDeepProfilerSetting.Instance.captureFrameRate
                = EditorGUILayout.IntField(LuaDeepProfilerSetting.Instance.captureFrameRate, GUILayout.Height(16), GUILayout.Width(50));
            LuaDeepProfilerSetting.Instance.captureFrameRate = Mathf.Max(0, LuaDeepProfilerSetting.Instance.captureFrameRate);
            GUILayout.Label("FPS", GUILayout.Height(30), GUILayout.Width(30));

            EditorGUILayout.EndHorizontal();

            GUILayout.EndVertical();
            */
#endregion

        }

        // Add menu named "My Window" to the Window menu
        [MenuItem("Window/Lua Profiler Setting")]
        static public void ShowWindow()
        {
            //InjectMethods.ChangeAttribute();
            // Get existing open window or if none, make a new one:

            var window = GetWindow<LuaProfilerSettingWindow>();
            window.titleContent = new GUIContent("Lua Profiler");
            window.Show();
        }
    }
}
#endif