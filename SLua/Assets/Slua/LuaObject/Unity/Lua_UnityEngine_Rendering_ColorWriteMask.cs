using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_Rendering_ColorWriteMask : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.Rendering.ColorWriteMask");
		addMember(l,1,"Alpha");
		addMember(l,2,"Blue");
		addMember(l,4,"Green");
		addMember(l,8,"Red");
		addMember(l,15,"All");
		LuaDLL.lua_pop(l, 1);
	}
}
