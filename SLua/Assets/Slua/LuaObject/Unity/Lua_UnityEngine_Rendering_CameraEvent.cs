using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_Rendering_CameraEvent : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.Rendering.CameraEvent");
		addMember(l,0,"BeforeDepthTexture");
		addMember(l,1,"AfterDepthTexture");
		addMember(l,2,"BeforeDepthNormalsTexture");
		addMember(l,3,"AfterDepthNormalsTexture");
		addMember(l,4,"BeforeGBuffer");
		addMember(l,5,"AfterGBuffer");
		addMember(l,6,"BeforeLighting");
		addMember(l,7,"AfterLighting");
		addMember(l,8,"BeforeFinalPass");
		addMember(l,9,"AfterFinalPass");
		addMember(l,10,"BeforeForwardOpaque");
		addMember(l,11,"AfterForwardOpaque");
		addMember(l,12,"BeforeImageEffectsOpaque");
		addMember(l,13,"AfterImageEffectsOpaque");
		addMember(l,14,"BeforeSkybox");
		addMember(l,15,"AfterSkybox");
		addMember(l,16,"BeforeForwardAlpha");
		addMember(l,17,"AfterForwardAlpha");
		addMember(l,18,"BeforeImageEffects");
		addMember(l,19,"AfterImageEffects");
		addMember(l,20,"AfterEverything");
		addMember(l,21,"BeforeReflections");
		addMember(l,22,"AfterReflections");
		addMember(l,23,"BeforeHaloAndLensFlares");
		addMember(l,24,"AfterHaloAndLensFlares");
		LuaDLL.lua_pop(l, 1);
	}
}
