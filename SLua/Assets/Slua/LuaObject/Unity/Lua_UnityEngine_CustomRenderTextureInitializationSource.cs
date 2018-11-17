using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_CustomRenderTextureInitializationSource : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.CustomRenderTextureInitializationSource");
		addMember(l,0,"TextureAndColor");
		addMember(l,1,"Material");
		LuaDLL.lua_pop(l, 1);
	}
}
