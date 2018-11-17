using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_Playables_PlayableOutputExtensions : LuaObject {
	[UnityEngine.Scripting.Preserve]
	static public void reg(IntPtr l) {
		getTypeTable(l,"UnityEngine.Playables.PlayableOutputExtensions");
		createTypeMetatable(l,null, typeof(UnityEngine.Playables.PlayableOutputExtensions));
	}
}
