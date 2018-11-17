using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_Rendering_ShadowSamplingMode : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.Rendering.ShadowSamplingMode");
		addMember(l,0,"CompareDepths");
		addMember(l,1,"RawDepth");
		addMember(l,2,"None");
		LuaDLL.lua_pop(l, 1);
	}
}
