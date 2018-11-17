using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_RigidbodyInterpolation : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.RigidbodyInterpolation");
		addMember(l,0,"None");
		addMember(l,1,"Interpolate");
		addMember(l,2,"Extrapolate");
		LuaDLL.lua_pop(l, 1);
	}
}
