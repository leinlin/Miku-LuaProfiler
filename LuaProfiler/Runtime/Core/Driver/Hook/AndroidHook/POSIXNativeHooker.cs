#if UNITY_EDITOR || (USE_LUA_PROFILER && UNITY_ANDROID)
using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MikuLuaProfiler
{
    public unsafe class POSIXNativeUtil : NativeUtilInterface
    {
        private static readonly IntPtr RTLD_DEFAULT = IntPtr.Zero;
        private static readonly int RTLD_NOW = 2;

#if UNITY_IOS && !UNITY_EDITOR
        public static IntPtr miku_dlopen(string path, int mode)
        {
            return IntPtr.Zero;
        }

        public static IntPtr miku_dlsymbol(IntPtr handle, string symbol)
        {
            return IntPtr.Zero;
        }
        
        public static IntPtr miku_get_dlopen_ptr()
        {
            return IntPtr.Zero;
        }
        
        public static int miku_Hook(IntPtr func_addr, IntPtr new_addr, void**  orig_addr) { return 0; }
        
        public static int miku_UnHook(IntPtr stub) { return 0; }
#else
        const string LIB_NAME = "miku_hook";
        
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr miku_dlopen(string path, int mode);
        
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr miku_dlsymbol(IntPtr handle, string symbol);
        
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr miku_get_dlopen_ptr();
        
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int miku_Hook(IntPtr func_addr, IntPtr new_addr, void**  orig_addr);
        
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int miku_UnHook(IntPtr stub);
#endif

        public IntPtr GetProcAddress(string InPath, string InProcName)
        {
            IntPtr handle = miku_dlopen(InPath, RTLD_NOW);
            return GetProcAddressByHandle(handle, InProcName);
        }

        public IntPtr GetProcAddressByHandle(IntPtr InModule, string InProcName)
        {
            return miku_dlsymbol(InModule, InProcName);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr dlopenfun(string libfile, int flag);
        private static dlopenfun dlopenF;
        private static INativeHooker hooker;
        private static Action<IntPtr> _callBack;
        public void HookLoadLibrary(Action<IntPtr> callBack)
        {
            IntPtr handle = miku_get_dlopen_ptr();
            if (handle != IntPtr.Zero)
            {
                // LoadLibraryExW is called by the other LoadLibrary functions, so we
                // only need to hook it.
                hooker = CreateHook();
                dlopenfun f = dlopen_replace;
                hooker.Init(handle, Marshal.GetFunctionPointerForDelegate(f));
                hooker.Install();

                dlopenF = (dlopenfun)hooker.GetProxyFun(typeof(dlopenfun));
                _callBack = callBack;
            }
            else
            {
                Debug.LogError("get dlopen addr fail");
            }
        }
        
        public bool NeedHookLua()
        {
#if UNITY_EDITOR
            return true;
#elif UNITY_ANDROID
            try
            {
                string localPath = Application.persistentDataPath;
                string fullPath = Path.Combine(localPath, "need_hook_miku_lua");
                bool exists = File.Exists(fullPath);
                Debug.Log($"check filePath: {fullPath}，isExit: {exists}");
                File.Delete(fullPath);

                return exists;
            }
            catch (System.Exception ex)
            {
                Debug.LogError("check filePath Error: " + ex);
                return false;
            }
#else
            return false;
#endif

        }

        private static bool isLoadLuaSo = false;
        
        [MonoPInvokeCallbackAttribute(typeof(dlopenfun))]
        static IntPtr dlopen_replace(string libfile, int flag)
        {
            var ret = dlopenF(libfile, flag);
            if (!isLoadLuaSo && miku_dlsymbol(ret, "luaL_newstate") != IntPtr.Zero)
            {
                isLoadLuaSo = true;
                _callBack.Invoke(ret);
                hooker.Uninstall();
            }
            return ret;
        }

        public INativeHooker CreateHook()
        {
            return new POSIXNativeHooker();
        }
    }

    public unsafe class POSIXNativeHooker : INativeHooker
    {
        public Delegate GetProxyFun(Type t)
        {
            if (_proxyFun == null) return null;
            return Marshal.GetDelegateForFunctionPointer((IntPtr)_proxyFun, t);
        }

        public bool isHooked { get; set; }
        private void* _proxyFun = null;
        private IntPtr _targetPtr = IntPtr.Zero;
        private IntPtr _replacementPtr = IntPtr.Zero;
        private int stub = 0;
        
        public void Init(IntPtr targetPtr, IntPtr replacementPtr)
        {
            _targetPtr = targetPtr;
            _replacementPtr = replacementPtr;
        }

        public void Install()
        {
            fixed (void** addr = &_proxyFun)
            {
                stub = POSIXNativeUtil.miku_Hook( _targetPtr, _replacementPtr, addr);
            }
        } 

        public void Uninstall()
        {
            if (stub != 0)
            {
                POSIXNativeUtil.miku_UnHook(_targetPtr);
                _proxyFun = null;
                stub = 0;
            }
        }
    }
}

#endif