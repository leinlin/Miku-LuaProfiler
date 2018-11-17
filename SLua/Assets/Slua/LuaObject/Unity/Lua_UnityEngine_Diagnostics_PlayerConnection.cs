using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_Diagnostics_PlayerConnection : LuaObject {
	[UnityEngine.Scripting.Preserve]
	static public void reg(IntPtr l) {
		getTypeTable(l,"UnityEngine.Diagnostics.PlayerConnection");
		createTypeMetatable(l,null, typeof(UnityEngine.Diagnostics.PlayerConnection));
	}
}
