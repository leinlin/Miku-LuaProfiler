using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_Rendering_ShadowCastingMode : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.Rendering.ShadowCastingMode");
		addMember(l,0,"Off");
		addMember(l,1,"On");
		addMember(l,2,"TwoSided");
		addMember(l,3,"ShadowsOnly");
		LuaDLL.lua_pop(l, 1);
	}
}
