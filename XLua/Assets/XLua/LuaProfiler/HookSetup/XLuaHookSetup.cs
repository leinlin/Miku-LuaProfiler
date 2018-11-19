/*
* ==============================================================================
* Filename: XLuaHookSetup
* Created:  2018/7/2 11:36:16
* Author:   エル・プサイ・コングルゥ
* Purpose:  
* ==============================================================================
*/

#define XLUA
#if XLUA

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using XLua;
using LuaState = XLua.LuaEnv;
using LuaDLL = XLua.LuaDLL.Lua;

namespace MikuLuaProfiler {

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
                Type typeEnv = typeof(XLua.LuaEnv);
                var clickFun = typeEnv.GetConstructors()[0];
                MethodInfo clickReplace = envReplace.GetMethod("Ctor");
                MethodInfo clickProxy = envReplace.GetMethod("Proxy", BindingFlags.Public | BindingFlags.Static);
                hookNewLuaEnv = new MethodHooker(clickFun, clickReplace, clickProxy);
                hookNewLuaEnv.Install();

                Type typeDll = typeof(XLua.LuaDLL.Lua);
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
            public static void Ctor(LuaState env)
            {
                Proxy(env);
                MikuLuaProfiler.HookSetup.SetMainLuaEnv(env);
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
                    LuaProfiler.SetMainLuaEnv(null);
                }

                isPlaying = EditorApplication.isPlaying;
            };
#endif
        }


        public static void SetMainLuaEnv(LuaState env)
        {
            if (LuaDeepProfilerSetting.Instance.isDeepProfiler)
            {
                if (env != null)
                {
                    env.DoString(@"
BeginMikuSample = CS.MikuLuaProfiler.LuaProfiler.BeginSample
EndMikuSample = CS.MikuLuaProfiler.LuaProfiler.EndSample

function miku_unpack_return_value(...)
	EndMikuSample()
	return ...
end
");
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

            public static int xluaL_loadbuffer(IntPtr L, byte[] buff, int size, string name)
            {
                if (LuaDeepProfilerSetting.Instance.isDeepProfiler)//&& name != "chunk"
                {
                    var utf8WithoutBom = new System.Text.UTF8Encoding(true);
                    string fileName = name.Replace("@", "").Replace("/", ".") + ".lua";
                    string value = utf8WithoutBom.GetString(buff);
                    value = Parse.InsertSample(value, fileName);

                    //System.IO.File.WriteAllText(fileName, value);

                    buff = utf8WithoutBom.GetBytes(value);
                    size = buff.Length;
                }

                return ProxyLoadbuffer(L, buff, size, name);
            }

            public static int ProxyLoadbuffer(IntPtr L, byte[] buff, int size, string name)
            {
                return 0;
            }

            public static string lua_tostring(IntPtr L, int index)
            {
                IntPtr strlen;

                IntPtr str = LuaDLL.lua_tolstring(L, index, out strlen);
                if (str != IntPtr.Zero)
                {
#if XLUA_GENERAL || (UNITY_WSA && !UNITY_EDITOR)
                int len = strlen.ToInt32();
                byte[] buffer = new byte[len];
                Marshal.Copy(str, buffer, 0, len);
                return Encoding.UTF8.GetString(buffer);
#else
                    string ret;
                    if (TryGetLuaString(str, out ret))
                    {
                        return ret;
                    }

                    ret = Marshal.PtrToStringAnsi(str, strlen.ToInt32());
                    if (ret == null)
                    {
                        int len = strlen.ToInt32();
                        byte[] buffer = new byte[len];
                        Marshal.Copy(str, buffer, 0, len);
                        ret = Encoding.UTF8.GetString(buffer);
                    }
                    if (ret != null)
                    {
                        RefString(str, index, ret, L);
                    }
                    return ret;
#endif
                }
                else
                {
                    return null;
                }
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

                tostringFun = typeLog.GetMethod("xluaL_loadbuffer");
                tostringReplace = typeLogReplace.GetMethod("xluaL_loadbuffer");
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
}
#endif

#endif