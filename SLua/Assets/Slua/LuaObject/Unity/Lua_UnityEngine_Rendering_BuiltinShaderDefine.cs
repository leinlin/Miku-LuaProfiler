using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_Rendering_BuiltinShaderDefine : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.Rendering.BuiltinShaderDefine");
		addMember(l,0,"UNITY_NO_DXT5nm");
		addMember(l,1,"UNITY_NO_RGBM");
		addMember(l,2,"UNITY_USE_NATIVE_HDR");
		addMember(l,3,"UNITY_ENABLE_REFLECTION_BUFFERS");
		addMember(l,4,"UNITY_FRAMEBUFFER_FETCH_AVAILABLE");
		addMember(l,5,"UNITY_ENABLE_NATIVE_SHADOW_LOOKUPS");
		addMember(l,6,"UNITY_METAL_SHADOWS_USE_POINT_FILTERING");
		addMember(l,7,"UNITY_NO_CUBEMAP_ARRAY");
		addMember(l,8,"UNITY_NO_SCREENSPACE_SHADOWS");
		addMember(l,9,"UNITY_USE_DITHER_MASK_FOR_ALPHABLENDED_SHADOWS");
		addMember(l,10,"UNITY_PBS_USE_BRDF1");
		addMember(l,11,"UNITY_PBS_USE_BRDF2");
		addMember(l,12,"UNITY_PBS_USE_BRDF3");
		addMember(l,13,"UNITY_NO_FULL_STANDARD_SHADER");
		addMember(l,14,"UNITY_SPECCUBE_BOX_PROJECTION");
		addMember(l,15,"UNITY_SPECCUBE_BLENDING");
		addMember(l,16,"UNITY_ENABLE_DETAIL_NORMALMAP");
		addMember(l,17,"SHADER_API_MOBILE");
		addMember(l,18,"SHADER_API_DESKTOP");
		addMember(l,19,"UNITY_HARDWARE_TIER1");
		addMember(l,20,"UNITY_HARDWARE_TIER2");
		addMember(l,21,"UNITY_HARDWARE_TIER3");
		addMember(l,22,"UNITY_COLORSPACE_GAMMA");
		addMember(l,23,"UNITY_LIGHT_PROBE_PROXY_VOLUME");
		addMember(l,24,"UNITY_LIGHTMAP_DLDR_ENCODING");
		addMember(l,25,"UNITY_LIGHTMAP_RGBM_ENCODING");
		addMember(l,26,"UNITY_LIGHTMAP_FULL_HDR");
		LuaDLL.lua_pop(l, 1);
	}
}
