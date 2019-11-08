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
* Filename: HookerBase
* Created:  2018/7/13 14:29:22
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/
using System;
using System.Diagnostics;

namespace MikuHook
{

    public unsafe abstract class HookerBase
    {
        public bool isHooked { get; private set; }

        protected IntPtr _targetPtr;          // 目标方法被 jit 后的地址指针
        protected IntPtr _replacPtr;
        protected byte* _proxyPtr;
        protected byte[] _backupArray;
        protected int _headSize;
        protected int _proxyBuffSize;

        protected static int s_addrOffset;
        protected static byte[] s_jmpBuff;
        protected static readonly byte[] s_jmpBuff32 = new byte[] // 6 bytes
        {
            0x68, 0x00, 0x00, 0x00, 0x00,                       // push $val
            0xC3                                                // ret
        };
        protected static byte[] s_jmpBuff64 = new byte[] // 14 bytes
        {
            0xFF, 0x25, 0x00, 0x00, 0x00, 0x00,                 // jmp [rip]
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,     // $val
        };

        protected static readonly byte[] s_jmpBuffArm32 = new byte[] // 8 bytes
        {
            0x04, 0xF0, 0x1F, 0xE5,                             // LDR PC, [PC, #-4]
            0x00, 0x00, 0x00, 0x00,                             // $val
        };
        protected static readonly byte[] s_jmpBuffArm64 = new byte[]
        {
            0x04, 0xF0, 0x1F, 0xE5,                             // LDR PC, [PC, #-4]
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,     // $val
        };

        protected void Install()
        {
            if (NativeAPI.IsiOS()) // iOS 不支持修改 code 所在区域 page
                return;

            if (isHooked)
                return;

            BackupHeader();
            PatchTargetMethod();
            PatchProxyMethod();
            isHooked = true;
        }

        #region virtual

        public virtual void Uninstall()
        {
            if (!isHooked)
                return;

            byte* pTarget = (byte*)_targetPtr.ToPointer();
            for (int i = 0; i < _backupArray.Length; i++)
            {
                *pTarget++ = _backupArray[i];
            }
            isHooked = false;
        }

        protected abstract void SetupJmpBuff();
        /// <summary>
        /// 备份原始方法头
        /// </summary>
        protected virtual void BackupHeader()
        {
            byte* pTarget = (byte*)_targetPtr.ToPointer();
            _backupArray = new byte[_headSize];
            for (int i = 0; i < _headSize; i++)
            {
                if (_proxyPtr != null)
                {
                    _proxyPtr[i] = pTarget[i];
                }
                _backupArray[i] = pTarget[i];
            }
        }

        /// <summary>
        /// 将原始方法跳转到我们的方法
        /// </summary>
        protected virtual void PatchTargetMethod()
        {
            fixed (byte* p = &s_jmpBuff[s_addrOffset])
            {
                if (IntPtr.Size == 8)
                {
                    IntPtr* ptr = (IntPtr*)p;
                    *ptr = _replacPtr;
                }
                else
                {
                    int* ptr = (int*)p;
                    *ptr = (int)_replacPtr;
                }
            }

            byte* pTarget = (byte*)_targetPtr.ToPointer();
            if (pTarget != null)
            {
                for (int i = 0, imax = s_jmpBuff.Length; i < imax; i++)
                {
                    pTarget[i] = s_jmpBuff[i];
                }
            }
        }

        /// <summary>
        /// 让 Proxy 方法的功能变成跳转向原始方法
        /// </summary>
        protected virtual void PatchProxyMethod()
        {
            if (_proxyPtr == null)
                return;

            fixed (byte* p = &s_jmpBuff[s_addrOffset])
            {
                if (IntPtr.Size == 8)
                {
                    ulong* ptr = (ulong*)p;
                    *ptr = (ulong)_targetPtr + (ulong)_headSize;
                }
                else
                {
                    uint* ptr = (uint*)p;
                    // cal offset
                    *ptr = (uint)_targetPtr + (uint)_headSize;
                }
            }

            // 跳过head
            byte* pProxy = _proxyPtr + _headSize;

            // 再填充跳转
            for (int i = 0; i < s_jmpBuff.Length; i++)
            {
                pProxy[i] = s_jmpBuff[i];
            }
        }

        #endregion

    }

}

