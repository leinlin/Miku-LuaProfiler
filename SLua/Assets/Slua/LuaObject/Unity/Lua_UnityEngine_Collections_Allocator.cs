using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_Collections_Allocator : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.Collections.Allocator");
		addMember(l,0,"Invalid");
		addMember(l,1,"None");
		addMember(l,2,"Temp");
		addMember(l,3,"TempJob");
		addMember(l,4,"Persistent");
		LuaDLL.lua_pop(l, 1);
	}
}
