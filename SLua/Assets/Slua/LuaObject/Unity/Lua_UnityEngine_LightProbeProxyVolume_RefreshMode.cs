using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_LightProbeProxyVolume_RefreshMode : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.LightProbeProxyVolume.RefreshMode");
		addMember(l,0,"Automatic");
		addMember(l,1,"EveryFrame");
		addMember(l,2,"ViaScripting");
		LuaDLL.lua_pop(l, 1);
	}
}
