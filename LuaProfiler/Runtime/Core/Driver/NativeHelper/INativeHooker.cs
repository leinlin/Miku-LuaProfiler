#if UNITY_EDITOR_WIN || USE_LUA_PROFILER
using System;

namespace MikuLuaProfiler
{
    public interface NativeUtilInterface
    {
        IntPtr GetProcAddress(string InPath, string InProcName);
        IntPtr GetProcAddressByHandle(IntPtr InModule, string InProcName);
        void HookLoadLibrary(Action<IntPtr> callBack);
        INativeHooker CreateHook();
    }

    public interface INativeHooker
    {
        void Init(IntPtr targetPtr, IntPtr replacementPtr);
        Delegate GetProxyFun(Type t);
        bool isHooked { get; set; }
        void Install();
        void Uninstall();
        
    }
}
#endif