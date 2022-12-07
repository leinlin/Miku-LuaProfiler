#if UNITY_EDITOR_WIN || USE_LUA_PROFILER
using System;

namespace MikuLuaProfiler
{
    public interface NativeUtilInterface
    {
        public IntPtr GetProcAddress(string InPath, string InProcName);
        public IntPtr GetProcAddressByHandle(IntPtr InModule, string InProcName);
        public void HookLoadLibrary(Action<IntPtr> callBack);
        public INativeHooker CreateHook();
    }

    public interface INativeHooker
    {
        public void Init(IntPtr targetPtr, IntPtr replacementPtr);
        public Delegate GetProxyFun(Type t);
        public bool isHooked { get; set; }
        public void Install();
        public void Uninstall();
        
    }
}
#endif