using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_SystemLanguage : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.SystemLanguage");
		addMember(l,0,"Afrikaans");
		addMember(l,1,"Arabic");
		addMember(l,2,"Basque");
		addMember(l,3,"Belarusian");
		addMember(l,4,"Bulgarian");
		addMember(l,5,"Catalan");
		addMember(l,6,"Chinese");
		addMember(l,7,"Czech");
		addMember(l,8,"Danish");
		addMember(l,9,"Dutch");
		addMember(l,10,"English");
		addMember(l,11,"Estonian");
		addMember(l,12,"Faroese");
		addMember(l,13,"Finnish");
		addMember(l,14,"French");
		addMember(l,15,"German");
		addMember(l,16,"Greek");
		addMember(l,17,"Hebrew");
		addMember(l,18,"Hungarian");
		addMember(l,18,"Hugarian");
		addMember(l,19,"Icelandic");
		addMember(l,20,"Indonesian");
		addMember(l,21,"Italian");
		addMember(l,22,"Japanese");
		addMember(l,23,"Korean");
		addMember(l,24,"Latvian");
		addMember(l,25,"Lithuanian");
		addMember(l,26,"Norwegian");
		addMember(l,27,"Polish");
		addMember(l,28,"Portuguese");
		addMember(l,29,"Romanian");
		addMember(l,30,"Russian");
		addMember(l,31,"SerboCroatian");
		addMember(l,32,"Slovak");
		addMember(l,33,"Slovenian");
		addMember(l,34,"Spanish");
		addMember(l,35,"Swedish");
		addMember(l,36,"Thai");
		addMember(l,37,"Turkish");
		addMember(l,38,"Ukrainian");
		addMember(l,39,"Vietnamese");
		addMember(l,40,"ChineseSimplified");
		addMember(l,41,"ChineseTraditional");
		addMember(l,42,"Unknown");
		LuaDLL.lua_pop(l, 1);
	}
}
