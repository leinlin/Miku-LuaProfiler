using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_Rendering_CompareFunction : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.Rendering.CompareFunction");
		addMember(l,0,"Disabled");
		addMember(l,1,"Never");
		addMember(l,2,"Less");
		addMember(l,3,"Equal");
		addMember(l,4,"LessEqual");
		addMember(l,5,"Greater");
		addMember(l,6,"NotEqual");
		addMember(l,7,"GreaterEqual");
		addMember(l,8,"Always");
		LuaDLL.lua_pop(l, 1);
	}
}
