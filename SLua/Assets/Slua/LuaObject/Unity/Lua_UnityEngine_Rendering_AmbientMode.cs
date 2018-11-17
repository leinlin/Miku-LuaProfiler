using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_Rendering_AmbientMode : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.Rendering.AmbientMode");
		addMember(l,0,"Skybox");
		addMember(l,1,"Trilight");
		addMember(l,3,"Flat");
		addMember(l,4,"Custom");
		LuaDLL.lua_pop(l, 1);
	}
}
