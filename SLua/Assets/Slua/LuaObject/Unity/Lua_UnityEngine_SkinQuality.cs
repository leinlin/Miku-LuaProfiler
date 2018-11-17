using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_SkinQuality : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.SkinQuality");
		addMember(l,0,"Auto");
		addMember(l,1,"Bone1");
		addMember(l,2,"Bone2");
		addMember(l,4,"Bone4");
		LuaDLL.lua_pop(l, 1);
	}
}
