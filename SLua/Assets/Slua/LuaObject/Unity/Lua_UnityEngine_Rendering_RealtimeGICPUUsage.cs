using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_Rendering_RealtimeGICPUUsage : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.Rendering.RealtimeGICPUUsage");
		addMember(l,25,"Low");
		addMember(l,50,"Medium");
		addMember(l,75,"High");
		addMember(l,100,"Unlimited");
		LuaDLL.lua_pop(l, 1);
	}
}
