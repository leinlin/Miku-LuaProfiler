using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_ForceMode : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.ForceMode");
		addMember(l,0,"Force");
		addMember(l,1,"Impulse");
		addMember(l,2,"VelocityChange");
		addMember(l,5,"Acceleration");
		LuaDLL.lua_pop(l, 1);
	}
}
