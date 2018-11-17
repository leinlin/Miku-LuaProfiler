using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_Rendering_ReflectionCubemapCompression : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.Rendering.ReflectionCubemapCompression");
		addMember(l,0,"Uncompressed");
		addMember(l,1,"Compressed");
		addMember(l,2,"Auto");
		LuaDLL.lua_pop(l, 1);
	}
}
