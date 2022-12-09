/*
               #########                       
              ############                     
              #############                    
             ##  ###########                   
            ###  ###### #####                  
            ### #######   ####                 
           ###  ########## ####                
          ####  ########### ####               
         ####   ###########  #####             
        #####   ### ########   #####           
       #####   ###   ########   ######         
      ######   ###  ###########   ######       
     ######   #### ##############  ######      
    #######  #####################  ######     
    #######  ######################  ######    
   #######  ###### #################  ######   
   #######  ###### ###### #########   ######   
   #######    ##  ######   ######     ######   
   #######        ######    #####     #####    
    ######        #####     #####     ####     
     #####        ####      #####     ###      
      #####       ###        ###      #        
        ###       ###        ###               
         ##       ###        ###               
__________#_______####_______####______________
                我们的未来没有BUG                
* ==============================================================================
* Filename: LuaDLL.cs
* Created:  2018/7/13 14:29:22
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/

#if UNITY_EDITOR_WIN || USE_LUA_PROFILER
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;


namespace MikuLuaProfiler
{
#region 通用结构体

    public enum LuaGCOptions
    {
        LUA_GCSTOP = 0,
        LUA_GCRESTART = 1,
        LUA_GCCOLLECT = 2,
        LUA_GCCOUNT = 3,
        LUA_GCCOUNTB = 4,
        LUA_GCSTEP = 5,
        LUA_GCSETPAUSE = 6,
        LUA_GCSETSTEPMUL = 7,
    }

    public class LuaIndexes
    {
        public static int LUA_REGISTRYINDEX = -10000;
        public static int LUA_GLOBALSINDEX = -10002;
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class MonoPInvokeCallbackAttribute : Attribute
    {
        public MonoPInvokeCallbackAttribute(Type type)
        {
        }
    }

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int LuaCSFunction(IntPtr luaState);
#else
    public delegate int LuaCSFunction(IntPtr luaState);
#endif
    #endregion

    public class LuaDLL
    {
        public static int LUA_VERSION = 510;
        public static bool IS_LUA_JIT = false;

        public static bool isHook = false;
        public static bool isOpenLibs = false;
        #region index
        public static void lua_getglobal51(IntPtr luaState, string name)
        {
            lua_getfield(luaState, LuaIndexes.LUA_GLOBALSINDEX, name);
        }

        public static void lua_setglobal51(IntPtr luaState, string name)
        {
            lua_setfield(luaState, LuaIndexes.LUA_GLOBALSINDEX, name);
        }

        public static void lua_getref(IntPtr luaState, int reference)
        {
            lua_rawgeti(luaState, LuaIndexes.LUA_REGISTRYINDEX, reference);
        }

        public static void lua_unref(IntPtr luaState, int reference)
        {
            luaL_unref(luaState, LuaIndexes.LUA_REGISTRYINDEX, reference);
        }
        #endregion

        #region hooks
        private static INativeHooker luaL_newstate_hook;
        private static INativeHooker lua_close_hook;
        private static INativeHooker lua_gc_hook;
        private static INativeHooker lua_call_hook;
        private static INativeHooker lua_error_hook;
        private static INativeHooker luaL_openlibs_hook;
        private static INativeHooker luaL_ref_hook;
        private static INativeHooker luaL_unref_hook;
        private static INativeHooker luaL_loadbuffer_hook;
        private static INativeHooker luaL_loadfile_hook;

        #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        private static NativeUtilInterface nativeUtil = new WindowsNativeUtil();
        #elif UNITY_ANDROID
        private static NativeUtilInterface nativeUtil = new AndroidNativeUtil();
        #endif

        #endregion

        #region 通用操作
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr luaL_newstate_fun();
        public static luaL_newstate_fun luaL_newstate;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void lua_close_fun(IntPtr L);
        public static lua_close_fun lua_close;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int luaL_ref_fun(IntPtr luaState, int t);
        public static luaL_ref_fun luaL_ref { get; private set; }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void luaL_unref_fun(IntPtr luaState, int registryIndex, int reference);
        public static luaL_unref_fun luaL_unref { get; private set; }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int luaL_loadbufferx_fun(IntPtr luaState, IntPtr buff, IntPtr size, string name, IntPtr mode);
        public static luaL_loadbufferx_fun luaL_loadbufferx { get; private set; }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int lua_error_fun(IntPtr luaState);
        public static lua_error_fun lua_error { get; private set; }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int luaL_loadbuffer_fun(IntPtr luaState, IntPtr buff, IntPtr size, string name);
        public static luaL_loadbuffer_fun luaL_loadbuffer { get; private set; }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void lua_getglobal_fun(IntPtr luaState, string name);
        public static lua_getglobal_fun lua_getglobal;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void lua_setglobal_fun(IntPtr luaState, string name);
        public static lua_setglobal_fun lua_setglobal;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int lua_gettop_fun(IntPtr luaState);
        public static lua_gettop_fun lua_gettop { get; private set; }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int lua_objlen_fun(IntPtr luaState, int stackPos);
        public static lua_objlen_fun lua_objlen { get; private set; }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void lua_settop_fun(IntPtr luaState, int top);
        public static lua_settop_fun lua_settop { get; private set; }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void lua_pushvalue_fun(IntPtr luaState, int idx);
        public static lua_pushvalue_fun lua_pushvalue { get; private set; }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void lua_remove_fun(IntPtr luaState, int idx);
        public static lua_remove_fun lua_remove { get; private set; }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void lua_rotate_fun(IntPtr luaState, int idx, int n);
        public static lua_rotate_fun lua_rotate { get; private set; }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void lua_insert_fun(IntPtr luaState, int idx);
        public static lua_insert_fun lua_insert { get; private set; }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate LuaTypes lua_type_fun(IntPtr luaState, int index);
        public static lua_type_fun lua_type { get; private set; }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate double lua_tonumberx_fun(IntPtr luaState, int idx, IntPtr isnum);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate double lua_tonumber_fun(IntPtr luaState, int idx);
        public static lua_tonumber_fun lua_tonumber { get; private set; }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void lua_pushnil_fun(IntPtr luaState);
        public static lua_pushnil_fun lua_pushnil { get; private set; }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void lua_pushnumber_fun(IntPtr luaState, double number);
        public static lua_pushnumber_fun lua_pushnumber { get; private set; }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void lua_pushstring_fun(IntPtr luaState, string str);
        public static lua_pushstring_fun lua_pushstring { get; private set; }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void lua_pushboolean_fun(IntPtr luaState, int value);
        public static lua_pushboolean_fun lua_pushboolean { get; private set; }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void lua_getfield_fun(IntPtr L, int idx, string key);
        public static lua_getfield_fun lua_getfield { get; private set; }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void lua_rawget_fun(IntPtr luaState, int idx);
        public static lua_rawget_fun lua_rawget { get; private set; }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void lua_rawgeti_fun(IntPtr luaState, int idx, int n);
        public static lua_rawgeti_fun lua_rawgeti { get; private set; }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void lua_createtable_fun(IntPtr luaState, int narr, int nrec);
        public static lua_createtable_fun lua_createtable { get; private set; }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void lua_setfield_fun(IntPtr L, int idx, string key);
        public static lua_setfield_fun lua_setfield { get; private set; }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void lua_rawset_fun(IntPtr luaState, int idx);
        public static lua_rawset_fun lua_rawset { get; private set; }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int lua_pcallk_fun(IntPtr luaState, int nArgs, int nResults, int errfunc, int ctx, IntPtr k);
        public static lua_pcallk_fun lua_pcallk { get; private set; }
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int lua_pcall_fun(IntPtr luaState, int nArgs, int nResults, int errfunc);
        public static lua_pcall_fun lua_pcall { get; private set; }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int lua_callk_fun(IntPtr luaState, int nArgs, int nResults, int ctx, IntPtr k);
        public static lua_callk_fun lua_callk;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int lua_call_fun(IntPtr luaState, int nArgs, int nResults);
        public static lua_call_fun lua_call;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int lua_gc_fun(IntPtr luaState, LuaGCOptions what, int data);
        public static lua_gc_fun lua_gc { get; private set; }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int lua_next_fun(IntPtr luaState, int index);
        public static lua_next_fun lua_next { get; private set; }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void luaL_openlibs_fun(IntPtr luaState);
        public static luaL_openlibs_fun luaL_openlibs { get; private set; }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr lua_tolstring_fun(IntPtr luaState, int index, out IntPtr strLen);
        public static lua_tolstring_fun lua_tolstring;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void lua_pushcclosure_fun(IntPtr L, IntPtr fn, int nup);
        public static lua_pushcclosure_fun lua_pushcclosure;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int luaL_loadfile_fun(IntPtr L, string filename);
        public static luaL_loadfile_fun luaL_loadfile;

        public static void lua_pop(IntPtr luaState, int amount)
        {
            LuaDLL.lua_settop(luaState, -(amount) - 1);
        }
        public static void lua_newtable(IntPtr luaState)
        {
            LuaDLL.lua_createtable(luaState, 0, 0);
        }
        public static bool lua_isfunction(IntPtr luaState, int n)
        {
            return lua_type(luaState, n) == LuaTypes.LUA_TFUNCTION;
        }
        public static bool lua_isnil(IntPtr luaState, int n)
        {
            return lua_type(luaState, n) == LuaTypes.LUA_TNIL;
        }
        public static bool lua_istable(IntPtr luaState, int n)
        {
            return lua_type(luaState, n) == LuaTypes.LUA_TTABLE;
        }
        public static bool lua_isuserdata(IntPtr luaState, int n)
        {
            return lua_type(luaState, n) == LuaTypes.LUA_TUSERDATA;
        }

        public static void luaL_initlibs(IntPtr luaState)
        {
            LuaDLL.isOpenLibs = false;
            luaL_openlibs(luaState);
            if (LUA_VERSION > 510)
            {
                LuaLib.DoString(luaState, env_script);
            }
        }

        public static unsafe IntPtr ConvertByteArrayToPtr(byte[] buff)
        {
            IntPtr intPtr;
            fixed (byte* b = buff)
            {
                intPtr = (IntPtr)b;
            }
            return intPtr;
        }

        #region script
        const string env_script = @"
local function getfunction(level)
    local info = debug.getinfo(level + 1, 'f')
    return info and info.func
end

function setfenv(fn, env)
    if type(fn) == 'number' then fn = getfunction(fn + 1) end
    local i = 1
    while true do
    local name = debug.getupvalue(fn, i)
    if name == '_ENV' then
        debug.upvaluejoin(fn, i, (function()
        return env
        end), 1)
        break
    elseif not name then
        break
    end

    i = i + 1
    end

    return fn
end

function getfenv(fn)
    if type(fn) == 'number' then fn = getfunction(fn + 1) end
    local i = 1
    while true do
    local name, val = debug.getupvalue(fn, i)
    if name == '_ENV' then
        return val
    elseif not name then
        break
    end
    i = i + 1
    end
end
";
        #endregion

        public static int luaL_loadbufferUnHook(IntPtr luaState, byte[] buff, IntPtr size, string name)
        {
            IntPtr intPtr = ConvertByteArrayToPtr(buff);
            int result;
            if (LUA_VERSION > 510)
            {
                result = luaL_loadbufferx(luaState, intPtr, size, name, IntPtr.Zero);
            }
            else
            {
                result = luaL_loadbuffer(luaState, intPtr, size, name);
            }
            return result;
        }
        public static string lua_tostring(IntPtr luaState, int index)
        {
            IntPtr len;
            IntPtr str = lua_tolstring(luaState, index, out len);

            if (str != IntPtr.Zero)
            {
                return lua_ptrtostring(str, (int)len);
            }

            return null;
        }
        public static string lua_ptrtostring(IntPtr str, int len)
        {
            string ss = Marshal.PtrToStringAnsi(str, len);

            if (ss == null)
            {
                byte[] buffer = new byte[len];
                Marshal.Copy(str, buffer, 0, len);
                return Encoding.UTF8.GetString(buffer);
            }

            return ss;
        }
        public static void lua_pushstdcallcfunction(IntPtr luaState, LuaCSFunction func)
        {
            IntPtr fn = Marshal.GetFunctionPointerForDelegate(func);
            lua_pushcclosure(luaState, fn, 0);
        }
        #endregion

        #region mono hook
        private static bool m_hooked = false;
        private static object m_Lock = 1;

        public static void Uninstall()
        {
            if (luaL_newstate_hook != null)
            {
                luaL_newstate_hook.Uninstall();
                luaL_newstate_hook = null;
            }

            if (lua_close_hook != null)
            {
                lua_close_hook.Uninstall();
                lua_close_hook = null;
            }

            if (luaL_ref_hook != null)
            {
                luaL_ref_hook.Uninstall();
                luaL_ref_hook = null;
            }

            if (luaL_unref_hook != null)
            {
                luaL_unref_hook.Uninstall();
                luaL_unref_hook = null;
            }

            if (luaL_loadbuffer_hook != null)
            {
                luaL_loadbuffer_hook.Uninstall();
                luaL_loadbuffer_hook = null;
            }

            if (lua_error_hook != null)
            {
                lua_error_hook.Uninstall();
                lua_error_hook = null;
            }
        }

        private static bool isBinding = false;
        public static void BindEasyHook(IntPtr module)
        {
            lock (m_Lock)
            {
                if (m_hooked) return;

                if (module == IntPtr.Zero) return;

                if (GetProcAddress(module, "luaopen_jit") != IntPtr.Zero)
                {
                    IS_LUA_JIT = true;
                }

                if (GetProcAddress(module, "lua_rotate") != IntPtr.Zero)
                {
                    LUA_VERSION = 530;
                    // LUA_REGISTRYINDEX == LUAI_FIRSTPSEUDOIDX with LUAI_FIRSTPSEUDOIDX == (-LUAI_MAXSTACK - 1000) with LUAI_MAXSTACK == 15000 (for 32 bits build...)
                    LuaIndexes.LUA_REGISTRYINDEX = -1001000;
                    // starting with Lua 5.2, there is no longer a LUA_GLOBALSINDEX pseudo-index. Instead the global table is stored in the registry at LUA_RIDX_GLOBALS
                    LuaIndexes.LUA_GLOBALSINDEX = 2;
                    IntPtr handle = GetProcAddress(module, "lua_rotate");
                    lua_rotate = (lua_rotate_fun)Marshal.GetDelegateForFunctionPointer(handle, typeof(lua_rotate_fun));
                }
                else if (GetProcAddress(module, "lua_open") != IntPtr.Zero)
                {
                    LUA_VERSION = 500;
                    LuaIndexes.LUA_REGISTRYINDEX = -10000;
                    LuaIndexes.LUA_GLOBALSINDEX = -10001;
                }
                else if (GetProcAddress(module, "lua_callk") != IntPtr.Zero)
                {
                    LUA_VERSION = 520;
                    // LUA_REGISTRYINDEX == LUAI_FIRSTPSEUDOIDX with LUAI_FIRSTPSEUDOIDX == (-LUAI_MAXSTACK - 1000) with LUAI_MAXSTACK == 15000 (for 32 bits build...)
                    LuaIndexes.LUA_REGISTRYINDEX = -1001000;
                    // starting with Lua 5.2, there is no longer a LUA_GLOBALSINDEX pseudo-index. Instead the global table is stored in the registry at LUA_RIDX_GLOBALS
                    LuaIndexes.LUA_GLOBALSINDEX = 2;
                }
                else if (GetProcAddress(module, "lua_gettop") != IntPtr.Zero) // should be ok for any version
                {
                    LUA_VERSION = 510;
                    LuaIndexes.LUA_REGISTRYINDEX = -10000;
                    LuaIndexes.LUA_GLOBALSINDEX = -10002;
                }
                else // if we get here, this means the module isn't related to Lua at all
                {
                    UnityEngine.Debug.Log("no version");
                    return;
                }

                UnityEngine.Debug.Log("lua versin:" + LUA_VERSION);

                if (luaL_newstate_hook == null)
                {
                    IntPtr handle = GetProcAddress(module, "luaL_newstate");
                    luaL_newstate_fun luaFun = new luaL_newstate_fun(luaL_newstate_replace);
                    INativeHooker hooker = nativeUtil.CreateHook();
                    hooker.Init(handle, Marshal.GetFunctionPointerForDelegate(luaFun));
                    hooker.Install();
                    luaL_newstate = (luaL_newstate_fun)hooker.GetProxyFun(typeof(luaL_newstate_fun));
                    luaL_newstate_hook = hooker;
                }

                if (lua_close_hook == null)
                {
                    IntPtr handle = GetProcAddress(module, "lua_close");
                    lua_close_fun luaFun = new lua_close_fun(lua_close_replace);
                    INativeHooker hooker = nativeUtil.CreateHook();
                    hooker.Init(handle, Marshal.GetFunctionPointerForDelegate(luaFun));
                    hooker.Install();
                    lua_close = (lua_close_fun)hooker.GetProxyFun(typeof(lua_close_fun));
                    lua_close_hook = hooker;
                }

                if (lua_gc_hook == null)
                {
                    IntPtr handle = GetProcAddress(module, "lua_gc");
                    lua_gc_fun luaFun = new lua_gc_fun(lua_gc_replace);
                    INativeHooker hooker = nativeUtil.CreateHook();
                    hooker.Init(handle, Marshal.GetFunctionPointerForDelegate(luaFun));
                    hooker.Install();
                    lua_gc = (lua_gc_fun)hooker.GetProxyFun(typeof(lua_gc_fun));
                    lua_gc_hook = hooker;
                }

                // if (lua_error_hook == null)
                // {
                //     IntPtr handle = GetProcAddress(moduleName, "lua_error");
                //     lua_error_fun luaFun = new lua_error_fun(lua_error_replace);
                //     NativeHooker hooker = new NativeHooker(handle, Marshal.GetFunctionPointerForDelegate(luaFun));
                //     hooker.Install();
                //     lua_error = (lua_error_fun)hooker.GetProxyFun(typeof(lua_error_fun));
                //     lua_error_hook = hooker;
                // }

                if (luaL_ref_hook == null)
                {
                    IntPtr handle = GetProcAddress(module, "luaL_ref");
                    luaL_ref_fun luaFun = new luaL_ref_fun(luaL_ref_replace);
                    INativeHooker hooker = nativeUtil.CreateHook();
                    hooker.Init(handle, Marshal.GetFunctionPointerForDelegate(luaFun));
                    hooker.Install();
                    luaL_ref = (luaL_ref_fun)hooker.GetProxyFun(typeof(luaL_ref_fun));
                    luaL_ref_hook = hooker;
                }

                if (luaL_unref_hook == null)
                {
                    IntPtr handle = GetProcAddress(module, "luaL_unref");
                    luaL_unref_fun luaFun = new luaL_unref_fun(luaL_unref_replace);
                    INativeHooker hooker = nativeUtil.CreateHook();
                    hooker.Init(handle, Marshal.GetFunctionPointerForDelegate(luaFun));
                    // release版本 开始的位置有个 test 短跳，占用5字节，如果碰上就直接 跳过这5字节
                    try
                    {
                        hooker.Install();
                    }
                    catch
                    {
                        hooker.Uninstall();
                        hooker = nativeUtil.CreateHook();
                        hooker.Init((IntPtr)((ulong)handle + 5), Marshal.GetFunctionPointerForDelegate(luaFun));
                        hooker.Install();
                    }

                    luaL_unref = (luaL_unref_fun)hooker.GetProxyFun(typeof(luaL_unref_fun));
                    luaL_unref_hook = hooker;
                }

                if (luaL_loadbuffer_hook == null)
                {
                    if (LUA_VERSION > 510)
                    {
                        IntPtr handle = GetProcAddress(module, "luaL_loadbufferx");
                        luaL_loadbufferx_fun luaFun = new luaL_loadbufferx_fun(luaL_loadbufferx_replace);
                        INativeHooker hooker = nativeUtil.CreateHook();
                        hooker.Init(handle, Marshal.GetFunctionPointerForDelegate(luaFun));
                        hooker.Install();
                        luaL_loadbufferx = (luaL_loadbufferx_fun)hooker.GetProxyFun(typeof(luaL_loadbufferx_fun));
                        luaL_loadbuffer_hook = hooker;
                    }
                    else
                    {
                        IntPtr handle = GetProcAddress(module, "luaL_loadbuffer");
                        luaL_loadbuffer_fun luaFun = new luaL_loadbuffer_fun(luaL_loadbuffer_replace);
                        INativeHooker hooker = nativeUtil.CreateHook();
                        hooker.Init(handle, Marshal.GetFunctionPointerForDelegate(luaFun));
                        hooker.Install();
                        luaL_loadbuffer = (luaL_loadbuffer_fun)hooker.GetProxyFun(typeof(luaL_loadbuffer_fun));
                        luaL_loadbuffer_hook = hooker;
                    }
                }

                if (luaL_openlibs_hook == null)
                {
                    IntPtr handle = GetProcAddress(module, "luaL_openlibs");
                    luaL_openlibs_fun luaFun = new luaL_openlibs_fun(luaL_openlibs_replace);
                    INativeHooker hooker = nativeUtil.CreateHook();
                    hooker.Init(handle, Marshal.GetFunctionPointerForDelegate(luaFun));
                    hooker.Install();

                    luaL_openlibs =
                        (luaL_openlibs_fun)hooker.GetProxyFun( typeof(luaL_openlibs_fun));
                    luaL_openlibs_hook = hooker;
                }

                if (LUA_VERSION > 510)
                {
                    {
                        IntPtr handle = GetProcAddress(module, "lua_getglobal");
                        if (handle != IntPtr.Zero)
                        {
                            lua_getglobal =
                                (lua_getglobal_fun)Marshal.GetDelegateForFunctionPointer(handle,
                                    typeof(lua_getglobal_fun));
                        }
                    }
                    {
                        IntPtr handle = GetProcAddress(module, "lua_setglobal");
                        if (handle != IntPtr.Zero)
                        {
                            lua_setglobal =
                                (lua_setglobal_fun)Marshal.GetDelegateForFunctionPointer(handle,
                                    typeof(lua_setglobal_fun));
                        }
                    }
                    {
                        IntPtr handle = GetProcAddress(module, "lua_rawlen");
                        if (handle != IntPtr.Zero)
                        {
                            lua_objlen =
                                (lua_objlen_fun)Marshal.GetDelegateForFunctionPointer(handle, typeof(lua_objlen_fun));
                        }
                    }
                }
                else
                {
                    lua_getglobal = new lua_getglobal_fun(lua_getglobal51);
                    lua_setglobal = new lua_setglobal_fun(lua_setglobal51);
                    {
                        IntPtr handle = GetProcAddress(module, "lua_objlen");
                        if (handle != IntPtr.Zero)
                        {
                            lua_objlen =
                                (lua_objlen_fun)Marshal.GetDelegateForFunctionPointer(handle, typeof(lua_objlen_fun));
                        }
                    }
                }

                {
                    IntPtr handle = GetProcAddress(module, "lua_gettop");
                    if (handle != IntPtr.Zero)
                    {
                        lua_gettop =
                            (lua_gettop_fun)Marshal.GetDelegateForFunctionPointer(handle, typeof(lua_gettop_fun));
                    }
                }

                {
                    IntPtr handle = GetProcAddress(module, "lua_settop");
                    if (handle != IntPtr.Zero)
                    {
                        lua_settop =
                            (lua_settop_fun)Marshal.GetDelegateForFunctionPointer(handle, typeof(lua_settop_fun));
                    }
                }

                {
                    IntPtr handle = GetProcAddress(module, "lua_pushvalue");
                    if (handle != IntPtr.Zero)
                    {
                        lua_pushvalue =
                            (lua_pushvalue_fun)Marshal.GetDelegateForFunctionPointer(handle, typeof(lua_pushvalue_fun));
                    }
                }

                {
                    IntPtr handle = GetProcAddress(module, "lua_remove");
                    if (handle != IntPtr.Zero)
                    {
                        lua_remove =
                            (lua_remove_fun)Marshal.GetDelegateForFunctionPointer(handle, typeof(lua_remove_fun));
                    }
                    else
                    {
                        lua_remove = lua_remove53;
                    }
                }

                {
                    IntPtr handle = GetProcAddress(module, "lua_insert");
                    if (handle != IntPtr.Zero)
                    {
                        lua_insert =
                            (lua_insert_fun)Marshal.GetDelegateForFunctionPointer(handle, typeof(lua_insert_fun));
                    }
                    else
                    {
                        lua_insert = lua_insert53;
                    }
                }

                {
                    IntPtr handle = GetProcAddress(module, "lua_type");
                    if (handle != IntPtr.Zero)
                    {
                        lua_type = (lua_type_fun)Marshal.GetDelegateForFunctionPointer(handle, typeof(lua_type_fun));
                    }
                }

                {
                    if (LUA_VERSION >= 520)
                    {
                        IntPtr handle = GetProcAddress(module, "lua_tonumberx");
                        if (handle != IntPtr.Zero)
                        {
                            var tonumx =
                                (lua_tonumberx_fun)Marshal.GetDelegateForFunctionPointer(handle,
                                    typeof(lua_tonumberx_fun));
                            lua_tonumber = (IntPtr luaState, int idx) => { return tonumx(luaState, idx, IntPtr.Zero); };
                        }
                    }
                    else
                    {
                        IntPtr handle = GetProcAddress(module, "lua_tonumber");
                        if (handle != IntPtr.Zero)
                        {
                            lua_tonumber =
                                (lua_tonumber_fun)Marshal.GetDelegateForFunctionPointer(handle,
                                    typeof(lua_tonumber_fun));
                        }
                    }
                }

                {
                    IntPtr handle = GetProcAddress(module, "lua_pushnil");
                    if (handle != IntPtr.Zero)
                    {
                        lua_pushnil =
                            (lua_pushnil_fun)Marshal.GetDelegateForFunctionPointer(handle, typeof(lua_pushnil_fun));
                    }
                }

                {
                    IntPtr handle = GetProcAddress(module, "lua_pushnumber");
                    if (handle != IntPtr.Zero)
                    {
                        lua_pushnumber =
                            (lua_pushnumber_fun)Marshal.GetDelegateForFunctionPointer(handle,
                                typeof(lua_pushnumber_fun));
                    }
                }

                {
                    IntPtr handle = GetProcAddress(module, "lua_pushstring");
                    if (handle != IntPtr.Zero)
                    {
                        lua_pushstring =
                            (lua_pushstring_fun)Marshal.GetDelegateForFunctionPointer(handle,
                                typeof(lua_pushstring_fun));
                    }
                }

                {
                    IntPtr handle = GetProcAddress(module, "lua_pushboolean");
                    if (handle != IntPtr.Zero)
                    {
                        lua_pushboolean =
                            (lua_pushboolean_fun)Marshal.GetDelegateForFunctionPointer(handle,
                                typeof(lua_pushboolean_fun));
                    }
                }

                {
                    IntPtr handle = GetProcAddress(module, "lua_getfield");
                    if (handle != IntPtr.Zero)
                    {
                        lua_getfield =
                            (lua_getfield_fun)Marshal.GetDelegateForFunctionPointer(handle, typeof(lua_getfield_fun));
                    }
                }

                {
                    IntPtr handle = GetProcAddress(module, "lua_rawget");
                    if (handle != IntPtr.Zero)
                    {
                        lua_rawget =
                            (lua_rawget_fun)Marshal.GetDelegateForFunctionPointer(handle, typeof(lua_rawget_fun));
                    }
                }

                {
                    IntPtr handle = GetProcAddress(module, "lua_rawgeti");
                    if (handle != IntPtr.Zero)
                    {
                        lua_rawgeti =
                            (lua_rawgeti_fun)Marshal.GetDelegateForFunctionPointer(handle, typeof(lua_rawgeti_fun));
                    }
                }

                {
                    IntPtr handle = GetProcAddress(module, "lua_createtable");
                    if (handle != IntPtr.Zero)
                    {
                        lua_createtable =
                            (lua_createtable_fun)Marshal.GetDelegateForFunctionPointer(handle,
                                typeof(lua_createtable_fun));
                    }
                }

                {
                    IntPtr handle = GetProcAddress(module, "lua_setfield");
                    if (handle != IntPtr.Zero)
                    {
                        lua_setfield =
                            (lua_setfield_fun)Marshal.GetDelegateForFunctionPointer(handle, typeof(lua_setfield_fun));
                    }
                }

                {
                    IntPtr handle = GetProcAddress(module, "lua_rawset");
                    if (handle != IntPtr.Zero)
                    {
                        lua_rawset =
                            (lua_rawset_fun)Marshal.GetDelegateForFunctionPointer(handle, typeof(lua_rawset_fun));
                    }
                }

                {
                    if (LUA_VERSION >= 520)
                    {
                        IntPtr handle = GetProcAddress(module, "lua_pcallk");
                        if (handle != IntPtr.Zero)
                        {
                            lua_pcallk =
                                (lua_pcallk_fun)Marshal.GetDelegateForFunctionPointer(handle, typeof(lua_pcallk_fun));
                            lua_pcall = (IntPtr luaState, int nArgs, int nResults, int errfunc) =>
                            {
                                return lua_pcallk(luaState, nArgs, nResults, errfunc, 0, IntPtr.Zero);
                            };
                        }
                    }
                    else
                    {
                        IntPtr handle = GetProcAddress(module, "lua_pcall");
                        if (handle != IntPtr.Zero)
                        {
                            lua_pcall = (lua_pcall_fun)Marshal.GetDelegateForFunctionPointer(handle,
                                typeof(lua_pcall_fun));
                        }
                    }
                }

                {
                    if (LUA_VERSION >= 520)
                    {
                        IntPtr handle = GetProcAddress(module, "lua_callk");
                        if (handle != IntPtr.Zero)
                        {
                            lua_callk =
                                (lua_callk_fun)Marshal.GetDelegateForFunctionPointer(handle, typeof(lua_callk_fun));
                            lua_call = (IntPtr luaState, int nArgs, int nResults) =>
                            {
                                return lua_callk(luaState, nArgs, nResults,  0, IntPtr.Zero);
                            };
                        }
                    }
                    else
                    {
                        IntPtr handle = GetProcAddress(module, "lua_call");
                        if (handle != IntPtr.Zero)
                        {
                            lua_call =
                                (lua_call_fun)Marshal.GetDelegateForFunctionPointer(handle, typeof(lua_call_fun));
                        }
                    }
                }

                {
                    IntPtr handle = GetProcAddress(module, "lua_next");
                    if (handle != IntPtr.Zero)
                    {
                        lua_next = (lua_next_fun)Marshal.GetDelegateForFunctionPointer(handle, typeof(lua_next_fun));
                    }
                }

                {
                    IntPtr handle = GetProcAddress(module, "lua_tolstring");
                    if (handle != IntPtr.Zero)
                    {
                        lua_tolstring =
                            (lua_tolstring_fun)Marshal.GetDelegateForFunctionPointer(handle, typeof(lua_tolstring_fun));
                    }
                }

                {
                    IntPtr handle = GetProcAddress(module, "lua_pushcclosure");
                    if (handle != IntPtr.Zero)
                    {
                        lua_pushcclosure =
                            (lua_pushcclosure_fun)Marshal.GetDelegateForFunctionPointer(handle,
                                typeof(lua_pushcclosure_fun));
                    }
                }
                isHook = true;
                m_hooked = true;
                isBinding = false;
            }
        }

        public static void HookLoadLibrary()
        {
            nativeUtil.HookLoadLibrary((ret) =>
            {
                if (!m_hooked)
                {
                    BindEasyHook(ret);
                }
            });
        }

        private static IntPtr GetProcAddress(IntPtr module, string funName)
        {
            IntPtr result = IntPtr.Zero;
            try
            {
                result = nativeUtil.GetProcAddressByHandle(module, funName);
            }
            catch{}
            return result;
        }
        
        public static IntPtr CheckHasLuaDLL()
        {
            IntPtr result = IntPtr.Zero;
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            var process = Process.GetCurrentProcess();
            var modules = process.Modules;
            foreach (ProcessModule item in modules)
            {
                result = GetProcAddress(item.BaseAddress, "luaL_newstate");
                if (result != IntPtr.Zero)
                {
                    return item.BaseAddress;
                }
            }
#else
            result = GetProcAddress(IntPtr.Zero, "luaL_newstate");
#endif
            return result;
        }

        [MonoPInvokeCallbackAttribute(typeof(luaL_newstate_fun))]
        public static IntPtr luaL_newstate_replace()
        {
			UnityEngine.Debug.Log("test newstate success");
            lock (m_Lock)
            {
                IntPtr intPtr = luaL_newstate();
                if (isHook)
                {
                    MikuLuaProfilerLuaProfilerWrap.RegisterError(intPtr);
                    LuaProfiler.mainL = intPtr;
                    MikuLuaProfilerLuaProfilerWrap.__Register(intPtr);
                }
                return intPtr;
            }
        }

        public static int lua_gc_unhook(IntPtr luaState, LuaGCOptions what, int data)
        {
            var tmp = isHook;
            isHook = false;
            int ret = lua_gc(luaState, what, data);
            isHook = tmp;

            return ret;
        }

        public static void lua_remove53(IntPtr luaState, int idx)
        {
            lua_rotate(luaState, (idx), -1);
            lua_pop(luaState, 1);
        }

        public static void lua_insert53(IntPtr luaState, int idx)
        {
            lua_rotate(luaState, (idx), 1);
        }

        [MonoPInvokeCallbackAttribute(typeof(lua_error_fun))]
        public static int lua_error_replace(IntPtr luaState)
        {
            LuaProfiler.BeginSample(luaState, "exception happen clear stack", true);
            LuaProfiler.PopAllSampleWhenLateUpdate(luaState);
            return lua_error(luaState);
        }

        [MonoPInvokeCallbackAttribute(typeof(lua_gc_fun))]
        public static int lua_gc_replace(IntPtr luaState, LuaGCOptions what, int data)
        {
            lock (m_Lock)
            {
                if (!isHook)
                {
                    return lua_gc(luaState, what, data);
                }
                else if (what == LuaGCOptions.LUA_GCCOUNT)
                {
                    return lua_gc(luaState, what, data);
                }
                else if (what == LuaGCOptions.LUA_GCCOUNTB)
                {
                    return lua_gc(luaState, what, data);
                }
                return 0;
            }
        }

        [MonoPInvokeCallbackAttribute(typeof(lua_close_fun))]
        public static void lua_close_replace(IntPtr luaState)
        {
            lock (m_Lock)
            {
                if (isHook)
                {
                    if (LuaProfiler.mainL == luaState)
                    {
                        LuaProfiler.mainL = IntPtr.Zero;
                    }
                }
                lua_close(luaState);
            }
        }

        [MonoPInvokeCallbackAttribute(typeof(luaL_loadfile_fun))]
        public static int luaL_loadfile_replace(IntPtr luaState, string filename)
        {
            return luaL_loadfile(luaState, filename);
        }

        [MonoPInvokeCallbackAttribute(typeof(luaL_openlibs_fun))]
        public static void luaL_openlibs_replace(IntPtr luaState)
        {
            if (!isOpenLibs)
            {
                luaL_openlibs(luaState);
                isOpenLibs = true;
            }
        }

        [MonoPInvokeCallbackAttribute(typeof(luaL_loadbufferx_fun))]
        public static int luaL_loadbufferx_replace(IntPtr luaState, IntPtr buff, IntPtr size, string name, IntPtr mode)
        {
            lock (m_Lock)
            {
                if (isHook)
                {
                    if (name == null)
                    {
                        name = "chunk";
                    }
                    byte[] buffer = new byte[(int)size];
                    Marshal.Copy(buff, buffer, 0, buffer.Length);

                    // dostring
                    if (name.Length == (int)size)
                    {
                        name = "chunk";
                    }
                    buffer = LuaHook.Hookloadbuffer(luaState, buffer, name);
                    buff = ConvertByteArrayToPtr(buffer);
                    size = (IntPtr)buffer.Length;
                }
                return luaL_loadbufferx(luaState, buff, size, name, mode);
            }
        }

        [MonoPInvokeCallbackAttribute(typeof(lua_callk_fun))]
        public static int lua_callk_replace(IntPtr luaState, int nArgs, int nResults, int ctx, IntPtr k)
        {
            int oldTop = lua_gettop(luaState) - nArgs - 1; //函数调用之前的栈顶索引
            lua_getglobal(luaState, "miku_handle_error");
            lua_insert(luaState, oldTop + 1);
            int result = lua_pcallk(luaState, nArgs, nResults, oldTop + 1, ctx, k);
            if (result == 0)
            {
                lua_remove(luaState, oldTop + 1);
            }
            else
            {
                lua_remove(luaState, oldTop + 1); //errorFunc 出栈
                lua_pop(luaState, 1); //抛出异常
            }

            return result;
        }

        [MonoPInvokeCallbackAttribute(typeof(lua_call_fun))]
        public static int lua_call_replace(IntPtr luaState, int nArgs, int nResults)
        {
            int oldTop = lua_gettop(luaState) - nArgs - 1; //函数调用之前的栈顶索引
            lua_getglobal(luaState, "miku_handle_error");
            lua_insert(luaState, oldTop + 1);
            int result = lua_pcall(luaState, nArgs, nResults, oldTop + 1);
            if (result == 0)
            {
                lua_remove(luaState, oldTop + 1);
            }
            else
            {
                lua_remove(luaState, oldTop + 1); //errorFunc 出栈
                lua_pop(luaState, 1); //抛出异常
            }

            return result;
        }

        [MonoPInvokeCallbackAttribute(typeof(luaL_loadbuffer_fun))]
        public static int luaL_loadbuffer_replace(IntPtr luaState, IntPtr buff, IntPtr size, string name)
        {
            lock (m_Lock)
            {
                if (isHook)
                {
                    if (name == null)
                    {
                        name = "chunk";
                    }
                    byte[] buffer = new byte[(int)size];
                    Marshal.Copy(buff, buffer, 0, buffer.Length);

                    // dostring
                    if (name.Length == (int)size)
                    {
                        name = "chunk";
                    }
                    buffer = LuaHook.Hookloadbuffer(luaState, buffer, name);
                    buff = ConvertByteArrayToPtr(buffer);
                    size = (IntPtr)buffer.Length;
                }
                return luaL_loadbuffer(luaState, buff, size, name);
            }
        }

        [MonoPInvokeCallbackAttribute(typeof(luaL_ref_fun))]
        public static int luaL_ref_replace(IntPtr luaState, int t)
        {
            lock (m_Lock)
            {
                int num = LuaDLL.luaL_ref(luaState, t);
                if (isHook)
                {
                    LuaHook.HookRef(luaState, num);
                }
                return num;
            }
        }

        [MonoPInvokeCallbackAttribute(typeof(luaL_unref_fun))]
        public static void luaL_unref_replace(IntPtr luaState, int registryIndex, int reference)
        {
            lock (m_Lock)
            {
                if (isHook)
                {
                    LuaHook.HookUnRef(luaState, reference);
                }
                LuaDLL.luaL_unref(luaState, registryIndex, reference);
            }
        }
        
        #endregion


        }
    }
#endif
      
      
