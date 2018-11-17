using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_DepthTextureMode : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.DepthTextureMode");
		addMember(l,0,"None");
		addMember(l,1,"Depth");
		addMember(l,2,"DepthNormals");
		addMember(l,4,"MotionVectors");
		LuaDLL.lua_pop(l, 1);
	}
}
