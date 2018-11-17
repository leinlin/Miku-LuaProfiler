using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_Rendering_CameraHDRMode : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.Rendering.CameraHDRMode");
		addMember(l,1,"FP16");
		addMember(l,2,"R11G11B10");
		LuaDLL.lua_pop(l, 1);
	}
}
