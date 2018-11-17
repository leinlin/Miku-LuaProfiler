using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_CompositeCollider2D_GeometryType : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.CompositeCollider2D.GeometryType");
		addMember(l,0,"Outlines");
		addMember(l,1,"Polygons");
		LuaDLL.lua_pop(l, 1);
	}
}
