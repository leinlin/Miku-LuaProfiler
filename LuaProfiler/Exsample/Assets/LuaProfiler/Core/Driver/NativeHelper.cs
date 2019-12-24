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
* Filename: NativeHelper
* Created:  2018/7/13 14:29:22
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/
#if UNITY_EDITOR_WIN || USE_LUA_PROFILER
namespace MikuLuaProfiler
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using UnityEngine;

#if UNITY_5_5_OR_NEWER
    using UnityEngine.Profiling;
#endif

    public static class NativeHelper
    {
        public static uint GetPass()
        {
#if UNITY_5_5_OR_NEWER
            return (uint)Profiler.GetTotalAllocatedMemoryLong();
#else
            return (uint)Profiler.GetTotalAllocatedMemory();
#endif
        }

        public static float GetBatteryLevel()
        {
            float result = 100;
            return result;
        }
    }
}
#endif