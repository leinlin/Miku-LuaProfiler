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
* Filename: NativeAPI
* Created:  2018/7/13 14:29:22
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MikuHook
{
    public unsafe static class NativeAPI
    {
        #region platform
        public static IntPtr ArrayToIntptr(byte[] source)
        {
            if (source == null)
                return IntPtr.Zero;
            unsafe
            {
                fixed (byte* point = source)
                {
                    IntPtr ptr = new IntPtr(point);
                    return ptr;
                }
            }
        }

        // -1 未初始化,0 不是 android arm 1 是android arm
        private static int _isAndroidARM = -1;
        public static bool IsAndroidARM()
        {
            if (_isAndroidARM == -1)
            {
                _isAndroidARM = 0;
                if (UnityEngine.SystemInfo.operatingSystem.Contains("Android")
                && UnityEngine.SystemInfo.processorType.Contains("ARM"))
                {
                    _isAndroidARM = 1;
                }
            }
            return _isAndroidARM == 1;
        }

        private static int _isIOS = -1;
        public static bool IsiOS()
        {
            if (_isIOS == -1)
            {
                _isIOS = 0;
                if (UnityEngine.SystemInfo.operatingSystem.ToLower().Contains("ios"))
                {
                    _isIOS = 1;
                }
            }
            return _isIOS == 1;
        }

        private static int _isIL2CPP = -1;
        public static bool IsIL2CPP()
        {
            if (_isIL2CPP == -1)
            {
                _isIL2CPP = 0;
                try
                {
                    byte[] ilBody = typeof(NativeAPI).GetMethod("IsIL2CPP").GetMethodBody().GetILAsByteArray();
                    if (ilBody == null || ilBody.Length == 0)
                        _isIL2CPP = 1;
                }
                catch
                {
                    _isIL2CPP = 1;
                }
            }

            return _isIL2CPP == 1;
        }

        /// <summary>
        /// 获取方法指令地址
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static IntPtr GetFunctionAddr(MethodBase method)
        {
            Debug.Assert(!NativeAPI.IsIL2CPP(), "暂时不支持IL2CPP");
            if (method == null)
            {
                return IntPtr.Zero;
            }
            else
            {
                return method.MethodHandle.GetFunctionPointer();
            }
        }
        #endregion
    }
}

