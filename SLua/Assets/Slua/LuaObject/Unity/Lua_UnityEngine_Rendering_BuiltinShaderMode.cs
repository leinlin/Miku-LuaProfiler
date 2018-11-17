using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_Rendering_BuiltinShaderMode : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.Rendering.BuiltinShaderMode");
		addMember(l,0,"Disabled");
		addMember(l,1,"UseBuiltin");
		addMember(l,2,"UseCustom");
		LuaDLL.lua_pop(l, 1);
	}
}
