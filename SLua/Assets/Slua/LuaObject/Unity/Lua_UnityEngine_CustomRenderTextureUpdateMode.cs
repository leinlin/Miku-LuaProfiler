using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_CustomRenderTextureUpdateMode : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.CustomRenderTextureUpdateMode");
		addMember(l,0,"OnLoad");
		addMember(l,1,"Realtime");
		addMember(l,2,"OnDemand");
		LuaDLL.lua_pop(l, 1);
	}
}
