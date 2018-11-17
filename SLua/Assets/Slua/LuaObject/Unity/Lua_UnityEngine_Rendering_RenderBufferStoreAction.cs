using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_Rendering_RenderBufferStoreAction : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.Rendering.RenderBufferStoreAction");
		addMember(l,0,"Store");
		addMember(l,1,"Resolve");
		addMember(l,2,"StoreAndResolve");
		addMember(l,3,"DontCare");
		LuaDLL.lua_pop(l, 1);
	}
}
