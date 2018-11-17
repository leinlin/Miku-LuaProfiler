using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_RigidbodyConstraints2D : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.RigidbodyConstraints2D");
		addMember(l,0,"None");
		addMember(l,1,"FreezePositionX");
		addMember(l,2,"FreezePositionY");
		addMember(l,3,"FreezePosition");
		addMember(l,4,"FreezeRotation");
		addMember(l,7,"FreezeAll");
		LuaDLL.lua_pop(l, 1);
	}
}
