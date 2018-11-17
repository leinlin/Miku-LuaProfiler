using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_ColorSpace : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.ColorSpace");
		addMember(l,0,"Gamma");
		addMember(l,1,"Linear");
		addMember(l,-1,"Uninitialized");
		LuaDLL.lua_pop(l, 1);
	}
}
