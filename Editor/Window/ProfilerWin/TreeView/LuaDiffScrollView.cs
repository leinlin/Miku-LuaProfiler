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
* Filename: LuaDiffScrollView
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
    public class LuaDiffScrollView
    {
        private bool m_isStaticRecord = false;
        private bool m_isRecord = false;
        private string m_staticDateTime = "";
        private string m_dateTime = "";
        LuaDiffInfo luaDiff;
        List<string> m_detailList;
        private Vector2 scrollPositionAdd;
        private Vector2 scrollPositionRm;
        private Vector2 scrollPositionNull;
        private Vector2 scrollPositionDetail;
        public void DoRefScroll()
        {
            if (m_isStaticRecord)
            {
                EditorGUILayout.LabelField(m_staticDateTime);
            }
            if (m_isRecord)
            {
                EditorGUILayout.LabelField(m_dateTime);
            }
            if (luaDiff == null) return;

            GUILayout.BeginHorizontal();

            #region value
            GUILayout.BeginVertical();

            scrollPositionAdd = GUILayout.BeginScrollView(scrollPositionAdd, EditorStyles.helpBox, GUILayout.Height(100));
            int drawCount = 0;
            foreach (var item in luaDiff.addRef)
            {
                string typeStr = item.Key.ToString();
                bool needBreak = false;
                foreach (var v in item.Value)
                {
                    DrawItemInfo(v, typeStr, luaDiff.addDetail);
                    drawCount++;
                    if (drawCount > 200)
                    {
                        GUILayout.Label("more please show log", GUILayout.Width(400));
                        needBreak = true;
                        break;
                    }
                }
                if (needBreak) break;
            }
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("add values count:" + drawCount, GUILayout.Width(150));
            if (GUILayout.Button("ShowLog", GUILayout.Width(100)))
            {
                ShowLog("add", luaDiff.addRef, luaDiff.addDetail);
            }
            GUILayout.EndHorizontal();

            scrollPositionRm = GUILayout.BeginScrollView(scrollPositionRm, EditorStyles.helpBox, GUILayout.Height(100));
            drawCount = 0;
            foreach (var item in luaDiff.rmRef)
            {
                string typeStr = item.Key.ToString();
                bool needBreak = false;
                foreach (var v in item.Value)
                {
                    DrawItemInfo(v, typeStr, luaDiff.rmDetail);
                    drawCount++;
                    if (drawCount > 200)
                    {
                        GUILayout.Label("more please show log", GUILayout.Width(400));
                        needBreak = true;
                        break;
                    }
                }
                if (needBreak) break;
            }
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("remain values count:" + drawCount, GUILayout.Width(150));
            if (GUILayout.Button("ShowLog", GUILayout.Width(100)))
            {
                ShowLog("remain", luaDiff.rmRef, luaDiff.rmDetail);
            }
            GUILayout.EndHorizontal();

            scrollPositionNull = GUILayout.BeginScrollView(scrollPositionNull, EditorStyles.helpBox, GUILayout.Height(100));
            drawCount = 0;
            foreach (var item in luaDiff.nullRef)
            {
                string typeStr = item.Key.ToString();
                bool needBreak = false;
                foreach (var v in item.Value)
                {
                    DrawItemInfo(v, typeStr, luaDiff.nullDetail);
                    drawCount++;
                    if (drawCount > 200)
                    {
                        GUILayout.Label("more please show log", GUILayout.Width(400));
                        needBreak = true;
                        break;
                    }
                }
                if (needBreak) break;
            }

            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Destory null values count:" + drawCount, GUILayout.Width(200));
            if (GUILayout.Button("ShowLog", GUILayout.Width(100)))
            {
                ShowLog("null", luaDiff.nullRef, luaDiff.nullDetail);
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            #endregion

            #region detail
            scrollPositionDetail = GUILayout.BeginScrollView(scrollPositionDetail, EditorStyles.helpBox, GUILayout.Width(250));
            if (m_detailList != null)
            {
                foreach (var item in m_detailList)
                {
                    GUILayout.Label(item);
                }
            }
            GUILayout.EndScrollView();
            #endregion

            GUILayout.EndHorizontal();

        }

        public void ShowLog(string fileName, Dictionary<LuaTypes, HashSet<string>> refDict, Dictionary<string, List<string>> detailDict)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in refDict)
            {
                string typeStr = item.Key.ToString();
                List<string> list = null;
                foreach (var v in item.Value)
                {
                    string key = v;
                    if (detailDict.TryGetValue(key, out list))
                    {
                        sb.AppendLine("type:" + typeStr);
                        for (int i = 0, imax = list.Count; i < imax; i++)
                        {
                            if (i == imax - 1)
                            {
                                sb.AppendLine(" └─ref" + (i + 1) + " url:" + list[i]);
                            }
                            else
                            {
                                sb.AppendLine(" ├─ref" + (i + 1) + " url:" + list[i]);
                            }

                        }
                    }
                }
            }
            LuaProfilerWindow.ClearConsole();
            if (!Directory.Exists(Application.dataPath.Replace("Assets", "Diff")))
            {
                Directory.CreateDirectory(Application.dataPath.Replace("Assets", "Diff"));
            }
            string path = Application.dataPath.Replace("Assets", "Diff/" + fileName + ".txt");
            File.WriteAllText(path, sb.ToString());
            System.Diagnostics.Process.Start("explorer.exe", Application.dataPath.Replace("Assets", "Diff").Replace("/", "\\"));
        }

        public void DrawItemInfo(string key, string value, Dictionary<string, List<string>> dict)
        {
            GUILayout.BeginHorizontal();
            List<string> list = null;
            dict.TryGetValue(key, out list);
            int count = list == null ? 0 : list.Count;
            GUILayout.Label(string.Format("Key:{0}", key), GUILayout.Width(400));
            GUILayout.Label(string.Format("Value:{0}", value));
            GUILayout.Label(string.Format("Ref Count:{0}", count));
            if (GUILayout.Button("detail", GUILayout.Width(100)))
            {
                m_detailList = list == null ? m_detailList : list;
                StringBuilder sb = new StringBuilder();
                foreach (var item in m_detailList)
                {
                    sb.AppendLine(item);
                }
                LuaProfilerWindow.ClearConsole();
                Debug.Log(sb.ToString());
            }
            GUILayout.EndHorizontal();
        }

        public void Clear()
        {
            m_isRecord = false;
            m_isStaticRecord = false;
            luaDiff = null;
            m_dateTime = "";
            m_staticDateTime = "";
            m_detailList = null;
            LuaHook.ClearStaticRecord();
            LuaHook.ClearRecord();
        }

        public void MarkIsStaticRecord()
        {
            m_staticDateTime = "static record:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            m_isStaticRecord = true;
        }

        public void MarkIsRecord()
        {
            m_dateTime = "record" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            m_isRecord = true;
        }

        public void DelDiffInfo(LuaDiffInfo info)
        {
            luaDiff = info;
        }

    }

}
#endif