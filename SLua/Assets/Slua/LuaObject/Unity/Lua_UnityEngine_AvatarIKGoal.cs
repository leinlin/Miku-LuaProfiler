using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_AvatarIKGoal : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.AvatarIKGoal");
		addMember(l,0,"LeftFoot");
		addMember(l,1,"RightFoot");
		addMember(l,2,"LeftHand");
		addMember(l,3,"RightHand");
		LuaDLL.lua_pop(l, 1);
	}
}
