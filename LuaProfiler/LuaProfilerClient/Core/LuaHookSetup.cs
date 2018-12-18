#define XLUA
/*
* ==============================================================================
* Filename: LuaHookSetup
* Created:  2018/7/2 11:36:16
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/

#if UNITY_EDITOR || USE_LUA_PROFILER
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

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

    public class HookLuaSetup : MonoBehaviour
    {
        #region field
        private static MethodHooker hookNewLuaEnv;
        public static int frameCount { private set; get; }
        public static bool isPlaying = false;
        private static MethodHooker tostringHook;
        private static MethodHooker loaderHook;
        private static bool m_hooked = false;
        #endregion

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnStartGame()
        {
            var setting = LuaDeepProfilerSetting.Instance;

#if XLUA || TOLUA || SLUA
            if (hookNewLuaEnv == null && setting.isDeepLuaProfiler)
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
            }
#endif
            if (LuaDeepProfilerSetting.Instance.isNeedCapture)
            {
                Screen.SetResolution(480, 270, true);
            }

            if (setting.isDeepLuaProfiler || setting.isDeepMonoProfiler)
            {
                GameObject go = new GameObject();
                go.name = "MikuLuaProfiler";
                //go.hideFlags = HideFlags.HideAndDontSave;
                go.AddComponent<HookLuaSetup>();
                NetWorkClient.ConnectServer(setting.ip, setting.port);
            }

        }

        private void Update()
        {
            isPlaying = Application.isPlaying;
            frameCount = Time.frameCount;
        }

        private void OnApplicationQuit()
        {
            NetWorkClient.Close();
        }

        #region hook when run
        private static void Install()
        {
#if XLUA || TOLUA || SLUA
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
#endif
        }

        private static void UnInstall()
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

        public static class LuaEnvReplace
        {
            public static void lua_close(IntPtr luaState)
            {
                if (LuaProfiler.mainL == luaState)
                {
                    LuaProfiler.mainL = IntPtr.Zero;
                    UnInstall();
                }
                Debug.Log("hooked");
                ProxyClose(luaState);
            }

            public static void ProxyClose(IntPtr luaState)
            {
            }

            public static IntPtr luaL_newstate()
            {
#if XLUA || TOLUA || SLUA
                IntPtr l = ProxyNewstate();
                LuaProfiler.mainL = l;
                MikuLuaProfilerLuaProfilerWrap.__Register(l);
                Install();
                return l;
#else
                return IntPtr.Zero;
#endif
            }
            public static IntPtr ProxyNewstate()
            {
                return IntPtr.Zero;
            }
        }
        #endregion
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
#if XLUA || TOLUA || SLUA
            int oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_pushvalue(L, index);
            //把字符串ref了之后就不GC了
            LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
            LuaDLL.lua_settop(L, oldTop);
            stringDict[(long)strPoint] = s;
#endif
        }
        #endregion

        #region hook fun
        public static int luaL_loadbuffer(IntPtr L, byte[] buff, int size, string name)
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

            return ProxyLoadbuffer(L, buff, size, name);
        }

        public static int ProxyLoadbuffer(IntPtr L, byte[] buff, int size, string name)
        {
            return 0;
        }

        public static string lua_tostring(IntPtr L, int index)
        {
#if XLUA || TOLUA || SLUA
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
                if (!string.IsNullOrEmpty(ret))
                {
                    ret = string.Intern(ret);
                }
                RefString(str, index, ret, L);
            }
            return ret;
#else
                return "";
#endif
        }

        public static string PoxyToString(IntPtr L, int index)
        {
            Debug.Log("ffffffffff");
            return null;
        }
        #endregion
    }

    public class LuaLib
    {
        #region memory
        public static void RunGC()
        {
#if XLUA || TOLUA || SLUA
            var env = LuaProfiler.mainL;
            if (env != IntPtr.Zero)
            {
                LuaDLL.lua_gc(env, LuaGCOptions.LUA_GCCOLLECT, 0);
            }
#endif
        }
        public static void StopGC()
        {
#if XLUA || TOLUA || SLUA
            var env = LuaProfiler.mainL;
            if (env != IntPtr.Zero)
            {
                LuaDLL.lua_gc(env, LuaGCOptions.LUA_GCSTOP, 0);
            }
#endif
        }
        public static void ResumeGC()
        {
#if XLUA || TOLUA || SLUA
            var env = LuaProfiler.mainL;
            if (env != IntPtr.Zero)
            {
                LuaDLL.lua_gc(env, LuaGCOptions.LUA_GCRESTART, 0);
            }
#endif
        }
        public static long GetLuaMemory(IntPtr luaState)
        {
            long result = 0;
#if XLUA || TOLUA || SLUA
            if (luaState != IntPtr.Zero)
            {
                result = LuaDLL.lua_gc(luaState, LuaGCOptions.LUA_GCCOUNT, 0);
                result = result * 1024 + LuaDLL.lua_gc(luaState, LuaGCOptions.LUA_GCCOUNTB, 0);
            }
#endif
            return result;
        }
        #endregion

#if XLUA || TOLUA || SLUA
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
#endif
    }

#if XLUA || TOLUA || SLUA
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
#endif
}

#endif