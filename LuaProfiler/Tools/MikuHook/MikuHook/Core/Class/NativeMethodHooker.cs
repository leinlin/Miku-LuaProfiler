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
* Filename: NativeMethodHooker
* Created:  2018/7/13 14:29:22
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MikuHook
{
    /// <summary>
    /// Native Hook 类，用来 Hook 某个 Native 方法
    /// </summary>
    public unsafe class NativeMethodHooker : HookerBase, IDisposable
    {
        private bool _disposed = false;
        public Delegate GetProxy<T>()
        {
            Debug.Assert(typeof(Delegate).IsAssignableFrom(typeof(T)));
            return Marshal.GetDelegateForFunctionPointer((IntPtr)_proxyPtr, typeof(T));
        }

        public static NativeMethodHooker HookNative(string libraryName, string symbol, Delegate replace)
        {
            NativeMethodHooker result = new NativeMethodHooker(libraryName, symbol, replace);
            return result;
        }

        /// <summary>
        /// 创建一个 native Hooker
        /// </summary>
        /// <param name="libraryName">C/C++ 本地库名字</param>
        /// <param name="symbol">方法符号</param>
        /// <param name="replace">替换委托</param>
        public NativeMethodHooker(string libraryName, string symbol, Delegate replace)
        {
            SetupJmpBuff();

            IntPtr targetPtr = NativeAPI.miku_hooker_get_address(libraryName, symbol);
            _headSize = (int)LDasm.SizeofMinNumByte(targetPtr.ToPointer(), s_jmpBuff.Length);
            _proxyBuffSize = _headSize + s_jmpBuff.Length;

            _targetPtr = targetPtr;
            _replacPtr = Marshal.GetFunctionPointerForDelegate(replace);
            _proxyPtr = (byte*)NativeAPI.miku_hooker_jit_alloc(IntPtr.Zero, _proxyBuffSize);

            Install();
        }

        public override void Uninstall()
        {
            Dispose();
        }

        public void Dispose()
        {
            try
            {
                base.Uninstall();
            }
            finally
            {
                NativeAPI.miku_hooker_free(_proxyPtr, _proxyBuffSize);
                _disposed = true;
            }
        }

        protected override void SetupJmpBuff()
        {
            if (NativeAPI.IsAndroidARM())
            {
                s_addrOffset = 0;
                if (IntPtr.Size == 4)
                {
                    s_jmpBuff = s_jmpBuffArm32;
                }
                else
                {
                    s_jmpBuff = s_jmpBuffArm64;
                }
            }
            else
            {
                s_jmpBuff = s_jmpBuffIntel;
                s_addrOffset = 1;
            }
        }

        ~NativeMethodHooker()
        {
            if (!_disposed)
            {
                Dispose();
            }
        }
    }

}
