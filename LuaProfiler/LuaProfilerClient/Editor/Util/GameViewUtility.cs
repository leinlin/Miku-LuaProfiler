#if UNITY_EDITOR
namespace MikuLuaProfiler
{

    using System;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    public static class GameViewUtility
    {
        public static void ChangeGameViewSize(int width, int height)
        {
            m_width = width;
            m_height = height;
            callback = new EditorApplication.CallbackFunction(Update);
            EditorApplication.update += callback;
        }

        #region member
        private enum GameViewSizeType
        {
            AspectRatio,
            FixedResolution
        }
        private static readonly string s_MiKuProfiler = "LuaProfiler";
        private static object s_GameViewSizesInstance;
        private static MethodInfo s_GetGroup;
        private static EditorApplication.CallbackFunction callback;
        private static int m_width;
        private static int m_height;
        #endregion

        #region private
        private static void Update()
        {
            try
            {
                var sizesType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");
                var singleType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
                var instanceProp = singleType.GetProperty("instance");
                s_GetGroup = sizesType.GetMethod("GetGroup");
                s_GameViewSizesInstance = instanceProp.GetValue(null, null);

                if (!SizeExists(GetCurrentGroupType(), s_MiKuProfiler))
                {
                    AddCustomSize(GameViewSizeType.FixedResolution, GetCurrentGroupType(), m_width, m_height, s_MiKuProfiler);
                }
                int type = FindSize(GetCurrentGroupType(), s_MiKuProfiler);
                SetSize(type);
            }
            catch(Exception e)
            {
                Debug.LogError(e);
            }
            EditorApplication.update -= callback;
        }

        private static void AddCustomSize(GameViewSizeType viewSizeType, GameViewSizeGroupType sizeGroupType, int width, int height, string text)
        {
            var group = GetGroup(sizeGroupType);
            var addCustomSize = s_GetGroup.ReturnType.GetMethod("AddCustomSize");
            var gvsType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSize");

            Type sizeType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizeType");

            var ctor = gvsType.GetConstructor(new Type[] { sizeType, typeof(int), typeof(int), typeof(string) });
            var newSize = ctor.Invoke(new object[] { (int)viewSizeType, width, height, text });
            addCustomSize.Invoke(group, new object[] { newSize });
        }

        private static void SetSize(int index)
        {
            var gameViewType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
            var mainGameView = gameViewType.GetMethod("GetMainGameView", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null);
            if (mainGameView == null || mainGameView.ToString().ToLowerInvariant().Equals("null"))
            {
                return;
            }
            gameViewType.GetMethod("Focus", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Invoke(mainGameView, null);
            var selectedSizeIndexProp = gameViewType.GetProperty("selectedSizeIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            selectedSizeIndexProp.SetValue(mainGameView, index, null);
            var sizeSelectionCallback = gameViewType.GetMethod("SizeSelectionCallback", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            sizeSelectionCallback.Invoke(mainGameView, new object[] { index, null });
        }

        public static bool SizeExists(GameViewSizeGroupType sizeGroupType, string text)
        {
            return FindSize(sizeGroupType, text) != -1;
        }

        private static int FindSize(GameViewSizeGroupType sizeGroupType, string text)
        {
            var group = GetGroup(sizeGroupType);
            var getDisplayTexts = group.GetType().GetMethod("GetDisplayTexts");
            var displayTexts = getDisplayTexts.Invoke(group, null) as string[];
            for (int i = 0; i < displayTexts.Length; i++)
            {
                string display = displayTexts[i];
                int pren = display.IndexOf('(');
                if (pren != -1)
                    display = display.Substring(0, pren - 1);
                if (display == text)
                    return i;
            }
            return -1;
        }

        static object GetGroup(GameViewSizeGroupType type)
        {
            return s_GetGroup.Invoke(s_GameViewSizesInstance, new object[] { (int)type });
        }

        private static GameViewSizeGroupType GetCurrentGroupType()
        {
            var getCurrentGroupTypeProp = s_GameViewSizesInstance.GetType().GetProperty("currentGroupType");
            return (GameViewSizeGroupType)(int)getCurrentGroupTypeProp.GetValue(s_GameViewSizesInstance, null);
        }
        #endregion

    }
}
#endif