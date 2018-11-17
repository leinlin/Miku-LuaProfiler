using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_FilterMode : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.FilterMode");
		addMember(l,0,"Point");
		addMember(l,1,"Bilinear");
		addMember(l,2,"Trilinear");
		LuaDLL.lua_pop(l, 1);
	}
}
