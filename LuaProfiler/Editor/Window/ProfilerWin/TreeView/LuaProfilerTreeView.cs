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
* Filename: LuaProfilerTreeView
* Created:  2018/7/13 14:29:22
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/

#if UNITY_5_6_OR_NEWER && UNITY_EDITOR_WIN
namespace MikuLuaProfiler
{
#if UNITY_5_6_OR_NEWER
    using System;
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEditor.IMGUI.Controls;
    using UnityEngine;


    #region item
    //The TreeElement data class is extended to hold extra data, which you can show and edit in the front-end TreeView.
    public class LuaProfilerTreeViewItem : TreeViewItem
    {
        private static ObjectPool<LuaProfilerTreeViewItem> objectPool = new ObjectPool<LuaProfilerTreeViewItem>(30);
        public static LuaProfilerTreeViewItem Create(LuaProfilerTreeViewItem item, LuaProfilerTreeViewItem father)
        {
            LuaProfilerTreeViewItem mt;
            mt = objectPool.GetObject();
            mt.ResetByItem(item, father);

            return mt;
        }

        public static LuaProfilerTreeViewItem Create(Sample sample, int depth, LuaProfilerTreeViewItem father)
        {
            var dict = LuaProfilerTreeView.m_nodeDict;

            LuaProfilerTreeViewItem mt;
            mt = objectPool.GetObject();
            mt.ResetBySample(sample, depth, father);
            dict[mt.fullName] = mt;

            return mt;
        }
        public void Restore()
        {
            objectPool.Store(this);
        }

        public int frameCalls { private set; get; }
        private long _showLuaGC = 0;
        public long showLuaGC
        {
            get
            {
                if (s_frameCount - _frameCount <= 10)
                {
                    return _showLuaGC / frameCalls;
                }
                else
                {
                    return 0;
                }
            }
        }
        private long _showMonoGC = 0;
        public long showMonoGC
        {
            get
            {
                if (s_frameCount - _frameCount <= 10)
                {
                    return _showMonoGC;
                }
                else
                {
                    return 0;
                }
            }
        }
        public static int s_frameCount = 0;
        public long totalMonoMemory { private set; get; }
        public long selfMonoMemory { private set; get; }
        public long totalLuaMemory { private set; get; }
        public long selfLuaMemory { private set; get; }
        public long selfCostTime { private set; get; }
        public long totalTime { private set; get; }
        public long averageTime { private set; get; }
        public long currentTime { private set; get; }
        public int totalCallTime { private set; get; }

        private bool m_isLua = false;
        public bool isLua
        {
            private set
            {
                m_isLua = value;
            }
            get
            {
                return m_isLua;
            }
        }
        public bool isError = false;
        public int line = 1;
        public string filePath { private set; get; }
        private string m_originName;

        public string fullName
        {
            get;
            private set;
        }
        public readonly List<LuaProfilerTreeViewItem> childs = new List<LuaProfilerTreeViewItem>();
        public LuaProfilerTreeViewItem father { private set; get; }
        private int _frameCount;
        public LuaProfilerTreeViewItem()
        {
        }

        private static readonly char[] splitDot = new char[] { ',' };
        private static readonly char[] splitFun = new char[] { '&' };
        private static readonly char[] splitLine = new char[] { ':' };
        public void ResetBySample(Sample sample, int depth, LuaProfilerTreeViewItem father)
        {
            if (sample != null)
            {
                if (sample.name.Length >= 6)
                {
                    m_isLua = sample.name.Substring(0, 6) == "[lua]:";
                }

                filePath = sample.name;
                line = 1;
                if (m_isLua)
                {
                    string[] array = sample.name.Split(splitDot, 2);
                    if (array.Length == 2)
                    {
                        filePath = array[1];
                        array = filePath.Split(splitFun, 2);
                        if (array.Length == 2)
                        {
                            filePath = array[0].Trim();
                            line = int.Parse(array[1].Split(splitLine, 2)[1]);
                        }
                    }
                }
                else
                {
                    isError = sample.name == "exception happen clear stack";
                }

                _showMonoGC = sample.costMonoGC;
                _showLuaGC = sample.costLuaGC;
                totalMonoMemory = sample.costMonoGC;
                totalLuaMemory = sample.costLuaGC;
                selfLuaMemory = sample.selfLuaGC;
                selfMonoMemory = sample.selfMonoGC;
                selfCostTime = sample.selfCostTime;
                totalTime = sample.costTime;
                displayName = sample.name;
                m_originName = sample.name;

                fullName = sample.fullName;
                frameCalls = sample.calls;
                currentTime = sample.costTime;
                totalCallTime = sample.calls;
            }
            else
            {
                _showMonoGC = 0;
                _showLuaGC = 0;
                totalMonoMemory = 0;
                totalLuaMemory = 0;
                selfMonoMemory = 0;
                selfLuaMemory = 0;
                selfCostTime = 0;

                totalTime = 0;
                displayName = "root";
                fullName = "root";
                frameCalls = 0;
                currentTime = 0;
                totalCallTime = 1;
            }
            averageTime = totalTime / Mathf.Max(totalCallTime, 1);

            this.id = LuaProfilerTreeView.GetUniqueId();
            this.depth = depth;


            childs.Clear();
            if (sample != null)
            {
                for (int i = 0, imax = sample.childs.Count; i < imax; i++)
                {
                    var dict = LuaProfilerTreeView.m_nodeDict;

                    LuaProfilerTreeViewItem mt;
                    var childSample = sample.childs[i];
                    if (dict.TryGetValue(childSample.fullName, out mt))
                    {
                        mt.AddSample(childSample);
                    }
                    else
                    {
                        var item = Create(sample.childs[i], depth + 1, this);
                        childs.Add(item);
                    }
                }
                _frameCount = sample.frameCount;
            }
            this.father = father;
        }

        public void ResetByItem(LuaProfilerTreeViewItem item, LuaProfilerTreeViewItem father)
        {
            filePath = item.filePath;
            m_isLua = item.m_isLua;
            isError = item.isError;

            _showMonoGC = item._showMonoGC;
            _showLuaGC = item._showLuaGC;
            totalMonoMemory = item.totalMonoMemory;
            totalLuaMemory = item.totalLuaMemory;
            selfLuaMemory = item.selfLuaMemory;
            selfMonoMemory = item.selfMonoMemory;
            selfCostTime = item.selfCostTime;
            totalTime = item.totalTime;
            displayName = item.displayName;
            m_originName = item.m_originName;

            fullName = item.fullName;
            frameCalls = item.frameCalls;
            currentTime = item.currentTime;
            totalCallTime = item.totalCallTime;

            averageTime = totalTime / Mathf.Max(totalCallTime, 1);

            childs.Clear();
            this.id = item.id;
            depth = 0;
            this.father = father;
        }

        public bool AddSample(Sample sample)
        {
            bool result = false;
            if (_frameCount == sample.frameCount)
            {
                frameCalls += sample.calls;
                currentTime += sample.costTime;
                _showMonoGC += Math.Max(sample.costMonoGC, 0);
                _showLuaGC += Math.Max(sample.costLuaGC, 0);
                selfCostTime += sample.selfCostTime;
            }
            else
            {
                frameCalls = sample.calls;
                currentTime = sample.costTime;
                _showMonoGC = sample.costMonoGC;
                _showLuaGC = sample.costLuaGC;
                selfCostTime = sample.selfCostTime;
            }

            totalLuaMemory += Math.Max(sample.costLuaGC, 0);
            selfLuaMemory += Math.Max(sample.selfLuaGC, 0);
            totalMonoMemory += Math.Max(sample.costMonoGC, 0);
            selfMonoMemory += Math.Max(sample.selfMonoGC, 0);

            totalTime += sample.costTime;
            totalCallTime += sample.calls;
            averageTime = totalTime / Mathf.Max(totalCallTime, 1);
            for (int i = 0, imax = sample.childs.Count; i < imax; i++)
            {
                LuaProfilerTreeViewItem childItem = null;
                var sampleChild = sample.childs[i];
                if (LuaProfilerTreeView.m_nodeDict.TryGetValue(sampleChild.fullName, out childItem))
                {
                    childItem.AddSample(sampleChild);
                }
                else
                {
                    result = true;
                    var treeItem = Create(sampleChild, depth + 1, this);
                    childs.Add(treeItem);
                }
            }
            _frameCount = sample.frameCount;
            s_frameCount = sample.frameCount;

            return result;
        }

        public Sample CopySelfToSample()
        {
            Sample s = new Sample();

            s.calls = totalCallTime;
            s.frameCount = 0;
            s.costLuaGC = (int)totalLuaMemory;
            s.costMonoGC = (int)totalMonoMemory;
            s.isCopy = true;
            s.copySelfLuaGC = selfLuaMemory;
            s.copySelfMonoGC = selfLuaMemory;
            s.copySelfCostTime = (int)selfCostTime;
            s.name = m_originName;
            s.costTime = (int)totalTime;

            s.currentLuaMemory = 0;
            s.currentMonoMemory = 0;
            return s;
        }

        public Sample CopyToSample()
        {
            Sample s = new Sample();

            s.calls = totalCallTime;
            s.frameCount = 0;
            s.costLuaGC = (int)totalLuaMemory;
            s.costMonoGC = (int)totalMonoMemory;
            s.name = m_originName;
            s.costTime = (int)totalTime;

            int childCount = childs.Count;
            for (int i = 0; i < childCount; i++)
            {
                Sample child = childs[i].CopyToSample();
                child.fahter = s;
            }
            s.currentLuaMemory = 0;
            s.currentMonoMemory = 0;
            return s;
        }
    }
    #endregion

    public class LuaProfilerTreeView : TreeView
    {
        #region pool
        private static int _uniqueId = 0;
        public static int GetUniqueId()
        {
            return _uniqueId++;
        }

        private readonly List<LuaProfilerTreeViewItem> roots = new List<LuaProfilerTreeViewItem>();
        #endregion

        #region field
        private static Color m_selectColor = new Color(1.0f, 1.0f, 0.0f, 1.0f);
        private static Color m_luaColor = new Color(0.2f, 0.5f, 0.7f, 1.0f);
        private static Color m_monoColor = new Color32(0, 180, 0, 255);
        private static Color m_errorColor = new Color32(255, 0, 0, 255);
        private List<int> m_expandIds = new List<int>();
        private readonly LuaProfilerTreeViewItem m_root;
        private readonly List<TreeViewItem> m_treeViewItems = new List<TreeViewItem>();
        private readonly Dictionary<string, LuaProfilerTreeViewItem> m_treeViewItemDict = new Dictionary<string, LuaProfilerTreeViewItem>();
        private GUIStyle m_gs;
        private Queue<Sample> m_runningSamplesQueue = new Queue<Sample>(256);
        private long m_luaMemory = 0;
        private long m_monoMemory = 0;
        private long m_pssMemory = 0;
        private float m_fps = 0;
        private float m_power = 0;
        private long m_catchLuaMemory = 0;

        private bool _needRebuild = false;
        public bool needRebuild {
            get {
                return _needRebuild;
            }
            set {
                _needRebuild = value;
            }
        }
        public readonly HistoryCurve historyCurve = new HistoryCurve(1024);
        public readonly List<Sample> history = new List<Sample>(2160);
        public string startUrl = null;
        public string endUrl = null;

        private bool m_toggleMerge = false;
        public bool toggleMerge
        {
            set
            {
                if (m_toggleMerge == value) return;
                m_toggleMerge = value;
                needRebuild = true;
            }
            get
            {
                return m_toggleMerge;
            }
        }

        private string m_searchString = "";
        public new string searchString
        {
            set
            {
                if (m_searchString == value) return;
                m_searchString = value;
                needRebuild = true;
            }
            get
            {
                return m_searchString;
            }
        }
        #endregion

        public LuaProfilerTreeView(TreeViewState treeViewState, float width)
            : base(treeViewState, CreateDefaultMultiColumnHeaderState(width))
        {
            //NetWorkServer.BeginListen("127.0.0.1", 23333);
            //NetWorkClient.ConnectServer("127.0.0.1", 23333);

            //LuaProfiler.SetSampleEnd(LoadRootSample);
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            m_root = LuaProfilerTreeViewItem.Create(null, -1, null);
            needRebuild = true;
            multiColumnHeader.sortingChanged += (header) => { needRebuild = true; };
            history.Clear();
            historyCurve.Clear();
            EditorApplication.update -= DequeueSample;
            EditorApplication.update += DequeueSample;
            Reload();
        }

        public void DequeueSample()
        {
            while (m_runningSamplesQueue.Count > 0)
            {
                Sample s = null;
                lock (this)
                {
                    if (m_runningSamplesQueue.Count > 0)
                    {
                        s = m_runningSamplesQueue.Dequeue();
                    }
                    else
                    {
                        break;
                    }
                }
                LuaProfilerTreeViewItem.s_frameCount = s.frameCount;
                var instance = LuaDeepProfilerSetting.Instance;
                if (!(instance.isRecord && !instance.isStartRecord))
                {
                    m_catchLuaMemory += s.costLuaGC;
                }
                LoadRootSample(s, LuaDeepProfilerSetting.Instance.isRecord);

                s.Restore();
            }
            if (LuaProfilerWindow.DoClear != null)
            {
                LuaProfilerWindow.DoClear();
                LuaProfilerWindow.DoClear = null;
            }
        }

        private static MultiColumnHeader CreateDefaultMultiColumnHeaderState(float treeViewWidth)
        {
            var columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Overview"),
                    contextMenuText = "Overview",
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = false,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 300,
                    minWidth = 200,
                    maxWidth = 10000,
                    autoResize = true,
                    canSort = false,
                    allowToggleVisibility = true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("totalLuaMemory"),
                    contextMenuText = "totalLuaMemory",
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = false,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 120,
                    minWidth = 120,
                    maxWidth = 120,
                    autoResize = true,
                    canSort = true,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("self"),
                    contextMenuText = "self",
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = false,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 70,
                    minWidth = 70,
                    maxWidth = 70,
                    autoResize = true,
                    canSort = true,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("totalMonoMemory"),
                    contextMenuText = "totalMonoMemory",
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = false,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 120,
                    minWidth = 120,
                    maxWidth = 120,
                    autoResize = true,
                    canSort = true,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("self"),
                    contextMenuText = "self",
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = false,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 70,
                    minWidth = 70,
                    maxWidth = 70,
                    autoResize = true,
                    canSort = true,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("LuaGC"),
                    contextMenuText = "LuaGC",
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = false,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 80,
                    minWidth = 80,
                    maxWidth = 120,
                    autoResize = true,
                    canSort = true,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("MonoGC"),
                    contextMenuText = "MonoGC",
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = false,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 80,
                    minWidth = 80,
                    maxWidth = 120,
                    autoResize = true,
                    canSort = true,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("currentTime"),
                    contextMenuText = "currentTime",
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = false,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 120,
                    minWidth = 120,
                    maxWidth = 120,
                    autoResize = true,
                    canSort = true,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("self"),
                    contextMenuText = "self",
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = false,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 120,
                    minWidth = 120,
                    maxWidth = 120,
                    autoResize = true,
                    canSort = true,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("averageTime"),
                    contextMenuText = "averageTime",
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = false,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 120,
                    minWidth = 120,
                    maxWidth = 120,
                    autoResize = true,
                    canSort = true,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("totalTime"),
                    contextMenuText = "totalTime",
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = false,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 120,
                    minWidth = 120,
                    maxWidth = 120,
                    autoResize = true,
                    canSort = true,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("totalCalls"),
                    contextMenuText = "totalCalls",
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = false,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 120,
                    minWidth = 120,
                    maxWidth = 120,
                    autoResize = true,
                    canSort = true,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Calls"),
                    contextMenuText = "Calls",
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = false,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 80,
                    minWidth = 80,
                    maxWidth = 80,
                    autoResize = true,
                    canSort = true,
                    allowToggleVisibility = false
                },
            };

            var state = new MultiColumnHeaderState(columns);
            return new MultiColumnHeader(state);
        }

        public void Clear(bool includeHistory)
        {
            m_nodeDict.Clear();
            roots.Clear();
            m_nodeDict.Clear();
            m_treeViewItems.Clear();
            m_expandIds.Clear();
            if (includeHistory)
            {
                history.Clear();
                startUrl = null;
                endUrl = null;
                historyCurve.Clear();
                //EditorUtility.UnloadUnusedAssetsImmediate();
            }

            m_luaMemory = 0;
            m_monoMemory = 0;
            m_pssMemory = 0;
            m_fps = 0;
            m_power = 0;
            m_catchLuaMemory = 0;
            needRebuild = true;
        }

        protected override void DoubleClickedItem(int id)
        {
            base.DoubleClickedItem(id);
            if (string.IsNullOrEmpty(searchString))
            {
                var selectItem = FindItem(id, BuildRoot());
                var item = (LuaProfilerTreeViewItem)selectItem;
                if (item.line == -1)
                {
                    Debug.Log("please wait");
                    return;
                }

                string fileName = item.filePath;
                try
                {
                    int line = item.line;
                    if (!File.Exists(fileName))
                    {
                        Debug.Log(fileName);
                    }
                    LocalToLuaIDE.OnOpenAsset(fileName, line);
                }
                catch
                {
                }
            }
            else
            {
                searchString = "";
                Reload();
                foreach (var item in roots)
                {
                    SetExpandedRecursive(item.id, false);
                }
                FrameItem(id);
            }
        }

        private bool CheckIsRootId(int id)
        {
            bool result = false;

            foreach (var item in roots)
            {
                if (item.id == id)
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        public static Dictionary<object, LuaProfilerTreeViewItem> m_nodeDict = new Dictionary<object, LuaProfilerTreeViewItem>();

        public int GetFrameCount(int frame)
        {
            if (frame >= history.Count) return 0;
            return history[frame].frameCount;
        }

        public void ReLoadSamples(int start, int end)
        {
            if (history.Count == 0) return;
            Clear(false);
            end = Mathf.Max(Mathf.Min(end, history.Count - 1), 0);
            start = Mathf.Max(Mathf.Min(start, history.Count - 1), 0);

            end = Mathf.Min(history.Count - 1, end);
            m_catchLuaMemory = 0;
            for (int i = start; i <= end; i++)
            {
                LoadHistoryRootSample(history[i]);
            }
        }

        public void LoadHistoryCurve()
        {
            historyCurve.Clear();
            for (int i = 0; i < history.Count; i++)
            {
                Sample sample = history[i];
                historyCurve.SlotLuaMemory(sample.currentLuaMemory);
                historyCurve.SlotMonoMemory(sample.currentMonoMemory);
                historyCurve.SlotFpsMemory(sample.fps);
                historyCurve.SlotPssMemory(sample.pss);
                historyCurve.SlotPowerMemory(sample.power);
            }
        }

        public void SaveResult()
        {
            string path = EditorUtility.SaveFilePanel("Save Sample", "", DateTime.Now.ToString("yyyy-MM-dd-HH-MM-ss"), "sample");
            List<Sample> rootSampeList = new List<Sample>();
            foreach (var item in roots)
            {
                rootSampeList.Add(item.CopyToSample());
            }
            Sample.SerializeList(rootSampeList, path);
        }

        public void SaveHisotry()
        {
            string path = EditorUtility.SaveFilePanel("Save Sample", "", DateTime.Now.ToString("yyyy-MM-dd-HH-MM-ss"), "sample");
            Sample.SerializeList(history, path);
        }

        public void LoadHistory()
        {
            string path = EditorUtility.OpenFilePanel("Load Sample", "", "sample");
            history.Clear();
            historyCurve.Clear();
            List<Sample> samples = Sample.DeserializeList(path);
            history.AddRange(samples);

            ReLoadSamples(0, history.Count);
            LoadHistoryCurve();
        }

        const long MaxB = 1024;
        const long MaxK = MaxB * 1024;
        const long MaxM = MaxK * 1024;
        const long MaxG = MaxM * 1024;

        public static string GetMemoryString(long value, string unit = "B")
        {
            string result = null;
            int sign = Math.Sign(value);

            value = Math.Abs(value);
            if (value < MaxB)
            {
                result = string.Format("{0}{1}", value, unit);
            }
            else if (value < MaxK)
            {
                result = string.Format("{0:N2}K{1}", (float)value / MaxB, unit);
            }
            else if (value < MaxM)
            {
                result = string.Format("{0:N2}M{1}", (float)value / MaxK, unit);
            }
            else if (value < MaxG)
            {
                result = string.Format("{0:N2}G{1}", (float)value / MaxM, unit);
            }
            if (sign < 0)
            {
                result = "-" + result;
            }
            return result;
        }

        public string GetLuaMemory()
        {
            return GetMemoryString(m_luaMemory);
        }
        public string GetMonoMemory()
        {
            return GetMemoryString(m_monoMemory);
        }
        public string GetPssMemory()
        {
            return GetMemoryString(m_pssMemory);
        }

        public string GetFPS()
        {
            return m_fps.ToString("0.00");
        }

        public string GetCatchedLuaMemory()
        {
            return GetMemoryString(m_catchLuaMemory);
        }

        public string GetPower()
        {
            return m_power.ToString("0.00") + "%";
        }

        private void LoadHistoryRootSample(Sample sample)
        {
            m_catchLuaMemory += sample.costLuaGC;
            LoadRootSample(sample, false, true);
        }

        public void LoadRootSample(Sample sample)
        {
            lock (this)
            {
                m_runningSamplesQueue.Enqueue(sample);
            }
        }

        public int GetNextFrame(int start, bool isChange)
        {
            start = Math.Max(1, Math.Min(start, history.Count - 1));
            int ret = start;
            int frameCount = history[start].frameCount;
            for (int i = start + 1, imax = history.Count; i < imax; i++)
            {
                if (frameCount != history[i].frameCount)
                {
                    if (isChange)
                    {
                        ret = i;
                    }
                    break;
                }
                ret = i;
            }
            return Mathf.Max(Mathf.Min(ret, history.Count - 1), 0);
        }

        public int GetPreFrame(int start, bool isChange)
        {
            if (history.Count <= 0) return 0;

            start = Math.Max(1, Math.Min(start, history.Count - 1));
            int ret = start;
            int frameCount = history[start].frameCount;
            for (int i = start - 1; i >= 0; i--)
            {
                if (frameCount != history[i].frameCount)
                {
                    if (isChange)
                    {
                        ret = i;
                    }
                    break;
                }
                ret = i;
            }

            return Mathf.Max(Mathf.Min(ret, history.Count - 1), 0);
        }

        public int GetNextProgramFrame(int start)
        {
            if (history.Count <= 0) return 0;
            start = Math.Max(0, Math.Min(start, history.Count - 1));
            int ret = start + 1;
            if (ret >= history.Count) return history.Count - 1;

            var setting = LuaDeepProfilerSetting.Instance;
            for (int i = start + 1, imax = history.Count; i < imax; i++)
            {
                ret = i;
                var s = history[i];
                if (s.costLuaGC > 0)
                {
                    break;
                }
                else if (s.costMonoGC > 0)
                {
                    break;
                }
                else if (s.costTime >= (1 / ((float)setting.captureFrameRate)) * 10000000)
                {
                    break;
                }
            }

            return Mathf.Max(Mathf.Min(ret, history.Count - 1), 0);
        }

        public int GetPreProgramFrame(int start)
        {
            int ret = start - 1;

            var setting = LuaDeepProfilerSetting.Instance;
            for (int i = start - 1; i >= 0; i--)
            {
                ret = i;
                var s = history[i];
                if (s.costLuaGC > 0)
                {
                    break;
                }
                else if (s.costMonoGC > 0)
                {
                    break;
                }
                else if (s.costTime >= (1 / ((float)setting.captureFrameRate)) * 10000000)
                {
                    break;
                }
            }

            return Mathf.Max(Mathf.Min(ret, history.Count - 1), 0);
        }

        private void LoadRootSample(Sample sample, bool needRecord, bool isHistory = false)
        {
            var instance = LuaDeepProfilerSetting.Instance;
            if (!isHistory)
            {
                if (instance.isRecord && !instance.isStartRecord)
                {
                    return;
                }
            }

            LuaProfilerTreeViewItem item;
            string f = sample.fullName;
            m_luaMemory = sample.currentLuaMemory;
            m_monoMemory = sample.currentMonoMemory;
            m_pssMemory = sample.pss;
            m_fps = sample.fps;
            m_power = sample.power;
            //if (isHistory)
            //{
            //    m_catchLuaMemory = sample.costLuaGC;
            //}

            if (!(instance.isRecord && !instance.isStartRecord))
            {
                historyCurve.SlotLuaMemory(sample.currentLuaMemory);
                historyCurve.SlotMonoMemory(sample.currentMonoMemory);
                historyCurve.SlotFpsMemory(sample.fps);
                historyCurve.SlotPssMemory(sample.pss);
                historyCurve.SlotPowerMemory(sample.power);
            }

            if (string.IsNullOrEmpty(sample.name))
            {
                
                return;
            }

            if (instance.isRecord && !isHistory)
            {
                history.Add(sample.Clone());
            }

            if (m_nodeDict.TryGetValue(f, out item))
            {
                bool isAddSample = item.AddSample(sample);
                needRebuild = needRebuild || isAddSample;
            }
            else
            {
                item = LuaProfilerTreeViewItem.Create(sample, 0, null);
                roots.Add(item);
                needRebuild = true;
            }

        }


        private void AddSearchStringItem(List<LuaProfilerTreeViewItem> roots, List<LuaProfilerTreeViewItem> rootList)
        {
            foreach (var item in roots)
            {
                if (item.displayName.IndexOf(m_searchString, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    rootList.Add(item);
                }
                AddSearchStringItem(item.childs, rootList);
            }
        }

        private void ReLoadTreeItems()
        {
            m_treeViewItems.Clear();
            List<LuaProfilerTreeViewItem> rootList = new List<LuaProfilerTreeViewItem>(roots);
            int sortIndex = multiColumnHeader.sortedColumnIndex;
            int sign = 0;
            if (sortIndex > 0)
            {
                sign = multiColumnHeader.IsSortedAscending(sortIndex) ? 1 : -1;
            }
            switch (sortIndex)
            {
                case 1: rootList.Sort((a, b) => { return sign * Math.Sign(a.totalLuaMemory - b.totalLuaMemory); }); break;
                case 2: rootList.Sort((a, b) => { return sign * Math.Sign(a.selfLuaMemory - b.selfLuaMemory); }); break;
                case 3: rootList.Sort((a, b) => { return sign * Math.Sign(a.totalMonoMemory - b.totalMonoMemory); }); break;
                case 4: rootList.Sort((a, b) => { return sign * Math.Sign(a.selfMonoMemory - b.selfMonoMemory); }); break;
                case 5: rootList.Sort((a, b) => { return sign * Math.Sign(a.showLuaGC - b.showLuaGC); }); break;
                case 6: rootList.Sort((a, b) => { return sign * Math.Sign(a.showMonoGC - b.showMonoGC); }); break;
                case 7: rootList.Sort((a, b) => { return sign * Math.Sign(a.currentTime - b.currentTime); }); break;
                case 8: rootList.Sort((a, b) => { return sign * Math.Sign(a.selfCostTime - b.selfCostTime); }); break;
                case 9: rootList.Sort((a, b) => { return sign * Math.Sign(a.averageTime - b.averageTime); }); break;
                case 10: rootList.Sort((a, b) => { return sign * Math.Sign(a.totalTime - b.totalTime); }); break;
                case 11: rootList.Sort((a, b) => { return sign * Math.Sign(a.totalCallTime - b.totalCallTime); }); break;
                case 12: rootList.Sort((a, b) => { return sign * Math.Sign(a.frameCalls - b.frameCalls); }); break;
            }
            foreach (var item in rootList)
            {
                SortChildren(sortIndex, item);
            }
            foreach (var item in rootList)
            {
                AddOneNode(item);
            }
        }

        private void SortChildren(int sortIndex, LuaProfilerTreeViewItem item)
        {
            int sign = -1;
            if (item.childs != null && item.childs.Count > 0)
            {
                List<LuaProfilerTreeViewItem> rootList = item.childs;
                switch (sortIndex)
                {
                    case 1: rootList.Sort((a, b) => { return sign * Math.Sign(a.totalLuaMemory - b.totalLuaMemory); }); break;
                    case 2: rootList.Sort((a, b) => { return sign * Math.Sign(a.selfLuaMemory - b.selfLuaMemory); }); break;
                    case 3: rootList.Sort((a, b) => { return sign * Math.Sign(a.totalMonoMemory - b.totalMonoMemory); }); break;
                    case 4: rootList.Sort((a, b) => { return sign * Math.Sign(a.selfMonoMemory - b.selfMonoMemory); }); break;
                    case 5: rootList.Sort((a, b) => { return sign * Math.Sign(a.showLuaGC - b.showLuaGC); }); break;
                    case 6: rootList.Sort((a, b) => { return sign * Math.Sign(a.showMonoGC - b.showMonoGC); }); break;
                    case 7: rootList.Sort((a, b) => { return sign * Math.Sign(a.currentTime - b.currentTime); }); break;
                    case 8: rootList.Sort((a, b) => { return sign * Math.Sign(a.selfCostTime - b.selfCostTime); }); break;
                    case 9: rootList.Sort((a, b) => { return sign * Math.Sign(a.averageTime - b.averageTime); }); break;
                    case 10: rootList.Sort((a, b) => { return sign * Math.Sign(a.totalTime - b.totalTime); }); break;
                    case 11: rootList.Sort((a, b) => { return sign * Math.Sign(a.totalCallTime - b.totalCallTime); }); break;
                    case 12: rootList.Sort((a, b) => { return sign * Math.Sign(a.frameCalls - b.frameCalls); }); break;
                }
                foreach (var t in rootList)
                {
                    SortChildren(sortIndex, t);
                }
            }
        }

        private void AddOneNode(LuaProfilerTreeViewItem root)
        {
            m_treeViewItems.Add(root);
            m_nodeDict[root.fullName] = root;

            if (root.children != null)
            {
                root.children.Clear();
            }
            foreach (var item in root.childs)
            {
                AddOneNode(item);
            }
        }

        const int MAX_DEAL_COUNT = 1024;
        protected override TreeViewItem BuildRoot()
        {
            if (!needRebuild)
            {
                return m_root;
            }
            if (string.IsNullOrEmpty(m_searchString))
            {
                ReLoadTreeItems();
                // Utility method that initializes the TreeViewItem.children and -parent for all items.
                SetupParentsAndChildrenFromDepths(m_root, m_treeViewItems);
            }
            else
            {
                m_treeViewItems.Clear();

                List<LuaProfilerTreeViewItem> rootList = new List<LuaProfilerTreeViewItem>();
                AddSearchStringItem(roots, rootList);

                foreach (var item in rootList)
                {
                    if (m_toggleMerge)
                    {
                        LuaProfilerTreeViewItem t;
                        if (m_treeViewItemDict.TryGetValue(item.displayName, out t))
                        {
                            t.AddSample(item.CopySelfToSample());
                        }
                        else
                        {
                            LuaProfilerTreeViewItem mt = LuaProfilerTreeViewItem.Create(item, m_root);
                            m_treeViewItemDict.Add(item.displayName, mt);
                            m_treeViewItems.Add(mt);
                        }
                    }
                    else
                    {
                        m_treeViewItems.Add(LuaProfilerTreeViewItem.Create(item, m_root));
                    }

                }
                m_treeViewItemDict.Clear();

                int sortIndex = multiColumnHeader.sortedColumnIndex;
                int sign = 0;
                if (sortIndex > 0)
                {
                    sign = multiColumnHeader.IsSortedAscending(sortIndex) ? 1 : -1;
                }
                switch (sortIndex)
                {
                    case 1: m_treeViewItems.Sort((a, b) => { return sign * Math.Sign(((LuaProfilerTreeViewItem)a).totalLuaMemory - ((LuaProfilerTreeViewItem)b).totalLuaMemory); }); break;
                    case 2: m_treeViewItems.Sort((a, b) => { return sign * Math.Sign(((LuaProfilerTreeViewItem)a).selfLuaMemory - ((LuaProfilerTreeViewItem)b).selfLuaMemory); }); break;
                    case 3: m_treeViewItems.Sort((a, b) => { return sign * Math.Sign(((LuaProfilerTreeViewItem)a).totalMonoMemory - ((LuaProfilerTreeViewItem)b).totalMonoMemory); }); break;
                    case 4: m_treeViewItems.Sort((a, b) => { return sign * Math.Sign(((LuaProfilerTreeViewItem)a).selfMonoMemory - ((LuaProfilerTreeViewItem)b).selfMonoMemory); }); break;
                    case 5: m_treeViewItems.Sort((a, b) => { return sign * Math.Sign(((LuaProfilerTreeViewItem)a).showLuaGC - ((LuaProfilerTreeViewItem)b).showLuaGC); }); break;
                    case 6: m_treeViewItems.Sort((a, b) => { return sign * Math.Sign(((LuaProfilerTreeViewItem)a).showMonoGC - ((LuaProfilerTreeViewItem)b).showMonoGC); }); break;
                    case 7: m_treeViewItems.Sort((a, b) => { return sign * Math.Sign(((LuaProfilerTreeViewItem)a).currentTime - ((LuaProfilerTreeViewItem)b).currentTime); }); break;
                    case 8: m_treeViewItems.Sort((a, b) => { return sign * Math.Sign(((LuaProfilerTreeViewItem)a).selfCostTime - ((LuaProfilerTreeViewItem)b).selfCostTime); }); break;
                    case 9: m_treeViewItems.Sort((a, b) => { return sign * Math.Sign(((LuaProfilerTreeViewItem)a).averageTime - ((LuaProfilerTreeViewItem)b).averageTime); }); break;
                    case 10: m_treeViewItems.Sort((a, b) => { return sign * Math.Sign(((LuaProfilerTreeViewItem)a).totalTime - ((LuaProfilerTreeViewItem)b).totalTime); }); break;
                    case 11: m_treeViewItems.Sort((a, b) => { return sign * Math.Sign(((LuaProfilerTreeViewItem)a).totalCallTime - ((LuaProfilerTreeViewItem)b).totalCallTime); }); break;
                    case 12: m_treeViewItems.Sort((a, b) => { return sign * Math.Sign(((LuaProfilerTreeViewItem)a).frameCalls - ((LuaProfilerTreeViewItem)b).frameCalls); }); break;
                }

                SetupParentsAndChildrenFromDepths(m_root, m_treeViewItems);
            }

            needRebuild = false;
            // Return root of the tree
            return m_root;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = (LuaProfilerTreeViewItem)args.item;
            if (m_gs == null)
            {
                m_gs = new GUIStyle(GUI.skin.label);
                m_gs.alignment = TextAnchor.MiddleCenter;
            }
            var color = m_gs.normal.textColor;
            if (!this.IsSelected(item.id))
            {
                if (item.isLua)
                {
                    m_gs.normal.textColor = m_luaColor;
                }
                else if (item.isError)
                {
                    m_gs.normal.textColor = m_errorColor;
                }
                else
                {
                    m_gs.normal.textColor = m_monoColor;
                }
            }
            else
            {
                m_gs.normal.textColor = m_selectColor;
            }

            Rect r = args.GetCellRect(0);
            args.rowRect = r;
            base.RowGUI(args);

            r = args.GetCellRect(1);
            GUI.Label(r, GetMemoryString(item.totalLuaMemory), m_gs);

            r = args.GetCellRect(2);
            GUI.Label(r, GetMemoryString(item.selfLuaMemory), m_gs);

            r = args.GetCellRect(3);
            GUI.Label(r, GetMemoryString(item.totalMonoMemory), m_gs);

            r = args.GetCellRect(4);
            GUI.Label(r, GetMemoryString(item.selfMonoMemory), m_gs);

            r = args.GetCellRect(5);
            GUI.Label(r, GetMemoryString(item.showLuaGC), m_gs);

            r = args.GetCellRect(6);
            GUI.Label(r, GetMemoryString(item.showMonoGC), m_gs);

            r = args.GetCellRect(7);
            GUI.Label(r, ((float)item.currentTime / 10000).ToString("f2") + "ms", m_gs);

            r = args.GetCellRect(8);
            GUI.Label(r, ((float)item.selfCostTime / 10000).ToString("f2") + "ms", m_gs);

            r = args.GetCellRect(9);
            GUI.Label(r, ((float)item.averageTime / 10000).ToString("f2") + "ms", m_gs);

            r = args.GetCellRect(10);
            GUI.Label(r, ((float)item.totalTime / 10000000).ToString("f6") + "s", m_gs);

            r = args.GetCellRect(11);
            GUI.Label(r, GetMemoryString(item.totalCallTime, ""), m_gs);

            r = args.GetCellRect(12);
            GUI.Label(r, item.frameCalls.ToString(), m_gs);

            m_gs.normal.textColor = color;
        }

    }
#endif
}
#endif