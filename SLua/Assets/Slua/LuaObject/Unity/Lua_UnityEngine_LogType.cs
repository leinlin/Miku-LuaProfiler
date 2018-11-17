using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_LogType : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.LogType");
		addMember(l,0,"Error");
		addMember(l,1,"Assert");
		addMember(l,2,"Warning");
		addMember(l,3,"Log");
		addMember(l,4,"Exception");
		LuaDLL.lua_pop(l, 1);
	}
}
