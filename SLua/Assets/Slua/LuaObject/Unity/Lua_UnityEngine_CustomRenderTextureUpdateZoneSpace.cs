using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_CustomRenderTextureUpdateZoneSpace : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.CustomRenderTextureUpdateZoneSpace");
		addMember(l,0,"Normalized");
		addMember(l,1,"Pixel");
		LuaDLL.lua_pop(l, 1);
	}
}
