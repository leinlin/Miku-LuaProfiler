using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_MaterialGlobalIlluminationFlags : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.MaterialGlobalIlluminationFlags");
		addMember(l,0,"None");
		addMember(l,1,"RealtimeEmissive");
		addMember(l,2,"BakedEmissive");
		addMember(l,3,"AnyEmissive");
		addMember(l,4,"EmissiveIsBlack");
		LuaDLL.lua_pop(l, 1);
	}
}
