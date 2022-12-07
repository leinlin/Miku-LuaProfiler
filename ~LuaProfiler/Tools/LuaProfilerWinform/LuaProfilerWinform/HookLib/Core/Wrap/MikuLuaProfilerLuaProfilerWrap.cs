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
* Filename: MikuLuaProfilerLuaProfilerWrap.cs
* Created:  2018/7/13 14:29:22
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/

using System;
using System.Windows.Forms;

namespace MikuLuaProfiler
{
    public class MikuLuaProfilerLuaProfilerWrap
    {
        public static LuaCSFunction beginSample = new LuaCSFunction(BeginSample);
        public static LuaCSFunction beginSampleCustom = new LuaCSFunction(BeginSampleCustom);
        public static LuaCSFunction endSample = new LuaCSFunction(EndSample);
        public static LuaCSFunction unpackReturnValue = new LuaCSFunction(UnpackReturnValue);
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

            LuaDLL.lua_pushstdcallcfunction(L, checkType);
            LuaDLL.lua_setglobal(L, "miku_check_type");

            LuaDLL.lua_pushstdcallcfunction(L, handleError);
            LuaDLL.lua_setglobal(L, "miku_handle_error");

            LuaDLL.lua_newtable(L);
            LuaDLL.lua_setglobal(L, "MikuLuaProfilerStrTb");
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int BeginSample(IntPtr L)
        {
            LuaProfiler.BeginSample(L, LuaDLL.GetRefString(L, 1));
            return 0;
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int BeginSampleCustom(IntPtr L)
        {
            LuaProfiler.BeginSample(L, LuaDLL.GetRefString(L, 1), true);
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
        static int HandleError(IntPtr L)
        {
            string error = LuaDLL.GetRefString(L, 1);
            MessageBox.Show(error);
            return 0;
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int EndSample(IntPtr L)
        {
            LuaProfiler.EndSample(L);
            return 0;
        }
    }
}
