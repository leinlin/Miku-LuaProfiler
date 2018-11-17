using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_Collections_NativeSliceExtensions : LuaObject {
	[UnityEngine.Scripting.Preserve]
	static public void reg(IntPtr l) {
		getTypeTable(l,"UnityEngine.Collections.NativeSliceExtensions");
		createTypeMetatable(l,null, typeof(UnityEngine.Collections.NativeSliceExtensions));
	}
}
