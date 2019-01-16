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
        private Vector2 scrollPositionAdd;
        private Vector2 scrollPositionRm;
        private Vector2 scrollPositionNull;
        public void DoRefScroll()
        {
            if (Event.current.type == EventType.MouseUp || Event.current.type == EventType.KeyDown)
            {
                return;
            }
            if (m_isRecord)
            {
                EditorGUILayout.LabelField(m_dateTime);
            }
            if (luaDiff == null) return;

            EditorGUILayout.LabelField("add values count:" + luaDiff.addRef.Count);

            scrollPositionAdd = GUILayout.BeginScrollView(scrollPositionAdd, EditorStyles.helpBox, GUILayout.Height(100));
            foreach (var item in luaDiff.addRef)
            {
                GUILayout.Label(item.Key + " " + item.Value.ToString());
            }
            GUILayout.EndScrollView();

            EditorGUILayout.LabelField("rm values count:" + luaDiff.rmRef.Count);

            scrollPositionRm = GUILayout.BeginScrollView(scrollPositionRm, EditorStyles.helpBox, GUILayout.Height(100));
            foreach (var item in luaDiff.rmRef)
            {
                GUILayout.Label(item.Key + " " + item.Value.ToString());
            }
            GUILayout.EndScrollView();

            EditorGUILayout.LabelField("Destory null values count:" + luaDiff.nullRef.Count);

            scrollPositionNull = GUILayout.BeginScrollView(scrollPositionNull, EditorStyles.helpBox, GUILayout.Height(100));
            foreach (var item in luaDiff.nullRef)
            {
                GUILayout.Label(item);
            }
            GUILayout.EndScrollView();
        }

        public void Clear()
        {
            m_isRecord = false;
            luaDiff = null;
            m_dateTime = "";
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
