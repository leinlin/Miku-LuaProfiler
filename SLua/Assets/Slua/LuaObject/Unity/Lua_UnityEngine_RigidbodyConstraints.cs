using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_RigidbodyConstraints : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.RigidbodyConstraints");
		addMember(l,0,"None");
		addMember(l,2,"FreezePositionX");
		addMember(l,4,"FreezePositionY");
		addMember(l,8,"FreezePositionZ");
		addMember(l,14,"FreezePosition");
		addMember(l,16,"FreezeRotationX");
		addMember(l,32,"FreezeRotationY");
		addMember(l,64,"FreezeRotationZ");
		addMember(l,112,"FreezeRotation");
		addMember(l,126,"FreezeAll");
		LuaDLL.lua_pop(l, 1);
	}
}
