using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace HookLib
{
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

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int LuaCSFunction(IntPtr luaState);

    public class LuaDLL
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate IntPtr luaL_newstate_fun();
        public static luaL_newstate_fun luaL_newstate;
        public static IntPtr luaL_newstate_hooked()
        {
            MessageBox.Show("hooked newstate");
            return luaL_newstate();
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void lua_close_fun(IntPtr L);
        public static lua_close_fun lua_close;
        public static void lua_close_hooked(IntPtr L)
        {
            MessageBox.Show("hooked lua_close");
            lua_close(L);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void lua_setglobal_fun(IntPtr L, string name);
        public static lua_setglobal_fun lua_setglobal_dll;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void lua_getglobal_fun(IntPtr L, string name);
        public static lua_getglobal_fun lua_getglobal_dll;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate int lua_gettop_fun(IntPtr L);
        public static lua_gettop_fun lua_gettop;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void lua_settop_fun(IntPtr L, int top);
        public static lua_settop_fun lua_settop;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void lua_pushvalue_fun(IntPtr L, int idx);
        public static lua_pushvalue_fun lua_pushvalue;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void lua_remove_fun(IntPtr L, int idx);
        public static lua_remove_fun lua_remove;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void lua_insert_fun(IntPtr L, int idx);
        public static lua_insert_fun lua_insert;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate LuaTypes lua_type_fun(IntPtr L, int idx);
        public static lua_type_fun lua_type;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate double lua_tonumber_fun(IntPtr L, int idx);
        public static lua_tonumber_fun lua_tonumber;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate double lua_pushnil_fun(IntPtr L);
        public static lua_pushnil_fun lua_pushnil;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void lua_pushnumber_fun(IntPtr L, double number);
        public static lua_pushnumber_fun lua_pushnumber;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void lua_pushstring_fun(IntPtr L, string str);                      //[-0, +1, m]
        public static lua_pushstring_fun lua_pushstring;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void lua_pushboolean_fun(IntPtr luaState, int value);
        public static lua_pushboolean_fun lua_pushboolean;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void lua_getfield_fun(IntPtr L, int idx, string key);
        public static lua_getfield_fun lua_getfield;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void lua_rawget_fun(IntPtr L, int idx);
        public static lua_rawget_fun lua_rawget;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void lua_rawgeti_fun(IntPtr L, int idx, int n);
        public static lua_rawgeti_fun lua_rawgeti;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void lua_createtable_fun(IntPtr L, int narr, int nrec);             //[-0, +1, m]        
        public static lua_createtable_fun lua_createtable;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void lua_setfield_fun(IntPtr L, int idx, string key);
        public static lua_setfield_fun lua_setfield;


        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void lua_rawset_fun(IntPtr L, int idx);                             //[-2, +0, m]       
        public static lua_rawset_fun lua_rawset;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate int lua_pcall_fun(IntPtr L, int nArgs, int nResults, int errfunc);
        public static lua_pcall_fun lua_pcall;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate int lua_gc_fun(IntPtr L, LuaGCOptions what, int data);              //[-0, +0, e]
        public static lua_gc_fun lua_gc;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate int lua_next_fun(IntPtr L, int index);                              //[-1, +(2|0), e]        
        public static lua_next_fun lua_next;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void luaL_openlibs_fun(IntPtr luaState);
        public static luaL_openlibs_fun luaL_openlibs;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate int luaL_ref_fun(IntPtr luaState, int t);                                                  //[-1, +0, m]
        public static luaL_ref_fun luaL_ref;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void luaL_unref_fun(IntPtr luaState, int registryIndex, int reference);
        public static luaL_unref_fun luaL_unref;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void luaL_loadbufferx_fun(IntPtr L, IntPtr buff, IntPtr sz, IntPtr name, IntPtr mode);
        public static luaL_loadbufferx_fun luaL_loadbufferx;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void luaL_loadfilex_fun(IntPtr L, IntPtr filename, IntPtr mode);
        public static luaL_loadfilex_fun luaL_loadfilex;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate IntPtr lua_tolstring_fun(IntPtr luaState, int index, out IntPtr strLen);
        public static lua_tolstring_fun lua_tolstring;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void lua_pushcclosure_fun(IntPtr L, IntPtr fn, int nup);
        public static lua_pushcclosure_fun lua_pushcclosure;

        public static void luaL_initlibs(IntPtr luaState)
        {
            luaL_openlibs(luaState);
#if XLUA
            LuaIndexes.LUA_REGISTRYINDEX = xlua_get_registry_index();
            LuaLib.DoString(luaState, env_script);
#endif
        }

        public static void lua_setglobal(IntPtr L, string name)
        {
            if (lua_setglobal_dll == null)
            {
                lua_setfield(L, -10002, name);
            }
            else
            {
                lua_setglobal_dll(L, name);
            }
        }

        public static void lua_getglobal(IntPtr L, string name)
        {
            if (lua_getglobal_dll == null)
            {
                lua_getfield(L, -10002, name);
            }
            else
            {
                lua_getglobal_dll(L, name);
            }
        }

        public static void lua_pop(IntPtr luaState, int amount)
        {
            lua_settop?.Invoke(luaState, -(amount) - 1);
        }

        public static void lua_newtable(IntPtr luaState)
        {
            lua_createtable?.Invoke(luaState, 0, 0);
        }

        public static bool lua_isfunction(IntPtr luaState, int n)
        {
            return lua_type?.Invoke(luaState, n) == LuaTypes.LUA_TFUNCTION;
        }

        public static bool lua_isnil(IntPtr luaState, int n)
        {
            return lua_type?.Invoke(luaState, n) == LuaTypes.LUA_TNIL;
        }

        public static bool lua_istable(IntPtr luaState, int n)
        {
            return lua_type?.Invoke(luaState, n) == LuaTypes.LUA_TTABLE;
        }

        public static void lua_pushstdcallcfunction(IntPtr luaState, LuaCSFunction func)
        {
            IntPtr fn = Marshal.GetFunctionPointerForDelegate(func);
            lua_pushcclosure(luaState, fn, 0);
        }

    }
}
