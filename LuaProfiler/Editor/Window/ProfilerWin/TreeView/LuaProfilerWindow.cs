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

#if UNITY_5_6_OR_NEWER && UNITY_EDITOR_WIN
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;

namespace MikuLuaProfiler
{
#if UNITY_5_6_OR_NEWER
    public class LuaProfilerWindow : EditorWindow
    {
        #region field
        [SerializeField] TreeViewState m_TreeViewState;
        private readonly GUILayoutOption[] _frameInfoRectsOption = new GUILayoutOption[]
        {
            GUILayout.ExpandHeight(false),
            GUILayout.ExpandWidth(true),
            GUILayout.Height(18f)
        };
        private readonly GUILayoutOption[] _mainRectsOption = new GUILayoutOption[]
        {
            GUILayout.ExpandHeight(false),
            GUILayout.ExpandWidth(true),
            GUILayout.Height(130f)
        };

        private static readonly Vector3[] CachedVec = new Vector3[2];
        private readonly SplitterState _minmaxSlider = new SplitterState(new int[]
            {
            1,
            1,
            1
            }, new int[]
            {
            1,
            1,
            1
            }, new int[]
            {
            100000,
            100000,
            100000
            }
        );

        private static LuaProfilerTreeView m_TreeView;
        SearchField m_SearchField;
        int startFrame = 0;
        int endFrame = 0;
        int sortColIndex = -1;
        bool isAscending = false;
        private int m_lastCount = 0;
        public static int port = 2333;
        private bool isShowLuaChart = true;
        private bool isShowMonoChart = true;
        private bool isShowFpsChart = true;
        private bool isShowPssChart = false;
        private bool isShowPowerChart = false;
        private bool isTouchInChart = false;
		private bool isShowRef = true;
        static private int currentFrameIndex = 0;

        private Texture2D disableChart;
        private Texture2D luaChart;
        private Texture2D monoChart;
        private Texture2D fpsChart;
        private Texture2D pssChart;
        private Texture2D powrChart;
        private Texture2D boxTex;
        private GUIStyle currentStyle;
        private GUIStyle m_gs;
        private static Color boxColor = new Color32(70, 70, 70, 255);
        private static Color disableColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
        private static Color luaColor = new Color(0.2f, 0.5f, 0.7f, 1.0f);
        private static Color monoColor = new Color32(0, 180, 0, 255);
        private static Color fpsColor = new Color32(255, 193, 37, 255);
        private static Color powerColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
        private static Color pssColor = Color.red;

        private string oldStartUrl = null;
        private string oldEndUrl = null;
        private Texture2D oldStartT = null;
        private Texture2D oldEndT = null;
        public static Action DoClear;

        private static LuaRefScrollView m_luaRefScrollView = null;
        private static LuaDiffScrollView m_luaDiffScrollView = null;
        #endregion

        private static void DoClearTreeView()
        {
            currentFrameIndex = 0;
            m_TreeView.Clear(true);
            m_luaRefScrollView.ClearRefInfo(true);
            m_luaDiffScrollView.Clear();
            ClearConsole();
        }

        public static void ClearTreeView()
        {
            DoClear = new Action(DoClearTreeView);
        }

        public static void ClearConsole()
        {
            // 找到需要 Hook 的方法
#if UNITY_2017_1_OR_NEWER
            var logEntries = System.Type.GetType("UnityEditor.LogEntries,UnityEditor.dll");
#else
            var logEntries = System.Type.GetType("UnityEditorInternal.LogEntries,UnityEditor.dll");
#endif
            var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            clearMethod.Invoke(null, null);
        }

        void OnEnable()
        {
            if (m_TreeViewState == null)
                m_TreeViewState = new TreeViewState();

            if (m_luaRefScrollView == null)
            {
                m_luaRefScrollView = new LuaRefScrollView();
            }

            if (m_luaDiffScrollView == null)
            {
                m_luaDiffScrollView = new LuaDiffScrollView();
            }

            m_SearchField = new SearchField();
            
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
            disableChart = null;
            Destory(disableChart);
            luaChart = null;
            Destory(luaChart);
            monoChart = null;
            Destory(monoChart);
            fpsChart = null;
            Destory(fpsChart);
            pssChart = null;
            Destory(pssChart);
            powrChart = null;
            Destory(powrChart);
            Destory(boxTex);
            boxTex = null;
            m_gs = null;
            currentStyle = null;
            m_SearchField.downOrUpArrowKeyPressed += m_TreeView.SetFocusAndEnsureSelectedItem;
            OpenLocalMode();

            EditorApplication.update -= m_TreeView.DequeueSample;
            EditorApplication.update += m_TreeView.DequeueSample;
            EditorApplication.update -= m_luaRefScrollView.DequeueLuaInfo;
            EditorApplication.update += m_luaRefScrollView.DequeueLuaInfo;
        }

        private void OnDisable()
        {
            Destory(disableChart);
            disableChart = null;
            Destory(luaChart);
            luaChart = null;
            Destory(monoChart);
            monoChart = null;
            Destory(fpsChart);
            fpsChart = null;
            Destory(pssChart);
            pssChart = null;
            Destory(powrChart);
            powrChart = null;
            Destory(boxTex);
            boxTex = null;
            EditorApplication.update -= m_TreeView.DequeueSample;
            EditorApplication.update -= m_luaRefScrollView.DequeueLuaInfo;
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
                GameObject.DestroyImmediate(o, true);
            }
        }

        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            DoToolbar();
            GUILayout.Space(10);
            DoRecord();

            EditorGUILayout.BeginHorizontal();
            DoChartToggle();
            DoChart();
            EditorGUILayout.EndHorizontal();

            m_luaDiffScrollView.DoRefScroll();
            //DoCapture();
            DoTreeView();

            EditorGUILayout.EndVertical();

            if (isShowRef)
            {
                EditorGUILayout.BeginVertical();
                if (GUILayout.Button("log to file"))
                {
                    m_luaRefScrollView.LogToFile();
                }

                //if (LuaDeepProfilerSetting.Instance.isRecord)
                //{
                //    if (GUILayout.Button("log history"))
                //    {
                //        int startGameFrame = m_TreeView.GetFrameCount(startFrame);
                //        int endGameFrame = m_TreeView.GetFrameCount(endFrame);
                //        m_luaRefScrollView.LogRefHistory(startGameFrame, endGameFrame);
                //    }
                //}

                m_luaRefScrollView.DoRefScroll();
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void LoadChartTexture()
        {
            if (disableChart == null)
            {
                disableChart = GenTextureColor(15, 15, disableColor);
            }
            if (luaChart == null)
            {
                luaChart = GenTextureColor(15, 15, luaColor);
            }
            if(monoChart == null)
            {
                monoChart = GenTextureColor(15, 15, monoColor);
            }
            if (fpsChart == null)
            {
                fpsChart = GenTextureColor(15, 15, fpsColor);
            }
            if (pssChart == null)
            {
                pssChart = GenTextureColor(15, 15, pssColor);
            }
            if (powrChart == null)
            {
                powrChart = GenTextureColor(15, 15, powerColor);
            }
        }

        private Texture2D GenTextureColor(int width, int height, Color color) 
        {
            Texture2D t = new Texture2D(width, height);
            for (int i = 0, imax = t.width; i < imax; i++) 
            {
                for (int j = 0, jmax = t.height; j < jmax; j++) 
                {
                    t.SetPixel(i, j, color);
                }
            }
            t.Apply();

            return t;
        }

        void DoToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            #region clear
            bool isClear = GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Height(30));
            if (isClear)
            {
                currentFrameIndex = 0;
                m_TreeView.Clear(true);
                m_luaRefScrollView.ClearRefInfo(true);
                m_luaDiffScrollView.Clear();
                ClearConsole();
            }
            GUILayout.Space(5);
            #endregion

            #region history
            string recordName = "Record";
            bool flag = GUILayout.Toggle(LuaDeepProfilerSetting.Instance.isRecord, recordName, EditorStyles.toolbarButton, GUILayout.Height(30));
            if (flag != LuaDeepProfilerSetting.Instance.isRecord)
            {
                LuaDeepProfilerSetting.Instance.isRecord = flag;
            }
            GUILayout.Space(25);
            #endregion

            var setting = LuaDeepProfilerSetting.Instance;

            #region socket
            bool oldIsLocal = LuaDeepProfilerSetting.Instance.isLocal;
            string localName = "local mode";
            if (!LuaDeepProfilerSetting.Instance.isLocal)
            {
                localName = "remote mode";
            }
            LuaDeepProfilerSetting.Instance.isLocal = GUILayout.Toggle(LuaDeepProfilerSetting.Instance.isLocal, localName, EditorStyles.toolbarButton, GUILayout.Height(30));
            if (!oldIsLocal && LuaDeepProfilerSetting.Instance.isLocal)
            {
                OpenLocalMode();
            }

            if (!LuaDeepProfilerSetting.Instance.isLocal)
            {
                GUILayout.Label("ip:", GUILayout.Height(30), GUILayout.Width(35));
                LuaDeepProfilerSetting.Instance.ip = EditorGUILayout.TextField(LuaDeepProfilerSetting.Instance.ip, GUILayout.Height(16), GUILayout.Width(150));

                GUILayout.Label("port:", GUILayout.Height(30), GUILayout.Width(35));
                port = EditorGUILayout.IntField(port, GUILayout.Height(16), GUILayout.Width(50));

                if (!NetWorkMgrClient.GetIsConnect())
                {
                    if (GUILayout.Button("Connect", GUILayout.Height(20)))
                    {
                        ClearConsole();
                        NetWorkMgrClient.Disconnect();
                        currentFrameIndex = 0;
                        m_TreeView.Clear(true);
                        LuaProfiler.UnRegistReceive();
                        Sample.UnRegAction();
                        LuaRefInfo.UnRegAction();

                        NetWorkMgrClient.Connect(LuaDeepProfilerSetting.Instance.ip, port);
                        Sample.RegAction(m_TreeView.LoadRootSample);
                        LuaRefInfo.RegAction(m_luaRefScrollView.DelRefInfo);
                        //NetWorkServer.RegisterOnReceiveDiffInfo(m_luaDiffScrollView.DelDiffInfo);
                    
                    }
                }
                else
                {
                    if (GUILayout.Button("Disconnect",GUILayout.Height(20)))
                    {
                        ClearConsole();
                        NetWorkMgrClient.Disconnect();
                        UnityEngine.Debug.Log("<color=#ff0000>disconnect</color>");
                    }
                }

            }
            else
            {
                GUILayout.Space(10);
                flag = GUILayout.Toggle(setting.isDeepLuaProfiler, "Deep Lua", EditorStyles.toolbarButton);
                if (flag != setting.isDeepLuaProfiler)
                {
                    setting.isDeepLuaProfiler = flag;
                    if (!flag)
                    {
                        setting.isCleanMode = false;
                    }
                    EditorApplication.isPlaying = false;
                }

                flag = GUILayout.Toggle(setting.discardInvalid, "Discard Invalid", EditorStyles.toolbarButton);
                if (flag != setting.discardInvalid)
                {
                    setting.discardInvalid = flag;
                }

                flag = GUILayout.Toggle(setting.isCleanMode, "PreCompile Lua", EditorStyles.toolbarButton);
                if (flag != setting.isCleanMode)
                {
                    setting.isCleanMode = flag;
                    if (setting.isCleanMode)
                    {
                        setting.isDeepLuaProfiler = true;
                        Selection.activeObject = LuaProfilerPrecompileSetting.Instance;
#if UNITY_2018_2_OR_NEWER
                        EditorApplication.ExecuteMenuItem("Window/General/Inspector");
#else
                        EditorApplication.ExecuteMenuItem("Window/Inspector");
#endif
                    }
                    EditorApplication.isPlaying = false;
                }

                if (GUILayout.Button("GC"))
                {
                    LuaDLL.lua_gc_unhook(LuaProfiler.mainL, LuaGCOptions.LUA_GCCOLLECT, 0);
                }
            }

            /*
            GUILayout.Space(25);
            if (GUILayout.Button("MarkStaticRecord", EditorStyles.toolbarButton, GUILayout.Height(30)))
            {
                if (!LuaDeepProfilerSetting.Instance.isLocal)
                {
                    NetWorkMgrClient.SendCmd(3);
                }
                else
                {
                    m_luaDiffScrollView.DelDiffInfo(LuaHook.RecordStatic());
                }
                m_luaDiffScrollView.MarkIsStaticRecord();
            }

            if (GUILayout.Button("MarkLuaRecord", EditorStyles.toolbarButton, GUILayout.Height(30)))
            {
                if (!LuaDeepProfilerSetting.Instance.isLocal)
                {
                    NetWorkServer.SendCmd(1);
                }
                else
                {
                    m_luaDiffScrollView.DelDiffInfo(LuaHook.Record());
                }
                m_luaDiffScrollView.MarkIsRecord();
            }
            if (GUILayout.Button("DiffRecord", EditorStyles.toolbarButton, GUILayout.Height(30)))
            {
                if (!LuaDeepProfilerSetting.Instance.isLocal)
                {
                    NetWorkServer.SendCmd(2);
                }
                else
                {
                    m_luaDiffScrollView.DelDiffInfo(LuaHook.Diff());
                }
            }
            if (GUILayout.Button("ClearDiff", EditorStyles.toolbarButton, GUILayout.Height(30)))
            {
                m_luaDiffScrollView.Clear();
            }*/

            GUILayout.Space(20);
            if (GUILayout.Button("AddLuaDir", EditorStyles.toolbarButton, GUILayout.Height(30)))
            {
                LocalToLuaIDE.AddExternalProjectRootPath();
            }
            if (GUILayout.Button("SetIDE", EditorStyles.toolbarButton, GUILayout.Height(30)))
            {
                LocalToLuaIDE.SetExternalEditorPath();
            }
            if (GUILayout.Button("ClearLuaDir", EditorStyles.toolbarButton, GUILayout.Height(30)))
            {
                LocalToLuaIDE.ClearPath();
            }
            if (!LuaDeepProfilerSetting.Instance.isRecord)
            {
                bool isSave = GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Height(30), GUILayout.Width(50));
                if (isSave)
                {
                    m_TreeView.SaveResult();
                }
                
                bool isLoad = GUILayout.Button("Load", EditorStyles.toolbarButton, GUILayout.Height(30), GUILayout.Width(50));
                if (isLoad)
                {
                    m_TreeView.LoadHistory();
                }
            }
            
            #endregion

            #region gc value
            GUILayout.Space(100);
            GUILayout.FlexibleSpace();
            #endregion

            if (m_TreeView != null)
            {
                m_TreeView.searchString = m_SearchField.OnToolbarGUI(m_TreeView.searchString);
                m_TreeView.toggleMerge = GUILayout.Toggle(m_TreeView.toggleMerge, "merge", EditorStyles.toolbarButton, GUILayout.Height(30));
            }
			isShowRef = GUILayout.Toggle(isShowRef, "show refInfo", EditorStyles.toolbarButton, GUILayout.Height(30));

            EditorGUILayout.EndHorizontal();
        }
        void DoRecord()
        {
            var instance = LuaDeepProfilerSetting.Instance;
            if (!instance.isRecord)
            {
                return;
            }

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            bool state = instance.isStartRecord;
            string recordName = "StopRecord";
            if (!state)
            {
                recordName = "StartRecord";
            }
            instance.isStartRecord = GUILayout.Toggle(instance.isStartRecord, recordName, EditorStyles.toolbarButton, GUILayout.Height(30));

            if (!state && instance.isStartRecord)
            {
                m_TreeView.Clear(true);
                m_luaRefScrollView.ClearRefInfo(true);
                //NetWorkServer.SendCmd(0);
            }

            if (state && !instance.isStartRecord)
            {
                m_TreeView.LoadHistoryCurve();
            }

            int count = m_TreeView.history.Count - 1;
            int delta = Mathf.Max(0, count - m_lastCount);
            if (delta != 0)
            {
                endFrame = count;
            }

            instance.isFrameRecord = GUILayout.Toggle(instance.isFrameRecord, "Frame", EditorStyles.toolbarButton, GUILayout.Height(30));
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

                int startGameFrame = m_TreeView.GetFrameCount(startFrame);
                int endGameFrame = m_TreeView.GetFrameCount(endFrame);
                m_luaRefScrollView.LoadHistory(startGameFrame, endGameFrame);
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
        void DoChartToggle()
        {
            LoadChartTexture();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(130));
            Texture2D t = null;
            #region chart

            EditorGUILayout.BeginHorizontal();
            t = isShowPssChart ? pssChart : disableChart;
            isShowPssChart = GUILayout.Toggle(isShowPssChart, t, EditorStyles.label, GUILayout.Width(15), GUILayout.Height(15));
            GUILayout.Label("Pss", GUILayout.Width(40));
            GUILayout.Label(m_TreeView.GetPssMemory());
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            t = isShowPowerChart ? powrChart : disableChart;
            isShowPowerChart = GUILayout.Toggle(isShowPowerChart, t, EditorStyles.label, GUILayout.Width(15), GUILayout.Height(15));
            GUILayout.Label("Power", GUILayout.Width(40));
            GUILayout.Label(m_TreeView.GetPower());
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            t = isShowMonoChart ? monoChart : disableChart;
            isShowMonoChart = GUILayout.Toggle(isShowMonoChart, t, EditorStyles.label, GUILayout.Width(15), GUILayout.Height(15));
            GUILayout.Label("Mono  ", GUILayout.Width(40));
            GUILayout.Label(m_TreeView.GetMonoMemory());
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            t = isShowLuaChart ? luaChart : disableChart;
            isShowLuaChart = GUILayout.Toggle(isShowLuaChart, t, EditorStyles.label, GUILayout.Width(15), GUILayout.Height(15));
            GUILayout.Label("Lua", GUILayout.Width(40));
            GUILayout.Label(m_TreeView.GetLuaMemory());
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            t = isShowFpsChart ? fpsChart : disableChart;
            isShowFpsChart = GUILayout.Toggle(isShowFpsChart, t, EditorStyles.label, GUILayout.Width(15), GUILayout.Height(15));
            GUILayout.Label("Fps", GUILayout.Width(40));
            GUILayout.Label(m_TreeView.GetFPS());
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(15);
            GUILayout.Label("catch", GUILayout.Width(40));
            GUILayout.Label(m_TreeView.GetCatchedLuaMemory());
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(1);
            GUILayout.FlexibleSpace();
            #endregion

            EditorGUILayout.EndVertical();
        }

        void DoChart()
        {
            EditorGUILayout.BeginVertical(new GUILayoutOption[0]);
            //curveScale = GUILayout.VerticalSlider(curveScale, 1f, 0.01f, this._surveScaleOption);
            EditorGUILayout.BeginVertical(new GUILayoutOption[0]);
            if (currentStyle == null || currentStyle.normal.background == null)
            {
                currentStyle = new GUIStyle(EditorStyles.helpBox);
                if (boxTex != null)
                {
                    Destory(boxTex);
                    boxTex = null;
                }
                boxTex = GenTextureColor(15, 15, boxColor);
                currentStyle.normal.background = boxTex;
            }
            EditorGUILayout.BeginVertical(currentStyle, new GUILayoutOption[]
                {
                GUILayout.MinHeight(50f),
                GUILayout.ExpandWidth(true)
                });
            Rect controlRect2 = EditorGUILayout.GetControlRect(false, this._mainRectsOption);
            GUILayout.Space(5f);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();

            GUILayout.Space(7f);
            SplitterGUILayout.BeginHorizontalSplit(this._minmaxSlider, new GUILayoutOption[0]);
            SplitterGUILayout.EndHorizontalSplit();
            GUILayout.Space(2f);

            DrawChart(controlRect2);
            EditorGUILayout.EndVertical();

        }
        void DoTreeView()
        {
            if (m_TreeView == null) return;
            Rect rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);
            rect.x += 4;
            rect.width -= 4;
            if (m_TreeView.needRebuild)
            {
                m_TreeView.Reload();
            }
            m_TreeView.OnGUI(rect);
            sortColIndex = m_TreeView.multiColumnHeader.sortedColumnIndex;
            if (sortColIndex > 0)
            {
                isAscending = m_TreeView.multiColumnHeader.IsSortedAscending(sortColIndex);
            }
        }

        #region chart
        private void DrawChart(Rect rect)
        {
            if (m_TreeView == null) return;
            Handles.color = new Color(1f, 1f, 1f, 0.2f);
            CachedVec[0].Set(rect.xMin, rect.y + 0.33f * rect.height, 0f);
            CachedVec[1].Set(rect.xMax, rect.y + 0.33f * rect.height, 0f);
            Handles.DrawAAPolyLine(2.5f, CachedVec);
            CachedVec[0].Set(rect.xMin, rect.y + 0.66f * rect.height, 0f);
            CachedVec[1].Set(rect.xMax, rect.y + 0.66f * rect.height, 0f);
            Handles.DrawAAPolyLine(2.5f, CachedVec);
            if (m_TreeView.historyCurve == null) return;

            if (isShowPssChart)
            {
                Handles.color = pssColor;
                DrawPssCurve(m_TreeView.historyCurve, rect);
            }

            if (isShowMonoChart)
            {
                Handles.color = monoColor;
                DrawMonoCurve(m_TreeView.historyCurve, rect);
            }

            if (isShowLuaChart)
            {
                Handles.color = luaColor;
                DrawLuaCurve(m_TreeView.historyCurve, rect);
            }

            if (isShowFpsChart)
            {
                Handles.color = fpsColor;
                DrawFpsCurve(m_TreeView.historyCurve, rect);
                DrawYAxis(rect);
            }

            if (isShowPowerChart)
            {
                Handles.color = powerColor;
                DrawPowerCurve(m_TreeView.historyCurve, rect);
            }

            var intance = LuaDeepProfilerSetting.Instance;
            if (intance.isRecord && !intance.isStartRecord)
            {
                HandleInputForChart(rect);
            }
            base.Repaint();
        }

        private void DrawYAxis(Rect xRect)
        {
            int intervalCount = 15;
            float intervalDistance = ((xRect.yMax - xRect.yMin) / 6);
            Rect rect = new Rect(xRect.x, xRect.yMax, xRect.width, xRect.yMin);
            if (m_gs == null)
            {
                m_gs = new GUIStyle(GUI.skin.label);
                m_gs.normal.textColor = fpsColor;
            }
            for (int f = 0; f <= 75; f += intervalCount)
            {
                rect.y -= intervalDistance;
                GUI.Label(rect, f.ToString(), m_gs);
            }
        }

        private void DrawLuaCurve(HistoryCurve curve, Rect rect)
        {
            if (curve.IsLuaEmpty()) return;
            rect.yMin = 1.2f * rect.yMin;
            float split = 1;
            int count = curve.GetLuaRecordCount(out split);
            float minValue = curve.minLuaValue;
            float maxValue = curve.maxLuaValue;
            float lastPoint = 0;
            curve.TryGetLuaMemory(0, out lastPoint);

            if (count > 1)
            {
                int len = 0;
                var setting = LuaDeepProfilerSetting.Instance;

                if (setting.isRecord && !setting.isStartRecord)
                {
                    len = count - 1;
                }
                else
                {
                    len = HistoryCurve.RECORD_FRAME_COUNT - 1;
                }

                for (int i = 1; i < count; i++)
                {
                    float currentMetric = 0;
                    int index = (int)(i * split);
                    if (!curve.TryGetLuaMemory(index, out currentMetric))
                    {
                        continue;
                    }
                    Vector3 currentPos = PointFromRect(0, len, len - count + 1 + i, minValue, maxValue, currentMetric, rect);
                    Vector3 lastPos = PointFromRect(0, len, len - count + i, minValue, maxValue, lastPoint, rect);
                    lastPoint = currentMetric;
                    CachedVec[0].Set(lastPos.x, lastPos.y, 0);
                    CachedVec[1].Set(currentPos.x, currentPos.y, 0f);
                    Handles.DrawAAPolyLine(2.5f, CachedVec);
                }
            }
        }

        private void DrawMonoCurve(HistoryCurve curve, Rect rect)
        {
            if (curve.IsMonoEmpty()) return;
            rect.yMin = 1.1f * rect.yMin;
            float split = 1;
            int count = curve.GetMonoRecordCount(out split);
            float minValue = curve.minMonoValue;
            float maxValue = curve.maxMonoValue;
            float lastPoint = 0;
            curve.TryGetMonoMemory(0, out lastPoint);

            if (count > 1)
            {
                int len = 0;
                var setting = LuaDeepProfilerSetting.Instance;

                if (setting.isRecord && !setting.isStartRecord)
                {
                    len = count - 1;
                }
                else
                {
                    len = HistoryCurve.RECORD_FRAME_COUNT - 1;
                }

                for (int i = 1; i < count; i++)
                {
                    float currentMetric = 0;
                    int index = (int)(i * split);
                    if (!curve.TryGetMonoMemory(index, out currentMetric))
                    {
                        continue;
                    }
                    Vector3 currentPos = PointFromRect(0, len, len - count + 1 + i, minValue, maxValue, currentMetric, rect);
                    Vector3 lastPos = PointFromRect(0, len, len - count + i, minValue, maxValue, lastPoint, rect);
                    lastPoint = currentMetric;
                    CachedVec[0].Set(lastPos.x, lastPos.y, 0);
                    CachedVec[1].Set(currentPos.x, currentPos.y, 0f);
                    Handles.DrawAAPolyLine(2.5f, CachedVec);
                }
            }
        }

        private void DrawPowerCurve(HistoryCurve curve, Rect rect)
        {
            if (curve.IsPowerEmpty()) return;
            rect.yMin = 1.3f * rect.yMin;
            float split = 1;
            int count = curve.GetPowerRecordCount(out split);
            float minValue = curve.minPowerValue;
            float maxValue = curve.maxPowerValue;
            float lastPoint = 0;
            curve.TryGetPowerMemory(0, out lastPoint);
            if (count > 1)
            {
                int len = 0;
                var setting = LuaDeepProfilerSetting.Instance;

                if (setting.isRecord && !setting.isStartRecord)
                {
                    len = count - 1;
                }
                else
                {
                    len = HistoryCurve.RECORD_FRAME_COUNT - 1;
                }

                for (int i = 1; i < count; i++)
                {
                    float currentMetric = 0;
                    int index = (int)(i * split);
                    if (!curve.TryGetPowerMemory(index, out currentMetric))
                    {
                        continue;
                    }
                    Vector3 currentPos = PointFromRect(0, len, len - count + 1 + i, minValue, maxValue, currentMetric, rect);
                    Vector3 lastPos = PointFromRect(0, len, len - count + i, minValue, maxValue, lastPoint, rect);
                    lastPoint = currentMetric;
                    CachedVec[0].Set(lastPos.x, lastPos.y, 0);
                    CachedVec[1].Set(currentPos.x, currentPos.y, 0f);
                    Handles.DrawAAPolyLine(2.5f, CachedVec);
                }
            }
        }

        private void DrawFpsCurve(HistoryCurve curve, Rect rect)
        {
            if (curve.IsFpsEmpty()) return;
            float split = 1;
            int count = curve.GetFpsRecordCount(out split);
            float minValue = curve.minFpsValue;
            float maxValue = curve.maxFpsValue;
            float lastPoint = 0;
            curve.TryGetFpsMemory(0, out lastPoint);

            if (count > 1)
            {
                int len = 0;
                var setting = LuaDeepProfilerSetting.Instance;

                if (setting.isRecord && !setting.isStartRecord)
                {
                    len = count - 1;
                }
                else
                {
                    len = HistoryCurve.RECORD_FRAME_COUNT - 1;
                }

                for (int i = 1; i < count; i++)
                {
                    float currentMetric = 0;
                    int index = (int)(i * split);
                    if (!curve.TryGetFpsMemory(index, out currentMetric))
                    {
                        continue;
                    }
                    Vector3 currentPos = PointFromRect(0, len, len - count + 1 + i, minValue, maxValue, currentMetric, rect);
                    Vector3 lastPos = PointFromRect(0, len, len - count + i, minValue, maxValue, lastPoint, rect);
                    lastPoint = currentMetric;
                    CachedVec[0].Set(lastPos.x, lastPos.y, 0);
                    CachedVec[1].Set(currentPos.x, currentPos.y, 0f);
                    Handles.DrawAAPolyLine(2.5f, CachedVec);
                }
            }
        }

        private void DrawPssCurve(HistoryCurve curve, Rect rect)
        {
            if (curve.IsPssEmpty()) return;
            float split = 1;
            int count = curve.GetPssRecordCount(out split);
            float minValue = curve.minPssValue;
            float maxValue = curve.maxPssValue;
            float lastPoint = 0;
            curve.TryGetPssMemory(0, out lastPoint);

            if (count > 1)
            {
                int len = 0;
                var setting = LuaDeepProfilerSetting.Instance;

                if (setting.isRecord && !setting.isStartRecord)
                {
                    len = count - 1;
                }
                else
                {
                    len = HistoryCurve.RECORD_FRAME_COUNT - 1;
                }

                for (int i = 1; i < count; i++)
                {
                    float currentMetric = 0;
                    int index = (int)(i * split);
                    if (!curve.TryGetPssMemory(index, out currentMetric))
                    {
                        continue;
                    }
                    Vector3 currentPos = PointFromRect(0, len, len - count + 1 + i, minValue, maxValue, currentMetric, rect);
                    Vector3 lastPos = PointFromRect(0, len, len - count + i, minValue, maxValue, lastPoint, rect);
                    lastPoint = currentMetric;
                    CachedVec[0].Set(lastPos.x, lastPos.y, 0);
                    CachedVec[1].Set(currentPos.x, currentPos.y, 0f);
                    Handles.DrawAAPolyLine(2.5f, CachedVec);
                }
            }
        }

        private Vector3 PointFromRect(float minH, float maxH, float h, float minV, float maxV, float v, Rect rect)
        {
            Vector3 v3 = new Vector3();
            float dh = maxH - minH;
            dh = (dh == 0) ? 1 : dh;
            v3.x = (rect.xMax - rect.xMin) * (h - minH) / dh + rect.xMin;
            //v3.y = (rect.yMax - rect.yMin) * (v - minV) / (maxV - minV) + rect.yMin;
            float dv = minV - maxV;
            if (dv != 0)
            {
                v3.y = (rect.yMax - rect.yMin) * (v - maxV) / dv + rect.yMin;
            }
            else
            {
                v3.y = rect.yMin;
            }
            return v3;
        }

        void HandleInputForChart(Rect expandRect)
        {
            int metricCount = m_TreeView.historyCurve.GetLuaRecordLength();

            if (metricCount == 0) return;

            bool isFrame = LuaDeepProfilerSetting.Instance.isFrameRecord;
            bool isEvent = false;
            bool isLeft = false;
            bool isRight = false;

            if (Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseUp)
            {
                Vector2 mousePosition = Event.current.mousePosition;
                if (mousePosition.x >= expandRect.xMin && mousePosition.x <= expandRect.xMax &&
                    mousePosition.y >= expandRect.yMin && mousePosition.y <= expandRect.yMax)
                {
                    currentFrameIndex = (int)(metricCount * (mousePosition.x - expandRect.xMin) / (expandRect.xMax - expandRect.xMin));
                    GUIUtility.keyboardControl = 0;
                    isEvent = true;
                    isTouchInChart = true;
                }
                else
                {
                    isTouchInChart = false;
                }
            }
            else if (isTouchInChart && Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == KeyCode.RightArrow)
                {
                    isEvent = true;
                    isRight = true;
                }
                if (Event.current.keyCode == KeyCode.LeftArrow)
                {
                    isEvent = true;
                    isLeft = true;
                }
            }
            if (isEvent)
            {
                if (isLeft)
                {
                    if (isFrame)
                    {
                        endFrame = m_TreeView.GetPreFrame(startFrame, true);
                        startFrame = m_TreeView.GetPreFrame(endFrame, false);
                    }
                    else
                    {
                        startFrame--;
                    }
                }
                else if (isRight)
                {
                    if (isFrame)
                    {
                        startFrame = m_TreeView.GetNextFrame(endFrame, true);
                        endFrame = m_TreeView.GetNextFrame(startFrame, false);
                    }
                    else
                    {
                        endFrame++;
                    }
                }
                else
                {
                    if (isFrame)
                    {
                        startFrame = m_TreeView.GetPreFrame(currentFrameIndex, false);
                        endFrame = m_TreeView.GetNextFrame(currentFrameIndex, false);
                    }
                    else
                    {
                        startFrame = currentFrameIndex;
                        endFrame = currentFrameIndex;
                    }

                }
                startFrame = Mathf.Min(Mathf.Max(0, startFrame), metricCount);
                endFrame = Mathf.Min(Mathf.Max(0, endFrame), metricCount);

                m_TreeView.ReLoadSamples(startFrame, endFrame);
                int startGameFrame = m_TreeView.GetFrameCount(startFrame);
                int endGameFrame = m_TreeView.GetFrameCount(endFrame);
                m_luaRefScrollView.LoadHistory(startGameFrame, endGameFrame);
            }

            Vector3 upPos = PointFromRect(0, metricCount, startFrame, 0, 1, 0, expandRect);
            Vector3 downPos = PointFromRect(0, metricCount, startFrame, 0, 1, 1, expandRect);

            Handles.color = new Color(0.8f, 0.2f, 0.5f, 1f);
            CachedVec[0].Set(upPos.x, upPos.y, 0f);
            CachedVec[1].Set(downPos.x, downPos.y, 0f);
            Handles.DrawAAPolyLine(3.5f, CachedVec);

            upPos = PointFromRect(0, metricCount, endFrame, 0, 1, 0, expandRect);
            downPos = PointFromRect(0, metricCount, endFrame, 0, 1, 1, expandRect);

            Handles.color = new Color(0.8f, 0.2f, 0.5f, 1f);
            CachedVec[0].Set(upPos.x, upPos.y, 0f);
            CachedVec[1].Set(downPos.x, downPos.y, 0f);
            Handles.DrawAAPolyLine(3.5f, CachedVec);
        }

        private void OpenLocalMode()
        {
            ClearConsole();
            NetWorkMgrClient.Disconnect();
            currentFrameIndex = 0;
            m_TreeView.Clear(true);
            LuaProfiler.UnRegistReceive();
            Sample.UnRegAction();
            LuaRefInfo.UnRegAction();
            LuaProfiler.RegisterOnReceiveSample(m_TreeView.LoadRootSample);
            LuaProfiler.RegisterOnReceiveRefInfo(m_luaRefScrollView.DelRefInfo);
            LuaProfiler.RegisterOnReceiveDiffInfo(m_luaDiffScrollView.DelDiffInfo);
        }

        #endregion

        // Add menu named "My Window" to the Window menu
        [MenuItem("Window/Analysis/Lua Profiler &#l", priority = 200)]
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
#endif