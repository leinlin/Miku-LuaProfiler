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

#if UNITY_EDITOR || USE_LUA_PROFILER
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using MikuHook;

#if TOLUA
using OldLuaDLL = LuaInterface.LuaDLL;
#elif XLUA
using OldLuaDLL = XLua.LuaDLL.Lua;
#elif SLUA
using OldLuaDLL = SLua.LuaDLL;
#else
using OldLuaDLL = MikuLuaProfiler.LuaDLL;
#endif


namespace MikuLuaProfiler
{
#region 通用结构体
    public enum LuaTypes
    {
        LUA_TNONE = -1,
        LUA_TNIL = 0,
        LUA_TBOOLEAN = 1,
        LUA_TLIGHTUSERDATA = 2,
        LUA_TNUMBER = 3,
        LUA_TSTRING = 4,
        LUA_TTABLE = 5,
        LUA_TFUNCTION = 6,
        LUA_TUSERDATA = 7,
        LUA_TTHREAD = 8,

    }

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
#if XLUA
        public static int LUA_REGISTRYINDEX { get; set; }
#else
        public static int LUA_REGISTRYINDEX = -10000;
        public static int LUA_ENVIRONINDEX = -10001;
        public static int LUA_GLOBALSINDEX = -10002;
#endif
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

#if !UNITY_EDITOR && UNITY_IPHONE
        const string LUADLL = "__Internal";
#else
#if TOLUA
        const string LUADLL = "tolua";
#elif XLUA
        const string LUADLL = "xlua";
#elif SLUA
        const string LUADLL = "slua";
#else
        const string LUADLL = "lua";
#endif

#endif

#region index
#if XLUA
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xlua_getglobal(IntPtr L, string name);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xlua_setglobal(IntPtr L, string name);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xlua_get_registry_index();
#endif
        public static void lua_setglobal(IntPtr luaState, string name)
        {
#if TOLUA || SLUA
            lua_setfield(luaState, LuaIndexes.LUA_GLOBALSINDEX, name);
#elif XLUA
            xlua_setglobal(luaState, name);
#endif
        }

        public static void lua_getglobal(IntPtr luaState, string name)
        {
#if TOLUA || SLUA
            lua_getfield(luaState, LuaIndexes.LUA_GLOBALSINDEX, name);
#elif XLUA
            xlua_getglobal(luaState, name);
#endif
        }
        public delegate void tolua_getref_fun(IntPtr luaState, int reference);

        public static void lua_getref(IntPtr luaState, int reference)
        {
            lua_rawgeti(luaState, LuaIndexes.LUA_REGISTRYINDEX, reference);
        }

        public static void lua_unref(IntPtr luaState, int reference)
        {
            luaL_unref(luaState, LuaIndexes.LUA_REGISTRYINDEX, reference);
        }
#endregion

#region 通用操作
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr luaL_newstate();

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_close(IntPtr luaState);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_gettop(IntPtr luaState);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_settop(IntPtr luaState, int top);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushvalue(IntPtr luaState, int idx);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_remove(IntPtr luaState, int idx);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_insert(IntPtr luaState, int idx);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern LuaTypes lua_type(IntPtr luaState, int index);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern double lua_tonumber(IntPtr luaState, int idx);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushnil(IntPtr luaState);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushnumber(IntPtr luaState, double number);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushstring(IntPtr luaState, string str);                      //[-0, +1, m]

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushboolean(IntPtr luaState, int value);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_getfield(IntPtr L, int idx, string key);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_rawget(IntPtr luaState, int idx);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_rawgeti(IntPtr luaState, int idx, int n);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_createtable(IntPtr luaState, int narr, int nrec);             //[-0, +1, m]        

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_setfield(IntPtr L, int idx, string key);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_rawset(IntPtr luaState, int idx);                             //[-2, +0, m]       

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_pcall(IntPtr luaState, int nArgs, int nResults, int errfunc);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_gc(IntPtr luaState, LuaGCOptions what, int data);              //[-0, +0, e]
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_next(IntPtr luaState, int index);                              //[-1, +(2|0), e]        
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
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void luaL_openlibs(IntPtr luaState);
        public static void luaL_initlibs(IntPtr luaState)
        {
            luaL_openlibs(luaState);
#if XLUA
            LuaIndexes.LUA_REGISTRYINDEX = xlua_get_registry_index();
            LuaLib.DoString(luaState, env_script);
#endif
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

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaL_ref(IntPtr luaState, int t);                                                  //[-1, +0, m]
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void luaL_unref(IntPtr luaState, int registryIndex, int reference);
#if TOLUA || SLUA || XLUA
#if TOLUA
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
#elif SLUA
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "luaLS_loadbuffer")]
#elif XLUA
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xluaL_loadbuffer")]
#endif
        public static extern int luaL_loadbuffer(IntPtr luaState, byte[] buff, IntPtr size, string name);
#else
        public static int luaL_loadbuffer(IntPtr luaState, byte[] buff, IntPtr size, string name)
        {
            return 1;
        }
#endif
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
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr lua_tolstring(IntPtr luaState, int index, out IntPtr strLen);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushcclosure(IntPtr L, IntPtr fn, int nup);

        public static void lua_pushstdcallcfunction(IntPtr luaState, LuaCSFunction func)
        {
            IntPtr fn = Marshal.GetFunctionPointerForDelegate(func);
            lua_pushcclosure(luaState, fn, 0);
        }
#endregion

#region mono hook

        private static CSharpMethodHooker luaL_newstate_hooker;
        private static CSharpMethodHooker lua_close_hooker;

        private static CSharpMethodHooker lua_load_buffer_hooker;
        private static CSharpMethodHooker luaL_ref_hooker;
        private static CSharpMethodHooker luaL_unref_hooker;

#if TOLUA
        private static CSharpMethodHooker tolua_ref_hooker;
        private static CSharpMethodHooker tolua_unref_hooker;
#endif
        private static bool m_hooked = false;
        private static object m_Lock = 1;
        public static void Install()
        {
#if TOLUA || XLUA || SLUA
            if (m_hooked) return;

            Type oldType = typeof(OldLuaDLL);
            Type replaceType = typeof(LuaDLL);
            if (luaL_newstate_hooker == null)
            {
                luaL_newstate_hooker = BindHook(oldType, replaceType, "luaL_newstate", "luaL_newstate_replace");
            }

            if (lua_close_hooker == null)
            {
                lua_close_hooker = BindHook(oldType, replaceType, "lua_close", "lua_close_replace");
            }

            if (luaL_ref_hooker == null)
            {
                luaL_ref_hooker = BindHook(oldType, replaceType, "luaL_ref", "luaL_ref_replace");
            }

            if (luaL_unref_hooker == null)
            {
                luaL_unref_hooker = BindHook(oldType, replaceType, "luaL_unref", "luaL_unref_replace");
            }


            if (lua_load_buffer_hooker == null)
            {
#if TOLUA
                lua_load_buffer_hooker = BindHook(oldType, replaceType, "tolua_loadbuffer", "luaL_loadbuffer_replace");
#elif XLUA
                lua_load_buffer_hooker = BindHook(oldType, replaceType, "xluaL_loadbuffer", "luaL_loadbuffer_replace");
#elif SLUA
                oldType = typeof(SLua.LuaDLLWrapper);
                lua_load_buffer_hooker = BindHook(oldType, replaceType, "luaLS_loadbuffer", "luaL_loadbuffer_replace");
#endif
            }
#if TOLUA
            if (tolua_ref_hooker == null)
            {
                MethodInfo oldFun = oldType.GetMethod("toluaL_ref");
                MethodInfo replaceFun = replaceType.GetMethod("toluaL_ref_replace");
                MethodInfo proxyFun = replaceType.GetMethod("toluaL_ref_proxy");

                tolua_ref_hooker = CSharpMethodHooker.HookCSMethod(oldFun, replaceFun, proxyFun);
            }

            if (tolua_unref_hooker == null)
            {
                MethodInfo oldFun = oldType.GetMethod("toluaL_unref");
                MethodInfo replaceFun = replaceType.GetMethod("toluaL_unref_replace");
                MethodInfo proxyFun = replaceType.GetMethod("toluaL_unref_proxy");

                tolua_unref_hooker = CSharpMethodHooker.HookCSMethod(oldFun, replaceFun, proxyFun);
            }
#endif
            m_hooked = true;
#endif
            }

        public static void UnInstall()
        {
#if TOLUA || XLUA || SLUA
            if (luaL_newstate_hooker != null)
            {
                luaL_newstate_hooker.Uninstall();
                luaL_newstate_hooker = null;
            }

            if (lua_close_hooker != null)
            {
                lua_close_hooker.Uninstall();
                lua_close_hooker = null;
            }

            if (lua_load_buffer_hooker != null)
            {
                lua_load_buffer_hooker.Uninstall();
                lua_load_buffer_hooker = null;
            }

            if (luaL_ref_hooker != null)
            {
                luaL_ref_hooker.Uninstall();
                luaL_ref_hooker = null;
            }

            if (luaL_unref_hooker != null)
            {
                luaL_unref_hooker.Uninstall();
                luaL_unref_hooker = null;
            }

#if TOLUA
            if (tolua_ref_hooker != null)
            {
                tolua_ref_hooker.Uninstall();
                tolua_ref_hooker = null;
            }

            if (tolua_unref_hooker != null)
            {
                tolua_unref_hooker.Uninstall();
                tolua_unref_hooker = null;
            }
#endif
            m_hooked = false;
#endif
        }
        private static CSharpMethodHooker BindHook(Type oldType, Type replaceType, string oldName, string replaceName)
        {
            MethodInfo oldFun = null;
            if (oldName == "luaL_ref")
            {
                oldFun = oldType.GetMethod(oldName, new Type[] { typeof(IntPtr), typeof(int) });
            }
            else if (oldName == "luaL_unref")
            {
                oldFun = oldType.GetMethod(oldName, new Type[] { typeof(IntPtr), typeof(int), typeof(int) });
            }
            else
            {
                oldFun = oldType.GetMethod(oldName);
            }
            MethodInfo replaceFun = replaceType.GetMethod(replaceName);

            CSharpMethodHooker result = CSharpMethodHooker.HookCSMethod(oldFun, replaceFun);

            return result;
        }

        public static IntPtr luaL_newstate_replace()
        {
            lock (m_Lock)
            {
                IntPtr intPtr = LuaDLL.luaL_newstate();
                LuaProfiler.mainL = intPtr;
                MikuLuaProfilerLuaProfilerWrap.__Register(intPtr);
                return intPtr;
            }
        }

        public static void lua_close_replace(IntPtr luaState)
        {
            lock (m_Lock)
            {
                if (LuaProfiler.mainL == luaState)
                {
                    LuaProfiler.mainL = IntPtr.Zero;
                }
                LuaDLL.lua_close(luaState);
            }
        }

        public static int luaL_loadbuffer_replace(IntPtr luaState, byte[] buff, int size, string name)
        {
#if TOLUA || XLUA || SLUA
            lock (m_Lock)
            {
                buff = LuaHook.Hookloadbuffer(luaState, buff, name);
                return LuaDLL.luaL_loadbuffer(luaState, buff, (IntPtr)(buff.Length), name);
            }
#else
            return 1;
#endif
        }

        public static int luaL_ref_replace(IntPtr luaState, int t)
        {
            lock (m_Lock)
            {
                int num = LuaDLL.luaL_ref(luaState, t);
                LuaHook.HookRef(luaState, num);
                return num;
            }
        }

        public static void luaL_unref_replace(IntPtr luaState, int registryIndex, int reference)
        {
            lock (m_Lock)
            {
                LuaHook.HookUnRef(luaState, reference);
                LuaDLL.luaL_unref(luaState, registryIndex, reference);
            }
        }

        public static tolua_getref_fun tolua_Getref_Fun = new tolua_getref_fun(toluaL_get_ref);
        public static void toluaL_get_ref(IntPtr L, int reference)
        {
            // _R[5] table放到栈顶
            lua_getref(L, 5);
            lua_rawgeti(L, -1, reference);
        }

        public static int toluaL_ref_replace(IntPtr L)
        {
            lock (m_Lock)
            {
                int num = toluaL_ref_proxy(L);
                LuaHook.HookRef(L, num, tolua_Getref_Fun);
                return num;
            }
        }

        public static int toluaL_ref_proxy(IntPtr L)
        {
            for (int i = 0, imax = 250;i<imax;i++)
            {
                UnityEngine.Debug.Log("fuck misaka");
            }
            return 0;
        }

        public static void toluaL_unref_replace(IntPtr L, int reference)
        {
            lock (m_Lock)
            {
                LuaHook.HookUnRef(L, reference, tolua_Getref_Fun);
                toluaL_unref_proxy(L, reference);
            }
        }

        public static void toluaL_unref_proxy(IntPtr L, int reference)
        {
            for (int i = 0, imax = 250; i < imax; i++)
            {
                UnityEngine.Debug.Log("fuck misaka");
            }
        }
        #endregion


    }
    }
#endif