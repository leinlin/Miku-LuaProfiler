/*
 Desc: 一个可以运行时 Hook Mono 方法的工具，让你可以无需修改 UnityEditor.dll 等文件就可以重写其函数功能
 Author: Misaka Mikoto
 Github: https://github.com/easy66/MonoHooker
 */
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MonoHooker
{
    public unsafe static class NativeAPI
    {
#if !UNITY_EDITOR && UNITY_IPHONE
        const string DLL_NAME = "__Internal";
#else
        const string DLL_NAME = "monohooker";
#endif

        [DllImport(DLL_NAME)]
        public static extern IntPtr mono_hooker_jit_alloc(IntPtr addr, int length);
        [DllImport(DLL_NAME)]
        public static extern bool mono_hooker_protect(void* addr, int length, int prot);
        [DllImport(DLL_NAME)]
        public static extern bool mono_hooker_free_library(string fileName);
        [DllImport(DLL_NAME)]
        public static extern IntPtr mono_hooker_get_address(string filename, string symbol);
        [DllImport(DLL_NAME)]
        public static extern IntPtr mono_hooker_free(void* addr, int length);
        [DllImport(DLL_NAME)]
        public static extern IntPtr mono_hooker_get_library_addr(string fileName);

        #region platform
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
            return method.MethodHandle.GetFunctionPointer();
        }
        #endregion
    }
}

