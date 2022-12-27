#if UNITY_EDITOR_WIN || (USE_LUA_PROFILER && UNITY_ANDROID)
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MikuLuaProfiler
{
    public class AndroidNativeUtil : NativeUtilInterface
    {
        static AndroidNativeUtil()
        {
            AndroidJavaClass act = new AndroidJavaClass("com.bytedance.shadowhook.ShadowHook");
            act.CallStatic<int>("init");
        }

        private static readonly IntPtr RTLD_DEFAULT = IntPtr.Zero;

        [DllImport("libc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr dlsym(IntPtr handle, string symbol);
        
        [DllImport("libc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr dlopen(string libfile);

        public IntPtr GetProcAddress(string InPath, string InProcName)
        {
            return dlsym(RTLD_DEFAULT, InProcName);
        }

        public IntPtr GetProcAddressByHandle(IntPtr InModule, string InProcName)
        {
            return dlsym(InModule, InProcName);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr dlopenfun(string libfile, int flag);
        private static dlopenfun dlopenF;
        private static INativeHooker hooker;
        private static Action<IntPtr> _callBack;
        public void HookLoadLibrary(Action<IntPtr> callBack)
        {
            IntPtr handle = GetProcAddress("libc.so", "dlopen");
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
                hooker = CreateHook();
                AndroidNativeHooker ah = hooker as AndroidNativeHooker;

                dlopenfun f = dlopen_replace;
                ah.InitLibSym("libc.so", "dlopen", Marshal.GetFunctionPointerForDelegate(f));
                ah.Install();

                Debug.LogError("install hook with lib symbol");
            }
        }

        private static bool isLoadLuaSo = false;
        
        [MonoPInvokeCallbackAttribute(typeof(dlopenfun))]
        static IntPtr dlopen_replace(string libfile, int flag)
        {
            var ret = dlopenF(libfile, flag);
            if (!isLoadLuaSo && dlsym(ret, "luaL_newstate") != IntPtr.Zero)
            {
                isLoadLuaSo = true;
                _callBack.Invoke(ret);
            }
            return ret;
        }

        public INativeHooker CreateHook()
        {
            return new AndroidNativeHooker();
        }
    }

    public unsafe class AndroidNativeHooker : INativeHooker
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

        private string _libName = "";
        private string _symName = "";
        
        private IntPtr stub = IntPtr.Zero;

        #region native
        [DllImport("libshadowhook.so", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr shadowhook_hook_sym_name(string lib_name, string sym_name, IntPtr new_addr, void**  orig_addr);

        [DllImport("libshadowhook.so", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr shadowhook_hook_func_addr(IntPtr func_addr, IntPtr new_addr, void**  orig_addr);
        
        [DllImport("libshadowhook.so", CallingConvention = CallingConvention.Cdecl)]
        private static extern int shadowhook_unhook(IntPtr stub);
        #endregion
        
        public void Init(IntPtr targetPtr, IntPtr replacementPtr)
        {
            _targetPtr = targetPtr;
            _replacementPtr = replacementPtr;
        }

        public void InitLibSym(string lib_name, string sym_name, IntPtr replacementPtr)
        {
            _targetPtr = IntPtr.Zero;
            _libName = lib_name;
            _symName = sym_name;
            _replacementPtr = replacementPtr;
        }

        public void Install()
        {
            if (_targetPtr != IntPtr.Zero)
            {
                fixed (void** addr = &_proxyFun)
                {
                    stub = shadowhook_hook_func_addr( _targetPtr, _replacementPtr, addr);
                }
            }
            else
            {
                fixed (void** addr = &_proxyFun)
                {
                    stub = shadowhook_hook_sym_name( _libName, _symName, _replacementPtr, addr);
                }
            }
        } 

        public void Uninstall()
        {
            if (stub != IntPtr.Zero)
            {
                shadowhook_unhook(stub);
                _proxyFun = null;
                stub = IntPtr.Zero;
            }
        }
    }
}

#endif