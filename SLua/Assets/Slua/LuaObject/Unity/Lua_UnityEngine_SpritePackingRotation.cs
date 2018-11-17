using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_SpritePackingRotation : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.SpritePackingRotation");
		addMember(l,0,"None");
		addMember(l,15,"Any");
		LuaDLL.lua_pop(l, 1);
	}
}
