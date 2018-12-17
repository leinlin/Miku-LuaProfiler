/*
* ==============================================================================
* Filename: LuaProfilerWindow
* Created:  2018/7/13 14:29:22
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using UnityEditor;
using System;
using System.IO;

namespace MikuLuaProfiler
{
#if UNITY_5_6_OR_NEWER
    public class LuaProfilerWindow : EditorWindow
    {
        private bool m_isStop = false;
        private string ip = "127.0.0.1";
        private int port = 23333;
        void OnEnable()
        {
        }

        void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            DoToolbar();
            EditorGUILayout.EndVertical();
        }

        void DoToolbar()
        {

            #region deep
            bool flag = GUILayout.Toggle(LuaDeepProfilerSetting.Instance.isDeepProfiler, "Deep Profiler Lua");
            if (flag != LuaDeepProfilerSetting.Instance.isDeepProfiler)
            {
                LuaDeepProfilerSetting.Instance.isDeepProfiler = flag;
                EditorApplication.isPlaying = false;
                InjectMethods.Recompile();
            }
            GUILayout.Space(5);

            flag = GUILayout.Toggle(LuaDeepProfilerSetting.Instance.profilerMono, "Include Mono");
            if (flag != LuaDeepProfilerSetting.Instance.profilerMono)
            {
                LuaDeepProfilerSetting.Instance.profilerMono = flag;
                EditorApplication.isPlaying = false;
                InjectMethods.Recompile();
            }
            GUILayout.Space(5);
            #endregion

            #region stop
            bool isStop = GUILayout.Toggle(m_isStop, "Stop GC");

            if (m_isStop != isStop)
            {
                if (isStop)
                {
                    LuaLib.StopGC();
                    m_isStop = true;
                }
                else
                {
                    LuaLib.ResumeGC();
                    m_isStop = false;
                }
            }
            GUILayout.Space(5);
            #endregion

            #region history
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

            flag = GUILayout.Toggle(LuaDeepProfilerSetting.Instance.isNeedCapture, "NeedCapture");
            if (flag != LuaDeepProfilerSetting.Instance.isNeedCapture)
            {
                LuaDeepProfilerSetting.Instance.isNeedCapture = flag;
                if (flag)
                {
                    GameViewUtility.ChangeGameViewSize(480, 270);
                }
            }

            GUILayout.Space(25);
            #endregion

            #region socket
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("ip:", GUILayout.Height(30), GUILayout.Width(35));
            ip = EditorGUILayout.TextField(ip, GUILayout.Height(16), GUILayout.Width(100));

            GUILayout.Label("port:", GUILayout.Height(30), GUILayout.Width(35));
            port = EditorGUILayout.IntField(port, GUILayout.Height(16), GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Connent", GUILayout.Height(50)))
            {
                NetWorkClient.ConnectServer(ip, port);
            }
            GUILayout.Space(5);

            if (GUILayout.Button("Disconnent", GUILayout.Height(50)))
            {
                EditorUtility.UnloadUnusedAssetsImmediate();
                NetWorkClient.Close();
            }
            GUILayout.Space(5);

            if (GUILayout.Button("Run GC", GUILayout.Height(50)))
            {
                LuaLib.RunGC();
            }
            GUILayout.Space(5);

            if (GUILayout.Button("Inject", GUILayout.Height(50)))
            {
                InjectMethods.InjectAllMethods();
                Debug.Log("Inject Success");
            }
            GUILayout.Space(5);
            if (GUILayout.Button("ReCompile", GUILayout.Height(50)))
            {
                InjectMethods.Recompile();
            }

            GUILayout.Space(5);
            #endregion

        }

        // Add menu named "My Window" to the Window menu
        [MenuItem("Window/Lua Profiler Window")]
        static public void ShowWindow()
        {
            // Get existing open window or if none, make a new one:
            var window = GetWindow<LuaProfilerWindow>();
            window.titleContent = new GUIContent("Lua Profiler");
            window.Show();
        }
    }
#endif
}
