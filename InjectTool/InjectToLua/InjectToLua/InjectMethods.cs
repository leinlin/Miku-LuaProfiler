/*
               #########                       
              ############                     
              #############                    
             ##  ###########                   
            ###  ###### #####                  
            ### #######   ####                 
           ###  ########## ####                
          ####  ########### ####               
         ####   ###########  #####             
        #####   ### ########   #####           
       #####   ###   ########   ######         
      ######   ###  ###########   ######       
     ######   #### ##############  ######      
    #######  #####################  ######     
    #######  ######################  ######    
   #######  ###### #################  ######   
   #######  ###### ###### #########   ######   
   #######    ##  ######   ######     ######   
   #######        ######    #####     #####    
    ######        #####     #####     ####     
     #####        ####      #####     ###      
      #####       ###        ###      #        
        ###       ###        ###               
         ##       ###        ###               
__________#_______####_______####______________
                我们的未来没有BUG                
* ==============================================================================
* Filename: StartUp
* Created:  2018/7/2 11:36:16
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/

using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace MikuLuaProfiler_Editor
{

    public static class InjectMethods
    {
        private static MethodDefinition m_beginSampleMethod;
        private static MethodDefinition m_endSampleMethod;
        private static MethodDefinition m_setMainL;
        private static MethodDefinition m_getMainL;
        private static MethodDefinition m_registerLua;
        private static MethodDefinition m_hookloadbuffer;
        private static MethodDefinition m_hookRef;
        private static MethodDefinition m_hookUnref;
        public delegate void InjectMethodAction(MethodDefinition method, ModuleDefinition module, MethodDefinition newMethod);

        private const string LUA_NEW_STATE = "luaL_newstate";
        private const string LUA_CLOSE = "lua_close";
        private const string LUA_REF = "luaL_ref";
        private const string LUA_UNREF = "luaL_unref";
#if XLUA
        private const string LUA_FULL_NAME = "XLua.LuaDLL.Lua";
        private const string LUA_LOAD_BUFFER = "xluaL_loadbuffer";
#elif SLUA
        private const string LUA_FULL_NAME = "SLua.LuaDLL";
        private const string LUA_LOAD_BUFFER = "luaLS_loadbuffer";
#elif TOLUA
        private const string LUA_FULL_NAME = "LuaInterface.LuaDLL";
        private const string LUA_LOAD_BUFFER = "tolua_loadbuffer";
#endif

        public static void InjectAllMethods(string injectPath, string profilerPath)
        {
            AssemblyDefinition injectAss = LoadAssembly(injectPath);
            AssemblyDefinition profilerAss = null;
            if (injectPath == profilerPath)
            {
                profilerAss = injectAss;
            }
            else
            {
                profilerAss = LoadAssembly(profilerPath);
            }
            var profilerType = profilerAss.MainModule.GetType("MikuLuaProfiler.LuaProfiler");
            foreach (var m in profilerType.Methods)
            {
                if (m.Name == "BeginSampleCSharp")
                {
                    m_beginSampleMethod = m;
                }
                if (m.Name == "EndSampleCSharp")
                {
                    m_endSampleMethod = m;
                }
            }

            var module = injectAss.MainModule;
            foreach (var type in injectAss.MainModule.Types)
            {
                if (type.FullName.Contains("MikuLuaProfiler"))
                {
                    continue;
                }

                foreach (var item in type.Methods)
                {
                    //丢弃协同 
                    if (item.ReturnType.Name.Contains("IEnumerator"))
                    {
                        continue;
                    }

                    if (item.Name == ".cctor")
                    {
                        continue;
                    }

                    if (item.Name == ".ctor")
                    {
                        if (item.DeclaringType.IsSerializable)
                        {
                            continue;
                        }
                        bool isMonoBehaviour = IsMonoBehavior(item.DeclaringType.BaseType.Resolve());
                        if (isMonoBehaviour)
                        {
                            continue;
                        }
                    }

                    if (item.IsAbstract)
                    {
                        continue;
                    }
                    if (item.IsPInvokeImpl)
                    {
                        continue;
                    }
                    if (item.Body == null)
                    {
                        continue;
                    }

                    InjectTryFinally(item, module);
                }
            }

            WriteAssembly(injectPath, injectAss);
        }

        public static void HookLuaFun(string luaPath, string profilerPath)
        {
#if XLUA || TOLUA || SLUA
            AssemblyDefinition luaAssembly = LoadAssembly(luaPath);
            AssemblyDefinition profilerAssembly = null;
            if (luaPath == profilerPath)
            {
                profilerAssembly = luaAssembly;
            }
            else
            {
                profilerAssembly = LoadAssembly(profilerPath);
            }


            #region find lua method
            var profilerType = luaAssembly.MainModule.GetType(LUA_FULL_NAME);
            foreach (var m in profilerType.Methods)
            {
                //已经注入了就不注入了
                if (m.Name == LUA_NEW_STATE + "_profiler")
                {
                    return;
                }
            }
            #endregion

            #region find profiler method
            var luaProfiler = profilerAssembly.MainModule.GetType("MikuLuaProfiler.LuaProfiler");

            foreach (var m in luaProfiler.Methods)
            {
                if (m.Name == "set_mainL")
                {
                    m_setMainL = m;
                }
                else if (m.Name == "get_mainL")
                {
                    m_getMainL = m;
                }
            }

            var luaHookType = profilerAssembly.MainModule.GetType("MikuLuaProfiler.LuaHook");
            foreach (var m in luaHookType.Methods)
            {
                if (m.Name == "Hookloadbuffer")
                {
                    m_hookloadbuffer = m;
                }
                else if (m.Name == "HookRef")
                {
                    m_hookRef = m;
                }
                else if (m.Name == "HookUnRef")
                {
                    m_hookUnref = m;
                }
            }

            var luaRegister = profilerAssembly.MainModule.GetType("MikuLuaProfiler.MikuLuaProfilerLuaProfilerWrap");
            foreach (var m in luaRegister.Methods)
            {
                if (m.Name == "__Register")
                {
                    m_registerLua = m;
                    break;
                }
            }
            #endregion

            Dictionary<string, InjectMethodAction> hookExternfunDict = new Dictionary<string, InjectMethodAction>
            {
                { LUA_NEW_STATE, InjectNewStateMethod},
                { LUA_CLOSE, InjectCloseMethod },
                { LUA_LOAD_BUFFER, InjectLoaderMethod },
                { LUA_REF, InjectRefMethod },
                { LUA_UNREF, InjectUnrefMethod },
            };

            HookDllFun(profilerType, hookExternfunDict, luaAssembly);
#if SLUA
            profilerType = luaAssembly.MainModule.GetType("SLua.LuaDLLWrapper");
            HookDllFun(profilerType, hookExternfunDict, luaAssembly);
#endif

            luaAssembly.Write(luaPath);
#endif
        }

        #region try finally
        private static bool IsMonoBehavior(TypeDefinition td)
        {
            if (td == null) return false;

            if (td.FullName == "UnityEngine.MonoBehaviour")
            {
                return true;
            }
            else
            {
                if (td.BaseType == null)
                {
                    return false;
                }
                else
                {
                    return IsMonoBehavior(td.BaseType.Resolve());
                }
            }
        }

        private static string GetReflectionName(this TypeReference type)
        {
            if (type.IsGenericInstance)
            {
                var genericInstance = (GenericInstanceType)type;
                return string.Format("{0}.{1}[{2}]", genericInstance.Namespace, type.Name, String.Join(",", genericInstance.GenericArguments.Select(p => p.GetReflectionName()).ToArray()));
            }
            return type.FullName;
        }

        internal static Instruction FixReturns(this ILProcessor ilProcessor)
        {
            var methodDefinition = ilProcessor.Body.Method;

            if (methodDefinition.ReturnType == methodDefinition.Module.TypeSystem.Void)
            {
                var instructions = ilProcessor.Body.Instructions.ToArray();

                var newReturnInstruction = ilProcessor.Create(OpCodes.Ret);
                ilProcessor.Append(newReturnInstruction);

                foreach (var instruction in instructions)
                {
                    if (instruction.OpCode == OpCodes.Ret)
                    {
                        var leaveInstruction = ilProcessor.Create(OpCodes.Leave, newReturnInstruction);
                        ilProcessor.Replace(instruction, leaveInstruction);
                        ilProcessor.ReplaceInstructionReferences(instruction, leaveInstruction);
                    }
                }

                return newReturnInstruction;
            }
            else
            {
                var instructions = ilProcessor.Body.Instructions.ToArray();

                var returnVariable = new VariableDefinition(methodDefinition.ReturnType);
                ilProcessor.Body.Variables.Add(returnVariable);

                var loadResultInstruction = ilProcessor.Create(OpCodes.Ldloc, returnVariable);
                ilProcessor.Append(loadResultInstruction);
                var newReturnInstruction = ilProcessor.Create(OpCodes.Ret);
                ilProcessor.Append(newReturnInstruction);

                foreach (var instruction in instructions)
                {
                    if (instruction.OpCode == OpCodes.Ret)
                    {
                        var leaveInstruction = ilProcessor.Create(OpCodes.Leave, loadResultInstruction);
                        ilProcessor.Replace(instruction, leaveInstruction);
                        ilProcessor.ReplaceInstructionReferences(instruction, leaveInstruction);
                        var saveResultInstruction = ilProcessor.Create(OpCodes.Stloc, returnVariable);
                        ilProcessor.InsertBefore(leaveInstruction, saveResultInstruction);
                        ilProcessor.ReplaceInstructionReferences(leaveInstruction, saveResultInstruction);
                    }
                }

                return loadResultInstruction;
            }
        }

        internal static void ReplaceInstructionReferences(
           this ILProcessor ilProcessor,
           Instruction oldInstruction,
           Instruction newInstruction)
        {
            foreach (var handler in ilProcessor.Body.ExceptionHandlers)
            {
                if (handler.FilterStart == oldInstruction)
                    handler.FilterStart = newInstruction;

                if (handler.TryStart == oldInstruction)
                    handler.TryStart = newInstruction;

                if (handler.TryEnd == oldInstruction)
                    handler.TryEnd = newInstruction;

                if (handler.HandlerStart == oldInstruction)
                    handler.HandlerStart = newInstruction;

                if (handler.HandlerEnd == oldInstruction)
                    handler.HandlerEnd = newInstruction;
            }

            // Update instructions with a target instruction
            foreach (var iteratedInstruction in ilProcessor.Body.Instructions)
            {
                var operand = iteratedInstruction.Operand;

                if (operand == oldInstruction)
                {
                    iteratedInstruction.Operand = newInstruction;
                    continue;
                }
                else if (operand is Instruction[])
                {
                    Instruction[] operands = (Instruction[])operand;
                    for (var i = 0; i < operands.Length; ++i)
                    {
                        if (operands[i] == oldInstruction)
                            operands[i] = newInstruction;
                    }
                }
            }
        }

        private static Instruction FirstInstructionSkipCtor(MethodDefinition Method)
        {
            var body = Method.Body;
            if (Method.IsConstructor && !Method.IsStatic)
            {
                return body.Instructions[1];
            }
            return body.Instructions[0];
        }

        private static void InjectTryFinally(MethodDefinition method, ModuleDefinition module)
        {
            if (method.Body == null) return;
            var il = method.Body.GetILProcessor();
            var firstInstruction = FirstInstructionSkipCtor(method);

            var beginSample = il.Create(
                OpCodes.Call,
                module.ImportReference(m_beginSampleMethod));
            il.InsertBefore(il.Body.Instructions[0], beginSample);
            il.InsertBefore(il.Body.Instructions[0], il.Create(OpCodes.Ldstr, "[C#]:" + method.DeclaringType.Name + "::" + method.Name));
            il.InsertBefore(il.Body.Instructions[0], il.Create(OpCodes.Nop));

            var returnInstruction = il.FixReturns();
            var beforeReturn = Instruction.Create(OpCodes.Nop);
            il.InsertBefore(returnInstruction, beforeReturn);

            var endSample = il.Create(
                OpCodes.Call,
                module.ImportReference(m_endSampleMethod));
            il.InsertBefore(returnInstruction, endSample);
            il.InsertBefore(returnInstruction, Instruction.Create(OpCodes.Endfinally));

            var handler = new ExceptionHandler(ExceptionHandlerType.Finally)
            {
                TryStart = firstInstruction,
                TryEnd = beforeReturn,
                HandlerStart = beforeReturn,
                HandlerEnd = returnInstruction,
            };

            method.Body.ExceptionHandlers.Add(handler);
            method.Body.InitLocals = true;
            method = method.Resolve();
        }
        #endregion

        #region tool
        private static SequencePoint GetSequencePoint(MethodBody body)
        {
            if (body == null)
            {
                return null;
            }
            Instruction instruction = body.Instructions.FirstOrDefault(x => x.SequencePoint != null);
            return instruction == null ? null : instruction.SequencePoint;
        }

        public static AssemblyDefinition LoadAssembly(string path)
        {
            AssemblyDefinition result = null;
            result = AssemblyDefinition.ReadAssembly(path);
            AddResolver(result);
            return result;
        }

        public static void WriteAssembly(string path, AssemblyDefinition ass)
        {
            ass.Write(path);
        }

        private static string GetMD5HashFromFile(string path)
        {
            if (!File.Exists(path))
                throw new ArgumentException(string.Format("<{0}>, 不存在", path));
            int bufferSize = 1024 * 1024;//自定义缓冲区大小16K 
            byte[] buffer = new byte[bufferSize];
            Stream inputStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            HashAlgorithm hashAlgorithm = new MD5CryptoServiceProvider();
            int readLength = 0;//每次读取长度 
            var output = new byte[bufferSize];
            while ((readLength = inputStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                //计算MD5 
                hashAlgorithm.TransformBlock(buffer, 0, readLength, output, 0);
            }
            //完成最后计算，必须调用(由于上一部循环已经完成所有运算，所以调用此方法时后面的两个参数都为0) 
            hashAlgorithm.TransformFinalBlock(buffer, 0, 0);
            string md5 = BitConverter.ToString(hashAlgorithm.Hash);
            hashAlgorithm.Clear();
            inputStream.Close();
            inputStream.Dispose();

            md5 = md5.Replace("-", "");
            return md5;
        }

        public static void AddResolver(AssemblyDefinition assembly)
        {
            DefaultAssemblyResolver defaultAssemblyResolver = assembly.MainModule.AssemblyResolver as DefaultAssemblyResolver;
            HashSet<string> hashSet = new HashSet<string>();
            hashSet.Add(Path.GetDirectoryName(InjectToLua.Program.filePath));
            foreach (string path in (from asm in AppDomain.CurrentDomain.GetAssemblies()
                                     select asm.ManifestModule.FullyQualifiedName).Distinct<string>())
            {
                try
                {
                    string directoryName = Path.GetDirectoryName(path);
                    if (!hashSet.Contains(directoryName))
                    {
                        hashSet.Add(directoryName);
                    }
                }
                catch
                {
                }
            }
            foreach (string text in hashSet)
            {
                defaultAssemblyResolver.AddSearchDirectory(text);
            }
        }
        #endregion

        #region hook
        private static void HookDllFun(TypeDefinition profilerType, Dictionary<string, InjectMethodAction> hookExternfunDict, AssemblyDefinition assembly)
        {
#if XLUA || TOLUA || SLUA
            var methods = new List<MethodDefinition>(profilerType.Methods);
            foreach (var m in methods)
            {
                InjectMethodAction action;
                if (hookExternfunDict.TryGetValue(m.Name, out action))
                {
                    if (!m.IsPInvokeImpl) continue;
                    MethodAttributes attr = MethodAttributes.Public;
                    PInvokeInfo pInfo = null;
                    attr = m.Attributes;
                    pInfo = m.PInvokeInfo;
                    m.ImplAttributes = MethodImplAttributes.IL;
                    m.Attributes = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig;

                    var method = new MethodDefinition(m.Name + "_profiler", attr, m.ReturnType);

                    method.ImplAttributes = MethodImplAttributes.PreserveSig;
                    method.PInvokeInfo = pInfo;
                    if (m.Body.Variables != null)
                    {
                        foreach (var item in m.Body.Variables)
                        {
                            method.Body.Variables.Add(item);
                        }
                    }

                    method.Parameters.Clear();
                    foreach (var item in m.Parameters)
                    {
                        method.Parameters.Add(item);
                    }

                    profilerType.Methods.Add(method);
                    if (action != null)
                    {
                        action(m, assembly.MainModule, method);
                    }
                }
            }
#endif
        }

        private static void InjectNewStateMethod(MethodDefinition method, ModuleDefinition module, MethodDefinition newMethod)
        {
            if (method.Body == null) return;
            VariableDefinition injection = null;
            injection = new VariableDefinition(module.ImportReference(newMethod.ReturnType));
            method.Body.Variables.Add(injection);
            injection = new VariableDefinition(module.ImportReference(newMethod.ReturnType));
            method.Body.Variables.Add(injection);

            var il = method.Body.GetILProcessor();

            il.Append(il.Create(OpCodes.Nop));
            il.Append(il.Create(OpCodes.Call, module.ImportReference(newMethod)));
            il.Append(il.Create(OpCodes.Stloc_0));
            il.Append(il.Create(OpCodes.Ldloc_0));
            il.Append(il.Create(OpCodes.Call, module.ImportReference(m_setMainL)));
            il.Append(il.Create(OpCodes.Ldloc_0));
            il.Append(il.Create(OpCodes.Call, module.ImportReference(m_registerLua)));
            il.Append(il.Create(OpCodes.Ldloc_0));
            il.Append(il.Create(OpCodes.Stloc_1));
            var ldloc1 = il.Create(OpCodes.Ldloc_1);
            il.Append(ldloc1);
            il.Append(il.Create(OpCodes.Ret));
            il.InsertBefore(ldloc1, il.Create(OpCodes.Br, ldloc1));
        }

        private static void InjectCloseMethod(MethodDefinition method, ModuleDefinition module, MethodDefinition newMethod)
        {
            if (method.Body == null) return;

            var il = method.Body.GetILProcessor();

            il.Append(il.Create(OpCodes.Nop));
            il.Append(il.Create(OpCodes.Call, module.ImportReference(m_getMainL)));
            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Call, module.ImportReference(typeof(IntPtr).GetMethod("op_Equality"))));
            var nop = il.Create(OpCodes.Nop);
            il.Append(nop);
            il.Append(il.Create(OpCodes.Ldsfld, module.ImportReference(typeof(IntPtr).GetField("Zero"))));
            il.Append(il.Create(OpCodes.Call, module.ImportReference(m_setMainL)));
            il.Append(il.Create(OpCodes.Nop));

            var ldarg0 = il.Create(OpCodes.Ldarg_0);
            il.Append(ldarg0);
            il.Append(il.Create(OpCodes.Call, module.ImportReference(newMethod)));
            il.Append(il.Create(OpCodes.Ret));

            il.InsertBefore(nop, il.Create(OpCodes.Brfalse, ldarg0));
        }

        private static void InjectLoaderMethod(MethodDefinition method, ModuleDefinition module, MethodDefinition newMethod)
        {
            if (method.Body == null) return;
            VariableDefinition injection = null;
            method.Body.Variables.Clear();
            injection = new VariableDefinition(module.ImportReference(module.TypeSystem.Int32));
            method.Body.Variables.Add(injection);
            injection = new VariableDefinition(module.ImportReference(module.TypeSystem.Int32));
            method.Body.Variables.Add(injection);

            var bufParam = method.Parameters.FirstOrDefault(p => p.Name == "buff");

            var il = method.Body.GetILProcessor();

            il.Append(il.Create(OpCodes.Nop));
            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Ldarg_1));
            il.Append(il.Create(OpCodes.Ldarg_3));

            il.Append(il.Create(OpCodes.Call, module.ImportReference(m_hookloadbuffer)));
            il.Append(il.Create(OpCodes.Starg_S, bufParam));
            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Ldarg_1));
            il.Append(il.Create(OpCodes.Ldarg_1));
            il.Append(il.Create(OpCodes.Ldlen));
            il.Append(il.Create(OpCodes.Conv_I4));
            il.Append(il.Create(OpCodes.Ldarg_3));
            il.Append(il.Create(OpCodes.Call, module.ImportReference(newMethod)));
            il.Append(il.Create(OpCodes.Stloc_0));
            il.Append(il.Create(OpCodes.Ldloc_0));
            il.Append(il.Create(OpCodes.Stloc_1));

            var ldloc1 = il.Create(OpCodes.Ldloc_1);
            il.Append(ldloc1);
            il.Append(il.Create(OpCodes.Ret));

            il.InsertBefore(ldloc1, il.Create(OpCodes.Br, ldloc1));
        }

        private static void InjectRefMethod(MethodDefinition method, ModuleDefinition module, MethodDefinition newMethod)
        {
            if (method.Body == null) return;
            VariableDefinition injection = null;
            method.Body.Variables.Clear();
            injection = new VariableDefinition(module.ImportReference(module.TypeSystem.Int32));
            method.Body.Variables.Add(injection);

            var il = method.Body.GetILProcessor();
            il.Append(il.Create(OpCodes.Nop));

            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Ldarg_1));
            il.Append(il.Create(OpCodes.Call, module.ImportReference(newMethod)));
            il.Append(il.Create(OpCodes.Stloc_0));

            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Ldloc_0));
            il.Append(il.Create(OpCodes.Call, module.ImportReference(m_hookRef)));
            il.Append(il.Create(OpCodes.Ldloc_0));
            il.Append(il.Create(OpCodes.Ret));
        }

        private static void InjectUnrefMethod(MethodDefinition method, ModuleDefinition module, MethodDefinition newMethod)
        {
            if (method.Body == null) return;

            var il = method.Body.GetILProcessor();
            il.Append(il.Create(OpCodes.Nop));
            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Ldarg_2));
            il.Append(il.Create(OpCodes.Call, module.ImportReference(m_hookUnref)));
            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Ldarg_1));
            il.Append(il.Create(OpCodes.Ldarg_2));
            il.Append(il.Create(OpCodes.Call, module.ImportReference(newMethod)));
            il.Append(il.Create(OpCodes.Ret));
        }

        private static void CopyMethod(MethodDefinition method, MethodDefinition newMethod)
        {
            foreach (var item in method.Body.Variables)
            {
                newMethod.Body.Variables.Add(item);
            }

            foreach (var item in method.Parameters)
            {
                newMethod.Parameters.Add(item);
            }

            foreach (var item in method.CustomAttributes)
            {
                newMethod.CustomAttributes.Add(item);
            }

            var il = method.Body.Instructions;
            var newIl = newMethod.Body.Instructions;
            newIl.Clear();
            foreach (var item in il)
            {
                newIl.Add(item);
            }
        }

        #endregion

    }
}