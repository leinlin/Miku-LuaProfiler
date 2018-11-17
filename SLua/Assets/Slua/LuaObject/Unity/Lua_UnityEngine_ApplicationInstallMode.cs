using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_ApplicationInstallMode : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.ApplicationInstallMode");
		addMember(l,0,"Unknown");
		addMember(l,1,"Store");
		addMember(l,2,"DeveloperBuild");
		addMember(l,3,"Adhoc");
		addMember(l,4,"Enterprise");
		addMember(l,5,"Editor");
		LuaDLL.lua_pop(l, 1);
	}
}
