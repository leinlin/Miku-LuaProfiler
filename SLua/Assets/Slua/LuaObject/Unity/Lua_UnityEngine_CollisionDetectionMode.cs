using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_CollisionDetectionMode : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.CollisionDetectionMode");
		addMember(l,0,"Discrete");
		addMember(l,1,"Continuous");
		addMember(l,2,"ContinuousDynamic");
		LuaDLL.lua_pop(l, 1);
	}
}
