using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_Rendering_BlendMode : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.Rendering.BlendMode");
		addMember(l,0,"Zero");
		addMember(l,1,"One");
		addMember(l,2,"DstColor");
		addMember(l,3,"SrcColor");
		addMember(l,4,"OneMinusDstColor");
		addMember(l,5,"SrcAlpha");
		addMember(l,6,"OneMinusSrcColor");
		addMember(l,7,"DstAlpha");
		addMember(l,8,"OneMinusDstAlpha");
		addMember(l,9,"SrcAlphaSaturate");
		addMember(l,10,"OneMinusSrcAlpha");
		LuaDLL.lua_pop(l, 1);
	}
}
