using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_Rendering_ReflectionProbeMode : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.Rendering.ReflectionProbeMode");
		addMember(l,0,"Baked");
		addMember(l,1,"Realtime");
		addMember(l,2,"Custom");
		LuaDLL.lua_pop(l, 1);
	}
}
