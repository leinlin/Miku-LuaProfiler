/*
* ==============================================================================
* Filename: LuaExport
* Created:  2018/7/13 14:29:22
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace MikuLuaProfiler
{
#if UNITY_5_6_OR_NEWER
    #region item
    [Serializable]
    //The TreeElement data class is extended to hold extra data, which you can show and edit in the front-end TreeView.
    public class LuaProfilerTreeViewItem : TreeViewItem
    {
        private static ObjectPool<LuaProfilerTreeViewItem> objectPool = new ObjectPool<LuaProfilerTreeViewItem>(30);
        public static LuaProfilerTreeViewItem Create(LuaProfiler.Sample sample, int depth, LuaProfilerTreeViewItem father)
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
        private long[] _gc = new long[] { 0, 0, 0, 0 };
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
                if (!UnityEditor.EditorApplication.isPlaying)
                {
                    return _showGC;
                }

                if (Time.frameCount == _frameCount)
                {
                    return _showGC;
                }
                else { return 0; }
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
        public LuaProfilerTreeViewItem father { private set; get; }
        private int _frameCount;
        public LuaProfilerTreeViewItem()
        {
        }

        public void ResetBySample(LuaProfiler.Sample sample, int depth, LuaProfilerTreeViewItem father)
        {
            if (sample != null)
            {
                totalMemory = sample.costGC;
                totalTime = (long)(sample.costTime * 1000000);
                displayName = sample.name;
                fullName = sample.fullName;
            }
            else
            {
                totalMemory = 0;
                totalTime = 0;
                displayName = "root";
                fullName = "root";
            }
            totalCallTime = 1;
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

            _frameCount = Time.frameCount;
        }
        public void AddSample(LuaProfiler.Sample sample)
        {
            if (_frameCount == Time.frameCount)
            {
                _gc[3] += sample.costGC;
                frameCalls += sample.oneFrameCall;
                currentTime += sample.costTime;
            }
            else
            {
                _gc[0] = _gc[1];
                _gc[1] = _gc[2];
                _gc[2] = _gc[3];
                _gc[3] = sample.costGC;
                frameCalls = sample.oneFrameCall;
                currentTime = sample.costTime;
            }
            totalMemory += sample.costGC;

            totalTime += (long)(sample.costTime * 1000000);
            totalCallTime += sample.oneFrameCall;
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
            //以下代码只不过为了 gc的显示数值不闪烁
            if (_gc[0] == _gc[1] || _gc[0] == _gc[2] || _gc[0] == _gc[3])
            {
                showGC = _gc[0];
            }
            else if (_gc[1] == _gc[2] || _gc[1] == _gc[3])
            {
                showGC = _gc[1];
            }
            else if (_gc[2] == _gc[3])
            {
                showGC = _gc[2];
            }
            else
            {
                showGC = _gc[3];
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
        private readonly LuaProfilerTreeViewItem root;
        private readonly List<TreeViewItem> treeViewItems = new List<TreeViewItem>();
        #endregion
        public LuaProfilerTreeView(TreeViewState treeViewState, float width)
            : base(treeViewState, CreateDefaultMultiColumnHeaderState(width))
        {
            LuaProfiler.SetSampleEnd(LoadRootSample);
            root = LuaProfilerTreeViewItem.Create(null, -1, null);
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
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = false,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 500,
                    minWidth = 500,
                    maxWidth = 500,
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
                    headerContent = new GUIContent("Now GC"),
                    contextMenuText = "Now GC",
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = false,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 60,
                    minWidth = 60,
                    maxWidth = 60,
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
                    headerContent = new GUIContent("Now Calls"),
                    contextMenuText = "Now Calls",
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

        public void Clear()
        {
            roots.Clear();
            m_nodeDict.Clear();
            treeViewItems.Clear();
        }

        protected override void DoubleClickedItem(int id)
        {
            /*
            base.DoubleClickedItem(id);
            var selectItem = FindItem(id, BuildRoot());
            string fileName = "/" + selectItem.displayName.Split(new char[] { ',' }, 2)[0].Replace(".", "/").Replace("/lua", ".lua").Trim();
            try
            {
                int line = 0;
                int.TryParse(Regex.Match(selectItem.displayName, @"(?<=(line:))\d*(?=( ))").Value, out line);
                //LocalToLuaIDE.OnOpenAsset(fileName, line);
            }     
            catch
            {
            }*/
        }

        public static Dictionary<string, LuaProfilerTreeViewItem> m_nodeDict = new Dictionary<string, LuaProfilerTreeViewItem>();

        private void LoadRootSample(LuaProfiler.Sample sample)
        {
            LuaProfilerTreeViewItem item;
            string f = sample.fullName;
            if (m_nodeDict.TryGetValue(f, out item))
            {
                item.AddSample(sample);
            }
            else
            {
                item = LuaProfilerTreeViewItem.Create(sample, 0, null);
                roots.Add(item);
            }
        }

        private void ReLoadTreeItems()
        {
            treeViewItems.Clear();
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
            treeViewItems.Add(root);
            
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

        protected override TreeViewItem BuildRoot()
        {
            ReLoadTreeItems();

            // Utility method that initializes the TreeViewItem.children and -parent for all items.
            SetupParentsAndChildrenFromDepths(root, treeViewItems);

            // Return root of the tree
            return root;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            GUIStyle gs = new GUIStyle(GUI.skin.label);
            gs.alignment = TextAnchor.MiddleCenter;

            var item = (LuaProfilerTreeViewItem)args.item;
            Rect r = args.rowRect;

            base.RowGUI(args);

            //r.x = r.x + 30;
            //GUI.Label(r, item.displayName);

            r.x = r.x + 500;
            r.width = 80;
            GUI.Label(r, LuaProfiler.GetMemoryString(item.totalMemory), gs);

            r.x = r.x + 80;
            r.width = 120;
            GUI.Label(r, item.currentTime.ToString("f6") + "s", gs);

            r.x = r.x + 120;
            r.width = 120;
            GUI.Label(r, ((float)item.averageTime / 1000000).ToString("f6") + "s", gs);

            r.x = r.x + 120;
            r.width = 120;
            GUI.Label(r, ((float)item.totalTime / 1000000).ToString("f6") + "s", gs);

            r.x = r.x + 120;
            r.width = 60;
            GUI.Label(r, LuaProfiler.GetMemoryString(item.showGC), gs);

            r.x = r.x + 60;
            r.width = 120;
            GUI.Label(r, LuaProfiler.GetMemoryString(item.totalCallTime, ""), gs);

            r.x = r.x + 120;
            r.width = 80;
            GUI.Label(r, item.frameCalls.ToString(), gs);

        }

    }
#endif
}
