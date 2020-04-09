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
* Filename: LuaDeepProfilerAssetSetting.cs
* Created:  2018/7/13 14:29:22
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/

#if UNITY_EDITOR_WIN || USE_LUA_PROFILER
using UnityEngine;
using UnityEditor;

namespace MikuLuaProfiler
{
    public class LuaDeepProfilerAssetSetting : ScriptableObject
    {

        #region memeber
        public bool isDeepMonoProfiler = false;
        public bool isDeepLuaProfiler = false;
        public bool isLocal = true;

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
