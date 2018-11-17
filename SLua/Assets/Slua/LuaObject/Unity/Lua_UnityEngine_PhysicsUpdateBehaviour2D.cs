using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_PhysicsUpdateBehaviour2D : LuaObject {
	[UnityEngine.Scripting.Preserve]
	static public void reg(IntPtr l) {
		getTypeTable(l,"UnityEngine.PhysicsUpdateBehaviour2D");
		createTypeMetatable(l,null, typeof(UnityEngine.PhysicsUpdateBehaviour2D),typeof(UnityEngine.Behaviour));
	}
}
