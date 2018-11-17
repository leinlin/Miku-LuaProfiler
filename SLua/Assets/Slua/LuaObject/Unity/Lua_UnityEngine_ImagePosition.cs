using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_ImagePosition : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.ImagePosition");
		addMember(l,0,"ImageLeft");
		addMember(l,1,"ImageAbove");
		addMember(l,2,"ImageOnly");
		addMember(l,3,"TextOnly");
		LuaDLL.lua_pop(l, 1);
	}
}
