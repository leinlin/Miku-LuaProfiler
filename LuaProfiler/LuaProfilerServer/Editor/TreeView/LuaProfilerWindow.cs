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
using System.Collections.Generic;

namespace MikuLuaProfiler
{
#if UNITY_5_6_OR_NEWER
    public class LuaProfilerWindow : EditorWindow
    {
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

        LuaProfilerTreeView m_TreeView;
        SearchField m_SearchField;
        int startFrame = 0;
        int endFrame = 0;
        int sortColIndex = -1;
        bool isAscending = false;
        private int m_lastCount = 0;
        int port = 2333;
        private bool isShowLuaChart = true;
        private bool isShowMonoChart = false;
        private bool isShowFpsChart = false;
        private int currentFrameIndex = 0;

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
            GUILayout.Space(10);
            DoRecord();
            if (isShowLuaChart || isShowMonoChart)
            {
                DoChart();
            }
            //DoCapture();
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
                currentFrameIndex = 0;
                m_TreeView.Clear(true);
            }
            GUILayout.Space(5);
            #endregion

            #region history
            bool flag = GUILayout.Toggle(LuaDeepProfilerSetting.Instance.isRecord, "Record", EditorStyles.toolbarButton, GUILayout.Height(30));
            if (flag != LuaDeepProfilerSetting.Instance.isRecord)
            {
                LuaDeepProfilerSetting.Instance.isRecord = flag;
            }
            GUILayout.Space(25);
            #endregion

            #region socket

            if (GUILayout.Button("OpenService", EditorStyles.toolbarButton, GUILayout.Height(30)))
            {
                NetWorkServer.Close();
                currentFrameIndex = 0;
                m_TreeView.Clear(true);
                NetWorkServer.RegisterOnReceive(m_TreeView.LoadRootSample);
                NetWorkServer.BeginListen("0.0.0.0", port);
            }
            GUILayout.Label("port:", GUILayout.Height(30), GUILayout.Width(35));
            port = EditorGUILayout.IntField(port, GUILayout.Height(16), GUILayout.Width(50));

            if (GUILayout.Button("CloseService", EditorStyles.toolbarButton, GUILayout.Height(30)))
            {
                NetWorkServer.Close();
            }
            #endregion

            #region chart
            isShowLuaChart = GUILayout.Toggle(isShowLuaChart, "LuaChart", EditorStyles.toolbarButton, GUILayout.Height(30));
            GUILayout.Space(5);
            isShowMonoChart = GUILayout.Toggle(isShowMonoChart, "MonoChart", EditorStyles.toolbarButton, GUILayout.Height(30));
            GUILayout.Space(5);
            isShowFpsChart = GUILayout.Toggle(isShowFpsChart, "FpsChart", EditorStyles.toolbarButton, GUILayout.Height(30));
            GUILayout.Space(25);
            #endregion

            #region path
            /*
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
            }*/
            #endregion

            #region gc value
            GUILayout.Space(100);
            GUILayout.FlexibleSpace();
            GUILayout.Label(string.Format("Lua Total:{0}", m_TreeView.GetLuaMemory()), EditorStyles.toolbarButton, GUILayout.Height(30));

            GUILayout.Space(15);
            GUILayout.Label(string.Format("Mono Total:{0}", m_TreeView.GetMonoMemory()), EditorStyles.toolbarButton, GUILayout.Height(30));
            GUILayout.Space(25);
            #endregion

            m_TreeView.searchString = m_SearchField.OnToolbarGUI(m_TreeView.searchString);

            EditorGUILayout.EndHorizontal();
        }
        void DoRecord()
        {
            var instance = LuaDeepProfilerSetting.Instance;
            if (!instance.isRecord) return;

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            bool state = instance.isStartRecord;
            instance.isStartRecord = GUILayout.Toggle(instance.isStartRecord, "StartRecord", EditorStyles.toolbarButton, GUILayout.Height(30));

            if (!state && instance.isStartRecord)
            {
                m_TreeView.Clear(true);
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
        void DoChart()
        {
            EditorGUILayout.BeginVertical(new GUILayoutOption[0]);
            EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
            //curveScale = GUILayout.VerticalSlider(curveScale, 1f, 0.01f, this._surveScaleOption);
            EditorGUILayout.BeginVertical(new GUILayoutOption[0]);
            EditorGUILayout.BeginVertical(GuiSkinManager.Styles.textField, new GUILayoutOption[]
                {
                GUILayout.MinHeight(50f),
                GUILayout.ExpandWidth(true)
                });
            GUILayout.Space(3f);
            Rect controlRect = EditorGUILayout.GetControlRect(false, this._frameInfoRectsOption);
            GUI.Label(controlRect, GUIContent.none, GuiSkinManager.Styles.entryOdd);
            GUILayout.Space(3f);
            Rect controlRect2 = EditorGUILayout.GetControlRect(false, this._mainRectsOption);
            GUILayout.Space(5f);
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(7f);
            SplitterGUILayout.BeginHorizontalSplit(this._minmaxSlider, new GUILayoutOption[0]);
            SplitterGUILayout.EndHorizontalSplit();
            GUILayout.Space(2f);

            DrawChart(controlRect2);
            EditorGUILayout.EndVertical();

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


        #region chart
        private void DrawChart(Rect rect)
        {
            Handles.color = new Color(1f, 1f, 1f, 0.2f);
            CachedVec[0].Set(rect.xMin, rect.y + 0.33f * rect.height, 0f);
            CachedVec[1].Set(rect.xMax, rect.y + 0.33f * rect.height, 0f);
            Handles.DrawAAPolyLine(2.5f, CachedVec);
            CachedVec[0].Set(rect.xMin, rect.y + 0.66f * rect.height, 0f);
            CachedVec[1].Set(rect.xMax, rect.y + 0.66f * rect.height, 0f);
            Handles.DrawAAPolyLine(2.5f, CachedVec);
            if (m_TreeView.historyCurve == null) return;

            if (isShowLuaChart)
            {
                Handles.color = new Color(0.4f, 0.7f, 0.9f, 1.0f);
                DrawLuaCurve(m_TreeView.historyCurve, rect);
            }

            if (isShowMonoChart)
            {
                Handles.color = new Color32(154, 255, 154, 255) * GUI.skin.label.normal.textColor;
                DrawMonoCurve(m_TreeView.historyCurve, rect);
            }

            if (isShowFpsChart)
            {
                Handles.color = new Color32(255, 193, 37, 255) * GUI.skin.label.normal.textColor;
                DrawFpsCurve(m_TreeView.historyCurve, rect);
                DrawYAxis(rect);
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
            Color c = GUI.color;
            GUI.color = new Color32(255, 193, 37, 255) * GUI.skin.label.normal.textColor;
            for (int f = 0; f <= 90; f += intervalCount)
            {
                rect.y -= intervalDistance;
                GUI.Label(rect, f.ToString());
            }
            GUI.color = c;
        }

        private void DrawLuaCurve(HistoryCurve curve, Rect rect)
        {
            if (curve.IsLuaEmpty()) return;
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
                    Vector3 currentPos = PointFromRect(0, len, i, minValue, maxValue, currentMetric, rect);
                    Vector3 lastPos = PointFromRect(0, len, i - 1, minValue, maxValue, lastPoint, rect);
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
                    Vector3 currentPos = PointFromRect(0, len, i, minValue, maxValue, currentMetric, rect);
                    Vector3 lastPos = PointFromRect(0, len, i - 1, minValue, maxValue, lastPoint, rect);
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
                    Vector3 currentPos = PointFromRect(0, len, i, minValue, maxValue, currentMetric, rect);
                    Vector3 lastPos = PointFromRect(0, len, i - 1, minValue, maxValue, lastPoint, rect);
                    lastPoint = currentMetric;
                    CachedVec[0].Set(lastPos.x, lastPos.y, 0);
                    CachedVec[1].Set(currentPos.x, currentPos.y, 0f);
                    Handles.DrawAAPolyLine(2.5f, CachedVec);
                }
            }
        }
        private Vector3 PointFromRect(float minH, float maxH, float h, float minV, float maxV, float v, Rect rect)
        {
            v = Mathf.Max(minV, v);
            Vector3 v3 = new Vector3();
            float dh = maxH - minH;
            dh = (dh == 0) ? 1 : dh;
            v3.x = (rect.xMax - rect.xMin) * (h - minH) / dh + rect.xMin;
            //v3.y = (rect.yMax - rect.yMin) * (v - minV) / (maxV - minV) + rect.yMin;
            float dv = minV - maxV;
            dv = (dv == 0) ? 1 : dv;
            v3.y = (rect.yMax - rect.yMin) * (v - maxV) / dv + rect.yMin;
            return v3;
        }

        void HandleInputForChart(Rect expandRect)
        {
            int metricCount = m_TreeView.historyCurve.GetLuaRecordLength();

            if (metricCount == 0) return;

            bool isEvent = false;
            if (Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseUp)
            {
                Vector2 mousePosition = Event.current.mousePosition;
                if (mousePosition.x >= expandRect.xMin && mousePosition.x <= expandRect.xMax &&
                    mousePosition.y >= expandRect.yMin && mousePosition.y <= expandRect.yMax)
                {
                    currentFrameIndex = (int)(metricCount * (mousePosition.x - expandRect.xMin) / (expandRect.xMax - expandRect.xMin));
                    GUIUtility.keyboardControl = 0;
                    isEvent = true;
                }
            }
            else if (Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == KeyCode.RightArrow)
                {
                    if (currentFrameIndex + 1 <= metricCount)
                    {
                        currentFrameIndex++;
                        isEvent = true;
                    }
                }
                if (Event.current.keyCode == KeyCode.LeftArrow)
                {
                    if (currentFrameIndex - 1 >= 0)
                    {
                        currentFrameIndex--;
                        isEvent = true;
                    }
                }
            }
            Vector3 upPos = PointFromRect(0, metricCount, currentFrameIndex, 0, 1, 0, expandRect);
            Vector3 downPos = PointFromRect(0, metricCount, currentFrameIndex, 0, 1, 1, expandRect);

            Handles.color = new Color(0.8f, 0.2f, 0.5f, 1f);
            CachedVec[0].Set(upPos.x, upPos.y, 0f);
            CachedVec[1].Set(downPos.x, downPos.y, 0f);
            Handles.DrawAAPolyLine(3.5f, CachedVec);

            if (isEvent)
            {
                startFrame = currentFrameIndex;
                endFrame = currentFrameIndex;
                m_TreeView.ReLoadSamples(startFrame, endFrame);
            }
        }
        #endregion

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
