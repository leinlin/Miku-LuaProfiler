using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_Rendering_TextureDimension : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.Rendering.TextureDimension");
		addMember(l,0,"None");
		addMember(l,1,"Any");
		addMember(l,2,"Tex2D");
		addMember(l,3,"Tex3D");
		addMember(l,4,"Cube");
		addMember(l,5,"Tex2DArray");
		addMember(l,6,"CubeArray");
		addMember(l,-1,"Unknown");
		LuaDLL.lua_pop(l, 1);
	}
}
