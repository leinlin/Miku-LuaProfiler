using UnityEngine;
using MonoHooker;
using System;
using System.Runtime.InteropServices;

public unsafe class Test : MonoBehaviour
{

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr LuaNewStateDelegate(IntPtr f, IntPtr ud);

    [DllImport("tolua", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr luaL_newstate();

    private NativeMethodHooker nativeHooker;
    private static LuaNewStateDelegate originCall;
    // Use this for initialization
    void Start()
    {
        //MethodInfo target = typeof(UnityEngine.Debug).GetMethod("LogError", new Type[] { typeof(object) });
        //MethodInfo replace = typeof(Test).GetMethod("LogError");
        //MethodInfo proxy = typeof(Test).GetMethod("LogErrorProxy");

        //UnityEngine.Debug.LogError("hello world");
        //CSharpMethodHooker hooker = new CSharpMethodHooker(target, replace, proxy);
        //hooker.Install();
        //UnityEngine.Debug.LogError("hello world");
        //hooker.Uninstall();
        //UnityEngine.Debug.LogError("hello world");

        //target = typeof(Debug).GetMethod("LogError");
        //replace = typeof(Test).GetMethod("LogErrorNative");

        //luaL_newstate();

        int i = -2;
        byte[] offset_bytes = BitConverter.GetBytes(i);
        foreach (var item in offset_bytes)
        {
            Debug.Log(item);
        }

        var l = luaL_newstate();
        Debug.Log(l);
        nativeHooker = new NativeMethodHooker("/data/app/com.amumu.test-1/lib/arm/libtolua.so", "lua_newstate", (LuaNewStateDelegate)lua_newstate_hooked);
        try
        {
            //originCall = (LuaNewStateDelegate)nativeHooker.GetProxy<LuaNewStateDelegate>();
            //l = luaL_newstate();
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    public static IntPtr ProxyCall()
    {
        return (IntPtr)1234;
    }

    public static IntPtr lua_newstate_hooked(IntPtr f, IntPtr ud)
    {
        IntPtr ptr = originCall(f, ud);
        Debug.Log("hooked:" + ptr);
        return ptr;
    }
    public static void LogErrorProxy(object message)
    {
        Debug.Log("fkdlfkdlf");
        Debug.Log("fkdlfkdlf");
        Debug.Log("fkdlfkdlf");
        Debug.Log("fkdlfkdlf");
    }

    public static void LogError(object v)
    {
        LogErrorProxy("hooked:" + v);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
