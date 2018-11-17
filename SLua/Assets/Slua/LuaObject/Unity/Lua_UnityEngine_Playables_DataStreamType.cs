using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_Playables_DataStreamType : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.Playables.DataStreamType");
		addMember(l,0,"Animation");
		addMember(l,1,"Audio");
		addMember(l,2,"Texture");
		addMember(l,3,"None");
		LuaDLL.lua_pop(l, 1);
	}
}
