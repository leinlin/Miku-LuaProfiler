using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_LightmapsModeLegacy : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.LightmapsModeLegacy");
		addMember(l,0,"Single");
		addMember(l,1,"Dual");
		addMember(l,2,"Directional");
		LuaDLL.lua_pop(l, 1);
	}
}
