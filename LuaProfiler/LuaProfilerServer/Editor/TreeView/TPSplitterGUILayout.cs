/*
* ==============================================================================
* Filename: TPSplitterGUILayout
* Created:  2018/7/13 14:29:22
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/


using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MikuLuaProfiler
{
    internal class TPSplitterGUILayout
    {
        private static MethodInfo _beginVerticalSplitMethod;

        private static MethodInfo _beginHorizontalSplitMethod;

        private static MethodInfo _endVerticalSplitMethod;

        private static MethodInfo _endHorizontalSplitMethod;
        public static Type SplitterGUILayoutType
        {
            get;
            private set;
        }

        static TPSplitterGUILayout()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(ActiveEditorTracker));
            TPSplitterGUILayout.SplitterGUILayoutType = assembly.GetType("UnityEditor.SplitterGUILayout");
        }

        public static void BeginVerticalSplit(SplitterState state, params GUILayoutOption[] options)
        {
            if (TPSplitterGUILayout._beginVerticalSplitMethod == null)
            {
                TPSplitterGUILayout._beginVerticalSplitMethod = TPSplitterGUILayout.SplitterGUILayoutType.GetMethod("BeginVerticalSplit", new Type[]
                {
                    SplitterState.SplitterStateType,
                    typeof(GUILayoutOption[])
                });
            }
            TPSplitterGUILayout._beginVerticalSplitMethod.Invoke(null, new object[]
            {
                state.InternalObject,
                options
            });
        }

        public static void BeginHorizontalSplit(SplitterState state, params GUILayoutOption[] options)
        {
            if (TPSplitterGUILayout._beginHorizontalSplitMethod == null)
            {
                TPSplitterGUILayout._beginHorizontalSplitMethod = TPSplitterGUILayout.SplitterGUILayoutType.GetMethod("BeginHorizontalSplit", new Type[]
                {
                    SplitterState.SplitterStateType,
                    typeof(GUILayoutOption[])
                });
            }
            TPSplitterGUILayout._beginHorizontalSplitMethod.Invoke(null, new object[]
            {
                state.InternalObject,
                options
            });
        }

        public static void EndVerticalSplit()
        {
            if (TPSplitterGUILayout._endVerticalSplitMethod == null)
            {
                TPSplitterGUILayout._endVerticalSplitMethod = TPSplitterGUILayout.SplitterGUILayoutType.GetMethod("EndVerticalSplit");
            }
            TPSplitterGUILayout._endVerticalSplitMethod.Invoke(null, null);
        }

        public static void EndHorizontalSplit()
        {
            if (TPSplitterGUILayout._endHorizontalSplitMethod == null)
            {
                TPSplitterGUILayout._endHorizontalSplitMethod = TPSplitterGUILayout.SplitterGUILayoutType.GetMethod("EndHorizontalSplit");
            }
            TPSplitterGUILayout._endHorizontalSplitMethod.Invoke(null, null);
        }
    }
}