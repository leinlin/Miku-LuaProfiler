using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_FlareLayer : LuaObject {
	[UnityEngine.Scripting.Preserve]
	static public void reg(IntPtr l) {
		getTypeTable(l,"UnityEngine.FlareLayer");
		createTypeMetatable(l,null, typeof(UnityEngine.FlareLayer),typeof(UnityEngine.Behaviour));
	}
}
