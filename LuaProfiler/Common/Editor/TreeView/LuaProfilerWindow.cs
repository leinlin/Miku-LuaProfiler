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
        [SerializeField] TreeViewState m_TreeViewState;

        LuaProfilerTreeView m_TreeView;
        SearchField m_SearchField;
        int startFrame = 0;
        int endFrame = 0;
        int sortColIndex = -1;
        bool isAscending = false;
        private bool m_isStop = false;
        private int m_lastCount = 0;

        private string oldStartUrl = null;
        private string oldEndUrl = null;
        private Texture2D oldStartT = null;
        private Texture2D oldEndT = null;

        void OnEnable()
        {
            if (m_TreeViewState == null)
                m_TreeViewState = new TreeViewState();

            LuaProfilerTreeView.m_nodeDict.Clear();
            startFrame = 0;
            endFrame = 0;
            m_TreeView = new LuaProfilerTreeView(m_TreeViewState, position.width - 40);
            if (sortColIndex > 0)
            {
                m_TreeView.multiColumnHeader.SetSorting(sortColIndex, isAscending);
            }
            oldStartUrl = null;
            oldEndUrl = null;
            Destory(oldStartT);
            Destory(oldEndT);
            m_SearchField = new SearchField();
            m_SearchField.downOrUpArrowKeyPressed += m_TreeView.SetFocusAndEnsureSelectedItem;
        }

        void Destory(UnityEngine.Object o)
        {
            if (o == null) return;
            if (Application.isPlaying)
            {
                GameObject.Destroy(o);
            }
            else
            {
                GameObject.DestroyImmediate(o);
            }
        }
        void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            DoToolbar();
            GUILayout.Space(20);
            DoRecord();
            DoCapture();
            DoTreeView();
            EditorGUILayout.EndVertical();
        }

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
                "Deep Profiler Lua", EditorStyles.toolbarButton, GUILayout.Height(30));
            if (flag != LuaDeepProfilerSetting.Instance.isDeepProfiler)
            {
                LuaDeepProfilerSetting.Instance.isDeepProfiler = flag;
                EditorApplication.isPlaying = false;
                InjectMethods.Recompile();
            }
            GUILayout.Space(5);

            flag = GUILayout.Toggle(LuaDeepProfilerSetting.Instance.profilerMono,
                "Include Mono", EditorStyles.toolbarButton, GUILayout.Height(30));
            if (flag != LuaDeepProfilerSetting.Instance.profilerMono)
            {
                LuaDeepProfilerSetting.Instance.profilerMono = flag;
                EditorApplication.isPlaying = false;
                InjectMethods.Recompile();
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

            if (GUILayout.Button("Inject", EditorStyles.toolbarButton, GUILayout.Height(30)))
            {
                InjectMethods.InjectAllMethods();
                Debug.Log("Inject Success");
            }

            if (GUILayout.Button("ReCompile", EditorStyles.toolbarButton, GUILayout.Height(30)))
            {
                InjectMethods.Recompile();
            }

            GUILayout.Space(5);
            #endregion

            #region history
            flag = GUILayout.Toggle(LuaDeepProfilerSetting.Instance.isRecord, "Record", EditorStyles.toolbarButton, GUILayout.Height(30));
            if (flag != LuaDeepProfilerSetting.Instance.isRecord)
            {
                LuaDeepProfilerSetting.Instance.isRecord = flag;
                if (flag)
                {
                    GameViewUtility.ChangeGameViewSize(480, 270);
                }
                EditorApplication.isPlaying = false;
            }



            GUILayout.Space(25);
            #endregion

            #region path
            if (GUILayout.Button("Lua Path", EditorStyles.toolbarButton, GUILayout.Height(30)))
            {
                LocalToLuaIDE.SetExternalProjectRootPath();
            }
            if (GUILayout.Button("IDE Path", EditorStyles.toolbarButton, GUILayout.Height(30)))
            {
                LocalToLuaIDE.SetExternalEditorPath();
            }
            if (GUILayout.Button("Clear Path", EditorStyles.toolbarButton, GUILayout.Height(30)))
            {
                LocalToLuaIDE.ClearPath();
            }
            GUILayout.Space(100);
            if (GUILayout.Button("Save Result", EditorStyles.toolbarButton, GUILayout.Height(30)))
            {
                m_TreeView.SaveResult();
            }
            if (GUILayout.Button("Load Result", EditorStyles.toolbarButton, GUILayout.Height(30)))
            {
                m_TreeView.LoadHistory();
            }
            #endregion

            #region gc value
            GUILayout.Space(5);
            GUILayout.FlexibleSpace();
            GUILayout.Label(string.Format("Lua Total:{0}", m_TreeView.GetLuaMemory()), EditorStyles.toolbarButton, GUILayout.Height(30));

            GUILayout.Space(5);
            GUILayout.FlexibleSpace();
            GUILayout.Label(string.Format("Mono Total:{0}", m_TreeView.GetMonoMemory()), EditorStyles.toolbarButton, GUILayout.Height(30));
            #endregion

            GUILayout.Space(100);
            GUILayout.FlexibleSpace();

            m_TreeView.searchString = m_SearchField.OnToolbarGUI(m_TreeView.searchString);

            EditorGUILayout.EndHorizontal();
        }

        void DoRecord()
        {
            if (!LuaDeepProfilerSetting.Instance.isRecord) return;

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            bool flag = GUILayout.Toggle(LuaDeepProfilerSetting.Instance.isNeedRecord,
    "NeedRecord", EditorStyles.toolbarButton, GUILayout.Height(30));
            if (flag != LuaDeepProfilerSetting.Instance.isNeedRecord)
            {
                LuaDeepProfilerSetting.Instance.isNeedRecord = flag;
            }

            int count = m_TreeView.history.Count - 1;
            int delta = Mathf.Max(0, count - m_lastCount);
            if (delta != 0)
            {
                endFrame = count;
            }

            int oldStartFrame = startFrame;
            GUILayout.Label("start", EditorStyles.toolbarButton, GUILayout.Height(30), GUILayout.Width(45));
            startFrame = EditorGUILayout.IntSlider(startFrame, 0, count, GUILayout.Width(150));
            if (GUILayout.Button("<< ", EditorStyles.toolbarButton, GUILayout.Height(30), GUILayout.Width(30)))
            {
                startFrame = m_TreeView.GetPreProgramFrame(startFrame);
                endFrame = startFrame;
            }
            if (GUILayout.Button("< ", EditorStyles.toolbarButton, GUILayout.Height(30), GUILayout.Width(30)))
            {
                startFrame = Mathf.Max(startFrame - 1, 0);
            }
            if (GUILayout.Button(" >", EditorStyles.toolbarButton, GUILayout.Height(30), GUILayout.Width(30)))
            {
                startFrame = Mathf.Min(startFrame + 1, count);
            }
            if (GUILayout.Button(" >>", EditorStyles.toolbarButton, GUILayout.Height(30), GUILayout.Width(30)))
            {
                startFrame = m_TreeView.GetNextProgramFrame(startFrame);
                endFrame = startFrame;
            }

            int oldEndFrame = endFrame;
            GUILayout.Space(15);

            GUILayout.Label("end", EditorStyles.toolbarButton, GUILayout.Height(30), GUILayout.Width(45));
            endFrame = EditorGUILayout.IntSlider(endFrame, 0, count, GUILayout.Width(150));

            if (GUILayout.Button("<< ", EditorStyles.toolbarButton, GUILayout.Height(30), GUILayout.Width(30)))
            {
                endFrame = m_TreeView.GetPreProgramFrame(endFrame);
                startFrame = endFrame;
            }
            if (GUILayout.Button("< ", EditorStyles.toolbarButton, GUILayout.Height(30), GUILayout.Width(30)))
            {
                endFrame = Mathf.Max(endFrame - 1, 0);
            }
            if (GUILayout.Button(" >", EditorStyles.toolbarButton, GUILayout.Height(30), GUILayout.Width(30)))
            {
                endFrame = Mathf.Min(endFrame + 1, count);
            }
            if (GUILayout.Button(" >>", EditorStyles.toolbarButton, GUILayout.Height(30), GUILayout.Width(30)))
            {
                endFrame = m_TreeView.GetNextProgramFrame(endFrame);
                startFrame = endFrame;
            }

            GUILayout.Space(25);
            GUILayout.Label("capture lua gc", EditorStyles.toolbarButton, GUILayout.Height(30), GUILayout.Width(80));
            LuaDeepProfilerSetting.Instance.captureLuaGC
                = EditorGUILayout.IntField(LuaDeepProfilerSetting.Instance.captureLuaGC, GUILayout.Height(16), GUILayout.Width(50));
            LuaDeepProfilerSetting.Instance.captureLuaGC = Mathf.Max(0, LuaDeepProfilerSetting.Instance.captureLuaGC);

            GUILayout.Label("capture mono gc", EditorStyles.toolbarButton, GUILayout.Height(30), GUILayout.Width(100));
            LuaDeepProfilerSetting.Instance.captureMonoGC
                = EditorGUILayout.IntField(LuaDeepProfilerSetting.Instance.captureMonoGC, GUILayout.Height(16), GUILayout.Width(50));
            LuaDeepProfilerSetting.Instance.captureMonoGC = Mathf.Max(0, LuaDeepProfilerSetting.Instance.captureMonoGC);

            GUILayout.Label("capture frame rate", EditorStyles.toolbarButton, GUILayout.Height(30), GUILayout.Width(100));
            LuaDeepProfilerSetting.Instance.captureFrameRate
                = EditorGUILayout.IntField(LuaDeepProfilerSetting.Instance.captureFrameRate, GUILayout.Height(16), GUILayout.Width(40));
            LuaDeepProfilerSetting.Instance.captureFrameRate = Mathf.Max(0, LuaDeepProfilerSetting.Instance.captureFrameRate);

            m_lastCount = count;

            GUILayout.Space(25);
            GUILayout.FlexibleSpace();

            if (oldStartFrame != startFrame || oldEndFrame != endFrame)
            {
                m_TreeView.ReLoadSamples(startFrame, endFrame);
                if (EditorApplication.isPlaying)
                    EditorApplication.isPaused = true;
            }

            bool isSave = GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Height(30), GUILayout.Width(50));
            if (isSave)
            {
                m_TreeView.SaveHisotry();
            }

            bool isLoad = GUILayout.Button("Load", EditorStyles.toolbarButton, GUILayout.Height(30), GUILayout.Width(50));
            if (isLoad)
            {
                m_TreeView.LoadHistory();
            }

            GUILayout.Space(10);
            EditorGUILayout.EndHorizontal();
        }

        void DoCapture()
        {
            // 游戏进行中别这么搞，闪屏
            if (Application.isPlaying && !EditorApplication.isPaused) return;

            EditorGUILayout.BeginHorizontal();
            if (oldStartUrl != m_TreeView.startUrl)
            {
                if (string.IsNullOrEmpty(m_TreeView.startUrl))
                {
                    Destory(oldStartT);
                    oldStartT = null;
                }
                else if (File.Exists(m_TreeView.startUrl))
                {
                    byte[] datas = File.ReadAllBytes(m_TreeView.startUrl);
                    Texture2D t = new Texture2D(480, 270, TextureFormat.RGB24, false);
                    if (t.LoadImage(datas))
                    {
                        t.wrapMode = TextureWrapMode.Clamp;
                        Destory(oldStartT);
                        oldStartT = t;
                    }
                }
                oldStartUrl = m_TreeView.startUrl;
            }

            if (oldStartT != null)
            {
                GUILayout.Box(oldStartT, GUILayout.Width(480), GUILayout.Height(270));
            }

            GUILayout.Space(15);
            GUILayout.FlexibleSpace();
            if (oldEndUrl != m_TreeView.endUrl)
            {
                if (string.IsNullOrEmpty(m_TreeView.endUrl))
                {
                    Destory(oldEndT);
                    oldEndT = null;
                }
                else if (File.Exists(m_TreeView.endUrl))
                {
                    byte[] datas = File.ReadAllBytes(m_TreeView.endUrl);
                    Texture2D t = new Texture2D(Screen.width, Screen.height);
                    if (t.LoadImage(datas))
                    {
                        t.wrapMode = TextureWrapMode.Clamp;
                        Destory(oldEndT);
                        oldEndT = t;
                    }
                }
                oldEndUrl = m_TreeView.endUrl;
            }

            if (oldEndT != null)
            {
                GUILayout.Box(oldEndT, GUILayout.Width(480), GUILayout.Height(270));
            }

            EditorGUILayout.EndHorizontal();
        }

        void DoTreeView()
        {
            Rect rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);
            m_TreeView.Reload();
            m_TreeView.OnGUI(rect);
            sortColIndex = m_TreeView.multiColumnHeader.sortedColumnIndex;
            if (sortColIndex > 0)
            {
                isAscending = m_TreeView.multiColumnHeader.IsSortedAscending(sortColIndex);
            }
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
