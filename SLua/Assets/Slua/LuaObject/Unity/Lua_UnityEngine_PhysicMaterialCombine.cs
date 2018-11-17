using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_PhysicMaterialCombine : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.PhysicMaterialCombine");
		addMember(l,0,"Average");
		addMember(l,1,"Multiply");
		addMember(l,2,"Minimum");
		addMember(l,3,"Maximum");
		LuaDLL.lua_pop(l, 1);
	}
}
