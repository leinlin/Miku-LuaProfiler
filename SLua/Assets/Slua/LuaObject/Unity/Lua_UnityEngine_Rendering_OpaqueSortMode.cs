using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_Rendering_OpaqueSortMode : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.Rendering.OpaqueSortMode");
		addMember(l,0,"Default");
		addMember(l,1,"FrontToBack");
		addMember(l,2,"NoDistanceSort");
		LuaDLL.lua_pop(l, 1);
	}
}
