using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_EffectorForceMode2D : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.EffectorForceMode2D");
		addMember(l,0,"Constant");
		addMember(l,1,"InverseLinear");
		addMember(l,2,"InverseSquared");
		LuaDLL.lua_pop(l, 1);
	}
}
