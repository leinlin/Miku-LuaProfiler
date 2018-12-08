#define XLUA
/*
* ==============================================================================
* Filename: LuaHookSetup
* Created:  2018/7/2 11:36:16
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/
//#define XLUA
//#define TOLUA
//#define SLUA

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using System.IO;

#if XLUA
using XLua;
using LuaDLL = XLua.LuaDLL.Lua;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
using StrLen = System.IntPtr;
#elif TOLUA
using LuaInterface;
using LuaDLL = LuaInterface.LuaDLL;
using LuaCSFunction = LuaInterface.LuaCSFunction;
using StrLen = System.Int32;
#elif SLUA
using SLua;
using LuaDLL = SLua.LuaDLL;
using LuaCSFunction = SLua.LuaCSFunction;
using StrLen = System.Int32;
#endif

namespace MikuLuaProfiler
{

    [InitializeOnLoad]
    public static class HookLuaUtil
    {
        private static MethodHooker hookNewLuaEnv;
        public static int frameCount { private set; get; }
        public static bool isPlaying { private set; get; }
        static HookLuaUtil()
        {
            if (hookNewLuaEnv == null)
            {
                Type envReplace = typeof(LuaEnvReplace);

                Type typeDll = typeof(LuaDLL);
                var newstateFun = typeDll.GetMethod("luaL_newstate");
                var clickReplace = envReplace.GetMethod("luaL_newstate");
                var clickProxy = envReplace.GetMethod("ProxyNewstate", BindingFlags.Public | BindingFlags.Static);
                hookNewLuaEnv = new MethodHooker(newstateFun, clickReplace, clickProxy);
                hookNewLuaEnv.Install();

                newstateFun = typeDll.GetMethod("lua_close");
                clickReplace = envReplace.GetMethod("lua_close");
                clickProxy = envReplace.GetMethod("ProxyClose", BindingFlags.Public | BindingFlags.Static);
                hookNewLuaEnv = new MethodHooker(newstateFun, clickReplace, clickProxy);
                hookNewLuaEnv.Install();

                if (LuaDeepProfilerSetting.Instance.isDeepProfiler && LuaDeepProfilerSetting.Instance.profilerMono)
                {
                    InjectMethods.InjectAllMethods();
                }

                EditorApplication.update += () =>
                {
                    frameCount = Time.frameCount;
                };
            }

        }

        public static class LuaEnvReplace
        {
            public static void lua_close(IntPtr luaState)
            {
                isPlaying = false;
                if (LuaProfiler.mainL == luaState)
                {
                    LuaProfiler.mainL = IntPtr.Zero;
                    UnInstall();
                }
                ProxyClose(luaState);
            }

            public static void ProxyClose(IntPtr luaState)
            {
            }

            public static IntPtr luaL_newstate()
            {
                IntPtr l = ProxyNewstate();
                LuaProfiler.mainL = l;
                if (LuaDeepProfilerSetting.Instance.isDeepProfiler)
                {
                    isPlaying = true;
                    MikuLuaProfilerLuaProfilerWrap.__Register(l);
                    Install();
                    if (LuaDeepProfilerSetting.Instance.isRecord)
                    {
                        GameViewUtility.ChangeGameViewSize(480, 270);
                    }
                }
                return l;
            }
            public static IntPtr ProxyNewstate()
            {
                return IntPtr.Zero;
            }
        }

        #region hook when run
        private static MethodHooker tostringHook;
        private static MethodHooker loaderHook;

        private static bool m_hooked = false;
        private static void Install()
        {
            if (m_hooked) return;
            if (tostringHook == null)
            {
                Type typeLogReplace = typeof(LuaDLLReplace);
                Type typeLog = typeof(LuaDLL);
#if XLUA
                MethodInfo tostringFun = typeLog.GetMethod("xluaL_loadbuffer");
#else
                MethodInfo tostringFun = typeLog.GetMethod("luaL_loadbuffer", new Type[] { typeof(IntPtr), typeof(byte[]), typeof(int), typeof(string) });
#endif
                MethodInfo tostringReplace = typeLogReplace.GetMethod("luaL_loadbuffer");
                MethodInfo tostringProxy = typeLogReplace.GetMethod("ProxyLoadbuffer");
                tostringHook = new MethodHooker(tostringFun, tostringReplace, tostringProxy);
                tostringHook.Install();

                tostringFun = typeLog.GetMethod("lua_tostring");
                tostringReplace = typeLogReplace.GetMethod("lua_tostring");
                tostringProxy = typeLogReplace.GetMethod("PoxyToString");
                tostringHook = new MethodHooker(tostringFun, tostringReplace, tostringProxy);
                tostringHook.Install();
            }

            m_hooked = true;
        }

        public static void UnInstall()
        {
            if (tostringHook != null)
            {
                tostringHook.Uninstall();
                tostringHook = null;
            }
            if (loaderHook != null)
            {

                loaderHook.Uninstall();
                loaderHook = null;
            }

            m_hooked = false;
        }

        public class LuaDLLReplace
        {
            #region luastring
            public static readonly Dictionary<long, string> stringDict = new Dictionary<long, string>();
            public static bool TryGetLuaString(IntPtr p, out string result)
            {

                return stringDict.TryGetValue((long)p, out result);
            }
            public static void RefString(IntPtr strPoint, int index, string s, IntPtr L)
            {
                int oldTop = LuaDLL.lua_gettop(L);
                LuaDLL.lua_pushvalue(L, index);
                //把字符串ref了之后就不GC了
                LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
                LuaDLL.lua_settop(L, oldTop);
                stringDict[(long)strPoint] = s;
            }
            #endregion

            #region hook fun
            public static int luaL_loadbuffer(IntPtr L, byte[] buff, int size, string name)
            {
                if (LuaDeepProfilerSetting.Instance.isDeepProfiler)//&& name != "chunk"
                {
                    string value = "";
                    string hookedValue = "";
                    try
                    {
                        string fileName = name.Replace(".lua", "");
                        fileName = fileName.Replace("@", "").Replace('.', '/');
                        if (buff[0] == 239 && buff[1] == 187 && buff[2] == 191)
                        {// utf-8
                            value = Encoding.UTF8.GetString(buff, 3, buff.Length - 3);
                        }
                        else
                        {
                            value = Encoding.UTF8.GetString(buff);
                        }
                        
                        hookedValue = Parse.InsertSample(value, fileName);

                        //System.IO.File.WriteAllText(fileName, value);

                        buff = Encoding.UTF8.GetBytes(hookedValue);
                        size = buff.Length;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(name + "\n解析出错:\n" + value);
                        throw e;
                    }
                }

                return ProxyLoadbuffer(L, buff, size, name);
            }

            public static int ProxyLoadbuffer(IntPtr L, byte[] buff, int size, string name)
            {
                return 0;
            }

            public static string lua_tostring(IntPtr L, int index)
            {
                StrLen strlen;

#if SLUA
                IntPtr str = LuaDLL.luaS_tolstring32(L, index, out strlen);
#else
                IntPtr str = LuaDLL.lua_tolstring(L, index, out strlen);
#endif
                string ret;
                if (!TryGetLuaString(str, out ret))
                {
                    ret = PoxyToString(L, index);
                    RefString(str, index, ret, L);
                }
                return ret;
            }

            public static string PoxyToString(IntPtr L, int index)
            {
                Debug.Log("ffffffffff");
                return null;
            }
            #endregion
        }
    #endregion
    }

    public class LuaLib
    {
#region memory
        public static void RunGC()
        {
            var env = LuaProfiler.mainL;
            if (env != IntPtr.Zero)
            {
                LuaDLL.lua_gc(env, LuaGCOptions.LUA_GCCOLLECT, 0);
            }
        }
        public static void StopGC()
        {
            var env = LuaProfiler.mainL;
            if (env != IntPtr.Zero)
            {
                LuaDLL.lua_gc(env, LuaGCOptions.LUA_GCSTOP, 0);
            }
        }
        public static void ResumeGC()
        {
            var env = LuaProfiler.mainL;
            if (env != IntPtr.Zero)
            {
                LuaDLL.lua_gc(env, LuaGCOptions.LUA_GCRESTART, 0);
            }
        }
        public static long GetLuaMemory(IntPtr luaState)
        {
            long result = 0;

            if (luaState != IntPtr.Zero)
            {
                result = LuaDLL.lua_gc(luaState, LuaGCOptions.LUA_GCCOUNT, 0);
                result = result * 1024 + LuaDLL.lua_gc(luaState, LuaGCOptions.LUA_GCCOUNTB, 0);
            }

            return result;
        }
#endregion

        public static void lua_pushstdcallcfunction(IntPtr L, LuaCSFunction fun)
        {
#if XLUA
            LuaDLL.lua_pushstdcallcfunction(L, fun);
#elif TOLUA
            LuaDLL.tolua_pushcfunction(L, fun);
#elif SLUA
            LuaDLL.lua_pushcfunction(L, fun);
#endif
        }

        public static void lua_setglobal(IntPtr L, string name)
        {
#if XLUA
            LuaDLL.xlua_setglobal(L, name);
#elif TOLUA
            LuaDLL.lua_setglobal(L, name);
#elif SLUA
            LuaDLL.lua_setglobal(L, name);
#endif
        }

        public static void lua_getglobal(IntPtr L, string name)
        {
#if XLUA
            LuaDLL.xlua_getglobal(L, name);
#elif TOLUA
            LuaDLL.lua_getglobal(L, name);
#elif SLUA
            LuaDLL.lua_getglobal(L, name);
#endif
        }
    }

    #region bind

    public class MikuLuaProfilerLuaProfilerWrap
    {
        public static void __Register(IntPtr L)
        {
            LuaDLL.lua_newtable(L);
            LuaDLL.lua_pushstring(L, "LuaProfiler");
            LuaDLL.lua_newtable(L);

            LuaDLL.lua_pushstring(L, "BeginSample");

            LuaLib.lua_pushstdcallcfunction(L, BeginSample);
            LuaDLL.lua_rawset(L, -3);

            LuaDLL.lua_pushstring(L, "EndSample");
            LuaLib.lua_pushstdcallcfunction(L, EndSample);
            LuaDLL.lua_rawset(L, -3);

            LuaDLL.lua_rawset(L, -3);
            LuaLib.lua_setglobal(L, "MikuLuaProfiler");

            LuaLib.lua_pushstdcallcfunction(L, UnpackReturnValue);
            LuaLib.lua_setglobal(L, "miku_unpack_return_value");

#if XLUA
            DoString(L);
#endif
        }
#if XLUA
        private static void DoString(IntPtr L)
        {
            const string script = @"
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
            int oldTop = LuaDLL.lua_gettop(L);
            int errFunc = LuaDLL.load_error_func(L, -1);
            byte[] chunk = Encoding.UTF8.GetBytes(script);
            if (LuaDLL.xluaL_loadbuffer(L, chunk, chunk.Length, "env") == 0)
            {
                if (LuaDLL.lua_pcall(L, 0, -1, errFunc) == 0)
                {
                    LuaDLL.lua_remove(L, errFunc);
                }
            }
            LuaDLL.lua_settop(L, oldTop);
        }
#endif
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int BeginSample(IntPtr L)
        {
            string _name = LuaDLL.lua_tostring(L, 1);
            MikuLuaProfiler.LuaProfiler.BeginSample(L, _name);
            return 0;
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int UnpackReturnValue(IntPtr L)
        {
            MikuLuaProfiler.LuaProfiler.EndSample(L);
            return LuaDLL.lua_gettop(L);
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int EndSample(IntPtr L)
        {
            MikuLuaProfiler.LuaProfiler.EndSample(L);
            return 0;
        }
    }
    #endregion
}

#endif