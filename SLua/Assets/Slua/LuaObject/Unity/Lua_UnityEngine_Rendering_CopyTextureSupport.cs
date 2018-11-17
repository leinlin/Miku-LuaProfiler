using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_Rendering_CopyTextureSupport : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.Rendering.CopyTextureSupport");
		addMember(l,0,"None");
		addMember(l,1,"Basic");
		addMember(l,2,"Copy3D");
		addMember(l,4,"DifferentTypes");
		addMember(l,8,"TextureToRT");
		addMember(l,16,"RTToTexture");
		LuaDLL.lua_pop(l, 1);
	}
}
