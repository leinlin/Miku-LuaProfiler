/*
 Desc: 一个可以运行时 Hook Mono 方法的工具，让你可以无需修改 UnityEditor.dll 等文件就可以重写其函数功能
 Author: Misaka Mikoto
 Github: https://github.com/easy66/MonoHooker
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MonoHooker
{

    public unsafe abstract class HookerBase
    {
        public bool isHooked { get; private set; }

        protected IntPtr _targetPtr;          // 目标方法被 jit 后的地址指针
        protected IntPtr _replacPtr;
        protected byte* _proxyPtr;
        protected int _headSize;
        protected int _proxyBuffSize;

        protected static int s_addrOffset;
        protected static byte[] s_jmpBuff;
        protected static byte[] s_jmpBuffIntel = new byte[] // 5 bytes
        {
            0xE9, 0x00, 0x00, 0x00, 0x00,                 // jmp $val 目标地址 - 指令地址 - 5 = 偏移
        };
        protected static readonly byte[] s_jmpArmBType = new byte[]
        {
            0x00, 0x00, 0x00, 0xEA                        // B int26 （目标地址 - 指令地址 - 8）/ 4 = 偏移
        };
        //protected static readonly byte[] s_jmpBuffArm32 = new byte[] // 8 bytes
        //{
        //    0x04, 0xF0, 0x1F, 0xE5,                             // LDR PC, [PC, #-4]
        //    0x00, 0x00, 0x00, 0x00,                             // $val
        //};
        //protected static readonly byte[] s_jmpBuffArm64 = new byte[]
        //{
        //    0x04, 0xF0, 0x1F, 0xE5,                             // LDR PC, [PC, #-4]
        //    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,     // $val
        //};

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
            UnityEngine.Debug.Log("installed");
            isHooked = true;
        }

        #region virtual

        public virtual void Uninstall()
        {
            if (!isHooked)
                return;

            byte* pTarget = (byte*)_targetPtr.ToPointer();
            for (int i = 0; i < _headSize; i++)
            {
                *pTarget++ = _proxyPtr[i];
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

            for (int i = 0; i < _headSize; i++)
            {
                _proxyPtr[i] = pTarget[i];
            }
        }

        /// <summary>
        /// 将原始方法跳转到我们的方法
        /// </summary>
        protected virtual void PatchTargetMethod()
        {
            if (NativeAPI.IsAndroidARM())
            {
                int offset = (int)(((long)_replacPtr - (long)_targetPtr - 8) / 4);
                UnityEngine.Debug.Log("offset is:" + offset);
                byte[] offset_bytes = BitConverter.GetBytes(offset);
                for (int i = 0; i < 3; i++)
                {
                    UnityEngine.Debug.Log(offset_bytes[i]);
                    s_jmpBuff[i] = offset_bytes[i];
                }
            }
            else
            {
                fixed (byte* p = &s_jmpBuff[s_addrOffset])
                {
                    int* ptr = (int*)p;
                    *ptr = (int)((long)_replacPtr - (long)_targetPtr - 5);
                }
            }

            bool ret = NativeAPI.mono_hooker_protect(_targetPtr.ToPointer(), _proxyBuffSize, 7);
            Debug.Assert(ret);
            byte* pTarget = (byte*)_targetPtr.ToPointer();
            for (int i = 0, imax = s_jmpBuff.Length; i < imax; i++)
            {
                pTarget[i] = 0;
            }
            if (pTarget != null)
            {
                for (int i = 0, imax = s_jmpBuff.Length; i < imax; i++)
                {
                    UnityEngine.Debug.Log("22222222222:" + i);
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

            UnityEngine.Debug.Log("33333333333333");
            if (NativeAPI.IsAndroidARM())
            {
                int offset = (int)(((long)_targetPtr - (long)_proxyPtr - 8) / 4);
                byte[] offset_bytes = BitConverter.GetBytes(offset);
                UnityEngine.Debug.Log("44444444444444");
                for (int i = 0; i < 3; i++)
                {
                    s_jmpBuff[i] = offset_bytes[i];
                    UnityEngine.Debug.Log("55555555555555");
                }
            }
            else
            {
                // 将跳转指向原函数跳过头的位置
                fixed (byte* p = &s_jmpBuff[s_addrOffset])
                {
                    int* ptr = (int*)p;
                    // cal offset
                    *ptr = (int)((long)_targetPtr - (long)_proxyPtr - 5);
                }
            }

            // 跳过head
            byte* pProxy = _proxyPtr + _headSize;
            // 再填充跳转
            for (int i = 0; i < s_jmpBuff.Length; i++)
                pProxy[i] = s_jmpBuff[i];
        }

        #endregion

    }

}

