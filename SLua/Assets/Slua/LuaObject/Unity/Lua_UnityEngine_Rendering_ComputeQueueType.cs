using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_Rendering_ComputeQueueType : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.Rendering.ComputeQueueType");
		addMember(l,0,"Default");
		addMember(l,1,"Background");
		addMember(l,2,"Urgent");
		LuaDLL.lua_pop(l, 1);
	}
}
