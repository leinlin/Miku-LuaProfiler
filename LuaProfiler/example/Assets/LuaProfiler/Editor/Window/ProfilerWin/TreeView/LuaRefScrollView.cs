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
* Filename: LuaRefScrollView
* Created:  2018/7/13 14:29:22
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/

#if UNITY_5_6_OR_NEWER && UNITY_EDITOR_WIN
namespace MikuLuaProfiler
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEditor;
    using UnityEngine;
    using RefDict = System.Collections.Generic.Dictionary<string, System.Collections.Generic.HashSet<string>>;
    public class LuaRefScrollView
    {
        private List<RefDict> m_refDict = new List<RefDict> { null, null, null };
        private List<LuaRefInfo> m_luaRefHistory = new List<LuaRefInfo>(4096);
        private Queue<LuaRefInfo> m_refQueue = new Queue<LuaRefInfo>(256);
        private Vector2 scrollPosition;
        private Vector2 scrollPositionFun;
        private Vector2 scrollPositionTb;
        private int dictCount = 0;
        private string showStr = "";
        public void DoRefScroll()
        {
            if (Event.current.type == EventType.MouseUp || Event.current.type == EventType.KeyDown)
            {
                return;
            }
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, EditorStyles.helpBox, GUILayout.Width(400));

            EditorGUILayout.LabelField("lua refs");
            try
            {
                for (byte i = 1, imax = 2; i <= imax; i++)
                {
                    RefDict dictItem = m_refDict[i];
                    if (dictItem == null) continue;
                    if (i == 1)
                    {
                        EditorGUILayout.LabelField("function ref: " + dictItem.Count);
                        scrollPositionFun = GUILayout.BeginScrollView(scrollPositionFun, EditorStyles.helpBox);
                    }
                    else
                    {
                        EditorGUILayout.LabelField("table ref: " + dictItem.Count);
                        scrollPositionTb = GUILayout.BeginScrollView(scrollPositionTb, EditorStyles.helpBox);
                    }
                    GUILayout.BeginVertical();
                    if (dictCount != dictItem.Count)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (var item in dictItem)
                        {
                            sb.AppendLine(string.Format("{0} count:{1}", item.Key, item.Value.Count));
                            if (sb.Length > 10000) break;
                        }
                        showStr = sb.ToString();
                    }

                    GUILayout.Label(showStr);
                    dictCount = dictItem.Count;
                    GUILayout.EndVertical();
                    GUILayout.EndScrollView();
                }
            }
            catch { }

            GUILayout.EndScrollView();
        }

#region ref
        public void LoadHistory(int startFrame, int endFrame)
        {
            ClearRefInfo(false);
            for (int i = 0,imax= m_luaRefHistory.Count; i < imax; i++)
            {
                var r = m_luaRefHistory[i];
                if (r.frameCount < startFrame)
                {
                    continue;
                }
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
        public void LogToFile()
        {
            if (m_refDict == null) return;
            for (byte i = 1, imax = 2; i <= imax; i++)
            {
                RefDict dictItem = m_refDict[i];
                StringBuilder sb = new StringBuilder();
                List<KeyValuePair<string, int>> list = new List<KeyValuePair<string, int>>();
                foreach (var item in dictItem)
                {
                    list.Add(new KeyValuePair<string, int>(item.Key, item.Value.Count));
                }

                list.Sort((a, b) =>
                {
                    return b.Value - a.Value;
                });

                foreach (var item in list)
                {
                    sb.AppendLine(string.Format("{0} count:{1}", item.Key, item.Value));
                }
                showStr = sb.ToString();

                LuaProfilerWindow.ClearConsole();
                if (!Directory.Exists(Application.dataPath.Replace("Assets", "Ref")))
                {
                    Directory.CreateDirectory(Application.dataPath.Replace("Assets", "Ref"));
                }
                if (i == 1)
                {
                    string path = Application.dataPath.Replace("Assets", "Ref/" + "function" + ".txt");
                    File.WriteAllText(path, sb.ToString());
                }
                else
                {
                    string path = Application.dataPath.Replace("Assets", "Ref/" + "table" + ".txt");
                    File.WriteAllText(path, sb.ToString());
                }
            }
            System.Diagnostics.Process.Start("explorer.exe", Application.dataPath.Replace("Assets", "Ref").Replace("/", "\\"));
        }
        public void LogRefHistory(int startFrame, int endFrame)
        {
            for (int i = 0, imax = m_luaRefHistory.Count; i < imax; i++)
            {
                var r = m_luaRefHistory[i];
                if (r.frameCount <= endFrame && r.frameCount >= startFrame)
                {
                    if (r.cmd == 1)
                    {
                        Debug.Log("<color=#00ff00>add " + r.name + "</color>");
                    }
                    else if (r.cmd == 0)
                    {
                        Debug.Log("<color=#ff0000>rm " + r.name + "</color>");
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
            m_refDict = new List<RefDict> { null, null, null };
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
            RefDict refDict = m_refDict[type];
            if (refDict == null)
            {
                refDict = new RefDict(2048);
                m_refDict[type] = refDict;
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
            RefDict refDict = m_refDict[type];

            if (refDict == null)
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
#endif