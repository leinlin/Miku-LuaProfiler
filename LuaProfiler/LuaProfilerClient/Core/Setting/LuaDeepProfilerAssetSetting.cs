using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
namespace MikuLuaProfiler
{
    public class LuaDeepProfilerAssetSetting : ScriptableObject
    {

        #region memeber
        public bool isDeepMonoProfiler = false;
        public bool isDeepLuaProfiler = false;

        private static LuaDeepProfilerAssetSetting instance;
        public static LuaDeepProfilerAssetSetting Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = AssetDatabase.LoadAssetAtPath<LuaDeepProfilerAssetSetting>("Assets/LuaDeepProfilerAssetSetting.asset");
                    if (instance == null)
                    {
                        UnityEngine.Debug.Log("Lua Profiler: cannot find integration settings, creating default settings");
                        instance = CreateInstance<LuaDeepProfilerAssetSetting>();
                        instance.name = "Lua Profiler Integration Settings";
#if UNITY_EDITOR
                        AssetDatabase.CreateAsset(instance, "Assets/LuaDeepProfilerAssetSetting.asset");
#endif
                    }
                }
                return instance;
            }
        }
        #endregion

    }
}
#endif
