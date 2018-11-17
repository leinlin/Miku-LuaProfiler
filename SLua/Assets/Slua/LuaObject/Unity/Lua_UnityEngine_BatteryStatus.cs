using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_BatteryStatus : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.BatteryStatus");
		addMember(l,0,"Unknown");
		addMember(l,1,"Charging");
		addMember(l,2,"Discharging");
		addMember(l,3,"NotCharging");
		addMember(l,4,"Full");
		LuaDLL.lua_pop(l, 1);
	}
}
