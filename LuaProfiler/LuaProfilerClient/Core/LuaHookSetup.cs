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

namespace MikuLuaProfiler
{

    #region monobehaviour
    public class HookLuaSetup : MonoBehaviour
    {
        #region field
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
        private static bool isInite = false;
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
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            if (isInite) return;

            isInite = true;
            setting = LuaDeepProfilerSetting.MakeInstance();
            LuaProfiler.mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            if (setting.isNeedCapture)
            {
                Screen.SetResolution(480, 270, true);
            }

            if (setting.isDeepLuaProfiler)
            {
                 LuaDLL.Install();
            }

            if (setting.isDeepLuaProfiler || setting.isDeepMonoProfiler || setting.isCleanMode)
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
            if (Input.touchCount == 4 || Input.GetKeyDown(KeyCode.Delete))
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
    #endregion

    public class LuaHook
    {
        public static bool isHook = true;
        public static byte[] Hookloadbuffer(IntPtr L, byte[] buff, string name)
        {
            if (LuaDeepProfilerSetting.Instance.isCleanMode)
            {
                return buff;
            }
            if (!isHook)
            {
                return buff;
            }
            if (buff.Length < 2)
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
            if (fileName.Contains("protobuf"))
            {
                return buff;
            }
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

        public static void HookRef(IntPtr L, int reference, LuaDLL.tolua_getref_fun refFun = null)
        {
            if (isHook)
            {
                LuaLib.DoRefLuaFun(L, "lua_miku_add_ref_fun_info", reference, refFun);
            }
        }

        public static void HookUnRef(IntPtr L, int reference, LuaDLL.tolua_getref_fun refFun = null)
        {
            if (isHook)
            {
                LuaLib.DoRefLuaFun(L, "lua_miku_remove_ref_fun_info", reference, refFun);
            }
        }

        #region check
        public static int historyRef = -100;
        public static void Record()
        {
            IntPtr L = LuaProfiler.mainL;
            if (L == IntPtr.Zero)
            {
                return;
            }
            isHook = false;

            ClearRecord();
            Resources.UnloadUnusedAssets();
            // 调用C# LuaTable LuaFunction WeakTable的析构 来清理掉lua的 ref
            GC.Collect();
            // 清理掉C#强ref后，顺便清理掉很多弱引用
            LuaDLL.lua_gc(L, LuaGCOptions.LUA_GCCOLLECT, 0);

            int oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_getglobal(L, "miku_handle_error");

            LuaDLL.lua_getglobal(L, "miku_do_record");
            LuaDLL.lua_getglobal(L, "_G");
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
            LuaDLL.lua_getglobal(L, "miku_handle_error");

            LuaDLL.lua_getglobal(L, "miku_do_record");
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
        }
        private static void ClearRecord()
        {
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
        }
        private static void SetTable(int refIndex, Dictionary<string, LuaTypes> dict, Dictionary<string, List<string>> detailDict)
        {
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
                if (!string.IsNullOrEmpty(firstKey))
                {
                    dict[firstKey] = (LuaTypes)LuaDLL.lua_type(L, -2);
                    detailDict[firstKey] = detailList;
                }

                /* 移除 'value' ；保留 'key' 做下一次迭代 */
                LuaDLL.lua_pop(L, 1);
            }
            LuaDLL.lua_settop(L, oldTop);
        }

        public static void DiffServer()
        {
            NetWorkClient.SendMessage(Diff());
        }

        public static LuaDiffInfo Diff()
        {
            IntPtr L = LuaProfiler.mainL;
            if (L == IntPtr.Zero)
            {
                return null;
            }
            isHook = false;
            Resources.UnloadUnusedAssets();
            // 调用C# LuaTable LuaFunction WeakTable的析构 来清理掉lua的 ref
            GC.Collect();
            // 清理掉C#强ref后，顺便清理掉很多弱引用
            LuaDLL.lua_gc(L, LuaGCOptions.LUA_GCCOLLECT, 0);
            if (historyRef == -100)
            {
                Debug.LogError("has no history");
                return null;
            }

            int oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_getglobal(L, "miku_handle_error");

            LuaDLL.lua_getglobal(L, "miku_diff");
            LuaDLL.lua_getref(L, historyRef);
            if (LuaDLL.lua_type(L, -1) != LuaTypes.LUA_TTABLE)
            {
                Debug.LogError(LuaDLL.lua_type(L, -1));
                LuaDLL.lua_settop(L, oldTop);
                historyRef = -100;
                return null;
            }

            if (LuaDLL.lua_pcall(L, 1, 3, oldTop + 1) == 0)
            {
                LuaDLL.lua_remove(L, oldTop + 1);
            }
            int nullObjectRef = LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
            int rmRef = LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
            int addRef = LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
            LuaDiffInfo ld = LuaDiffInfo.Create();
            SetTable(nullObjectRef, ld.nullRef, ld.nullDetail);
            SetTable(rmRef, ld.rmRef, ld.rmDetail);
            SetTable(addRef, ld.addRef, ld.addDetail);

            LuaDLL.lua_unref(L, nullObjectRef);
            LuaDLL.lua_unref(L, rmRef);
            LuaDLL.lua_unref(L, addRef);
            LuaDLL.lua_settop(L, oldTop);

            isHook = true;

            return ld;
        }
        #endregion

        #region luastring
        public static readonly Dictionary<long, object> stringDict = new Dictionary<long, object>();
        public static bool TryGetLuaString(IntPtr p, out object result)
        {
            return stringDict.TryGetValue(p.ToInt64(), out result);
        }
        public static void RefString(IntPtr strPoint, int index, object s, IntPtr L)
        {
            int oldTop = LuaDLL.lua_gettop(L);
            //把字符串ref了之后就不GC了
            LuaDLL.lua_getglobal(L, "MikuLuaProfilerStrTb");
            LuaDLL.lua_pushvalue(L, index);
            LuaDLL.lua_insert(L, -2);

            LuaDLL.lua_settop(L, oldTop);
            stringDict[(long)strPoint] = s;
        }
        public static string GetRefString(IntPtr L, int index)
        {
            IntPtr len;
            IntPtr intPtr = LuaDLL.lua_tolstring(L, index, out len);
            object text;
            if (!TryGetLuaString(intPtr, out text))
            {
                string tmpText = LuaDLL.lua_tostring(L, index);
                if (!string.IsNullOrEmpty(tmpText))
                {
                    text = string.Intern(tmpText);
                }
                else
                {
                    text = "nil";
                }
                RefString(intPtr, index, text, L);
            }
            return (string)text;
        }
        #endregion
    }

    public class LuaLib
    {
        public static long GetLuaMemory(IntPtr luaState)
        {
            long result = 0;
            if (LuaProfiler.m_hasL)
            {
                result = LuaDLL.lua_gc(luaState, LuaGCOptions.LUA_GCCOUNT, 0);
                result = result * 1024 + LuaDLL.lua_gc(luaState, LuaGCOptions.LUA_GCCOUNTB, 0);
            }
            return result;
        }
        public static void DoString(IntPtr L, string script)
        {
            LuaHook.isHook = false;
            byte[] chunk = Encoding.UTF8.GetBytes(script);
            int oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_getglobal(L, "miku_handle_error");
            if (LuaDLL.luaL_loadbuffer(L, chunk, (IntPtr)chunk.Length, "chunk") == 0)
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
        }
        public static void DoRefLuaFun(IntPtr L, string funName, int reference, LuaDLL.tolua_getref_fun refFun)
        {
            int moreOldTop = LuaDLL.lua_gettop(L);
            if (refFun == null)
            {
                LuaDLL.lua_getref(L, reference);
            }
            else
            {
                refFun(L, reference);
            }

            if (LuaDLL.lua_isfunction(L, -1) || LuaDLL.lua_istable(L, -1))
            {
                int oldTop = LuaDLL.lua_gettop(L);
                LuaDLL.lua_getglobal(L, "miku_handle_error");
                do
                {
                    LuaDLL.lua_getglobal(L, funName);
                    if (!LuaDLL.lua_isfunction(L, -1)) break;
                    LuaDLL.lua_pushvalue(L, oldTop);
                    if (LuaDLL.lua_pcall(L, 1, 0, oldTop + 1) == 0)
                    {
                        LuaDLL.lua_remove(L, oldTop + 1);
                    }

                } while (false);
                LuaDLL.lua_settop(L, oldTop);
            }

            LuaDLL.lua_settop(L, moreOldTop);
        }
    }

    #region bind

    public class MikuLuaProfilerLuaProfilerWrap
    {
        public static LuaDeepProfilerSetting setting = LuaDeepProfilerSetting.Instance;
        public static LuaCSFunction beginSample = new LuaCSFunction(BeginSample);
        public static LuaCSFunction beginSampleCustom = new LuaCSFunction(BeginSampleCustom);
        public static LuaCSFunction endSample = new LuaCSFunction(EndSample);
        public static LuaCSFunction unpackReturnValue = new LuaCSFunction(UnpackReturnValue);
        public static LuaCSFunction addRefFunInfo = new LuaCSFunction(AddRefFunInfo);
        public static LuaCSFunction removeRefFunInfo = new LuaCSFunction(RemoveRefFunInfo);
        public static LuaCSFunction checkType = new LuaCSFunction(CheckType);
        public static LuaCSFunction handleError = new LuaCSFunction(HandleError);
        public static void __Register(IntPtr L)
        {
            LuaDLL.lua_newtable(L);
            LuaDLL.lua_pushstring(L, "LuaProfiler");
            LuaDLL.lua_newtable(L);

            LuaDLL.lua_pushstring(L, "BeginSample");
            LuaDLL.lua_pushstdcallcfunction(L, beginSample);
            LuaDLL.lua_rawset(L, -3);

            LuaDLL.lua_pushstring(L, "EndSample");
            LuaDLL.lua_pushstdcallcfunction(L, endSample);
            LuaDLL.lua_rawset(L, -3);

            LuaDLL.lua_pushstring(L, "BeginSampleCustom");
            LuaDLL.lua_pushstdcallcfunction(L, beginSampleCustom);
            LuaDLL.lua_rawset(L, -3);

            LuaDLL.lua_pushstring(L, "EndSampleCustom");
            LuaDLL.lua_pushstdcallcfunction(L, endSample);
            LuaDLL.lua_rawset(L, -3);

            LuaDLL.lua_rawset(L, -3);
            LuaDLL.lua_setglobal(L, "MikuLuaProfiler");

            LuaDLL.lua_pushstdcallcfunction(L, unpackReturnValue);
            LuaDLL.lua_setglobal(L, "miku_unpack_return_value");

            LuaDLL.lua_pushstdcallcfunction(L, addRefFunInfo);
            LuaDLL.lua_setglobal(L, "miku_add_ref_fun_info");

            LuaDLL.lua_pushstdcallcfunction(L, removeRefFunInfo);
            LuaDLL.lua_setglobal(L, "miku_remove_ref_fun_info");

            LuaDLL.lua_pushstdcallcfunction(L, checkType);
            LuaDLL.lua_setglobal(L, "miku_check_type");

            LuaDLL.lua_pushstdcallcfunction(L, handleError);
            LuaDLL.lua_setglobal(L, "miku_handle_error");

            LuaDLL.lua_newtable(L);
            LuaDLL.lua_setglobal(L, "MikuLuaProfilerStrTb");

            LuaLib.DoString(L, get_ref_string);
            LuaLib.DoString(L, null_script);
            LuaLib.DoString(L, diff_script);
        }

#region script
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
        local metaTb = getmetatable(tb)
        if metaTb and miku_check_type(metaTb) == 2 and rawget(metaTb, '__tostring') then
            tostringFun = rawget(metaTb, '__tostring')
            rawset(metaTb, '__tostring', nil)
        end
        local addStr = tostring(tb)
        if tostringFun then
            rawset(getmetatable(tb), '__tostring', tostringFun)
        end
        result = rawget(tb, '__name') or rawget(tb, 'name') or rawget(tb, '__cname') or rawget(tb, '.name')
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
local cache_key = 'miku_record_prefix_cache'

function miku_do_record(val, prefix, key, record, history, null_list)
    if val == infoTb then
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

    if typeStr == 'table' then
        local isEmpty = true
        for k,v in pairs(val) do
            isEmpty = false
            break
        end
        if isEmpty then return end
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
                if null_list[val] == nil then
                    null_list[val] = { }
                end
                table.insert(null_list[val], tmp_prefix)
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

    local prefix_cache
    if history == nil then
        if record[cache_key] == nil then
            record[cache_key] = {}
        end
        prefix_cache = record[cache_key]
        prefix_cache[tmp_prefix] = tmp_prefix
        if record[val] == nil then
            record[val] = {}
        end
        table.insert(record[val], tmp_prefix)
    else
        prefix_cache = history[cache_key]
        if prefix_cache[tmp_prefix] == nil or history[val] then
            if record[val] == nil then
                record[val] = { }
            end
            table.insert(record[val], tmp_prefix)
        end
    end

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
    setmetatable(null_list, weak_meta_key_table)
    miku_do_record(_G, '', '_G', add, record, null_list)
    miku_do_record(debug.getregistry(), '', '_R', add, record, null_list)
    local rm = { }

    for key, val in pairs(record) do
        if not add[key] and key ~= cache_key then
            rm[key] = val
        else
            add[key] = nil
        end
    end

    return add,rm,null_list
end";
#endregion

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int BeginSample(IntPtr L)
        {
            LuaProfiler.BeginSample(L, LuaHook.GetRefString(L, 1));
            return 0;
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int BeginSampleCustom(IntPtr L)
        {
            LuaProfiler.BeginSample(L, LuaHook.GetRefString(L, 1), true);
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

}
#endif