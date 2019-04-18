/*
 Desc: 一个可以运行时 Hook Mono 方法的工具，让你可以无需修改 UnityEditor.dll 等文件就可以重写其函数功能
 Author: Misaka Mikoto
 Github: https://github.com/easy66/MonoHooker
 */
using System;
using System.Diagnostics;
using System.Reflection;


namespace MonoHooker
{
    /// <summary>
    /// C# Hook 类，用来 Hook 某个 C# 方法
    /// </summary>
    public unsafe class CSharpMethodHooker : HookerBase
    {
        /// <summary>
        /// 创建一个 Hooker
        /// </summary>
        /// <param name="target">需要替换的目标方法</param>
        /// <param name="replace">准备好的替换方法</param>
        public CSharpMethodHooker(MethodBase target, MethodBase replace, MethodBase proxy)
        {
            SetupJmpBuff();

            IntPtr targetPtr = NativeAPI.GetFunctionAddr(target);
            IntPtr replacePtr = NativeAPI.GetFunctionAddr(replace);
            IntPtr proxyPtr = NativeAPI.GetFunctionAddr(proxy);

            _headSize = (int)LDasm.SizeofMinNumByte(targetPtr.ToPointer(), s_jmpBuff.Length);
            _proxyBuffSize = _headSize + s_jmpBuff.Length;
            _proxyPtr = (byte*)proxyPtr.ToPointer();
            Debug.Assert(_proxyPtr != null);
            _targetPtr = targetPtr;
            _replacPtr = replacePtr;

            Install();
        }

        protected override void SetupJmpBuff()
        {
            if (NativeAPI.IsAndroidARM())
            {
                s_addrOffset = 1;
                s_jmpBuff = s_jmpArmBType;
            }
            else
            {
                s_jmpBuff = s_jmpBuffIntel;
                s_addrOffset = 1;
            }
        }
    }

}
