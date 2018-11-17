using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_RigidbodySleepMode2D : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.RigidbodySleepMode2D");
		addMember(l,0,"NeverSleep");
		addMember(l,1,"StartAwake");
		addMember(l,2,"StartAsleep");
		LuaDLL.lua_pop(l, 1);
	}
}
