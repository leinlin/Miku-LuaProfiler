using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_TexGenMode : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.TexGenMode");
		addMember(l,0,"None");
		addMember(l,1,"SphereMap");
		addMember(l,2,"Object");
		addMember(l,3,"EyeLinear");
		addMember(l,4,"CubeReflect");
		addMember(l,5,"CubeNormal");
		LuaDLL.lua_pop(l, 1);
	}
}
