#if USE_UNI_LUA
using LuaAPI = UniLua.Lua;
using RealStatePtr = UniLua.ILuaState;
using LuaCSFunction = UniLua.CSharpFunctionDelegate;
#else
using LuaAPI = XLua.LuaDLL.Lua;
using RealStatePtr = System.IntPtr;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
#endif

using XLua;
using System.Collections.Generic;


namespace XLua.CSObjectWrap
{
    using Utils = XLua.Utils;
    public class UnityEngineSkinnedMeshRendererWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(UnityEngine.SkinnedMeshRenderer);
			Utils.BeginObjectRegister(type, L, translator, 0, 3, 7, 7);
			
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "BakeMesh", _m_BakeMesh);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "GetBlendShapeWeight", _m_GetBlendShapeWeight);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "SetBlendShapeWeight", _m_SetBlendShapeWeight);
			
			
			Utils.RegisterFunc(L, Utils.GETTER_IDX, "bones", _g_get_bones);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "rootBone", _g_get_rootBone);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "quality", _g_get_quality);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "sharedMesh", _g_get_sharedMesh);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "updateWhenOffscreen", _g_get_updateWhenOffscreen);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "skinnedMotionVectors", _g_get_skinnedMotionVectors);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "localBounds", _g_get_localBounds);
            
			Utils.RegisterFunc(L, Utils.SETTER_IDX, "bones", _s_set_bones);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "rootBone", _s_set_rootBone);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "quality", _s_set_quality);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "sharedMesh", _s_set_sharedMesh);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "updateWhenOffscreen", _s_set_updateWhenOffscreen);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "skinnedMotionVectors", _s_set_skinnedMotionVectors);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "localBounds", _s_set_localBounds);
            
			
			Utils.EndObjectRegister(type, L, translator, null, null,
			    null, null, null);

		    Utils.BeginClassRegister(type, L, __CreateInstance, 1, 0, 0);
			
			
            
			
			
			
			Utils.EndClassRegister(type, L, translator);
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CreateInstance(RealStatePtr L)
        {
            
			try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
				if(LuaAPI.lua_gettop(L) == 1)
				{
					
					UnityEngine.SkinnedMeshRenderer gen_ret = new UnityEngine.SkinnedMeshRenderer();
					translator.Push(L, gen_ret);
                    
					return 1;
				}
				
			}
			catch(System.Exception gen_e) {
				return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
			}
            return LuaAPI.luaL_error(L, "invalid arguments to UnityEngine.SkinnedMeshRenderer constructor!");
            
        }
        
		
        
		
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_BakeMesh(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.SkinnedMeshRenderer gen_to_be_invoked = (UnityEngine.SkinnedMeshRenderer)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.Mesh _mesh = (UnityEngine.Mesh)translator.GetObject(L, 2, typeof(UnityEngine.Mesh));
                    
                    gen_to_be_invoked.BakeMesh( _mesh );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetBlendShapeWeight(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.SkinnedMeshRenderer gen_to_be_invoked = (UnityEngine.SkinnedMeshRenderer)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    int _index = LuaAPI.xlua_tointeger(L, 2);
                    
                        float gen_ret = gen_to_be_invoked.GetBlendShapeWeight( _index );
                        LuaAPI.lua_pushnumber(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetBlendShapeWeight(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.SkinnedMeshRenderer gen_to_be_invoked = (UnityEngine.SkinnedMeshRenderer)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    int _index = LuaAPI.xlua_tointeger(L, 2);
                    float _value = (float)LuaAPI.lua_tonumber(L, 3);
                    
                    gen_to_be_invoked.SetBlendShapeWeight( _index, _value );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_bones(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.SkinnedMeshRenderer gen_to_be_invoked = (UnityEngine.SkinnedMeshRenderer)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.bones);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_rootBone(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.SkinnedMeshRenderer gen_to_be_invoked = (UnityEngine.SkinnedMeshRenderer)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.rootBone);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_quality(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.SkinnedMeshRenderer gen_to_be_invoked = (UnityEngine.SkinnedMeshRenderer)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.quality);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_sharedMesh(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.SkinnedMeshRenderer gen_to_be_invoked = (UnityEngine.SkinnedMeshRenderer)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.sharedMesh);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_updateWhenOffscreen(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.SkinnedMeshRenderer gen_to_be_invoked = (UnityEngine.SkinnedMeshRenderer)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.updateWhenOffscreen);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_skinnedMotionVectors(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.SkinnedMeshRenderer gen_to_be_invoked = (UnityEngine.SkinnedMeshRenderer)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.skinnedMotionVectors);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_localBounds(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.SkinnedMeshRenderer gen_to_be_invoked = (UnityEngine.SkinnedMeshRenderer)translator.FastGetCSObj(L, 1);
                translator.PushUnityEngineBounds(L, gen_to_be_invoked.localBounds);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_bones(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.SkinnedMeshRenderer gen_to_be_invoked = (UnityEngine.SkinnedMeshRenderer)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.bones = (UnityEngine.Transform[])translator.GetObject(L, 2, typeof(UnityEngine.Transform[]));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_rootBone(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.SkinnedMeshRenderer gen_to_be_invoked = (UnityEngine.SkinnedMeshRenderer)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.rootBone = (UnityEngine.Transform)translator.GetObject(L, 2, typeof(UnityEngine.Transform));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_quality(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.SkinnedMeshRenderer gen_to_be_invoked = (UnityEngine.SkinnedMeshRenderer)translator.FastGetCSObj(L, 1);
                UnityEngine.SkinQuality gen_value;translator.Get(L, 2, out gen_value);
				gen_to_be_invoked.quality = gen_value;
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_sharedMesh(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.SkinnedMeshRenderer gen_to_be_invoked = (UnityEngine.SkinnedMeshRenderer)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.sharedMesh = (UnityEngine.Mesh)translator.GetObject(L, 2, typeof(UnityEngine.Mesh));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_updateWhenOffscreen(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.SkinnedMeshRenderer gen_to_be_invoked = (UnityEngine.SkinnedMeshRenderer)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.updateWhenOffscreen = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_skinnedMotionVectors(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.SkinnedMeshRenderer gen_to_be_invoked = (UnityEngine.SkinnedMeshRenderer)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.skinnedMotionVectors = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_localBounds(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.SkinnedMeshRenderer gen_to_be_invoked = (UnityEngine.SkinnedMeshRenderer)translator.FastGetCSObj(L, 1);
                UnityEngine.Bounds gen_value;translator.Get(L, 2, out gen_value);
				gen_to_be_invoked.localBounds = gen_value;
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
		
		
		
		
    }
}
