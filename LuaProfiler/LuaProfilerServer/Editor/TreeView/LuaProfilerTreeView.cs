/*
* ==============================================================================
* Filename: LuaProfilerTreeView
* Created:  2018/7/13 14:29:22
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/

namespace MikuLuaProfiler
{
#if UNITY_5_6_OR_NEWER
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;
    using UnityEditor;
    using UnityEditor.IMGUI.Controls;
    using UnityEngine;


    #region item
    //The TreeElement data class is extended to hold extra data, which you can show and edit in the front-end TreeView.
    public class LuaProfilerTreeViewItem : TreeViewItem
    {
        private static ObjectPool<LuaProfilerTreeViewItem> objectPool = new ObjectPool<LuaProfilerTreeViewItem>(30);
        public static LuaProfilerTreeViewItem Create(Sample sample, int depth, LuaProfilerTreeViewItem father)
        {
            var dict = LuaProfilerTreeView.m_nodeDict;

            LuaProfilerTreeViewItem mt;
            mt = objectPool.GetObject();
            mt.ResetBySample(sample, depth, father);
            dict[mt.fullName] =  mt;

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
                if (!NetWorkServer.CheckIsReceiving())
                {
                    return totalLuaMemory / totalCallTime;
                }
                else if (s_frameCount - _frameCount <= 30)
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
                if (!NetWorkServer.CheckIsReceiving())
                {
                    return totalMonoMemory / totalCallTime;
                }
                else if (s_frameCount - _frameCount <= 30)
                {
                    return _showMonoGC;
                }
                else
                {
                    return 0;
                }
            }
        }
        private static int s_frameCount = 0;
        public long totalMonoMemory { private set; get; }
        public long selfMonoMemory { private set; get; }
        public long totalLuaMemory { private set; get; }
        public long selfLuaMemory { private set; get; }
        public long totalTime { private set; get; }
        public long averageTime { private set; get; }
        public long currentTime { private set; get; }
        public int totalCallTime { private set; get; }

        private int m_line = -1;
        public int line {
            private set
            {
                m_line = value;
            }
            get
            {
                return m_line;
            }
        }
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
        //private static readonly char[] splitFun = new char[] { '&' };
        public void ResetBySample(Sample sample, int depth, LuaProfilerTreeViewItem father)
        {
            if (sample != null)
            {
                filePath = sample.name.Split(splitDot, 2)[0].Trim();
                int tmpLine = -1;
                if (int.TryParse(Regex.Match(sample.name, @"(?<=(line:))\d*(?=(&))").Value, out tmpLine))
                {
                    line = tmpLine;
                }
                else
                {
                    line = -1;
                }

                _showMonoGC = sample.costMonoGC;
                _showLuaGC = sample.costLuaGC;
                totalMonoMemory = sample.costMonoGC;
                totalLuaMemory = sample.costLuaGC;
                selfLuaMemory = sample.selfLuaGC;
                selfMonoMemory = sample.selfMonoGC;
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
                s_frameCount = sample.frameCount;
            }
            this.father = father;
        }

        public void AddSample(Sample sample)
        {
            if (_frameCount == sample.frameCount)
            {
                frameCalls += sample.calls;
                currentTime += sample.costTime;
                _showMonoGC += sample.costMonoGC;
                _showLuaGC += sample.costLuaGC;
            }
            else
            {
                frameCalls = sample.calls;
                currentTime = sample.costTime;
                _showMonoGC = sample.costMonoGC;
                _showLuaGC = sample.costLuaGC;
            }

            totalLuaMemory += sample.costLuaGC;
            selfLuaMemory += sample.selfLuaGC;
            totalMonoMemory += sample.costMonoGC;
            selfMonoMemory += sample.selfMonoGC;

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
                    var treeItem = Create(sampleChild, depth + 1, this);
                    childs.Add(treeItem);
                }
            }
            _frameCount = sample.frameCount;
            s_frameCount = sample.frameCount;
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
            s.captureUrl = "";
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
        private static Color m_luaColor = new Color(0.4f, 0.7f, 0.9f, 1.0f);
        private List<int> m_expandIds = new List<int>();
        private readonly LuaProfilerTreeViewItem m_root;
        private readonly List<TreeViewItem> m_treeViewItems = new List<TreeViewItem>();
        private GUIStyle m_gs;
        private Queue<Sample> m_runningSamplesQueue = new Queue<Sample>(256);
        private Queue<Sample> m_historySamplesQueue = new Queue<Sample>(256);
        private long m_luaMemory = 0;
        private long m_monoMemory = 0;
        private long m_pssMemory = 0;

        public bool needRebuild = true;
        public readonly HistoryCurve historyCurve = new HistoryCurve(1024);
        public readonly List<Sample> history = new List<Sample>(2160);
        public string startUrl = null;
        public string endUrl = null;
        #endregion

        public LuaProfilerTreeView(TreeViewState treeViewState, float width)
            : base(treeViewState, CreateDefaultMultiColumnHeaderState(width))
        {
            //NetWorkServer.BeginListen("127.0.0.1", 23333);
            //NetWorkClient.ConnectServer("127.0.0.1", 23333);

            //LuaProfiler.SetSampleEnd(LoadRootSample);
            m_root = LuaProfilerTreeViewItem.Create(null, -1, null);
            needRebuild = true;
            multiColumnHeader.sortingChanged += (header) => { needRebuild = true; };
            history.Clear();
            historyCurve.Clear();
            EditorApplication.update -= DequeueSample;
            EditorApplication.update += DequeueSample;
            Reload();
        }

        private void DequeueSample()
        {
            lock (this)
            {
                if (m_runningSamplesQueue.Count > 0)
                {
                    while (m_runningSamplesQueue.Count > 0)
                    {
                        Sample s = null;
                        lock (this)
                        {
                            s = m_runningSamplesQueue.Dequeue();
                            LoadRootSample(s, LuaDeepProfilerSetting.Instance.isRecord);

                            s.Restore();
                        }
                    }
                }
                else if (m_historySamplesQueue.Count > 0)
                {
                    int delNum = 0;
                    while (m_historySamplesQueue.Count > 0 && delNum < MAX_DEAL_COUNT)
                    {
                        lock (this)
                        {
                            Sample s = m_historySamplesQueue.Dequeue();
                            LoadRootSample(s, false, true);
                        }
                        delNum++;
                    }
                }
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
                    maxWidth = 1000,
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
                    canSort = false,
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
                    canSort = false,
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
            needRebuild = true;
        }

        protected override void DoubleClickedItem(int id)
        {
            base.DoubleClickedItem(id);
            if (string.IsNullOrEmpty(searchString))
            {
                var selectItem = FindItem(id, BuildRoot());
                var item = (LuaProfilerTreeViewItem)selectItem;
                Debug.Log(item.displayName);
                /*
                if (item.line == -1)
                {
                    Debug.Log("please wait");
                    return;
                }

                string fileName = item.filePath;
                Debug.Log(fileName);
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
                }*/
            }
            else
            {
                searchString = "";
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

        public static Dictionary<string, LuaProfilerTreeViewItem> m_nodeDict = new Dictionary<string, LuaProfilerTreeViewItem>();
        public void ReLoadSamples(int start, int end)
        {
            if (history.Count == 0) return;
            Clear(false);
            end = Mathf.Max(Mathf.Min(end, history.Count - 1), 0);
            start = Mathf.Max(Mathf.Min(start, history.Count - 1), 0);

            if (end == start)
            {
                LoadRootSample(history[start], false, true);
                return;
            }

            startUrl = history[start].captureUrl;
            endUrl = history[end].captureUrl;

            end = Mathf.Min(history.Count - 1, end);
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
        private void LoadHistoryRootSample(Sample sample)
        {
            lock (this)
            {
                m_historySamplesQueue.Enqueue(sample);
            }
        }

        public void LoadRootSample(Sample sample)
        {
            lock (this)
            {
                m_runningSamplesQueue.Enqueue(sample);
            }
        }

        public int GetNextProgramFrame(int start)
        {
            int ret = start + 1;

            var setting = LuaDeepProfilerSetting.Instance;
            for (int i = start + 1, imax = history.Count; i < imax; i++)
            {
                ret = i;
                var s = history[i];
                if (s.costLuaGC > setting.captureLuaGC)
                {
                    break;
                }
                else if (s.costMonoGC > setting.captureMonoGC)
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
                if (s.costLuaGC > setting.captureLuaGC)
                {
                    break;
                }
                else if (s.costMonoGC > setting.captureMonoGC)
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

            if (!(instance.isRecord && !instance.isStartRecord))
            {
                historyCurve.SlotLuaMemory(sample.currentLuaMemory);
                historyCurve.SlotMonoMemory(sample.currentMonoMemory);
                historyCurve.SlotFpsMemory(sample.fps);
                historyCurve.SlotPssMemory(sample.pss);
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
                item.AddSample(sample);
            }
            else
            {
                item = LuaProfilerTreeViewItem.Create(sample, 0, null);
                roots.Add(item);
                needRebuild = true;
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
                case 5: rootList.Sort((a, b) => { return sign * Math.Sign(a.currentTime - b.currentTime); }); break;
                case 6: rootList.Sort((a, b) => { return sign * Math.Sign(a.averageTime - b.averageTime); }); break;
                case 7: rootList.Sort((a, b) => { return sign * Math.Sign(a.totalTime - b.totalTime); }); break;
                case 8: rootList.Sort((a, b) => { return sign * Math.Sign(a.showLuaGC - b.showLuaGC); }); break;
                case 9: rootList.Sort((a, b) => { return sign * Math.Sign(a.showMonoGC - b.showMonoGC); }); break;
                case 10: rootList.Sort((a, b) => { return sign * Math.Sign(a.totalCallTime - b.totalCallTime); }); break;
                case 11: rootList.Sort((a, b) => { return sign * Math.Sign(a.frameCalls - b.frameCalls); }); break;
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
                    case 5: rootList.Sort((a, b) => { return sign * Math.Sign(a.currentTime - b.currentTime); }); break;
                    case 6: rootList.Sort((a, b) => { return sign * Math.Sign(a.averageTime - b.averageTime); }); break;
                    case 7: rootList.Sort((a, b) => { return sign * Math.Sign(a.totalTime - b.totalTime); }); break;
                    case 8: rootList.Sort((a, b) => { return sign * Math.Sign(a.showLuaGC - b.showLuaGC); }); break;
                    case 9: rootList.Sort((a, b) => { return sign * Math.Sign(a.showMonoGC - b.showMonoGC); }); break;
                    case 10: rootList.Sort((a, b) => { return sign * Math.Sign(a.totalCallTime - b.totalCallTime); }); break;
                    case 11: rootList.Sort((a, b) => { return sign * Math.Sign(a.frameCalls - b.frameCalls); }); break;
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
            ReLoadTreeItems();
            // Utility method that initializes the TreeViewItem.children and -parent for all items.
            SetupParentsAndChildrenFromDepths(m_root, m_treeViewItems);
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
            Color color = m_gs.normal.textColor;
            if (item.line != -1)
            {
                m_gs.normal.textColor = m_luaColor * color;
            }
            else
            {
                m_gs.normal.textColor = color * Color.green;
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
            GUI.Label(r, ((float)item.currentTime / 10000000).ToString("f6") + "s", m_gs);

            r = args.GetCellRect(6);
            GUI.Label(r, ((float)item.averageTime / 10000000).ToString("f6") + "s", m_gs);

            r = args.GetCellRect(7);
            GUI.Label(r, ((float)item.totalTime / 10000000).ToString("f6") + "s", m_gs);

            r = args.GetCellRect(8);
            GUI.Label(r, GetMemoryString(item.showLuaGC), m_gs);

            r = args.GetCellRect(9);
            GUI.Label(r, GetMemoryString(item.showMonoGC), m_gs);

            r = args.GetCellRect(10);
            GUI.Label(r, GetMemoryString(item.totalCallTime, ""), m_gs);

            r = args.GetCellRect(11);
            GUI.Label(r, item.frameCalls.ToString(), m_gs);

            m_gs.normal.textColor = color;
        }

    }
#endif
}
