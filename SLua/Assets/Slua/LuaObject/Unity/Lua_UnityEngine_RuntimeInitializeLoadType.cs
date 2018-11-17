using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_RuntimeInitializeLoadType : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.RuntimeInitializeLoadType");
		addMember(l,0,"AfterSceneLoad");
		addMember(l,1,"BeforeSceneLoad");
		LuaDLL.lua_pop(l, 1);
	}
}
