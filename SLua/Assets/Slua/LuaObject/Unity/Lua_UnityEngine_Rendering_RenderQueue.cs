using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_Rendering_RenderQueue : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.Rendering.RenderQueue");
		addMember(l,1000,"Background");
		addMember(l,2000,"Geometry");
		addMember(l,2450,"AlphaTest");
		addMember(l,2500,"GeometryLast");
		addMember(l,3000,"Transparent");
		addMember(l,4000,"Overlay");
		LuaDLL.lua_pop(l, 1);
	}
}
