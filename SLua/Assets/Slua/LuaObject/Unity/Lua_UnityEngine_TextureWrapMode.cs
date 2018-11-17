using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_TextureWrapMode : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.TextureWrapMode");
		addMember(l,0,"Repeat");
		addMember(l,1,"Clamp");
		addMember(l,2,"Mirror");
		addMember(l,3,"MirrorOnce");
		LuaDLL.lua_pop(l, 1);
	}
}
