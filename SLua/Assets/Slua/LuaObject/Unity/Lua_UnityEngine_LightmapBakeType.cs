using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_LightmapBakeType : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.LightmapBakeType");
		addMember(l,1,"Mixed");
		addMember(l,2,"Baked");
		addMember(l,4,"Realtime");
		LuaDLL.lua_pop(l, 1);
	}
}
