/*
 Desc: 一个可以运行时 Hook Mono 方法的工具，让你可以无需修改 UnityEditor.dll 等文件就可以重写其函数功能
 Author: Misaka Mikoto
 Github: https://github.com/easy66/MonoHooker
 */

using DotNetDetour;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;


/*
>>>>>>> 原始 UnityEditor.LogEntries.Clear 一型(.net 4.x)
0000000000403A00 < | 55                                 | push rbp                                     |
0000000000403A01   | 48 8B EC                           | mov rbp,rsp                                  |
0000000000403A04   | 48 81 EC 80 00 00 00               | sub rsp,80                                   |
0000000000403A0B   | 48 89 65 B0                        | mov qword ptr ss:[rbp-50],rsp                |
0000000000403A0F   | 48 89 6D A8                        | mov qword ptr ss:[rbp-58],rbp                |
0000000000403A13   | 48 89 5D C8                        | mov qword ptr ss:[rbp-38],rbx                | <<
0000000000403A17   | 48 89 75 D0                        | mov qword ptr ss:[rbp-30],rsi                |
0000000000403A1B   | 48 89 7D D8                        | mov qword ptr ss:[rbp-28],rdi                |
0000000000403A1F   | 4C 89 65 E0                        | mov qword ptr ss:[rbp-20],r12                |
0000000000403A23   | 4C 89 6D E8                        | mov qword ptr ss:[rbp-18],r13                |
0000000000403A27   | 4C 89 75 F0                        | mov qword ptr ss:[rbp-10],r14                |
0000000000403A2B   | 4C 89 7D F8                        | mov qword ptr ss:[rbp-8],r15                 |
0000000000403A2F   | 49 BB 00 2D 1E 1A FE 7F 00 00      | mov r11,7FFE1A1E2D00                         |
0000000000403A39   | 4C 89 5D B8                        | mov qword ptr ss:[rbp-48],r11                |
0000000000403A3D   | 49 BB 08 2D 1E 1A FE 7F 00 00      | mov r11,7FFE1A1E2D08                         |


>>>>>>> 二型(.net 2.x)
0000000000403E8F   | 55                                 | push rbp                                     |
0000000000403E90   | 48 8B EC                           | mov rbp,rsp                                  |
0000000000403E93   | 48 83 EC 70                        | sub rsp,70                                   |
0000000000403E97   | 48 89 65 C8                        | mov qword ptr ss:[rbp-38],rsp                |
0000000000403E9B   | 48 89 5D B8                        | mov qword ptr ss:[rbp-48],rbx                |
0000000000403E9F   | 48 89 6D C0                        | mov qword ptr ss:[rbp-40],rbp                | <<(16)
0000000000403EA3   | 48 89 75 F8                        | mov qword ptr ss:[rbp-8],rsi                 |
0000000000403EA7   | 48 89 7D F0                        | mov qword ptr ss:[rbp-10],rdi                |
0000000000403EAB   | 4C 89 65 D0                        | mov qword ptr ss:[rbp-30],r12                |
0000000000403EAF   | 4C 89 6D D8                        | mov qword ptr ss:[rbp-28],r13                |
0000000000403EB3   | 4C 89 75 E0                        | mov qword ptr ss:[rbp-20],r14                |
0000000000403EB7   | 4C 89 7D E8                        | mov qword ptr ss:[rbp-18],r15                |
0000000000403EBB   | 48 83 EC 20                        | sub rsp,20                                   |
0000000000403EBF   | 49 BB 18 3F 15 13 FE 7F 00 00      | mov r11,7FFE13153F18                         |
0000000000403EC9   | 41 FF D3                           | call r11                                     |
0000000000403ECC   | 48 83 C4 20                        | add rsp,20                                   |

 */


/// <summary>
/// Hook 类，用来 Hook 某个 C# 方法
/// </summary>
public unsafe class MethodHooker
{
    public bool isHooked { get; private set; }

    private MethodBase  _targetMethod;       // 需要被hook的目标方法
    private MethodBase  _replacementMethod;  // 被hook后的替代方法
    private MethodBase  _proxyMethod;        // 目标方法的代理方法(可以通过此方法调用被hook后的原方法)

    private IntPtr      _targetPtr;          // 目标方法被 jit 后的地址指针
    private IntPtr      _replacementPtr;
    private IntPtr      _proxyPtr;

    private static readonly byte[] s_jmpBuff;
    private static readonly byte[] s_jmpBuff_32 = new byte[] // 6 bytes
    {
        0x68, 0x00, 0x00, 0x00, 0x00,                       // push $val
        0xC3                                                // ret
    };
    private static readonly byte[] s_jmpBuff_64 = new byte[] // 14 bytes
    {
        0xFF, 0x25, 0x00, 0x00, 0x00, 0x00,                 // jmp [rip]
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,     // $val
    };
    private static readonly byte[] s_jmpBuff_arm32_arm = new byte[] // 8 bytes
    {
        0x04, 0xF0, 0x1F, 0xE5,                             // LDR PC, [PC, #-4]
        0x00, 0x00, 0x00, 0x00,                             // $val
    };
    private static readonly byte[] s_jmpBuff_arm32_thumb = new byte[] // 38 bytes
    {
        0x00, 0xB5, // PUSH {LR}
        0x10, 0xB4, // PUSH {R0}
        0x03, 0xB4, // PUSH {R0, R1}
        0x78, 0x46, // MOV R0, PC
        0x16, 0x30, // ADD R0, #0x16
        0x00, 0x68, // LDR R0, [R0, #0x00]
        0x69, 0x46, // MOV R1, SP
        0x08, 0x31, // ADD R1, #0x08
        0x08, 0x60, // STR R0, [R1, #0x00]
        0x79, 0x46, // MOV R1, PC
        0x0E, 0x31, // ADD R1, #0x0E
        0x8E, 0x46, // MOV LR, R1
        0x01, 0xBC, // POP {R0}
        0x02, 0xBC, // POP {R1}
        0x00, 0xBD, // POP {PC}
        0xC0, 0x46, // NOP

        0x00, 0x00, 0x00, 0x00, // $val
        0x00, 0xBD, // POP {PC}
    };
    private static readonly byte[] s_jmpBuff_arm64 = new byte[]
    {
        0x04, 0xF0, 0x1F, 0xE5,                             // LDR PC, [PC, #-4]
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,     // $val
    };
    private static readonly int             s_addrOffset;


    private byte[]      _jmpBuff;
    private byte[]      _proxyBuff;

    static MethodHooker()
    {
        if (LDasm.IsAndroidARM())
        {
            s_addrOffset = 4;
            if (IntPtr.Size == 4)
            {
                s_jmpBuff = s_jmpBuff_arm32_arm;
                //if (!LDasm.IsIL2CPP())
                //    s_jmpBuff = s_jmpBuff_arm32_arm;
                //else
                //{
                //    s_jmpBuff = s_jmpBuff_arm32_thumb;
                //    s_addrOffset = 32;
                //}

            }
            else
                s_jmpBuff = s_jmpBuff_arm64;
        }
        else
        {
            if (IntPtr.Size == 4)
            {
                s_jmpBuff = s_jmpBuff_32;
                s_addrOffset = 1;
            }
            else
            {
                s_jmpBuff = s_jmpBuff_64;
                s_addrOffset = 6;
            }
        }
        
    }

    /// <summary>
    /// 创建一个 Hooker
    /// </summary>
    /// <param name="targetMethod">需要替换的目标方法</param>
    /// <param name="replacementMethod">准备好的替换方法</param>
    /// <param name="proxyMethod">如果还需要调用原始目标方法，可以通过此参数的方法调用，如果不需要可以填 null</param>
    public MethodHooker(MethodBase targetMethod, MethodBase replacementMethod, MethodBase proxyMethod = null)
    {
        _targetMethod       = targetMethod;
        _replacementMethod  = replacementMethod;
        _proxyMethod        = proxyMethod;

        _targetPtr      = GetFunctionAddr(_targetMethod);
        _replacementPtr = GetFunctionAddr(_replacementMethod);
        if(proxyMethod != null)
            _proxyPtr       = GetFunctionAddr(_proxyMethod);

        _jmpBuff = new byte[s_jmpBuff.Length];
    }

    public void Install()
    {
        if(LDasm.IsiOS()) // iOS 不支持修改 code 所在区域 page
            return;

        if (isHooked)
            return;

        HookerPool.AddHooker(_targetMethod, this);

        InitProxyBuff();
        BackupHeader();
        PatchTargetMethod();
        PatchProxyMethod();

        isHooked = true;
    }

    public void Uninstall()
    {
        if (!isHooked)
            return;

        byte* pTarget = (byte*)_targetPtr.ToPointer();
        for (int i = 0; i < _proxyBuff.Length; i++)
            *pTarget++ = _proxyBuff[i];

        isHooked = false;
        HookerPool.RemoveHooker(_targetMethod);
    }

#region private
    /// <summary>
    ///  根据具体指令填充 ProxyBuff
    /// </summary>
    /// <returns></returns>
    private void InitProxyBuff()
    {
        byte* pTarget = (byte*)_targetPtr.ToPointer();

        uint requireSize = DotNetDetour.LDasm.SizeofMinNumByte(pTarget, s_jmpBuff.Length);
        _proxyBuff = new byte[requireSize];
        EnableAddrModifiable(_targetPtr, requireSize);
    }

    /// <summary>
    /// 备份原始方法头
    /// </summary>
    private void BackupHeader()
    {
        byte* pTarget = (byte*)_targetPtr.ToPointer();
        for (int i = 0; i < _proxyBuff.Length; i++)
            _proxyBuff[i] = *pTarget++;
    }

    // 将原始方法跳转到我们的方法
    private void PatchTargetMethod()
    {
        Array.Copy(s_jmpBuff, _jmpBuff, _jmpBuff.Length);
        fixed (byte* p = &_jmpBuff[s_addrOffset])
        {
            if(IntPtr.Size == 4)
                *((uint*)p) = (uint)_replacementPtr.ToInt32();
            else
                *((ulong*)p) = (ulong)_replacementPtr.ToInt64();
        }

        byte* pTarget = (byte*)_targetPtr.ToPointer();
        
        if(pTarget != null)
        {
            for (int i = 0, imax = _jmpBuff.Length; i < imax; i++)
                *pTarget++ = _jmpBuff[i];
        }
        
    }

    /// <summary>
    /// 让 Proxy 方法的功能变成跳转向原始方法
    /// </summary>
    private void PatchProxyMethod()
    {
        if (_proxyPtr == IntPtr.Zero)
            return;

        EnableAddrModifiable(_proxyPtr, (uint)_proxyBuff.Length);
        byte * pProxy = (byte*)_proxyPtr.ToPointer();
        for (int i = 0; i < _proxyBuff.Length; i++)     // 先填充头
            *pProxy++ = _proxyBuff[i];

        fixed (byte* p = &_jmpBuff[s_addrOffset])                  // 将跳转指向原函数跳过头的位置
        {
            if (IntPtr.Size == 4)
                * ((uint*)p) = (uint)_targetPtr.ToInt32() + (uint)_proxyBuff.Length;
            else
                *((ulong*)p) = (ulong)_targetPtr.ToInt64() + (ulong)_proxyBuff.Length;
        }

        for (int i = 0; i < _jmpBuff.Length; i++)       // 再填充跳转
            *pProxy++ = _jmpBuff[i];
    }

    private void EnableAddrModifiable(IntPtr ptr, uint size)
    {
        if (!LDasm.IsIL2CPP())
            return;

        uint oldProtect;
        bool ret = IL2CPPHelper.VirtualProtect(ptr, size, IL2CPPHelper.Protection.PAGE_EXECUTE_READWRITE, out oldProtect);
        Debug.Assert(ret);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)] // 好像在 IL2CPP 里无效
    private struct __ForCopy
    {
        public long         __dummy;
        public MethodBase   method;
    }
    /// <summary>
    /// 获取方法指令地址
    /// </summary>
    /// <param name="method"></param>
    /// <returns></returns>
    private IntPtr GetFunctionAddr(MethodBase method)
    {
        if (!LDasm.IsIL2CPP())
            return method.MethodHandle.GetFunctionPointer();
        else
        {
            __ForCopy __forCopy = new __ForCopy() { method = method };

            long* ptr = &__forCopy.__dummy;
            ptr++; // addr of _forCopy.method

            IntPtr methodAddr = IntPtr.Zero;
            if(sizeof(IntPtr) == 8)
            {
                long methodDataAddr = *(long*)ptr;
                byte* ptrData = (byte *)methodDataAddr + sizeof(IntPtr) * 2; // offset of Il2CppReflectionMethod::const MethodInfo *method;

                long methodPtr = 0;
                methodPtr = *(long*)ptrData;
                methodAddr = new IntPtr(*(long*)methodPtr); // MethodInfo::Il2CppMethodPointer methodPointer;
            }
            else
            {
                int methodDataAddr = *(int*)ptr;
                byte* ptrData = (byte *)methodDataAddr + sizeof(IntPtr) * 2; // offset of Il2CppReflectionMethod::const MethodInfo *method;

                int methodPtr = 0;
                methodPtr = *(int*)ptrData;
                methodAddr = new IntPtr(*(int*)methodPtr);
            }
            return methodAddr;
        }
    }

#endregion
}
