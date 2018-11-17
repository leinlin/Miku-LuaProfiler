using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_Rendering_ShadowMapPass : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.Rendering.ShadowMapPass");
		addMember(l,1,"PointlightPositiveX");
		addMember(l,2,"PointlightNegativeX");
		addMember(l,4,"PointlightPositiveY");
		addMember(l,8,"PointlightNegativeY");
		addMember(l,16,"PointlightPositiveZ");
		addMember(l,32,"PointlightNegativeZ");
		addMember(l,63,"Pointlight");
		addMember(l,64,"DirectionalCascade0");
		addMember(l,128,"DirectionalCascade1");
		addMember(l,256,"DirectionalCascade2");
		addMember(l,512,"DirectionalCascade3");
		addMember(l,960,"Directional");
		addMember(l,1024,"Spotlight");
		addMember(l,2047,"All");
		LuaDLL.lua_pop(l, 1);
	}
}
