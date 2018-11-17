using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_UserAuthorization : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.UserAuthorization");
		addMember(l,1,"WebCam");
		addMember(l,2,"Microphone");
		LuaDLL.lua_pop(l, 1);
	}
}
