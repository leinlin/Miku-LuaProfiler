namespace MikuLuaProfiler
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using UnityEditor;
    using UnityEngine;
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
            if (m_isRecord)
            {
                EditorGUILayout.LabelField(m_dateTime);
            }
            if (luaDiff == null) return;

            GUILayout.BeginHorizontal();

            #region value
            GUILayout.BeginVertical();
            EditorGUILayout.LabelField("add values count:" + luaDiff.addRef.Count);

            scrollPositionAdd = GUILayout.BeginScrollView(scrollPositionAdd, EditorStyles.helpBox, GUILayout.Height(100));
            foreach (var item in luaDiff.addRef)
            {
                DrawItemInfo(item.Key, item.Value.ToString(), luaDiff.addDetail);
            }
            GUILayout.EndScrollView();

            EditorGUILayout.LabelField("rm values count:" + luaDiff.rmRef.Count);

            scrollPositionRm = GUILayout.BeginScrollView(scrollPositionRm, EditorStyles.helpBox, GUILayout.Height(100));
            foreach (var item in luaDiff.rmRef)
            {
                DrawItemInfo(item.Key, item.Value.ToString(), luaDiff.rmDetail);
            }
            GUILayout.EndScrollView();

            EditorGUILayout.LabelField("Destory null values count:" + luaDiff.nullRef.Count);

            scrollPositionNull = GUILayout.BeginScrollView(scrollPositionNull, EditorStyles.helpBox, GUILayout.Height(100));
            foreach (var item in luaDiff.nullRef)
            {
                GUILayout.Label(item);
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

        public void DrawItemInfo(string key, string value, Dictionary<string, List<string>> dict)
        {
            GUILayout.BeginHorizontal();
            List<string> list = null;
            dict.TryGetValue(key, out list);
            int count = list == null ? 0 : list.Count;

            GUILayout.Label(string.Format("Key:{0} Value:{1}", key, value));
            GUILayout.Label(string.Format("Ref Count:{0}", count));
            if (GUILayout.Button("detail", GUILayout.Width(100)))
            {
                m_detailList = list == null ? m_detailList : list;
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
