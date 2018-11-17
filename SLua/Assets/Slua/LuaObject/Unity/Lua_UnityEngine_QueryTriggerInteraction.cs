using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_QueryTriggerInteraction : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.QueryTriggerInteraction");
		addMember(l,0,"UseGlobal");
		addMember(l,1,"Ignore");
		addMember(l,2,"Collide");
		LuaDLL.lua_pop(l, 1);
	}
}
