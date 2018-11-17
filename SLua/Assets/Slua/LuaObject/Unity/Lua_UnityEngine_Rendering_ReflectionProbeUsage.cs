using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_Rendering_ReflectionProbeUsage : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.Rendering.ReflectionProbeUsage");
		addMember(l,0,"Off");
		addMember(l,1,"BlendProbes");
		addMember(l,2,"BlendProbesAndSkybox");
		addMember(l,3,"Simple");
		LuaDLL.lua_pop(l, 1);
	}
}
