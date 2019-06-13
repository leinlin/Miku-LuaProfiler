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
        protected static byte[] s_jmpBuff32 = new byte[] // 5 bytes
        {
            0xE9, 0x00, 0x00, 0x00, 0x00,                 // jmp $val 目标地址 - 指令地址 - 5 = 偏移
        };		
        protected static byte[] s_jmpBuff64 = new byte[] // 14 bytes
        {
            0xFF, 0x25, 0x00, 0x00, 0x00, 0x00,                 // jmp [rip]
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,     // $val
        };
        //protected static readonly byte[] s_jmpArmBType = new byte[]
        //{
        //    0x00, 0x00, 0x00, 0xEA                        // B int26 （目标地址 - 指令地址 - 8）/ 4 = 偏移
        //};

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

            HookerPool.AddHooker(_targetPtr, this);

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
            HookerPool.RemoveHooker(_targetPtr);
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

            if (_proxyPtr != null)
            {
                int index = 0;
                if (LDasm.CheckShortCall(_proxyPtr, s_jmpBuff.Length, out index))
                {
					if(IntPtr.Size == 8)
					{
	                    // 目标地址 = 偏移 + 5 + 指令地址
	                    int oldOffsetAddr = *((int*)(_proxyPtr + index + 1));
	                    long targetAddr = oldOffsetAddr + 5 + (long)_targetPtr + index;
	                    fixed (byte* p = &s_jmpBuff[s_addrOffset])
	                    {
	                        IntPtr* ptr = (IntPtr*)p;
	                        *ptr = (IntPtr)targetAddr;
	                    }
	                    // 原来的跳转指令长度为5，现在为 14所以把_headsize 拓宽
	                    _headSize = _headSize - 5 + 14;
	                    for (int i = index; i < 14 + index; i++)
	                    {
	                        _proxyPtr[i] = s_jmpBuff[i - index];
	                    }					
					}
					else
					{
                        // 目标地址 = 偏移 + 5 + 指令地址
                        int oldOffsetAddr = *((int*)(_proxyPtr + index + 1));
                        long targetAddr = oldOffsetAddr + 5 + (long)_targetPtr + index;
                        // 因为指令地址发生了改变，所以要重新计算偏移 公式: 偏移 = 目标地址 - 指令地址 - 5
                        int newOffsetAddr = (int)(targetAddr - ((long)_proxyPtr + index) - 5);
                        *((int*)(_proxyPtr + index + 1)) = newOffsetAddr;
					}
                }
            }
        }

        /// <summary>
        /// 将原始方法跳转到我们的方法
        /// </summary>
        protected virtual void PatchTargetMethod()
        {
            if (NativeAPI.IsAndroidARM())
            {
                fixed (byte* p = &s_jmpBuff[s_addrOffset])
                {
                    IntPtr* ptr = (IntPtr*)p;
                    *ptr = _replacPtr;
                }
            }
            else
            {
                fixed (byte* p = &s_jmpBuff[s_addrOffset])
                {
					if(IntPtr.Size == 8)
					{				
	                    IntPtr* ptr = (IntPtr*)p;
	                    *ptr = _replacPtr;
					}
					else
					{
	                    int* ptr = (int*)p;
	                    *ptr = (int)((long)_replacPtr - (long)_targetPtr - 5);					
					}
                }
            }

            bool ret = NativeAPI.miku_hooker_protect(_targetPtr.ToPointer(), _proxyBuffSize, 7);
            Debug.Assert(ret);
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

            if (NativeAPI.IsAndroidARM())
            {
                int offset = (int)(((long)_targetPtr - (long)_proxyPtr - 8) / 4);
                byte[] offset_bytes = BitConverter.GetBytes(offset);
                for (int i = 0; i < 3; i++)
                {
                    s_jmpBuff[i] = offset_bytes[i];
                }
            }
            else
            {
                fixed (byte* p = &s_jmpBuff[s_addrOffset])
                {
					if(IntPtr.Size == 8)
					{	
	                    ulong* ptr = (ulong*)p;
	                    *ptr = (ulong)_targetPtr + (ulong)_headSize;
					}
					else
					{
	                    int* ptr = (int*)p;
	                    // cal offset
	                    *ptr = (int)((long)_targetPtr - (long)_proxyPtr - 5);					
					}
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

