/*
 Desc: 一个可以运行时 Hook Mono 方法的工具，让你可以无需修改 UnityEditor.dll 等文件就可以重写其函数功能
 Author: Misaka Mikoto
 Github: https://github.com/easy66/MonoHooker
 */
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MonoHooker
{
    /// <summary>
    /// C# Hook 类，用来 Hook 某个 C# 方法
    /// </summary>
    public unsafe class NativeMethodHooker : HookerBase, IDisposable
    {
        private NativeLibraryPool _libraryPool;
        private bool _disposed = false;
        public Delegate GetProxy<T>()
        {
            Debug.Assert(typeof(Delegate).IsAssignableFrom(typeof(T)));
            return Marshal.GetDelegateForFunctionPointer((IntPtr)_proxyPtr, typeof(T));
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

            _libraryPool = NativeLibraryPool.GetLibraryPool(libraryName);
            IntPtr targetPtr = NativeAPI.mono_hooker_get_address(libraryName, symbol);
            UnityEngine.Debug.Log(targetPtr);
            UnityEngine.Debug.Log(libraryName);

            _headSize = (int)LDasm.SizeofMinNumByte(targetPtr.ToPointer(), s_jmpBuff.Length);
            _proxyBuffSize = _headSize + s_jmpBuff.Length;

            _targetPtr = targetPtr;
            _replacPtr = Marshal.GetFunctionPointerForDelegate(replace);
            _proxyPtr = _libraryPool.Alloc();

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
                _libraryPool.Free(_proxyPtr);
                _disposed = true;
            }
        }

        protected override void SetupJmpBuff()
        {
            if (NativeAPI.IsAndroidARM())
            {
                s_addrOffset = 0;
                s_jmpBuff = s_jmpArmBType;
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
