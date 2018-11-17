using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_FocusType : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.FocusType");
		addMember(l,0,"Native");
		addMember(l,1,"Keyboard");
		addMember(l,2,"Passive");
		LuaDLL.lua_pop(l, 1);
	}
}
