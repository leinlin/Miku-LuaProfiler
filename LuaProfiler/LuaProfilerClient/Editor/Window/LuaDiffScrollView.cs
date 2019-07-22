#if (UNITY_5 || UNITY_2017_1_OR_NEWER)
namespace MikuLuaProfiler
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEditor;
    using UnityEngine;
    using MikuLuaProfiler;
    public class LuaDiffScrollView
    {
        private bool m_isRecord = false;
        private string m_dateTime = "";
        LuaDiffInfo luaDiff;
        List<string> m_detailList;
        private Vector2 scrollPositionAdd;
        private Vector2 scrollPositionRm;
        private Vector2 scrollPositionNull;
        private Vector2 scrollPositionDetail;
        public void DoRefScroll()
        {
            if (!Directory.Exists(Application.dataPath.Replace("Assets", "Diff")))
            {
                Directory.CreateDirectory(Application.dataPath.Replace("Assets", "Diff"));
            }
            if (m_isRecord)
            {
                EditorGUILayout.LabelField(m_dateTime);
            }
            if (luaDiff == null) return;

            GUILayout.BeginHorizontal();

#region value
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("add values count:" + luaDiff.addRef.Count, GUILayout.Width(150));
            if (GUILayout.Button("ShowLog", GUILayout.Width(100)))
            {
                ShowLog("add", luaDiff.addRef, luaDiff.addDetail);
            }
            GUILayout.EndHorizontal();

            scrollPositionAdd = GUILayout.BeginScrollView(scrollPositionAdd, EditorStyles.helpBox, GUILayout.Height(100));
            int drawCount = 0;
            foreach (var item in luaDiff.addRef)
            {
                DrawItemInfo(item.Key, item.Value.ToString(), luaDiff.addDetail);
                drawCount++;
                if (drawCount > 200)
                {
                    GUILayout.Label("more please show log", GUILayout.Width(400));
                    break;
                }
            }
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("rm values count:" + luaDiff.rmRef.Count, GUILayout.Width(150));
            if (GUILayout.Button("ShowLog", GUILayout.Width(100)))
            {
                ShowLog("rm", luaDiff.rmRef, luaDiff.rmDetail);
            }
            GUILayout.EndHorizontal();

            scrollPositionRm = GUILayout.BeginScrollView(scrollPositionRm, EditorStyles.helpBox, GUILayout.Height(100));
            drawCount = 0;
            foreach (var item in luaDiff.rmRef)
            {
                DrawItemInfo(item.Key, item.Value.ToString(), luaDiff.rmDetail);
                drawCount++;
                if (drawCount > 200)
                {
                    GUILayout.Label("more please show log", GUILayout.Width(400));
                    break;
                }
            }
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Destory null values count:" + luaDiff.nullRef.Count, GUILayout.Width(200));
            if (GUILayout.Button("ShowLog", GUILayout.Width(100)))
            {
                ShowLog("null", luaDiff.nullRef, luaDiff.nullDetail);
            }
            GUILayout.EndHorizontal();

            scrollPositionNull = GUILayout.BeginScrollView(scrollPositionNull, EditorStyles.helpBox, GUILayout.Height(100));
            drawCount = 0;
            foreach (var item in luaDiff.nullRef)
            {
                DrawItemInfo(item.Key, item.Value.ToString(), luaDiff.nullDetail);
                drawCount++;
                if (drawCount > 200)
                {
                    GUILayout.Label("more please show log", GUILayout.Width(400));
                    break;
                }
            }

            GUILayout.EndScrollView();
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

        public void ShowLog(string fileName, Dictionary<string, LuaTypes> refDict, Dictionary<string, List<string>> detailDict)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in refDict)
            {
                List<string> list = null;
                string key = item.Key;
                string value = item.Value.ToString();
                if (detailDict.TryGetValue(key, out list))
                {
                    sb.AppendLine("type:" + value);
                    for (int i = 0,imax=list.Count;i<imax;i++)
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
                Debug.Log(sb.ToString());
            }
            GUILayout.EndHorizontal();
        }

        public void Clear()
        {
            m_isRecord = false;
            luaDiff = null;
            m_dateTime = "";
            m_detailList = null;
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