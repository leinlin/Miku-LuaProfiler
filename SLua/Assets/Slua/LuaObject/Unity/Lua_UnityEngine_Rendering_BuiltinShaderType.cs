using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_Rendering_BuiltinShaderType : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.Rendering.BuiltinShaderType");
		addMember(l,0,"DeferredShading");
		addMember(l,1,"DeferredReflections");
		addMember(l,2,"LegacyDeferredLighting");
		addMember(l,3,"ScreenSpaceShadows");
		addMember(l,4,"DepthNormals");
		addMember(l,5,"MotionVectors");
		addMember(l,6,"LightHalo");
		addMember(l,7,"LensFlare");
		LuaDLL.lua_pop(l, 1);
	}
}
