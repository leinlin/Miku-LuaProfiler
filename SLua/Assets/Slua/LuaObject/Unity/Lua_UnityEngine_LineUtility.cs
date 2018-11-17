using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_LineUtility : LuaObject {
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int constructor(IntPtr l) {
		try {
			#if DEBUG
			var method = System.Reflection.MethodBase.GetCurrentMethod();
			string methodName = GetMethodName(method);
			#if UNITY_5_5_OR_NEWER
			UnityEngine.Profiling.Profiler.BeginSample(methodName);
			#else
			Profiler.BeginSample(methodName);
			#endif
			#endif
			UnityEngine.LineUtility o;
			o=new UnityEngine.LineUtility();
			pushValue(l,true);
			pushValue(l,o);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if DEBUG
		finally {
			#if UNITY_5_5_OR_NEWER
			UnityEngine.Profiling.Profiler.EndSample();
			#else
			Profiler.EndSample();
			#endif
		}
		#endif
	}
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int Simplify_s(IntPtr l) {
		try {
			#if DEBUG
			var method = System.Reflection.MethodBase.GetCurrentMethod();
			string methodName = GetMethodName(method);
			#if UNITY_5_5_OR_NEWER
			UnityEngine.Profiling.Profiler.BeginSample(methodName);
			#else
			Profiler.BeginSample(methodName);
			#endif
			#endif
			int argc = LuaDLL.lua_gettop(l);
			if(matchType(l,argc,1,typeof(List<UnityEngine.Vector2>),typeof(float),typeof(List<System.Int32>))){
				System.Collections.Generic.List<UnityEngine.Vector2> a1;
				checkType(l,1,out a1);
				System.Single a2;
				checkType(l,2,out a2);
				System.Collections.Generic.List<System.Int32> a3;
				checkType(l,3,out a3);
				UnityEngine.LineUtility.Simplify(a1,a2,a3);
				pushValue(l,true);
				return 1;
			}
			else if(matchType(l,argc,1,typeof(List<UnityEngine.Vector2>),typeof(float),typeof(List<UnityEngine.Vector2>))){
				System.Collections.Generic.List<UnityEngine.Vector2> a1;
				checkType(l,1,out a1);
				System.Single a2;
				checkType(l,2,out a2);
				System.Collections.Generic.List<UnityEngine.Vector2> a3;
				checkType(l,3,out a3);
				UnityEngine.LineUtility.Simplify(a1,a2,a3);
				pushValue(l,true);
				return 1;
			}
			else if(matchType(l,argc,1,typeof(List<UnityEngine.Vector3>),typeof(float),typeof(List<System.Int32>))){
				System.Collections.Generic.List<UnityEngine.Vector3> a1;
				checkType(l,1,out a1);
				System.Single a2;
				checkType(l,2,out a2);
				System.Collections.Generic.List<System.Int32> a3;
				checkType(l,3,out a3);
				UnityEngine.LineUtility.Simplify(a1,a2,a3);
				pushValue(l,true);
				return 1;
			}
			else if(matchType(l,argc,1,typeof(List<UnityEngine.Vector3>),typeof(float),typeof(List<UnityEngine.Vector3>))){
				System.Collections.Generic.List<UnityEngine.Vector3> a1;
				checkType(l,1,out a1);
				System.Single a2;
				checkType(l,2,out a2);
				System.Collections.Generic.List<UnityEngine.Vector3> a3;
				checkType(l,3,out a3);
				UnityEngine.LineUtility.Simplify(a1,a2,a3);
				pushValue(l,true);
				return 1;
			}
			pushValue(l,false);
			LuaDLL.lua_pushstring(l,"No matched override function Simplify to call");
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if DEBUG
		finally {
			#if UNITY_5_5_OR_NEWER
			UnityEngine.Profiling.Profiler.EndSample();
			#else
			Profiler.EndSample();
			#endif
		}
		#endif
	}
	[UnityEngine.Scripting.Preserve]
	static public void reg(IntPtr l) {
		getTypeTable(l,"UnityEngine.LineUtility");
		addMember(l,Simplify_s);
		createTypeMetatable(l,constructor, typeof(UnityEngine.LineUtility));
	}
}
