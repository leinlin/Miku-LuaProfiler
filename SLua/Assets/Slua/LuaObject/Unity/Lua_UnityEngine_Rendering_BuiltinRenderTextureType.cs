using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_Rendering_BuiltinRenderTextureType : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.Rendering.BuiltinRenderTextureType");
		addMember(l,0,"None");
		addMember(l,1,"CurrentActive");
		addMember(l,2,"CameraTarget");
		addMember(l,3,"Depth");
		addMember(l,4,"DepthNormals");
		addMember(l,5,"ResolvedDepth");
		addMember(l,7,"PrepassNormalsSpec");
		addMember(l,8,"PrepassLight");
		addMember(l,9,"PrepassLightSpec");
		addMember(l,10,"GBuffer0");
		addMember(l,11,"GBuffer1");
		addMember(l,12,"GBuffer2");
		addMember(l,13,"GBuffer3");
		addMember(l,14,"Reflections");
		addMember(l,15,"MotionVectors");
		addMember(l,16,"GBuffer4");
		addMember(l,17,"GBuffer5");
		addMember(l,18,"GBuffer6");
		addMember(l,19,"GBuffer7");
		addMember(l,-3,"PropertyName");
		addMember(l,-2,"BufferPtr");
		addMember(l,-1,"BindableTexture");
		LuaDLL.lua_pop(l, 1);
	}
}
