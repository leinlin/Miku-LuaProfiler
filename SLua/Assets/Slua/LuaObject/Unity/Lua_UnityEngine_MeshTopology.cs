using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_MeshTopology : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.MeshTopology");
		addMember(l,0,"Triangles");
		addMember(l,2,"Quads");
		addMember(l,3,"Lines");
		addMember(l,4,"LineStrip");
		addMember(l,5,"Points");
		LuaDLL.lua_pop(l, 1);
	}
}
