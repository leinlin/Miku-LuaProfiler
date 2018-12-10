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
    using System.Reflection;
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
            dict.Add(mt.fullName, mt);

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
                //return _showGC;
                if (!EditorApplication.isPlaying)
                {
                    return totalLuaMemory / totalCallTime;
                }

                if (Mathf.Abs(Time.frameCount - _frameCount) <= 30)
                {
                    return _showLuaGC;
                }
                else { return 0; }
            }
        }
        private long _showMonoGC = 0;
        public long showMonoGC
        {
            get
            {
                //return _showGC;
                if (!EditorApplication.isPlaying)
                {
                    return totalMonoMemory / totalCallTime;
                }

                if (Mathf.Abs(Time.frameCount - _frameCount) <= 30)
                {
                    return _showMonoGC;
                }
                else { return 0; }
            }
        }
        public long totalMonoMemory { private set; get; }
        public long totalLuaMemory { private set; get; }
        public long totalTime { private set; get; }
        public long averageTime { private set; get; }
        public long currentTime { private set; get; }
        public int totalCallTime { private set; get; }

        public int line { private set; get; }
        public string filePath { private set; get; }
        private string m_originName;

        public string fullName
        {
            get;
            private set;
        }
        public readonly List<LuaProfilerTreeViewItem> childs = new List<LuaProfilerTreeViewItem>();
        public LuaProfilerTreeViewItem rootFather { private set; get; }
        public LuaProfilerTreeViewItem father { private set; get; }
        private int _frameCount;
        public LuaProfilerTreeViewItem()
        {
        }

        private static readonly char[] splitDot = new char[] { ',' };
        private static readonly char[] splitFun = new char[] { '&' };
        public void ResetBySample(Sample sample, int depth, LuaProfilerTreeViewItem father)
        {
            if (sample != null)
            {
                filePath = sample.name.Split(splitDot, 2)[0].Trim();
                int tmpLine = 0;
                int.TryParse(Regex.Match(sample.name, @"(?<=(line:))\d*(?=(&))").Value, out tmpLine);
                line = tmpLine;

                _showMonoGC = sample.costMonoGC;
                _showLuaGC = sample.costLuaGC;
                totalMonoMemory = sample.costMonoGC;
                totalLuaMemory = sample.costLuaGC;
                totalTime = sample.costTime;
                string[] tmp = sample.name.Split(splitFun, 2);
                if (tmp.Length >= 2)
                {
                    displayName = tmp[1].Trim();
                }
                else
                {
                    displayName = sample.name;
                }
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
                totalTime = 0;
                displayName = "root";
                fullName = "root";
                frameCalls = 0;
                currentTime = 0;
                totalCallTime = 1;
            }
            averageTime = totalTime / totalCallTime;

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
                        if (LuaProfilerTreeView.CheckSampleValid(sample))
                        {
                            var item = Create(sample.childs[i], depth + 1, this);
                            childs.Add(item);
                        }
                    }
                }
            }
            this.father = father;

            rootFather = this;
            while (true)
            {
                if (rootFather.father == null)
                {
                    break;
                }
                rootFather = rootFather.father;
            }

            _frameCount = Time.frameCount;
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
            totalMonoMemory += sample.costMonoGC;

            totalTime += sample.costTime;
            totalCallTime += sample.calls;
            averageTime = totalTime / totalCallTime;
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
                    if (LuaProfilerTreeView.CheckSampleValid(sample))
                    {
                        var treeItem = Create(sampleChild, depth + 1, this);
                        childs.Add(treeItem);
                    }
                }
            }
            _frameCount = Time.frameCount;
        }

        public Sample CopyToSample()
        {
            Sample s = new Sample();

            s.calls = totalCallTime;
            s.frameCount = LuaProfiler.m_frameCount;
            s.costLuaGC = totalLuaMemory;
            s.costMonoGC = totalMonoMemory;
            s.name = m_originName;
            s.costTime = totalTime;

            int childCount = childs.Count;
            for (int i = 0; i < childCount; i++)
            {
                Sample child = childs[i].CopyToSample();
                child.fahter = s;
            }
            s.captureUrl = "";
            s.currentLuaMemory = 0;

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
        private List<int> m_expandIds = new List<int>();
        private readonly LuaProfilerTreeViewItem m_root;
        private readonly List<TreeViewItem> m_treeViewItems = new List<TreeViewItem>();
        private GUIStyle m_gs;
        private Queue<Sample> m_historySamplesQueue = new Queue<Sample>(256);
        private Queue<Sample> m_runningSamplesQueue = new Queue<Sample>(256);
        private Dictionary<string, Sample> m_runingSampleDict = new Dictionary<string, Sample>(256);
        private long m_luaMemory = 0;

        public bool needRebuild = true;
        public readonly List<Sample> history = new List<Sample>(216000);
        public string startUrl = null;
        public string endUrl = null;
        #endregion

        public LuaProfilerTreeView(TreeViewState treeViewState, float width)
            : base(treeViewState, CreateDefaultMultiColumnHeaderState(width))
        {
            LuaProfiler.SetSampleEnd(LoadRootSample);
            m_root = LuaProfilerTreeViewItem.Create(null, -1, null);
            needRebuild = true;
            multiColumnHeader.sortingChanged += (header) => { needRebuild = true; };
            history.Clear();
            Reload();
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
            }
            needRebuild = true;
        }

        protected override void DoubleClickedItem(int id)
        {
            base.DoubleClickedItem(id);
            var selectItem = FindItem(id, BuildRoot());
            var item = (LuaProfilerTreeViewItem)selectItem;
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

            startUrl = history[start].captureUrl;
            endUrl = history[end].captureUrl;

            end = Mathf.Min(history.Count - 1, end);
            for (int i = start; i <= end; i++)
            {
                LoadHistoryRootSample(history[i]);
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
            history.AddRange(Sample.DeserializeList(path));
            ReLoadSamples(0, history.Count);
        }
        public string GetLuaMemory()
        {
            if (LuaProfiler.mainL != IntPtr.Zero)
            {
                return LuaProfiler.GetLuaMemory();
            }
            else
            {
                return LuaProfiler.GetMemoryString(m_luaMemory);
            }
        }

        private void LoadHistoryRootSample(Sample sample)
        {
            lock (this)
            {
                m_historySamplesQueue.Enqueue(sample);
            }
        }

        private void LoadRootSample(Sample sample)
        {
            lock (this)
            {
                if (!CheckSampleValid(sample))
                {
                    sample.Restore();
                    return;
                }
                Sample item = null;

                if (m_runingSampleDict.TryGetValue(sample.name, out item))
                {
                    item.AddSample(sample); 
                    sample.Restore();
                    return;
                }
                m_runingSampleDict.Add(sample.name, sample);
                m_runningSamplesQueue.Enqueue(sample);
            }
        }

        public int GetNextProgramFrame(int start)
        {
            int ret = start + 1;

            for (int i = start + 1, imax = history.Count; i < imax; i++)
            {
                ret = i;
                var s = history[i];
                if (s.costLuaGC > LuaDeepProfilerSetting.Instance.captureGC)
                {
                    break;
                }
                else if (s.costTime >= 1 / 30.0f * 10000000)
                {
                    break;
                }
            }

            return Mathf.Max(Mathf.Min(ret, history.Count - 1), 0);
        }

        public int GetPreProgramFrame(int start)
        {
            int ret = start - 1;

            for (int i = start - 1; i >= 0; i--)
            {
                ret = i;
                var s = history[i];
                if (s.costLuaGC > LuaDeepProfilerSetting.Instance.captureGC)
                {
                    break;
                }
                else if (s.costTime >= 1 / 30.0f * 10000000)
                {
                    break;
                }
            }

            return Mathf.Max(Mathf.Min(ret, history.Count - 1), 0);
        }

        public static bool CheckSampleValid(Sample sample)
        {
            bool result = false;

            do
            {
                if (sample.costLuaGC > 0)
                {
                    result = true;
                    break;
                }

                if (sample.costMonoGC > 0)
                {
                    result = true;
                    break;
                }

                if (sample.costTime > 100000)
                {
                    result = true;
                    break;
                }

            } while (false);


            return result;
        }

        private void LoadRootSample(Sample sample, bool needRecord)
        {
            LuaProfilerTreeViewItem item;
            string f = sample.fullName;
            m_luaMemory = sample.currentLuaMemory;
            if (m_nodeDict.TryGetValue(f, out item))
            {
                item.AddSample(sample);
                if (needRecord)
                {
                    history.Add(sample.Clone());
                }
            }
            else
            {
                if (CheckSampleValid(sample))
                {
                    item = LuaProfilerTreeViewItem.Create(sample, 0, null);
                    roots.Add(item);
                    needRebuild = true;
                    if (needRecord)
                    {
                        history.Add(sample.Clone());
                    }
                }
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
                case 2: rootList.Sort((a, b) => { return sign * Math.Sign(a.totalMonoMemory - b.totalMonoMemory); }); break;
                case 3: rootList.Sort((a, b) => { return sign * Math.Sign(a.currentTime - b.currentTime); }); break;
                case 4: rootList.Sort((a, b) => { return sign * Math.Sign(a.averageTime - b.averageTime); }); break;
                case 5: rootList.Sort((a, b) => { return sign * Math.Sign(a.totalTime - b.totalTime); }); break;
                case 6: rootList.Sort((a, b) => { return sign * Math.Sign(a.showLuaGC - b.showLuaGC); }); break;
                case 7: rootList.Sort((a, b) => { return sign * Math.Sign(a.showMonoGC - b.showMonoGC); }); break;
                case 8: rootList.Sort((a, b) => { return sign * Math.Sign(a.totalCallTime - b.totalCallTime); }); break;
                case 9: rootList.Sort((a, b) => { return sign * Math.Sign(a.frameCalls - b.frameCalls); }); break;
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
                List<LuaProfilerTreeViewItem> childList = item.childs;
                switch (sortIndex)
                {
                    case 1: childList.Sort((a, b) => { return sign * Math.Sign(a.totalLuaMemory - b.totalLuaMemory); }); break;
                    case 2: childList.Sort((a, b) => { return sign * Math.Sign(a.totalMonoMemory - b.totalMonoMemory); }); break;
                    case 3: childList.Sort((a, b) => { return sign * Math.Sign(a.currentTime - b.currentTime); }); break;
                    case 4: childList.Sort((a, b) => { return sign * Math.Sign(a.averageTime - b.averageTime); }); break;
                    case 5: childList.Sort((a, b) => { return sign * Math.Sign(a.totalTime - b.totalTime); }); break;
                    case 6: childList.Sort((a, b) => { return sign * Math.Sign(a.showLuaGC - b.showLuaGC); }); break;
                    case 7: childList.Sort((a, b) => { return sign * Math.Sign(a.showMonoGC - b.showMonoGC); }); break;
                    case 8: childList.Sort((a, b) => { return sign * Math.Sign(a.totalCallTime - b.totalCallTime); }); break;
                    case 9: childList.Sort((a, b) => { return sign * Math.Sign(a.frameCalls - b.frameCalls); }); break;
                }
                foreach (var t in childList)
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
                m_runingSampleDict.Clear();
            }
            else if (m_historySamplesQueue.Count > 0)
            {
                int delNum = 0;
                while (m_historySamplesQueue.Count > 0 && delNum < MAX_DEAL_COUNT)
                {
                    lock (this)
                    {
                        Sample s = m_historySamplesQueue.Dequeue();
                        LoadRootSample(s, false);
                    }
                    delNum++;
                }
            }

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
            Rect r = args.GetCellRect(0);
            args.rowRect = r;
            base.RowGUI(args);

            r = args.GetCellRect(1);
            GUI.Label(r, LuaProfiler.GetMemoryString(item.totalLuaMemory), m_gs);

            r = args.GetCellRect(2);
            GUI.Label(r, LuaProfiler.GetMemoryString(item.totalMonoMemory), m_gs);

            r = args.GetCellRect(3);
            GUI.Label(r, ((float)item.currentTime / 10000000).ToString("f6") + "s", m_gs);

            r = args.GetCellRect(4);
            GUI.Label(r, ((float)item.averageTime / 10000000).ToString("f6") + "s", m_gs);

            r = args.GetCellRect(5);
            GUI.Label(r, ((float)item.totalTime / 10000000).ToString("f6") + "s", m_gs);

            r = args.GetCellRect(6);
            GUI.Label(r, LuaProfiler.GetMemoryString(item.showLuaGC), m_gs);

            r = args.GetCellRect(7);
            GUI.Label(r, LuaProfiler.GetMemoryString(item.showMonoGC), m_gs);

            r = args.GetCellRect(8);
            GUI.Label(r, LuaProfiler.GetMemoryString(item.totalCallTime, ""), m_gs);

            r = args.GetCellRect(9);
            GUI.Label(r, item.frameCalls.ToString(), m_gs);

        }

    }
#endif
}
