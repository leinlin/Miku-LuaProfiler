/*
* ==============================================================================
* Filename: LuaExport
* Created:  2018/7/13 14:29:22
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using UnityEditor;
using System;

namespace MikuLuaProfiler
{
#if UNITY_5_6_OR_NEWER
    public class LuaProfilerWindow : EditorWindow
    {
        [SerializeField] TreeViewState m_TreeViewState;

        LuaProfilerTreeView m_TreeView;
        SearchField m_SearchField;
        int startFrame = 0;
        int endFrame = 0;

        void OnEnable()
        {
            if (m_TreeViewState == null)
                m_TreeViewState = new TreeViewState();

            LuaProfilerTreeView.m_nodeDict.Clear();
            startFrame = 0;
            endFrame = 0;
            m_TreeView = new LuaProfilerTreeView(m_TreeViewState, position.width - 40);
            m_SearchField = new SearchField();
            m_SearchField.downOrUpArrowKeyPressed += m_TreeView.SetFocusAndEnsureSelectedItem;
        }
        void OnGUI()
        {
            DoToolbar();
            DoTreeView();
        }

        private bool m_isStop = false;
        private int m_lastCount = 0;
        void DoToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            #region clear
            bool isClear = GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Height(30));
            if (isClear)
            {
                m_TreeView.Clear(true);
            }
            GUILayout.Space(5);
            #endregion

            #region deep
            bool flag = GUILayout.Toggle(LuaDeepProfilerSetting.Instance.isDeepProfiler,
                "Deep Profiler", EditorStyles.toolbarButton, GUILayout.Height(30));
            if (flag != LuaDeepProfilerSetting.Instance.isDeepProfiler)
            {
                LuaDeepProfilerSetting.Instance.isDeepProfiler = flag;
                EditorApplication.isPlaying = false;
            }
            GUILayout.Space(5);
            #endregion

            #region stop
            bool isStop = GUILayout.Toggle(m_isStop, "Stop GC", EditorStyles.toolbarButton, GUILayout.Height(30));

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

            #region run gc
            bool isRunGC = GUILayout.Button("Run GC", EditorStyles.toolbarButton, GUILayout.Height(30));
            if (isRunGC)
            {
                LuaLib.RunGC();
            }
            GUILayout.Space(5);
            #endregion

            #region history
            flag = GUILayout.Toggle(LuaDeepProfilerSetting.Instance.isRecord,
                "Record", EditorStyles.toolbarButton, GUILayout.Height(30));
            if (flag != LuaDeepProfilerSetting.Instance.isRecord)
            {
                LuaDeepProfilerSetting.Instance.isRecord = flag;
                EditorApplication.isPlaying = false;
            }

            GUILayout.Space(5);
            if (LuaDeepProfilerSetting.Instance.isRecord)
            {
                int count = m_TreeView.history.Count - 1;
                int delta = Mathf.Max(0, count - m_lastCount);
                endFrame += delta;

                int oldStartFrame = startFrame;
                GUILayout.Label("start", EditorStyles.toolbarButton, GUILayout.Height(30));
                startFrame = EditorGUILayout.IntSlider(startFrame, 0, count, GUILayout.Width(150));

                GUILayout.Space(25);

                int oldEndFrame = endFrame;
                GUILayout.Label("end", EditorStyles.toolbarButton, GUILayout.Height(30));
                endFrame = EditorGUILayout.IntSlider(endFrame, 0, count, GUILayout.Width(150));

                m_lastCount = count;

                if (oldStartFrame != startFrame || oldEndFrame != endFrame)
                {
                    m_TreeView.ReLoadSamples(startFrame, endFrame);
                    if(EditorApplication.isPlaying)
                        EditorApplication.isPaused = true;
                }

                bool isSave = GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Height(30));
                if (isSave)
                {
                    m_TreeView.SaveHisotry();
                }

                bool isLoad = GUILayout.Button("Load", EditorStyles.toolbarButton, GUILayout.Height(30));
                if (isLoad)
                {
                    m_TreeView.LoadHistory();
                }
            }
            GUILayout.Space(5);
            GUILayout.FlexibleSpace();
            #endregion

            #region gc value
            GUILayout.Label(string.Format("Lua Total:{0}", LuaProfiler.GetLuaMemory()), EditorStyles.toolbarButton, GUILayout.Height(30));
            #endregion

            GUILayout.Space(100);
            GUILayout.FlexibleSpace();
            //m_TreeView.searchString = m_SearchField.OnToolbarGUI(m_TreeView.searchString);
            EditorGUILayout.EndHorizontal();
        }

        void DoTreeView()
        {
            Rect rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);
            m_TreeView.Reload();
            m_TreeView.OnGUI(rect);
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
