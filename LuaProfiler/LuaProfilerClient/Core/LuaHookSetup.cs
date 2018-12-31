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
using System.Text;
using UnityEngine;
using System.Runtime;

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
        private static IntPtr m_debug;
        private static IntPtr m_pssFun;
        public static float fps { private set; get; }
        public static int frameCount { private set; get; }
        public static int pss { private set; get; }
        public static float power { private set; get; }

        public static LuaDeepProfilerSetting setting { private set; get; }

        private bool needShowMenu = false;
        public float showTime = 1f;
        private int count = 0;
        private float deltaTime = 0f;

        public const float DELTA_TIME = 0.1f;
        public float currentTime = 0;
        #endregion

#if UNITY_5 || UNITY_2017_1_OR_NEWER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        public static void OnStartGame()
        {
            setting = LuaDeepProfilerSetting.MakeInstance();
            LuaProfiler.mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            if (setting.isNeedCapture)
            {
                Screen.SetResolution(480, 270, true);
            }

            if (setting.isDeepLuaProfiler || setting.isDeepMonoProfiler)
            {
                GameObject go = new GameObject();
                go.name = "MikuLuaProfiler";
                go.hideFlags = HideFlags.HideAndDontSave;
                go.AddComponent<HookLuaSetup>();
                NetWorkClient.ConnectServer(setting.ip, setting.port);
            }
            if (setting.isDeepMonoProfiler)
            {
                GCSettings.LatencyMode = GCLatencyMode.LowLatency;
            }
        }

        private void Awake()
        {
            NativeHelper.RunAyncPass();
            setting = LuaDeepProfilerSetting.Instance;
        }

        private void LateUpdate()
        {
            frameCount = Time.frameCount;
            count++;
            deltaTime += Time.unscaledDeltaTime;
            if (deltaTime >= showTime)
            {
                fps = count / deltaTime;
                count = 0;
                deltaTime = 0f;
            }
            if (Time.unscaledTime - currentTime > DELTA_TIME)
            {
                pss = NativeHelper.GetPass();
                power = NativeHelper.GetBatteryLevel();
                currentTime = Time.unscaledTime;
            }
            if ((Input.touchCount == 4 && Input.touches[0].phase == TouchPhase.Began) || Input.GetKeyDown(KeyCode.Escape))
            {
                needShowMenu = !needShowMenu;
                if (needShowMenu)
                {
                    Menu.EnableMenu(gameObject);
                }
                else
                {
                    Menu.DisableMenu();
                }
            }
        }

        private void OnApplicationQuit()
        {
            NetWorkClient.Close();
            Destroy(gameObject);
        }

    }

    public class Menu : MonoBehaviour
    {
        private static Menu m_menu;
        public static void EnableMenu(GameObject go)
        {
            if (m_menu == null)
            {
                m_menu = go.AddComponent<Menu>();
            }
            m_menu.enabled = true;
        }

        public static void DisableMenu()
        {
            if (m_menu == null)
            {
                return;
            }
            m_menu.enabled = false;
        }

        private void OnGUI()
        {
            var setting = HookLuaSetup.setting;

            if (GUI.Button(new Rect(0, 0, 200, 100), "Connect"))
            {
                NetWorkClient.ConnectServer(setting.ip, setting.port);
            }

            setting.ip = GUI.TextField(new Rect(210, 20, 200, 60), setting.ip);

            if (GUI.Button(new Rect(0, 110, 200, 100), "Disconnect"))
            {
                NetWorkClient.Close();
            }
            if (setting.discardInvalid)
            {
                if (GUI.Button(new Rect(0, 220, 200, 100), "ShowAll"))
                {
                    setting.discardInvalid = false;
                }
            }
            else
            {
                if (GUI.Button(new Rect(0, 220, 200, 100), "HideUseless"))
                {
                    setting.discardInvalid = true;
                }
            }
        }
    }

    public class LuaHook
    {
        public static bool isHook = true;
        public static byte[] Hookloadbuffer(IntPtr L, byte[] buff, string name)
        {
            if (!isHook)
            {
                return buff;
            }
            if (buff[0] == 0x1b && buff[1] == 0x4c)
            {
                return buff;
            }

            string value = "";
            string hookedValue = "";

            string fileName = name.Replace(".lua", "");
            fileName = fileName.Replace("@", "").Replace('.', '/');

            // utf-8
            if (buff[0] == 239 && buff[1] == 187 && buff[2] == 191)
            {
                value = Encoding.UTF8.GetString(buff, 3, buff.Length - 3);
            }
            else
            {
                value = Encoding.UTF8.GetString(buff);
            }

            hookedValue = Parse.InsertSample(value, fileName);

            buff = Encoding.UTF8.GetBytes(hookedValue);

            return buff;
        }

        public static void HookRef(IntPtr L)
        {
#if XLUA || TOLUA || SLUA
            LuaLib.DoRefLuaFun(L, "lua_miku_add_ref_fun_info");
#endif
        }

        public static void HookUnRef(IntPtr L)
        {
#if XLUA || TOLUA || SLUA
            LuaLib.DoRefLuaFun(L, "lua_miku_remove_ref_fun_info");
#endif
        }

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
            //把字符串ref了之后就不GC了
            LuaLib.lua_getglobal(L, "MikuLuaProfilerStrTb");
            LuaDLL.lua_pushvalue(L, index);
            LuaDLL.lua_insert(L, -2);

            LuaDLL.lua_settop(L, oldTop);
            stringDict[(long)strPoint] = s;
#endif
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

        public static IntPtr lua_tostringptr(IntPtr L, int index, out StrLen len)
        {
#if XLUA
            return LuaDLL.lua_tolstring(L, index, out len);
#elif TOLUA
            return LuaDLL.tolua_tolstring(L, index, out len);
#elif SLUA
            return LuaDLL.luaS_tolstring32(L, index, out len);
#endif
        }

        public static int luaL_loadbuffer(IntPtr L, byte[] buff, int size, string name)
        {
#if XLUA
            return LuaDLL.xluaL_loadbuffer(L, buff, size, name);
#elif TOLUA
            return LuaDLL.tolua_loadbuffer(L, buff, size, name);
#elif SLUA
            return SLua.LuaDLLWrapper.luaLS_loadbuffer(L, buff, size, name);
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

        public static void DoString(IntPtr L, string script)
        {
            LuaHook.isHook = false;
            byte[] chunk = Encoding.UTF8.GetBytes(script);
            int oldTop = LuaDLL.lua_gettop(L);
#if XLUA
            int errFunc = LuaDLL.load_error_func(L, -1);

            if (LuaLib.luaL_loadbuffer(L, chunk, chunk.Length, "chunk") == 0)
            {
                if (LuaDLL.lua_pcall(L, 0, -1, errFunc) == 0)
                {
                    LuaDLL.lua_remove(L, errFunc);
                }
            }
            else
            {
                Debug.Log(script);
            }
#elif TOLUA
            if (LuaLib.luaL_loadbuffer(L, chunk, chunk.Length, "chunk") == 0)
            {
                LuaDLL.lua_call(L, 0, -1);
            }
#elif SLUA
            if (LuaLib.luaL_loadbuffer(L, chunk, chunk.Length, "chunk") == 0)
            {
                LuaDLL.lua_call(L, 0, -1);
            }
#endif
            LuaHook.isHook = true;
            LuaDLL.lua_settop(L, oldTop);
        }

        public static void DoRefLuaFun(IntPtr L, string funName)
        {
            int oldTop = LuaDLL.lua_gettop(L);

            do
            {
#if XLUA
                int errFunc = LuaDLL.load_error_func(L, -1);
                LuaLib.lua_getglobal(L, funName);
                if (!LuaDLL.lua_isfunction(L, -1)) break;
                LuaDLL.lua_pushvalue(L, -3);
                if (LuaDLL.lua_pcall(L, 1, 0, errFunc) == 0)
                {
                    LuaDLL.lua_remove(L, errFunc);
                }
#elif TOLUA
                LuaLib.lua_getglobal(L, funName);
                if (!LuaDLL.lua_isfunction(L, -1)) break;
                LuaDLL.lua_pushvalue(L, -2);
                LuaDLL.lua_call(L, 1, 0);
#elif SLUA
                LuaLib.lua_getglobal(L, funName);
                if (!LuaDLL.lua_isfunction(L, -1)) break;
                LuaDLL.lua_pushvalue(L, -2);
                LuaDLL.lua_call(L, 1, 0);
#endif
            } while (false);

            LuaDLL.lua_settop(L, oldTop);
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

            LuaLib.lua_pushstdcallcfunction(L, AddRefFunInfo);
            LuaLib.lua_setglobal(L, "miku_add_ref_fun_info");

            LuaLib.lua_pushstdcallcfunction(L, RemoveRefFunInfo);
            LuaLib.lua_setglobal(L, "miku_remove_ref_fun_info");

            LuaDLL.lua_newtable(L);
            LuaLib.lua_setglobal(L, "MikuLuaProfilerStrTb");
#if XLUA
            LuaLib.DoString(L, env_script);
#endif
            LuaLib.DoString(L, get_ref_string);
        }
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
        const string get_ref_string = @"
local infoTb = {}
local funAddrTb = {}

local function get_fun_info(fun)
    local result = infoTb[fun]
    local addr = funAddrTb[fun]
    if not result then
        local info = debug.getinfo(fun, 'Sl')
        result = string.format('%s&line:%d', info.source, info.linedefined)
        addr = string.sub(tostring(fun), 11)
        infoTb[fun] = result
        funAddrTb[fun] = addr
    end
    return result,addr
end

function lua_miku_add_ref_fun_info(fun)
    local result,addr = get_fun_info(fun)
    miku_add_ref_fun_info(result, addr)
end

function lua_miku_remove_ref_fun_info(fun)
    local result,addr = get_fun_info(fun)
    miku_remove_ref_fun_info(result, addr)
end
";
        public static string GetRefString(IntPtr L, int index)
        {
            StrLen len;
            IntPtr intPtr = LuaLib.lua_tostringptr(L, index, out len);
            string text;
            if (!LuaHook.TryGetLuaString(intPtr, out text))
            {
                text = LuaDLL.lua_tostring(L, index);
                if (!string.IsNullOrEmpty(text))
                {
                    text = string.Intern(text);
                }
                LuaHook.RefString(intPtr, index, text, L);
            }
            return text;
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int BeginSample(IntPtr L)
        {
            LuaProfiler.BeginSample(L, GetRefString(L, 1));
            return 0;
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int UnpackReturnValue(IntPtr L)
        {
            LuaProfiler.EndSample(L);
            return LuaDLL.lua_gettop(L);
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int AddRefFunInfo(IntPtr L)
        {
            string funName = GetRefString(L, 1);
            string funAddr = GetRefString(L, 2);
            LuaProfiler.AddRefFun(funName, funAddr);
            return 0;
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int RemoveRefFunInfo(IntPtr L)
        {
            string funName = GetRefString(L, 1);
            string funAddr = GetRefString(L, 2);
            LuaProfiler.RemoveRefFun(funName, funAddr);
            return 0;
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int EndSample(IntPtr L)
        {
            LuaProfiler.EndSample(L);
            return 0;
        }
    }
    #endregion
    #endif
}
#endif