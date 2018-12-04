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
        //没什么意义，因为Lua 执行代码的同时 异步GC，所以导致GC的数字一直闪烁，用上这个去闪烁
        private long _showGC = 0;
        public long showGC
        {
            private set
            {
                _showGC = value;
            }
            get
            {
                return _showGC;
                //if (!UnityEditor.EditorApplication.isPlaying)
                //{
                //    return _showGC;
                //}

                //if (Time.frameCount == _frameCount)
                //{
                //    return _showGC;
                //}
                //else { return 0; }
            }
        }
        public long totalMemory { private set; get; }
        public long totalTime { private set; get; }
        public long averageTime { private set; get; }
        public float currentTime { private set; get; }
        public int totalCallTime { private set; get; }
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

        public void ResetBySample(Sample sample, int depth, LuaProfilerTreeViewItem father)
        {
            if (sample != null)
            {
                totalMemory = sample.costGC;
                _showGC = sample.costGC;
                totalTime = (long)(sample.costTime * 1000000);
                displayName = sample.name;
                fullName = sample.fullName;
                frameCalls = sample.calls;
                currentTime = sample.costTime;
                totalCallTime = sample.calls;
            }
            else
            {
                totalMemory = 0;
                _showGC = 0;
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
                        var item = Create(sample.childs[i], depth + 1, this);
                        childs.Add(item);
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
                _showGC += sample.costGC;
                frameCalls += sample.calls;
                currentTime += sample.costTime;
            }
            else
            {
                _showGC = sample.costGC;
                frameCalls = sample.calls;
                currentTime = sample.costTime;
            }
            totalMemory += sample.costGC;

            totalTime += (long)(sample.costTime * 1000000);
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
                    var treeItem = Create(sampleChild, depth + 1, this);
                    childs.Add(treeItem);
                }
            }
            _frameCount = Time.frameCount;
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
                    width = 500,
                    minWidth = 200,
                    maxWidth = 2000,
                    autoResize = true,
                    canSort = false,
                    allowToggleVisibility = true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("totalMemory"),
                    contextMenuText = "totalMemory",
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
                    headerContent = new GUIContent("GC"),
                    contextMenuText = "GC",
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = false,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 60,
                    minWidth = 60,
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
            string fileName = selectItem.displayName.Split(new char[] { ',' }, 2)[0].Trim();
            try
            {
                int line = 0;
                int.TryParse(Regex.Match(selectItem.displayName, @"(?<=(line:))\d*(?=( ))").Value, out line);
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
                if (s.costGC > LuaDeepProfilerSetting.Instance.captureGC)
                {
                    break;
                }
                else if (s.costTime >= 1 / 30.0f)
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
                if (s.costGC > LuaDeepProfilerSetting.Instance.captureGC)
                {
                    break;
                }
                else if (s.costTime >= 1 / 30.0f)
                {
                    break;
                }
            }

            return Mathf.Max(Mathf.Min(ret, history.Count - 1), 0);
        }

        private void LoadRootSample(Sample sample, bool needRecord)
        {
            LuaProfilerTreeViewItem item;
            string f = sample.fullName;
            m_luaMemory = sample.currentLuaMemory;
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
            if (needRecord)
            {
                history.Add(sample.Clone());
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
                sign = multiColumnHeader.IsSortedAscending(sortIndex) ? -1 : 1;
            }
            switch (sortIndex)
            {
                case 1: rootList.Sort((a, b) => { return sign * Math.Sign(a.totalMemory - b.totalMemory); }); break;
                case 2: rootList.Sort((a, b) => { return sign * Math.Sign(a.currentTime - b.currentTime); }); break;
                case 3: rootList.Sort((a, b) => { return sign * Math.Sign(a.averageTime - b.averageTime); }); break;
                case 4: rootList.Sort((a, b) => { return sign * Math.Sign(a.totalTime - b.totalTime); }); break;
                case 5: rootList.Sort((a, b) => { return sign * Math.Sign(a.showGC - b.showGC); }); break;
                case 6: rootList.Sort((a, b) => { return sign * Math.Sign(a.totalCallTime - b.totalCallTime); }); break;
                case 7: rootList.Sort((a, b) => { return sign * Math.Sign(a.frameCalls - b.frameCalls); }); break;
            }
            foreach (var item in rootList)
            {
                AddOneNode(item);
                SortChildren(sortIndex, sign, item);
            }
        }

        private void SortChildren(int sortIndex, int sign, LuaProfilerTreeViewItem item)
        {
            if (item.childs != null && item.childs.Count > 0)
            {
                List<LuaProfilerTreeViewItem> rootList = item.childs;
                switch (sortIndex)
                {
                    case 1: rootList.Sort((a, b) => { return sign * Math.Sign(a.totalMemory - b.totalMemory); }); break;
                    case 2: rootList.Sort((a, b) => { return sign * Math.Sign(a.currentTime - b.currentTime); }); break;
                    case 3: rootList.Sort((a, b) => { return sign * Math.Sign(a.averageTime - b.averageTime); }); break;
                    case 4: rootList.Sort((a, b) => { return sign * Math.Sign(a.totalTime - b.totalTime); }); break;
                    case 5: rootList.Sort((a, b) => { return sign * Math.Sign(a.showGC - b.showGC); }); break;
                    case 6: rootList.Sort((a, b) => { return sign * Math.Sign(a.totalCallTime - b.totalCallTime); }); break;
                    case 7: rootList.Sort((a, b) => { return sign * Math.Sign(a.frameCalls - b.frameCalls); }); break;
                }
                foreach (var t in rootList)
                {
                    SortChildren(sortIndex, sign, t);
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
                if (m_historySamplesQueue.Count == 0)
                {
                    foreach (var t in roots)
                    {
                        ExpandNodeRecursive(t);
                    }
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

        private void ExpandNodeRecursive(LuaProfilerTreeViewItem t)
        {
            SetExpanded(t.id, true);
            foreach (var child in t.childs)
            {
                ExpandNodeRecursive(child);
            }
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

            GUI.Label(r, LuaProfiler.GetMemoryString(item.totalMemory), m_gs);

            r = args.GetCellRect(2);
            GUI.Label(r, item.currentTime.ToString("f6") + "s", m_gs);

            r = args.GetCellRect(3);
            GUI.Label(r, ((float)item.averageTime / 1000000).ToString("f6") + "s", m_gs);

            r = args.GetCellRect(4);
            GUI.Label(r, ((float)item.totalTime / 1000000).ToString("f6") + "s", m_gs);

            r = args.GetCellRect(5);
            GUI.Label(r, LuaProfiler.GetMemoryString(item.showGC), m_gs);

            r = args.GetCellRect(6);
            GUI.Label(r, LuaProfiler.GetMemoryString(item.totalCallTime, ""), m_gs);

            r = args.GetCellRect(7);
            GUI.Label(r, item.frameCalls.ToString(), m_gs);

        }

    }
#endif
}
