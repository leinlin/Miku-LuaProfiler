using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_Rendering_DefaultReflectionMode : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.Rendering.DefaultReflectionMode");
		addMember(l,0,"Skybox");
		addMember(l,1,"Custom");
		LuaDLL.lua_pop(l, 1);
	}
}
