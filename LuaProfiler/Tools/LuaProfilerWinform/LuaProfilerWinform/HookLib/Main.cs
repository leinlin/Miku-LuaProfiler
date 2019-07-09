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
* Filename: NetWorkClient
* Created:  2018/7/13 14:29:22
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/

using System;
using System.Runtime.InteropServices;
using EasyHook;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;

namespace MikuLuaProfiler_Winform
{
    [Serializable]
    public class HookParameter
    {
        public string Msg { get; set; }
        public int HostProcessId { get; set; }
    }

    public class Main : IEntryPoint
    {
        #region field
        public static HookServer server = null;
        public LocalHook MessageBoxWHook = null;
        public LocalHook MessageBoxAHook = null;
        public static int frameCount { private set; get; }
        #endregion

        public void Uninstall()
        {
            MessageBox.Show("fuck you");
            NativeAPI.LhUninstallAllHooks();
        }

        public Main(
            RemoteHooking.IContext context,
            string channelName
            , HookParameter parameter
            )
        {
        }

        public void Run( 
            RemoteHooking.IContext context,
            string channelName
            , HookParameter parameter
            )
        {
            frameCount = 0;
            server = RemoteHooking.IpcConnectClient<HookServer>(channelName);
            server.isHook = false;
            try
            {
                var process = Process.GetCurrentProcess();
                var modules = process.Modules;
                foreach (ProcessModule item in modules)
                {
                    string moduleName = item.ModuleName;
                    try
                    {
                        if (LocalHook.GetProcAddress(moduleName, "luaL_newstate") != IntPtr.Zero)
                        {
                            HookAllLuaFun(moduleName);
                            break;
                        }
                    }
                    catch
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            server.isHook = true;
            while (true)
            {
                Thread.Sleep(10);
                frameCount++;
                if (!server.isHook)
                {
                    Uninstall();
                    break;
                }
            }

        }

        #region hook lua fun
        public static void HookAllLuaFun(string moduleName)
        {
            IntPtr luaL_newstate_handle = LocalHook.GetProcAddress(moduleName, "luaL_newstate");
            LuaDLL.luaL_newstate_hook = LocalHook.Create(luaL_newstate_handle, new LuaDLL.luaL_newstate_fun(LuaDLL.luaL_newstate_hooked), null);
            LuaDLL.luaL_newstate_hook.ThreadACL.SetExclusiveACL(new int[1]);
            LuaDLL.luaL_newstate = (LuaDLL.luaL_newstate_fun)Marshal.GetDelegateForFunctionPointer(luaL_newstate_handle, typeof(LuaDLL.luaL_newstate_fun));

            IntPtr lua_close_handle = LocalHook.GetProcAddress(moduleName, "lua_close");
            LuaDLL.lua_close_hook = LocalHook.Create(lua_close_handle, new LuaDLL.lua_close_fun(LuaDLL.lua_close_hooked), null);
            LuaDLL.lua_close_hook.ThreadACL.SetExclusiveACL(new int[1]);
            LuaDLL.lua_close = (LuaDLL.lua_close_fun)Marshal.GetDelegateForFunctionPointer(lua_close_handle, typeof(LuaDLL.lua_close_fun));

            IntPtr lua_close_handle = LocalHook.GetProcAddress(moduleName, "lua_close");
            LuaDLL.lua_close_hook = LocalHook.Create(lua_close_handle, new LuaDLL.lua_close_fun(LuaDLL.lua_close_hooked), null);
            LuaDLL.lua_close_hook.ThreadACL.SetExclusiveACL(new int[1]);
            LuaDLL.lua_close = (LuaDLL.lua_close_fun)Marshal.GetDelegateForFunctionPointer(lua_close_handle, typeof(LuaDLL.lua_close_fun));
        }
        #endregion

    }
}
