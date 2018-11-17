using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_Playables_PlayState : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.Playables.PlayState");
		addMember(l,0,"Paused");
		addMember(l,1,"Playing");
		addMember(l,2,"Delayed");
		LuaDLL.lua_pop(l, 1);
	}
}
