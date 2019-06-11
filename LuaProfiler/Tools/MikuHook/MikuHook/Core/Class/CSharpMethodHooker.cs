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
* Filename: CSharpMethodHooker
* Created:  2018/7/13 14:29:22
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/
using System;
using System.Diagnostics;
using System.Reflection;


namespace MikuHook
{
    /// <summary>
    /// C# Hook 类，用来 Hook 某个 C# 方法
    /// </summary>
    public unsafe class CSharpMethodHooker : HookerBase
    {
        public static CSharpMethodHooker HookCSMethod(MethodBase target, MethodBase replace)
        {
            return new CSharpMethodHooker(target, replace, null);
        }

        public static CSharpMethodHooker HookCSMethod(MethodBase target, MethodBase replace, MethodBase proxy)
        {
            return new CSharpMethodHooker(target, replace, proxy);
        }

        /// <summary>
        /// 创建一个 Hooker
        /// </summary>
        /// <param name="target">需要替换的目标方法</param>
        /// <param name="replace">准备好的替换方法</param>
        private CSharpMethodHooker(MethodBase target, MethodBase replace, MethodBase proxy)
        {
            SetupJmpBuff();

            IntPtr targetPtr = NativeAPI.GetFunctionAddr(target);
            IntPtr replacePtr = NativeAPI.GetFunctionAddr(replace);
            IntPtr proxyPtr = NativeAPI.GetFunctionAddr(proxy);
            _headSize = (int)LDasm.SizeofMinNumByte(targetPtr.ToPointer(), s_jmpBuff.Length);
            _proxyBuffSize = _headSize + s_jmpBuff.Length;
            _proxyPtr = (byte*)proxyPtr.ToPointer();

            _targetPtr = targetPtr;
            _replacPtr = replacePtr;

            Install();
        }

        protected override void SetupJmpBuff()
        {
            if (NativeAPI.IsAndroidARM())
            {
                s_addrOffset = 4;
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
    }

}
