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

        private static Queue<Action> actionQueue = new Queue<Action>();
        public static void RegisterAction(Action a)
        {
            lock (actionQueue)
            {
                actionQueue.Enqueue(a);
            }
        }
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
                DontDestroyOnLoad(go);
                go.AddComponent<HookLuaSetup>();
                NetWorkClient.ConnectServer(setting.ip, setting.port);
            }
        }

        private void Awake()
        {
            NativeHelper.RunAyncPass();
            setting = LuaDeepProfilerSetting.Instance;
        }

        private void LateUpdate()
        {
            if (actionQueue.Count > 0)
            {
                lock (actionQueue)
                {
                    while (actionQueue.Count > 0)
                    {
                        actionQueue.Dequeue()();
                    }
                }
            }
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
#if UNITY_EDITOR
            desotryCount = 0;
            Destroy(gameObject);
            UnityEditor.EditorApplication.update += WaitDestory;
#else
            NetWorkClient.Close();
#endif
        }
#if UNITY_EDITOR
        int desotryCount = 0;
        private void WaitDestory()
        {
#if XLUA || TOLUA || SLUA
            desotryCount++;
            if (desotryCount > 10)
            {
                UnityEditor.EditorApplication.update -= WaitDestory;
                if (LuaProfiler.mainL != IntPtr.Zero)
                {
                    LuaDLL.lua_close(LuaProfiler.mainL);
                }
                LuaProfiler.mainL = IntPtr.Zero;
                NetWorkClient.Close();
                desotryCount = 0;
            }
#endif
        }
#endif
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

        public static void HookRef(IntPtr L, int reference)
        {
#if XLUA || TOLUA || SLUA
            if (isHook)
            {
                HookLuaSetup.RegisterAction(()=> 
                {
                    LuaLib.DoRefLuaFun(L, "lua_miku_add_ref_fun_info", reference);
                });
            }
#endif
        }

        public static void HookUnRef(IntPtr L, int reference)
        {
#if XLUA || TOLUA || SLUA
            if (isHook)
            {
                HookLuaSetup.RegisterAction(() =>
                {
                    LuaLib.DoRefLuaFun(L, "lua_miku_remove_ref_fun_info", reference);
                });
            }
#endif
        }

        #region check
        public static int historyRef = -100;
        public static void Record()
        {
#if XLUA || TOLUA || SLUA
            IntPtr L = LuaProfiler.mainL;
            if (L == IntPtr.Zero)
            {
                return;
            }
            isHook = false;
            ClearRecord();
            int oldTop = LuaDLL.lua_gettop(L);
            LuaLib.lua_getglobal(L, "miku_handle_error");

            LuaLib.lua_getglobal(L, "miku_do_record");
            LuaLib.lua_getglobal(L, "_G");
            LuaDLL.lua_pushstring(L, "");
            LuaDLL.lua_pushstring(L, "_G");
            //recrod
            LuaDLL.lua_newtable(L);
            historyRef = LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
            LuaDLL.lua_getref(L, historyRef);
            //history
            LuaDLL.lua_pushnil(L);
            //null_list
            LuaDLL.lua_newtable(L);

            if (LuaDLL.lua_pcall(L, 6, 0, oldTop + 1) == 0)
            {
                LuaDLL.lua_remove(L, oldTop + 1);
            }
            LuaDLL.lua_settop(L, oldTop);

            oldTop = LuaDLL.lua_gettop(L);
            LuaLib.lua_getglobal(L, "miku_handle_error");

            LuaLib.lua_getglobal(L, "miku_do_record");
            LuaDLL.lua_pushvalue(L, LuaIndexes.LUA_REGISTRYINDEX);
            LuaDLL.lua_pushstring(L, "");
            LuaDLL.lua_pushstring(L, "_R");
            LuaDLL.lua_getref(L, historyRef);
            //history
            LuaDLL.lua_pushnil(L);
            //null_list
            LuaDLL.lua_newtable(L);

            if (LuaDLL.lua_pcall(L, 6, 0, oldTop + 1) == 0)
            {
                LuaDLL.lua_remove(L, oldTop + 1);
            }
            LuaDLL.lua_settop(L, oldTop);

            isHook = true;
#endif
        }
        private static void ClearRecord()
        {
#if XLUA || TOLUA || SLUA
            IntPtr L = LuaProfiler.mainL;
            if (L == IntPtr.Zero)
            {
                return;
            }
            if (historyRef != -100)
            {
                LuaDLL.lua_unref(L, historyRef);
                historyRef = -100;
            }
#endif
        }
        private static void SetAddOrRm(int refIndex, Dictionary<string, int> dict, Dictionary<string, List<string>> detailDict)
        {
#if XLUA || TOLUA || SLUA
            IntPtr L = LuaProfiler.mainL;
            if (L == IntPtr.Zero)
            {
                return;
            }
            dict.Clear();
            int oldTop = LuaDLL.lua_gettop(L);

            LuaDLL.lua_getref(L, refIndex);
            if (LuaDLL.lua_type(L, -1) != LuaTypes.LUA_TTABLE)
            {
                LuaDLL.lua_pop(L, 1);
                return;
            }
            int t = oldTop + 1;
            LuaDLL.lua_pushnil(L);  /* 第一个 key */
            while (LuaDLL.lua_next(L, t) != 0)
            {
                /* 用一下 'key' （在索引 -2 处） 和 'value' （在索引 -1 处） */
                int key_t = LuaDLL.lua_gettop(L);
                LuaDLL.lua_pushnil(L);  /* 第一个 key */
                string firstKey = null;
                List<string> detailList = new List<string>();
                while (LuaDLL.lua_next(L, key_t) != 0)
                {
                    string key = LuaHook.GetRefString(L, -1);
                    if (string.IsNullOrEmpty(firstKey))
                    {
                        firstKey = key;
                    }
                    detailList.Add(key);
                    LuaDLL.lua_pop(L, 1);
                }
                LuaDLL.lua_settop(L, key_t);
                dict[firstKey] = (int)LuaDLL.lua_type(L, -2);
                detailDict[firstKey] = detailList;
                /* 移除 'value' ；保留 'key' 做下一次迭代 */
                LuaDLL.lua_pop(L, 1);
            }
            LuaDLL.lua_settop(L, oldTop);
#endif
        }
        private static void SetNullObject(int nullObjectRef, List<string> list)
        {
#if XLUA || TOLUA || SLUA
            IntPtr L = LuaProfiler.mainL;
            if (L == IntPtr.Zero)
            {
                return;
            }
            list.Clear();
            int oldTop = LuaDLL.lua_gettop(L);

            LuaDLL.lua_getref(L, nullObjectRef);
            if (LuaDLL.lua_type(L, -1) != LuaTypes.LUA_TTABLE)
            {
                LuaDLL.lua_pop(L, 1);
                return;
            }
            int t = oldTop + 1;
            LuaDLL.lua_pushnil(L);  /* 第一个 key */
            while (LuaDLL.lua_next(L, t) != 0)
            {
                /* 用一下 'key' （在索引 -2 处） 和 'value' （在索引 -1 处） */
                list.Add(LuaHook.GetRefString(L, -2));
                /* 移除 'value' ；保留 'key' 做下一次迭代 */
                LuaDLL.lua_pop(L, 1);
            }
            LuaDLL.lua_settop(L, oldTop);
#endif
        }

        public static void GCDiff()
        {
            LuaLib.RunGC();
            Diff();
        }

        public static void Diff()
        {
#if XLUA || TOLUA || SLUA
            IntPtr L = LuaProfiler.mainL;
            if (L == IntPtr.Zero)
            {
                return;
            }
            isHook = false;

            if (historyRef == -100)
            {
                Debug.LogError("has no history");
                return;
            }

            int oldTop = LuaDLL.lua_gettop(L);
            LuaLib.lua_getglobal(L, "miku_handle_error");

            LuaLib.lua_getglobal(L, "miku_diff");
            LuaDLL.lua_getref(L, historyRef);
            if (LuaDLL.lua_type(L, -1) != LuaTypes.LUA_TTABLE)
            {
                Debug.LogError(LuaDLL.lua_type(L, -1));
                LuaDLL.lua_settop(L, oldTop);
                historyRef = -100;
                return;
            }

            if (LuaDLL.lua_pcall(L, 1, 3, oldTop + 1) == 0)
            {
                LuaDLL.lua_remove(L, oldTop + 1);
            }
            int nullObjectRef = LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
            int rmRef = LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
            int addRef = LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
            LuaDiffInfo ld = LuaDiffInfo.Create();
            SetNullObject(nullObjectRef, ld.nullRef);
            SetAddOrRm(rmRef, ld.rmRef, ld.rmDetail);
            SetAddOrRm(addRef, ld.addRef, ld.addDetail);

            NetWorkClient.SendMessage(ld);

            LuaDLL.lua_unref(L, nullObjectRef);
            LuaDLL.lua_unref(L, rmRef);
            LuaDLL.lua_unref(L, addRef);
            LuaDLL.lua_settop(L, oldTop);

            isHook = true;
#endif
        }

        #endregion

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
        public static string GetRefString(IntPtr L, int index)
        {
#if XLUA || TOLUA || SLUA
            StrLen len;
            IntPtr intPtr = LuaLib.lua_tostringptr(L, index, out len);
            string text;
            if (!TryGetLuaString(intPtr, out text))
            {
                text = LuaDLL.lua_tostring(L, index);
                if (!string.IsNullOrEmpty(text))
                {
                    text = string.Intern(text);
                }
                RefString(intPtr, index, text, L);
            }
            return text;
#else
            return "";
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

        public static void lua_openLib(IntPtr L)
        {
#if XLUA || TOLUA || SLUA
            LuaDLL.luaL_openlibs(L);
#endif
        }
        public static void DoString(IntPtr L, string script)
        {
#if XLUA || TOLUA || SLUA
            LuaHook.isHook = false;
            byte[] chunk = Encoding.UTF8.GetBytes(script);
            int oldTop = LuaDLL.lua_gettop(L);
            lua_getglobal(L, "miku_handle_error");
            if (LuaLib.luaL_loadbuffer(L, chunk, chunk.Length, "chunk") == 0)
            {
                if (LuaDLL.lua_pcall(L, 0, -1, oldTop + 1) == 0)
                {
                    LuaDLL.lua_remove(L, oldTop + 1);
                }
            }
            else
            {
                Debug.Log(script);
            }
            LuaHook.isHook = true;
            LuaDLL.lua_settop(L, oldTop);
#endif
        }
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

        public static void DoRefLuaFun(IntPtr L, string funName, int reference)
        {
            LuaDLL.lua_getref(L, reference);
            int oldTop = LuaDLL.lua_gettop(L);
            lua_getglobal(L, "miku_handle_error");
            do
            {
                LuaLib.lua_getglobal(L, funName);
                if (!LuaDLL.lua_isfunction(L, -1)) break;
                LuaDLL.lua_pushvalue(L, oldTop);
                if (LuaDLL.lua_pcall(L, 1, 0, oldTop + 1) == 0)
                {
                    LuaDLL.lua_remove(L, oldTop + 1);
                }

            } while (false);
            LuaDLL.lua_settop(L, oldTop);
            LuaDLL.lua_pop(L, 1);
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

            LuaLib.lua_pushstdcallcfunction(L, CheckType);
            LuaLib.lua_setglobal(L, "miku_check_type");

            LuaLib.lua_pushstdcallcfunction(L, HandleError);
            LuaLib.lua_setglobal(L, "miku_handle_error");

            LuaDLL.lua_newtable(L);
            LuaLib.lua_setglobal(L, "MikuLuaProfilerStrTb");
#if XLUA
            LuaLib.DoString(L, env_script);
#endif
            LuaLib.DoString(L, get_ref_string);
            LuaLib.DoString(L, null_script);
            LuaLib.DoString(L, diff_script);
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
local weak_meta_table = {__mode = 'k'}
local infoTb = {}
local funAddrTb = {}
setmetatable(infoTb, weak_meta_table)
setmetatable(funAddrTb, weak_meta_table)

local function miku_get_fun_info(fun)
    local result = infoTb[fun]
    local addr = funAddrTb[fun]
    if not result then
        local info = debug.getinfo(fun, 'Sl')
        result = string.format('function:%s&line:%d', info.source, info.linedefined)
        addr = string.sub(tostring(fun), 11)
        infoTb[fun] = result
        funAddrTb[fun] = addr
    end
    return result,addr
end

local function serialize(obj)
    if obj == _G then
        return '_G'
    end
    local lua = ''
    lua = lua .. '{\n'
    local count = 0
    for k, v in pairs(obj) do
        lua = lua .. '[' .. tostring(k) .. ']=' .. tostring(v) .. ',\n'
        count = count + 1
        if count > 5 then
            break
        end
    end
    lua = lua .. '}'
    if lua == '{\n}' then
        lua = tostring(obj)
    end
    return lua
end

local function get_table_info(tb)
    local result = infoTb[tb]
    local addr = funAddrTb[tb]
    if not result then
        local tostringFun
        if getmetatable(tb) and rawget(getmetatable(tb), '__tostring') then
            tostringFun = rawget(getmetatable(tb), '__tostring')
            rawset(getmetatable(tb), '__tostring', nil)
        end
        local addStr = tostring(tb)
        if tostringFun then
            rawset(getmetatable(tb), '__tostring', tostringFun)
        end
        result = rawget(tb, '__name') or rawget(tb, 'name') or rawget(tb, '__cname')
        if not result then
            result = serialize(tb)
        end

        addr = string.sub(addStr, 7)
        infoTb[tb] = result
        funAddrTb[tb] = addr
    end
    return result,addr
end

function lua_miku_add_ref_fun_info(data)
    local result = ''
    local addr = ''
    local t = 1
    local typeStr = miku_check_type(data)
    if typeStr == 1 then
        result,addr = miku_get_fun_info(data)
        t = 1
    elseif typeStr == 2 then
        result,addr = get_table_info(data)
        t = 2
    end
    miku_add_ref_fun_info(result, addr, t)
end

function lua_miku_remove_ref_fun_info(data)
    local result = infoTb[data]
    local addr = funAddrTb[data]
    local typeStr = miku_check_type(data)
    local t = 1
    if typeStr == 1 then
        t = 1
    elseif typeStr == 2 then
        t = 2
    end

    miku_remove_ref_fun_info(result, addr, t)
end
";
        const string null_script = @"
function miku_is_null(val)
    local metaTable = getmetatable(val)
    if type(metaTable) == 'table' and metaTable.__index and val.Equals then
        local status,retval = pcall(val.Equals, val, nil)
        if status then
            return retval
        else
            return true
        end
    end
    return false
end
";
        const string diff_script = @"
local weak_meta_key_table = {__mode = 'k'}
local weak_meta_value_table = {__mode = 'v'}
local infoTb = {}
local tolua_object_tb
function miku_do_record(val, prefix, key, record, history, null_list)
    if val == infoTb then
        return
    end
    if not tolua_object_tb then
        tolua_object_tb = debug.getregistry()[4]
    end
    if val == tolua_object_tb then
        return
    end
    if val == miku_do_record then
        return
    end
    if val == miku_diff then
        return
    end
    if val == lua_miku_remove_ref_fun_info then
        return
    end
    if val == lua_miku_add_ref_fun_info then
        return
    end

    if getmetatable(record) ~= weak_meta_key_table then
        setmetatable(record, weak_meta_key_table)
    end

    local typeStr = type(val)
    if typeStr ~= 'table' and typeStr ~= 'userdata' and typeStr ~= 'function' then
        return
    end

    local strKey = tostring(key)
    if not strKey then
        strKey = 'empty'
    end
    local prefixTb = infoTb[prefix]
    if not prefixTb then
        prefixTb = {}
        infoTb[prefix] = prefixTb
    end
    local tmp_prefix = prefixTb[strKey]
    if not tmp_prefix then
        tmp_prefix = prefix.. (prefix == '' and '' or '.') .. strKey
        prefixTb[strKey] = tmp_prefix
    end
    if null_list then
        if type(val) == 'userdata' then
            local st,ret = pcall(miku_is_null, val)
            if st and ret then
                null_list[tmp_prefix] = val
            end
        end
    end

    if record[val] then
        if record[val] == nil then
            record[val] = {}
        end
        table.insert(record[val], tmp_prefix)
        return
    end
    if val == history then
        return
    end
    if val == record then
        return
    end

    if record[val] == nil then
        record[val] = {}
    end
    table.insert(record[val], tmp_prefix)

    if typeStr == 'table' then
        for k,v in pairs(val) do
            local typeKStr = type(k)
            if typeKStr == 'table' or typeKStr == 'userdata' or typeKStr == 'function' then
                miku_do_record(k, tmp_prefix, v, record, history, null_list)
            else
                miku_do_record(v, tmp_prefix, k, record, history, null_list)
            end
        end

    elseif typeStr == 'function' then
        if val ~= lua_miku_add_ref_fun_info and val ~= lua_miku_rm_ref_fun_info then
            local i = 1
            while true do
                local k, v = debug.getupvalue(val, i)
                if not k then
                    break
                end
                if v then
                    miku_do_record(v, tmp_prefix, k, record, history, null_list)
                end
                i = i + 1
            end
        end
    end

    local metaTable = getmetatable(val)
    if metaTable then
        miku_do_record(metaTable, tmp_prefix, 'metaTable', record, history, null_list)
    end
    metaTable = getmetatable(key)
    if metaTable then
        miku_do_record(metaTable, tmp_prefix, 'metaTable', record, history, null_list)
    end
end

function miku_diff(record)
    local add = { }
    setmetatable(add, weak_meta_key_table)
    local null_list = { }
    setmetatable(null_list, weak_meta_value_table)
    miku_do_record(_G, '', '_G', add, record, null_list)
    miku_do_record(debug.getregistry(), '', '_R', add, record, null_list)
    local rm = { }
    for key, val in pairs(record) do
        if not add[key] then
            rm[key] = val
        else
            add[key] = nil
        end
    end
    return add,rm,null_list
end";
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int BeginSample(IntPtr L)
        {
            LuaProfiler.BeginSample(L, LuaHook.GetRefString(L, 1));
            return 0;
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int UnpackReturnValue(IntPtr L)
        {
            LuaProfiler.EndSample(L);
            return LuaDLL.lua_gettop(L);
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int CheckType(IntPtr L)
        {
            if (LuaDLL.lua_isfunction(L, 1))
            {
                LuaDLL.lua_pushnumber(L, 1);
            }
            else if (LuaDLL.lua_istable(L, 1))
            {
                LuaDLL.lua_pushnumber(L, 2);
            }
            else
            {
                LuaDLL.lua_pushnumber(L, 0);
            }
            return 1;
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int AddRefFunInfo(IntPtr L)
        {
            string funName = LuaHook.GetRefString(L, 1);
            string funAddr = LuaHook.GetRefString(L, 2);
            byte type = (byte)LuaDLL.lua_tonumber(L, 3);
            LuaProfiler.AddRef(funName, funAddr, type);
            return 0;
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int RemoveRefFunInfo(IntPtr L)
        {
            string funName = LuaHook.GetRefString(L, 1);
            string funAddr = LuaHook.GetRefString(L, 2);
            byte type = (byte)LuaDLL.lua_tonumber(L, 3);
            LuaProfiler.RemoveRef(funName, funAddr, type);
            return 0;
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int HandleError(IntPtr L)
        {
            string error = LuaHook.GetRefString(L, 1);
            Debug.LogError(error);
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