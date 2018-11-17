using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_Rendering_LightEvent : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.Rendering.LightEvent");
		addMember(l,0,"BeforeShadowMap");
		addMember(l,1,"AfterShadowMap");
		addMember(l,2,"BeforeScreenspaceMask");
		addMember(l,3,"AfterScreenspaceMask");
		addMember(l,4,"BeforeShadowMapPass");
		addMember(l,5,"AfterShadowMapPass");
		LuaDLL.lua_pop(l, 1);
	}
}
