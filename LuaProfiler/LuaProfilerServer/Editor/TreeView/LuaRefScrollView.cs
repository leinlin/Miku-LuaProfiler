

namespace MikuLuaProfiler
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using UnityEditor;
    using UnityEngine;
    using RefDict = System.Collections.Generic.Dictionary<string, System.Collections.Generic.HashSet<string>>;
    public class LuaRefScrollView
    {
        private Dictionary<byte, RefDict> m_refDict = new Dictionary<byte, RefDict>(4);
        private List<LuaRefInfo> m_luaRefHistory = new List<LuaRefInfo>(4096);
        private Queue<LuaRefInfo> m_refQueue = new Queue<LuaRefInfo>(256);
        private Vector2 scrollPosition;
        private Vector2 scrollPositionFun;
        private Vector2 scrollPositionTb;
        public void DoRefScroll()
        {
            if (Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseUp || Event.current.type == EventType.KeyDown)
            {
                return;
            }

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, EditorStyles.helpBox, GUILayout.Width(350));
            EditorGUILayout.LabelField("lua refs");
            foreach (var dictItem in m_refDict)
            {
                if (dictItem.Key == 1)
                {
                    EditorGUILayout.LabelField("function ref: " + dictItem.Value.Count);
                    scrollPositionFun = GUILayout.BeginScrollView(scrollPositionFun);
                }
                else
                {
                    EditorGUILayout.LabelField("table ref: " + dictItem.Value.Count);
                    scrollPositionTb = GUILayout.BeginScrollView(scrollPositionTb);
                }
                GUILayout.BeginVertical();
                foreach (var item in dictItem.Value)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(item.Key);
                    int count = item.Value.Count;
                    GUILayout.Label("count:" + count);
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
                GUILayout.EndScrollView();
            }


            GUILayout.EndScrollView();
        }

        #region ref
        public void LoadHistory(int startFrame, int endFrame)
        {
            ClearRefInfo(false);
            for (int i = 0,imax= m_luaRefHistory.Count; i < imax; i++)
            {
                var r = m_luaRefHistory[i];
                if (r.frameCount <= endFrame)
                {
                    if (r.cmd == 1)
                    {
                        AddRef(r);
                    }
                    else if (r.cmd == 0)
                    {
                        RemoveRef(r);
                    }
                }
                if (r.frameCount > endFrame)
                {
                    break;
                }
            }
        }

        public void DelRefInfo(LuaRefInfo info)
        {
            var instance = LuaDeepProfilerSetting.Instance;
            lock (this)
            {
                m_refQueue.Enqueue(info);
            }
            if (instance.isRecord && !instance.isStartRecord)
            {
                return;
            }
            m_luaRefHistory.Add(info.Clone());
        }
        public void DequeueLuaInfo()
        {
            lock (this)
            {
                while (m_refQueue.Count > 0)
                {
                    LuaRefInfo r = null;
                    lock (this)
                    {
                        r = m_refQueue.Dequeue();
                        if (r.cmd == 1)
                        {
                            AddRef(r);
                        }
                        else if (r.cmd == 0)
                        {
                            RemoveRef(r);
                        }
                        r.Restore();
                    }
                }
            }
        }
        public void ClearRefInfo(bool includeHistory)
        {
            m_refDict.Clear();
            m_refQueue.Clear();
            if (includeHistory)
            {
                m_luaRefHistory.Clear();
            }
        }
        private void AddRef(LuaRefInfo info)
        {
            string refName = info.name;
            string addr = info.addr;
            byte type = info.type;
            RefDict refDict;

            if (!m_refDict.TryGetValue(type, out refDict))
            {
                refDict = new RefDict(2048);
                m_refDict.Add(type, refDict);
            }

            HashSet<string> addrList;
            if (!refDict.TryGetValue(refName, out addrList))
            {
                addrList = new HashSet<string>();
                refDict.Add(refName, addrList);
            }
            if (!addrList.Contains(addr))
            {
                addrList.Add(addr);
            }

        }
        private void RemoveRef(LuaRefInfo info)
        {
            string refName = info.name;
            string refAddr = info.addr;
            byte type = info.type;
            RefDict refDict;

            if (!m_refDict.TryGetValue(type, out refDict))
            {
                return;
            }

            HashSet<string> addrList;
            if (!refDict.TryGetValue(refName, out addrList))
            {
                return;
            }
            if (!addrList.Contains(refAddr))
            {
                return;
            }
            addrList.Remove(refAddr);
            if (addrList.Count == 0)
            {
                refDict.Remove(refName);
            }
        }

        #endregion
    }
}
