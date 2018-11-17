using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_Rendering_RenderBufferLoadAction : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.Rendering.RenderBufferLoadAction");
		addMember(l,0,"Load");
		addMember(l,1,"Clear");
		addMember(l,2,"DontCare");
		LuaDLL.lua_pop(l, 1);
	}
}
