using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_LightProbeProxyVolume_BoundingBoxMode : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.LightProbeProxyVolume.BoundingBoxMode");
		addMember(l,0,"AutomaticLocal");
		addMember(l,1,"AutomaticWorld");
		addMember(l,2,"Custom");
		LuaDLL.lua_pop(l, 1);
	}
}
