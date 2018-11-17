using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_Rendering_ReflectionProbeClearFlags : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.Rendering.ReflectionProbeClearFlags");
		addMember(l,1,"Skybox");
		addMember(l,2,"SolidColor");
		LuaDLL.lua_pop(l, 1);
	}
}
