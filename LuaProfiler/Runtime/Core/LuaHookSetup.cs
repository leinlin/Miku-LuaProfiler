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
                既见未来为何不拜                
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
using System.Threading;
using UnityEngine;

namespace MikuLuaProfiler
{

    #region monobehaviour
    public class HookLuaSetup : MonoBehaviour
    {
        #region field
        public static LuaDeepProfilerSetting setting { private set; get; }

        public float showTime = 1f;
        private int count = 0;
        private float deltaTime = 0f;

        public const float DELTA_TIME = 0.1f;
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
#if UNITY_EDITOR_OSX
            return;
#endif
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            
            if (isInite) return;
#if !UNITY_EDITOR
            {
                GameObject go = new GameObject();
                go.name = "MikuLuaProfiler OpenMenu";
                go.hideFlags = HideFlags.HideAndDontSave;
                DontDestroyOnLoad(go);
                go.AddComponent<OpenMenu>();
            }
            if (!LuaProfiler.CheckServerIsOpen())
            {
                return;       
            }
#endif
            isInite = true;
            setting = LuaDeepProfilerSetting.Instance;
            LuaProfiler.mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            
            if (setting.isDeepLuaProfiler || !setting.isLocal)
            {
#if UNITY_EDITOR
                if (!setting.ProfilerWinOpen)
                {
                    return;
                }
#endif
                Debug.Log("<color=#00ff00>OnStartGame</color>");
                LuaDLL.Uninstall();
                IntPtr LuaModule = LuaDLL.CheckHasLuaDLL();
                if (LuaModule != IntPtr.Zero)
                {
                    LuaDLL.BindEasyHook(LuaModule);
                }
                else
                {
                    LuaDLL.HookLoadLibrary();
                }
            }

            if (setting.isDeepLuaProfiler || setting.isCleanMode || !setting.isLocal)
            {
#if UNITY_EDITOR
                if (!setting.ProfilerWinOpen)
                {
                    return;
                }
#endif
                GameObject go = new GameObject();
                go.name = "MikuLuaProfiler";
                go.hideFlags = HideFlags.HideAndDontSave;
                DontDestroyOnLoad(go);
                go.AddComponent<HookLuaSetup>();
                if (!setting.isLocal)
                {
                    {
                        NetWorkMgr.BeginListen("0.0.0.0", setting.port);
#if !UNITY_EDITOR
                        while (!NetWorkMgr.CheckIsConnected())
                        {
                            Thread.Sleep(100);
                        }
#endif
                    }


                }
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
            SampleData.frameCount = Time.frameCount;
            count++;
            deltaTime += Time.unscaledDeltaTime;
            if (deltaTime >= showTime)
            {
                SampleData.fps = count / deltaTime;
                count = 0;
                deltaTime = 0f;
            }
            LuaProfiler.SendFrameSample();
        }

        private void OnApplicationQuit()
        {
#if UNITY_EDITOR
            desotryCount = 0;
            Destroy(gameObject);
            UnityEditor.EditorApplication.update += WaitDestory;
#endif
            NetWorkMgr.Close();
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
                LuaDLL.Uninstall();
                LuaProfiler.mainL = IntPtr.Zero;
                NetWorkMgr.Close();
                desotryCount = 0;
            }
        }
#endif
    }

    public class OpenMenu : MonoBehaviour
    {
        private bool needShowMenu = false;
        private int count = 0;

        private float recordTime = 0;
        private float DELTA_TIME = 2;
        private void LateUpdate()
        {
            if (Input.touchCount == 4 || Input.GetKeyDown(KeyCode.Delete))
            {
                count++;
                if (count >= 5)
                {
                    count = 0;
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
            if (Time.time - recordTime > DELTA_TIME)
            {
                count = 0;
                recordTime = Time.time;
            }
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

            if (GUI.Button(new Rect(0, 0, 200, 100), "Open Lua Profiler"))
            {
                LuaProfiler.OpenServer();
            }
            
            if (GUI.Button(new Rect(220, 0, 200, 100), "Close Lua Profiler"))
            {
                LuaProfiler.CloseServer();
            }
            
            if (GUI.Button(new Rect(440, 0, 200, 100), "Hide Menu"))
            {
                enabled = false;
            }
            
            if (GUI.Button(new Rect(0, 150, 200, 100), "Quit Game"))
            {
                Application.Quit();
            }
        }
    }
    #endregion

    public class LuaHook
    {
        public static byte[] Hookloadbuffer(IntPtr L, byte[] buff, string name)
        {
            if (LuaDeepProfilerSetting.Instance.isCleanMode)
            {
                return buff;
            }
            if (!LuaDLL.isHook)
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
            // utf-8
            if (buff[0] == 239 && buff[1] == 187 && buff[2] == 191)
            {
                value = Encoding.UTF8.GetString(buff, 3, buff.Length - 3);
            }
            else
            {
                value = Encoding.UTF8.GetString(buff);
            }
            string fileName = name.Replace(".lua", "");
            if (name == value)
            {
                fileName = "chunk";
            }
            fileName = fileName.Replace("@", "").Replace('.', '/').Replace('\\', '/');
            hookedValue = Parse.InsertSample(value, fileName);
            buff = Encoding.UTF8.GetBytes(hookedValue);
            return buff;
        }

        public static void HookRef(IntPtr L, int reference)
        {
            if (LuaDLL.isHook)
            {
                LuaLib.DoRefLuaFun(L, "lua_miku_add_ref_fun_info", reference);
            }
        }

        public static void HookUnRef(IntPtr L, int reference)
        {
            if (LuaDLL.isHook)
            {
                LuaLib.DoRefLuaFun(L, "lua_miku_remove_ref_fun_info", reference);
            }
        }

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
            int len = LuaDLL.lua_objlen(L, -1);
            LuaDLL.lua_pushnumber(L, len + 1);
            LuaDLL.lua_pushvalue(L, index);
            LuaDLL.lua_rawset(L, -3);

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

        #region check
        public static int staticHistoryRef = -100;
        public static LuaDiffInfo RecordStatic()
        {
            IntPtr L = LuaProfiler.mainL;
            if (L == IntPtr.Zero)
            {
                return null;
            }
            LuaDLL.isHook = false;

            ClearStaticRecord();
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
            staticHistoryRef = LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
            LuaDLL.lua_getref(L, staticHistoryRef);
            //history
            LuaDLL.lua_pushnil(L);
            //null_list
            LuaDLL.lua_newtable(L);
            LuaDLL.lua_pushvalue(L, -1);
            int nullObjectRef = LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
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
            LuaDLL.lua_getref(L, staticHistoryRef);
            //history
            LuaDLL.lua_pushnil(L);
            //null_list
            LuaDLL.lua_getref(L, nullObjectRef);

            if (LuaDLL.lua_pcall(L, 6, 0, oldTop + 1) == 0)
            {
                LuaDLL.lua_remove(L, oldTop + 1);
            }
            LuaDLL.lua_settop(L, oldTop);

            LuaDiffInfo ld = LuaDiffInfo.Create();
            SetTable(nullObjectRef, ld.nullRef, ld.nullDetail);

            LuaDLL.lua_unref(L, nullObjectRef);
            LuaDLL.isHook = true;
            return ld;
        }

        public static int historyRef = -100;
        public static LuaDiffInfo Record()
        {
            IntPtr L = LuaProfiler.mainL;
            if (L == IntPtr.Zero)
            {
                return null;
            }
            LuaDLL.isHook = false;

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
            LuaDLL.lua_pushvalue(L, -1);
            int nullObjectRef = LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
            LuaDLL.lua_getref(L, staticHistoryRef);
            if (LuaDLL.lua_pcall(L, 7, 0, oldTop + 1) == 0)
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
            LuaDLL.lua_getref(L, nullObjectRef);
            LuaDLL.lua_getref(L, staticHistoryRef);
            if (LuaDLL.lua_pcall(L, 7, 0, oldTop + 1) == 0)
            {
                LuaDLL.lua_remove(L, oldTop + 1);
            }
            LuaDLL.lua_settop(L, oldTop);

            LuaDiffInfo ld = LuaDiffInfo.Create();
            SetTable(nullObjectRef, ld.nullRef, ld.nullDetail);

            LuaDLL.lua_unref(L, nullObjectRef);
            LuaDLL.isHook = true;
            return ld;
        }

        public static void ClearStaticRecord()
        {
            IntPtr L = LuaProfiler.mainL;
            if (L == IntPtr.Zero)
            {
                return;
            }
            if (staticHistoryRef != -100)
            {
                LuaDLL.lua_unref(L, staticHistoryRef);
                staticHistoryRef = -100;
            }
        }

        public static void ClearRecord()
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

        public static void ClearDiffCache()
        {
            IntPtr L = LuaProfiler.mainL;
            if (L == IntPtr.Zero)
            {
                return;
            }

            int oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_getglobal(L, "miku_handle_error");

            LuaDLL.lua_getglobal(L, "miku_clear_diff_cache");
            if (LuaDLL.lua_pcall(L, 0, 0, oldTop + 1) == 0)
            {
                LuaDLL.lua_remove(L, oldTop + 1);
            }
            LuaDLL.lua_settop(L, oldTop);
            LuaDLL.lua_gc(L, LuaGCOptions.LUA_GCCOLLECT, 0);
        }

        private static void SetTable(int refIndex, Dictionary<LuaTypes, HashSet<string>> dict, Dictionary<string, List<string>> detailDict)
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
                    HashSet<string> list;
                    LuaTypes luaType = (LuaTypes)LuaDLL.lua_type(L, -2);
                    if (!dict.TryGetValue(luaType, out list))
                    {
                        list = new HashSet<string>();
                        dict.Add(luaType, list);
                    }
                    if (!list.Contains(firstKey))
                    {
                        list.Add(firstKey);
                    }
                    detailDict[firstKey] = detailList;
                }

                /* 移除 'value' ；保留 'key' 做下一次迭代 */
                LuaDLL.lua_pop(L, 1);
            }
            LuaDLL.lua_settop(L, oldTop);
        }

        // public static void DiffServer()
        // {
        //     NetWorkClient.SendMessage(Diff());
        // }
        //
        // public static void MarkRecordServer()
        // {
        //     NetWorkClient.SendMessage(Record());
        // }
        //
        // public static void MarkStaticServer()
        // {
        //     NetWorkClient.SendMessage(Record());
        // }

        public static LuaDiffInfo Diff()
        {
            IntPtr L = LuaProfiler.mainL;
            if (L == IntPtr.Zero)
            {
                return null;
            }
            LuaDLL.isHook = false;
            Resources.UnloadUnusedAssets();
            // 调用C# LuaTable LuaFunction WeakTable的析构 来清理掉lua的 ref
            GC.Collect();
            // 清理掉C#强ref后，顺便清理掉很多弱引用
            LuaDLL.lua_gc(L, LuaGCOptions.LUA_GCCOLLECT, 0);


            if (staticHistoryRef == -100)
            {
                Debug.LogError("has no history");
                return null;
            }

            if (historyRef == -100)
            {
                Debug.LogError("has no history");
                return null;
            }

            int oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_getglobal(L, "miku_handle_error");

            LuaDLL.lua_getglobal(L, "miku_diff");
            LuaDLL.lua_getref(L, historyRef);
            LuaDLL.lua_getref(L, staticHistoryRef);
            if (LuaDLL.lua_type(L, -1) != LuaTypes.LUA_TTABLE &&
                LuaDLL.lua_type(L, -2) != LuaTypes.LUA_TTABLE)
            {
                Debug.LogError(LuaDLL.lua_type(L, -1));
                LuaDLL.lua_settop(L, oldTop);
                historyRef = -100;
                staticHistoryRef = -100;
                return null;
            }

            if (LuaDLL.lua_pcall(L, 2, 3, oldTop + 1) == 0)
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

            LuaDLL.isHook = true;

            return ld;
        }
        #endregion
    }

    public class LuaLib
    {
        public static long GetLuaMemory(IntPtr luaState)
        {
            return LuaDLL.GetAllocSize();
        }
        public static void DoString(IntPtr L, string script)
        {
            LuaDLL.isHook = false;
            byte[] chunk = Encoding.UTF8.GetBytes(script);
            int oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_getglobal(L, "miku_handle_error");
            if (LuaDLL.luaL_loadbufferUnHook(L, chunk, (IntPtr)chunk.Length, "chunk") == 0)
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
            LuaDLL.isHook = true;
            LuaDLL.lua_settop(L, oldTop);
        }
        public static void DoRefLuaFun(IntPtr L, string funName, int reference)
        {
            int moreOldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_getref(L, reference);

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
        public static LuaCSFunction beginSample = new LuaCSFunction(BeginSample);
        public static LuaCSFunction beginSampleCustom = new LuaCSFunction(BeginSampleCustom);
        public static LuaCSFunction endSample = new LuaCSFunction(EndSample);
        public static LuaCSFunction endSampleCustom = new LuaCSFunction(EndSampleCustom);
        public static LuaCSFunction getStackDepth = new LuaCSFunction(GetStackDepth);
        public static LuaCSFunction endSampleCortoutine = new LuaCSFunction(EndSampleCortoutine);
        public static LuaCSFunction unpackReturnValue = new LuaCSFunction(UnpackReturnValue);
        public static LuaCSFunction addRefFunInfo = new LuaCSFunction(AddRefFunInfo);
        public static LuaCSFunction removeRefFunInfo = new LuaCSFunction(RemoveRefFunInfo);
        public static LuaCSFunction checkType = new LuaCSFunction(CheckType);
        public static LuaCSFunction handleError = new LuaCSFunction(HandleError);
        public static void __Register(IntPtr L)
        {
            LuaDLL.lua_getglobal(L, "MikuLuaProfiler");
            if(!LuaDLL.lua_isnil(L, -1))
            {
                LuaDLL.lua_pop(L, 1);
                return;
            }
            LuaDLL.lua_pop(L, 1);

            IntPtr tempL = LuaDLL.luaL_newstate();
            LuaDLL.lua_close(tempL);
            
            RegisterError(L);

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
            LuaDLL.lua_pushstdcallcfunction(L, endSampleCustom);
            LuaDLL.lua_rawset(L, -3);

            LuaDLL.lua_pushstring(L, "GetStackDepth");
            LuaDLL.lua_pushstdcallcfunction(L, getStackDepth);
            LuaDLL.lua_rawset(L, -3);
            
            LuaDLL.lua_pushstring(L, "EndSampleCortoutine");
            LuaDLL.lua_pushstdcallcfunction(L, endSampleCortoutine);
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

            LuaDLL.lua_newtable(L);
            LuaDLL.lua_setglobal(L, "MikuLuaProfilerStrTb");

            LuaLib.DoString(L, get_ref_string);
            LuaLib.DoString(L, null_script);
            LuaLib.DoString(L, diff_script);
        }

        private static void RegisterError(IntPtr L)
        {
            LuaDLL.lua_pushstdcallcfunction(L, handleError);
            LuaDLL.lua_setglobal(L, "miku_handle_error");
        }

#region script
        const string get_ref_string = @"

local weak_meta_table = {__mode = 'k'}
local infoTb = {}
setmetatable(infoTb, weak_meta_table)
local funAddrTb = {}
setmetatable(funAddrTb, weak_meta_table)

function miku_get_fun_info(fun)
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
        lua = lua .. '[' .. tostring(tostring(k)) .. ']=' .. tostring(tostring(v)) .. ',\n'
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
        result = rawget(tb, '__name') or rawget(tb, 'name') or rawget(tb, '__cname') or rawget(tb, '.name')
        if not result then
            result = serialize(tb)
        end
        if tostringFun then
            rawset(getmetatable(tb), '__tostring', tostringFun)
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

function miku_clear_diff_cache(fun)
    infoTb = {}
end

local cache_key = 'miku_record_prefix_cache'

local BeginMikuSample = MikuLuaProfiler.LuaProfiler.BeginSample
local EndMikuSample = miku_unpack_return_value
local GetStackDepth = MikuLuaProfiler.LuaProfiler.GetStackDepth
local EndSampleCortoutine = MikuLuaProfiler.LuaProfiler.EndSampleCortoutine

local oldResume = coroutine.resume
local oldWrap = coroutine.wrap
local rawequal = rawequal
local getinfo = debug.getinfo

local weakKeyMeta = { __mode = 'k' }
local coroutineInfoTb = setmetatable({}, weakKeyMeta)
local coroutineTickTb = setmetatable({}, weakKeyMeta)

local function recordCoroutineTick(co, tick, ...)
    coroutineTickTb[co] = tick
    return ...
end

coroutine.resume = function(co, ...)
    local coInfo = coroutineInfoTb[co]
    if not coInfo then
        local info = getinfo(co, 1, 'nSl')
        if info then
            coInfo = string.format('[lua]:coroutine.resume %s,%s&line:%d', info.name, info.short_src, info.currentline)
            coroutineInfoTb[co] = coInfo
        else
            coInfo = '[lua]:coroutine.resume'
        end
    end

    local stackDepth = GetStackDepth()
    local tick = coroutineTickTb[co]
    if tick then
      for i = 1, tick do
        BeginMikuSample(coInfo)
      end
    end
    return recordCoroutineTick(co, EndSampleCortoutine(stackDepth, oldResume(co, ...)))
end

coroutine.wrap = function(co)
    local coInfo = coroutineInfoTb[co]
    if not coInfo then
        local info = getinfo(co, 'nSl')
        if info then
            coInfo = string.format('[lua]:coroutine.resume %s,%s&line:%d', info.name, info.short_src, info.currentline)
            coroutineInfoTb[co] = coInfo
        else
            coInfo = '[lua]:coroutine.resume'
        end
    end

    local result = oldWrap(co)
    return function()
        local stackDepth = GetStackDepth()
        local tick = coroutineTickTb[co]
        if tick then
          for i = 1, tick do
            BeginMikuSample(coInfo)
          end
        end
        return recordCoroutineTick(co, EndSampleCortoutine(stackDepth, result()))
    end
end

function miku_do_record(val, prefix, key, record, history, null_list, staticRecord)
    if rawequal(val, staticRecord) then
        return
    end
    if rawequal(val, infoTb) then
        return
    end
    if rawequal(val,miku_do_record) then
        return
    end
    if rawequal(val,coroutineInfoTb) then
        return
    end
    if rawequal(val,miku_diff) then
        return
    end
    if rawequal(val, lua_miku_remove_ref_fun_info) then
        return
    end
    if rawequal(val, lua_miku_add_ref_fun_info) then
        return
    end
    if rawequal(val, history) then
        return
    end
    if rawequal(val, record) then
        return
    end
    if rawequal(val, miku_clear_diff_cache) then
        return
    end
    if rawequal(val, miku_get_fun_info) then
        return
    end
    if rawequal(val, MikuLuaProfilerStrTb) then
        return
    end
    if rawequal(val, null_list) then
        return
    end
    if rawequal(val, coroutine) then
        return
    end

    if getmetatable(record) ~= weak_meta_key_table then
        setmetatable(record, weak_meta_key_table)
    end

    local typeStr = type(val)
    if typeStr ~= 'table' and typeStr ~= 'userdata' and typeStr ~= 'function' then
        return
    end

    local tmp_prefix
    local strKey = tostring(key)
    if not strKey then
        strKey = 'empty'
    end
    local prefixTb = infoTb[prefix]
    if not prefixTb then
        prefixTb = {}
        infoTb[prefix] = prefixTb
    end
    tmp_prefix = prefixTb[strKey]
    if not tmp_prefix then
        tmp_prefix = prefix.. (prefix == '' and '' or '.') .. string.format('[%s]', strKey)
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
        table.insert(record[val], tmp_prefix)
        return
    end
    
    if not record[val] then
        record[val] = {}
        if typeStr == 'function' then
            local funUrl = miku_get_fun_info(val)
            table.insert(record[val], funUrl)
        end
        table.insert(record[val], tmp_prefix)
    end

    if typeStr == 'table' then
        for k,v in pairs(val) do
            local typeKStr = type(k)
            local typeVStr = type(v)
            local key = k
            if typeKStr == 'table' or typeKStr == 'userdata' or typeKStr == 'function' then
                key = 'table:'
                miku_do_record(k, tmp_prefix, 'table:', record, history, null_list, staticRecord)
            end
            miku_do_record(v, tmp_prefix, key, record, history, null_list, staticRecord)
        end

    elseif typeStr == 'function' then
        if val ~= lua_miku_add_ref_fun_info and val ~= lua_miku_remove_ref_fun_info then
            local i = 1
            while true do
                local k, v = debug.getupvalue(val, i)
                if not k then
                    break
                end
                if v and k ~= 'MikuSample' then
                    local funPrefix = miku_get_fun_info(val)
                    miku_do_record(v, funPrefix, k, record, history, null_list, staticRecord)
                end
                i = i + 1
            end
        end
    end

    local metaTable = getmetatable(val)
    if metaTable then
        miku_do_record(metaTable, tmp_prefix, 'metaTable', record, history, null_list, staticRecord)
    end
end

-- staticRecord为打开UI前的快照， record为打开UI后的快照，add为关闭并释放UI后的快照
function miku_diff(record, staticRecord)
    local add = { }
    setmetatable(add, weak_meta_key_table)
    local null_list = { }
    setmetatable(null_list, weak_meta_key_table)
    miku_do_record(_G, '', '_G', add, record, null_list, staticRecord)
    miku_do_record(debug.getregistry(), '', '_R', add, record, null_list, staticRecord)
    local remain = { }

    for key, val in pairs(record) do
        if not add[key] and  not rawequal(key, cache_key) then
        else
            -- 如果打开UI前的快照没有这个数据
            -- 但是打开UI后及关闭并释放UI后的快照都拥有这个数据，视为泄漏
            if not staticRecord[key] 
			    and not rawequal(key, staticRecord) 
			    and not rawequal(key, cache_key) then
                remain[key] = val
            end
            add[key] = nil
        end
    end

    return add,remain,null_list
end

";
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
            int oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_getglobal(L, "debug");
            LuaDLL.lua_getfield(L, -1, "traceback");
            LuaDLL.lua_remove(L, -2);
            LuaDLL.lua_pushvalue(L, 1);
            LuaDLL.lua_pushnumber(L, 2);
            LuaDLL.lua_call(L, 2, 1);
            string debugInfo = LuaDLL.lua_tostring(L, -1);

            string error = LuaHook.GetRefString(L, 1);
            LuaDLL.lua_settop(L, oldTop);

            Debug.LogError(string.Format("{0}\n{1}", error, debugInfo));
            return 0;
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int EndSampleCustom(IntPtr L)
        {
            LuaProfiler.EndSample(L);
            return 0; 
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int EndSample(IntPtr L)
        {
            LuaProfiler.EndSample(L);
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int EndSampleCortoutine(IntPtr L)
        {
            LuaProfiler.EndSampleCortoutine(L);
            return LuaDLL.lua_gettop(L);
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int GetStackDepth(IntPtr L)
        {
            LuaDLL.lua_pushnumber(L, LuaProfiler.GetStackDepth());
            return 1;
        }

    }
    #endregion

}
#endif
