using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_Rendering_IndexFormat : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.Rendering.IndexFormat");
		addMember(l,0,"UInt16");
		addMember(l,1,"UInt32");
		LuaDLL.lua_pop(l, 1);
	}
}
