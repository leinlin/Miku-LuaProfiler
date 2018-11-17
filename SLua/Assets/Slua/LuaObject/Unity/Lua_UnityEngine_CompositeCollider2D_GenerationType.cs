using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_CompositeCollider2D_GenerationType : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.CompositeCollider2D.GenerationType");
		addMember(l,0,"Synchronous");
		addMember(l,1,"Manual");
		LuaDLL.lua_pop(l, 1);
	}
}
