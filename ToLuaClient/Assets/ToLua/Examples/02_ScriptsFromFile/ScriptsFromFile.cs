using UnityEngine;
using System.Collections;
using LuaInterface;
using System;
using System.IO;

//展示searchpath 使用，require 与 dofile 区别
public class ScriptsFromFile : MonoBehaviour 
{
    LuaState lua = null;
    private string strLog = "";    

	void Start () 
    {
#if UNITY_5 || UNITY_2017 || UNITY_2018		
        Application.logMessageReceived += Log;
#else
        Application.RegisterLogCallback(Log);
#endif         
        lua = new LuaState();                
        lua.Start();
		LuaBinder.Bind(lua);
        //如果移动了ToLua目录，自己手动修复吧，只是例子就不做配置了
        string fullPath = Application.dataPath + "\\ToLua/Examples/02_ScriptsFromFile";
        lua.AddSearchPath(fullPath);
		lua.DoString(@"
local GameObject = UnityEngine.GameObject

fk1 = 
{
    k1 = 
    {
        kk1 = GameObject('go')
    },
    k2 = {}
}

kk = {}

local x = fk1.k2
function test() 
    return xZ
end

function freex()
    x = nil
end

go = GameObject('go')
");
    }

    [ContextMenu("destory")]
    void Free()
    {
        MikuLuaProfiler.LuaHook.Record();
        lua.DoString(@"
local GameObject = UnityEngine.GameObject
kk = nil
freex()
fk1.k2 = nil
GameObject.Destroy(fk1.k1.kk1)
GameObject.Destroy(go)
go = nil
ab = {}
ab.ab1 = GameObject('go')
");
        MikuLuaProfiler.LuaHook.Diff();
    }

    [ContextMenu("diff")]
    void Diff()
    {
        lua.DoString(@"
collectgarbage('collect')
");
        MikuLuaProfiler.LuaHook.Diff();
    }
    void Log(string msg, string stackTrace, LogType type)
    {
        strLog += msg;
        strLog += "\r\n";
    }

    void OnApplicationQuit()
    {
        lua.Dispose();
        lua = null;
#if UNITY_5 || UNITY_2017 || UNITY_2018	
        Application.logMessageReceived -= Log;
#else
        Application.RegisterLogCallback(null);
#endif 
    }
}
