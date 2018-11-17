using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_AvatarTarget : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.AvatarTarget");
		addMember(l,0,"Root");
		addMember(l,1,"Body");
		addMember(l,2,"LeftFoot");
		addMember(l,3,"RightFoot");
		addMember(l,4,"LeftHand");
		addMember(l,5,"RightHand");
		LuaDLL.lua_pop(l, 1);
	}
}
