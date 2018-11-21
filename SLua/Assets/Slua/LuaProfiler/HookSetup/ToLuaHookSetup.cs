/*
* ==============================================================================
* Filename: ToLuaHookSetup
* Created:  2018/7/2 11:36:16
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/

//#define ToLua

#if ToLua
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using LuaInterface;
using LuaDLL = LuaInterface.LuaDLL;

namespace MikuLuaProfiler
{

    public class LuaLib
    {
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

            result = LuaDLL.lua_gc(luaState, LuaGCOptions.LUA_GCCOUNT, 0);
            result = result * 1024 + LuaDLL.lua_gc(luaState, LuaGCOptions.LUA_GCCOUNTB, 0);

            return result;
        }
    }

    [InitializeOnLoad]
    public static class Startup
    {
        private static MethodHooker hookNewLuaEnv;

        public static readonly string luaPath;
        static Startup()
        {
            HookNewLuaEnv();
        }

        public static void HookNewLuaEnv()
        {
            if (hookNewLuaEnv == null)
            {
                Type envReplace = typeof(LuaEnvReplace);
                Type typeEnv = typeof(LuaState);
                var clickFun = typeEnv.GetMethod("Start");
                MethodInfo clickReplace = envReplace.GetMethod("Start");
                MethodInfo clickProxy = envReplace.GetMethod("Proxy", BindingFlags.Public | BindingFlags.Static);
                hookNewLuaEnv = new MethodHooker(clickFun, clickReplace, clickProxy);
                hookNewLuaEnv.Install();

                Type typeDll = typeof(LuaDLL);
                var newstateFun = typeDll.GetMethod("luaL_newstate");
                clickReplace = envReplace.GetMethod("luaL_newstate");
                clickProxy = envReplace.GetMethod("ProxyNewstate", BindingFlags.Public | BindingFlags.Static);
                hookNewLuaEnv = new MethodHooker(newstateFun, clickReplace, clickProxy);
                hookNewLuaEnv.Install();

                newstateFun = typeDll.GetMethod("lua_close");
                clickReplace = envReplace.GetMethod("lua_close");
                clickProxy = envReplace.GetMethod("ProxyClose", BindingFlags.Public | BindingFlags.Static);
                hookNewLuaEnv = new MethodHooker(newstateFun, clickReplace, clickProxy);
                hookNewLuaEnv.Install();
            }
        }

        public static class LuaEnvReplace
        {
            public static void Start(LuaState env)
            {
                HookSetup.Uninstall();
                Proxy(env);
                HookSetup.SetMainLuaEnv(env);
            }
            public static void Proxy(LuaState env)
            {
            }

            public static void lua_close(IntPtr luaState)
            {
                if (LuaProfiler.mainL == luaState)
                {
                    LuaProfiler.mainL = IntPtr.Zero;
                    HookSetup.Uninstall();
                }
            }
            public static void ProxyClose(IntPtr luaState)
            {
            }

            public static IntPtr luaL_newstate()
            {
                IntPtr l = ProxyNewstate();
                MikuLuaProfiler.LuaProfiler.mainL = l;
                return l;
            }
            public static IntPtr ProxyNewstate()
            {
                return IntPtr.Zero;
            }
        }

    }

    [InitializeOnLoad]
    static class HookSetup
    {
#if !UNITY_2017_1_OR_NEWER
        static bool isPlaying = false;
#endif
        static HookSetup()
        {
#if UNITY_2017_1_OR_NEWER
            EditorApplication.playModeStateChanged += OnEditorPlaying;
#else
            EditorApplication.playmodeStateChanged += () =>
            {

                if (isPlaying == true && EditorApplication.isPlaying == false)
                {
                    SetMainLuaEnv(null);
                }

                isPlaying = EditorApplication.isPlaying;
            };
#endif
        }
        public const string LOCAL_PROFILER = @"
local BeginMikuSample = MikuLuaProfiler.LuaProfiler.BeginSample
local EndMikuSample = MikuLuaProfiler.LuaProfiler.EndSample

local function miku_unpack_return_value(...)
    EndMikuSample()
    return ...
end
";

        public static void SetMainLuaEnv(LuaState env)
        {
            if (LuaDeepProfilerSetting.Instance.isDeepProfiler)
            {
                if (env != null)
                {
                    env.BeginModule(null);
                    env.BeginModule("MikuLuaProfiler");
                    MikuLuaProfiler_LuaProfilerWrap.Register(env);
                    env.EndModule();
                    env.EndModule();
                    HookSetup.HookLuaFuns();
                }
            }

            if (env == null)
            {
                HookSetup.Uninstall();
                LuaProfiler.mainL = IntPtr.Zero;
            }
        }

#if UNITY_2017_1_OR_NEWER
        public static void OnEditorPlaying(PlayModeStateChange playModeStateChange)
        {
            if (playModeStateChange == PlayModeStateChange.ExitingPlayMode)
            {
                SetMainLuaEnv(null);
            }
        }
#endif

#region hook

#region hook tostring

        public class LuaDll
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

            public static int luaL_loadbuffer(IntPtr L, byte[] buff, int size, string name)
            {
                if (LuaDeepProfilerSetting.Instance.isDeepProfiler)//&& name != "chunk"
                {
                    string value = "";
                    string hookedValue = "";
                    try
                    {
                        var utf8WithoutBom = new System.Text.UTF8Encoding(true);
                        string fileName = name.Replace("@", "").Replace("/", ".") + ".lua";
                        value = utf8WithoutBom.GetString(buff);
                        hookedValue = Parse.InsertSample(value, fileName);

                        //System.IO.File.WriteAllText(fileName, value);

                        buff = utf8WithoutBom.GetBytes(hookedValue);
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
                int len = 0;
                IntPtr str = LuaDLL.tolua_tolstring(L, index, out len);

                if (str != IntPtr.Zero)
                {
                    string s;
                    if (!TryGetLuaString(str, out s))
                    {
                        s = LuaDLL.lua_ptrtostring(str, len);
                    }
                    return s;
                }

                return null;
            }

            public static string PoxyToString(IntPtr L, int index)
            {
                return null;
            }
        }
#endregion


#region hook profiler
        public class Profiler
        {
            private static Stack<string> m_Stack = new Stack<string>();
            private static int m_currentFrame = 0;
            public static void BeginSampleOnly(string name)
            {
                if (ProfilerDriver.deepProfiling) return;
                if (Time.frameCount != m_currentFrame)
                {
                    m_Stack.Clear();
                    m_currentFrame = Time.frameCount;
                }
                m_Stack.Push(name);
                ProxyBeginSample(name);
            }
            public static void BeginSample(string name, UnityEngine.Object targetObject)
            {
                if (ProfilerDriver.deepProfiling) return;
                m_Stack.Push(name);
                ProxyBeginSample(name, targetObject);
            }

            public static void EndSample()
            {
                if (ProfilerDriver.deepProfiling) return;
                if (m_Stack.Count <= 0)
                {
                    return;
                }
                m_Stack.Pop();
                ProxyEndSample();
            }

            public static void ProxyBeginSample(string name)
            {
            }
            public static void ProxyBeginSample(string name, UnityEngine.Object targetObject)
            {
            }

            public static void ProxyEndSample()
            {
            }
        }
#endregion

#region do hook
        private static MethodHooker beginSampeOnly;
        private static MethodHooker beginObjetSample;
        private static MethodHooker endSample;
        private static MethodHooker tostringHook;
        private static MethodHooker loaderHook;

        private static bool m_hooked = false;
        public static void HookLuaFuns()
        {
            if (m_hooked) return;
            if (tostringHook == null)
            {
                Type typeLogReplace = typeof(LuaDll);
                Type typeLog = typeof(LuaDLL);
                MethodInfo tostringFun = typeLog.GetMethod("lua_tostring");
                MethodInfo tostringReplace = typeLogReplace.GetMethod("lua_tostring");
                MethodInfo tostringProxy = typeLogReplace.GetMethod("ProxyToString");

                tostringHook = new MethodHooker(tostringFun, tostringReplace, tostringProxy);
                tostringHook.Install();

                tostringFun = typeLog.GetMethod("luaL_loadbuffer");
                tostringReplace = typeLogReplace.GetMethod("luaL_loadbuffer");
                tostringProxy = typeLogReplace.GetMethod("ProxyLoadbuffer");

                tostringHook = new MethodHooker(tostringFun, tostringReplace, tostringProxy);
                tostringHook.Install();
            }

            if (beginSampeOnly == null)
            {
                Type typeTarget = typeof(UnityEngine.Profiling.Profiler);
                Type typeReplace = typeof(Profiler);

                MethodInfo hookTarget = typeTarget.GetMethod("BeginSampleOnly", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(string) }, null);
                MethodInfo hookReplace = typeReplace.GetMethod("BeginSampleOnly");
                MethodInfo hookProxy = typeReplace.GetMethod("ProxyBeginSample", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string) }, null);
                beginSampeOnly = new MethodHooker(hookTarget, hookReplace, hookProxy);
                beginSampeOnly.Install();

                hookTarget = typeTarget.GetMethod("BeginSample", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string), typeof(UnityEngine.Object) }, null);
                hookReplace = typeReplace.GetMethod("BeginSample");
                hookProxy = typeReplace.GetMethod("ProxyBeginSample", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string), typeof(UnityEngine.Object) }, null);
                beginObjetSample = new MethodHooker(hookTarget, hookReplace, hookProxy);
                beginObjetSample.Install();

                hookTarget = typeTarget.GetMethod("EndSample", BindingFlags.Public | BindingFlags.Static, null, new Type[] { }, null);
                hookReplace = typeReplace.GetMethod("EndSample");
                hookProxy = typeReplace.GetMethod("ProxyEndSample", BindingFlags.Public | BindingFlags.Static, null, new Type[] { }, null);
                endSample = new MethodHooker(hookTarget, hookReplace, hookProxy);
                endSample.Install();
            }

            m_hooked = true;
        }

        public static void Uninstall()
        {
            if (beginSampeOnly != null)
            {
                beginSampeOnly.Uninstall();
                beginSampeOnly = null;
            }
            if (beginObjetSample != null)
            {
                beginObjetSample.Uninstall();
                beginObjetSample = null;
            }
            if (endSample != null)
            {
                endSample.Uninstall();
                endSample = null;
            }
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
#endregion

#endregion
    }

    #region bind
    public class MikuLuaProfiler_LuaProfilerWrap
    {
        public static void Register(LuaState L)
        {
            L.BeginClass(typeof(MikuLuaProfiler.LuaProfiler), typeof(System.Object));
            L.RegFunction("BeginSample", BeginSample);
            L.RegFunction("EndSample", EndSample);
            L.EndClass();
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int BeginSample(IntPtr L)
        {
            try
            {
                string arg0 = ToLua.ToString(L, 1);
                MikuLuaProfiler.LuaProfiler.BeginSample(arg0);
                return 0;
            }
            catch (Exception e)
            {
                return LuaDLL.toluaL_exception(L, e);
            }
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int EndSample(IntPtr L)
        {
            try
            {
                MikuLuaProfiler.LuaProfiler.EndSample();
                return 0;
            }
            catch (Exception e)
            {
                return LuaDLL.toluaL_exception(L, e);
            }
        }
    }
    #endregion
}

#endif

#endif