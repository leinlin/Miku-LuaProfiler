using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_BlendWeights : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.BlendWeights");
		addMember(l,1,"OneBone");
		addMember(l,2,"TwoBones");
		addMember(l,4,"FourBones");
		LuaDLL.lua_pop(l, 1);
	}
}
