using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_ComputeBufferType : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.ComputeBufferType");
		addMember(l,0,"Default");
		addMember(l,1,"Raw");
		addMember(l,2,"Append");
		addMember(l,4,"Counter");
		addMember(l,256,"IndirectArguments");
		addMember(l,256,"DrawIndirect");
		addMember(l,512,"GPUMemory");
		LuaDLL.lua_pop(l, 1);
	}
}
