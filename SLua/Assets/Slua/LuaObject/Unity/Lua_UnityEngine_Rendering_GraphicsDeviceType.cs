using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_Rendering_GraphicsDeviceType : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.Rendering.GraphicsDeviceType");
		addMember(l,0,"OpenGL2");
		addMember(l,1,"Direct3D9");
		addMember(l,2,"Direct3D11");
		addMember(l,3,"PlayStation3");
		addMember(l,4,"Null");
		addMember(l,6,"Xbox360");
		addMember(l,8,"OpenGLES2");
		addMember(l,11,"OpenGLES3");
		addMember(l,12,"PlayStationVita");
		addMember(l,13,"PlayStation4");
		addMember(l,14,"XboxOne");
		addMember(l,15,"PlayStationMobile");
		addMember(l,16,"Metal");
		addMember(l,17,"OpenGLCore");
		addMember(l,18,"Direct3D12");
		addMember(l,19,"N3DS");
		addMember(l,21,"Vulkan");
		addMember(l,22,"Switch");
		addMember(l,23,"XboxOneD3D12");
		LuaDLL.lua_pop(l, 1);
	}
}
