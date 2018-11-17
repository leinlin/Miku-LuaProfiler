using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_JointLimitState2D : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.JointLimitState2D");
		addMember(l,0,"Inactive");
		addMember(l,1,"LowerLimit");
		addMember(l,2,"UpperLimit");
		addMember(l,3,"EqualLimits");
		LuaDLL.lua_pop(l, 1);
	}
}
