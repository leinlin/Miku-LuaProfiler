using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_GradientMode : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.GradientMode");
		addMember(l,0,"Blend");
		addMember(l,1,"Fixed");
		LuaDLL.lua_pop(l, 1);
	}
}
