using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_AvatarMaskBodyPart : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.AvatarMaskBodyPart");
		addMember(l,0,"Root");
		addMember(l,1,"Body");
		addMember(l,2,"Head");
		addMember(l,3,"LeftLeg");
		addMember(l,4,"RightLeg");
		addMember(l,5,"LeftArm");
		addMember(l,6,"RightArm");
		addMember(l,7,"LeftFingers");
		addMember(l,8,"RightFingers");
		addMember(l,9,"LeftFootIK");
		addMember(l,10,"RightFootIK");
		addMember(l,11,"LeftHandIK");
		addMember(l,12,"RightHandIK");
		addMember(l,13,"LastBodyPart");
		LuaDLL.lua_pop(l, 1);
	}
}
