using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_Rendering_ReflectionProbeTimeSlicingMode : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.Rendering.ReflectionProbeTimeSlicingMode");
		addMember(l,0,"AllFacesAtOnce");
		addMember(l,1,"IndividualFaces");
		addMember(l,2,"NoTimeSlicing");
		LuaDLL.lua_pop(l, 1);
	}
}
