#define XLUA
/*
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
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
#if XLUA
using LuaDLL = XLua.LuaDLL.Lua;
#elif TOLUA
using LuaDLL = LuaInterface.LuaDLL;
#elif SLUA
using LuaDLL = SLua.LuaDLL;
#endif


namespace MikuLuaProfiler
{

    [InitializeOnLoad]
    public static class StartUp
    {
        //private static int tickNum = 0;
        static StartUp()
        {
            if (LuaDeepProfilerSetting.Instance.isDeepLuaProfiler)
            {
                InjectMethods.HookLuaFun();
            }
            if (LuaDeepProfilerSetting.Instance.isDeepMonoProfiler)
            {
                InjectMethods.InjectAllMethods();
            }

#if XLUA || TOLUA || SLUA
            if (LuaDeepProfilerSetting.Instance.isInited) return;
#endif
            string[] paths = Directory.GetFiles(Application.dataPath, "*.dll", SearchOption.AllDirectories);
            foreach (var item in paths)
            {
                string fileName = Path.GetFileName(item);
                if (fileName == "slua.dll")
                {
                    AppendMacro("#define SLUA");
                }

                if (fileName == "xlua.dll")
                {
                    AppendMacro("#define XLUA");
                    break;
                }

                if (fileName == "tolua.dll")
                {
                    AppendMacro("#define TOLUA");
                    break;
                }
            }

            LuaDeepProfilerSetting.Instance.isInited = true;
        }

        //private static void Update()
        //{
        //    tickNum++;
        //    if (tickNum < 60)
        //    {
        //        return;
        //    }
        //    Debug.Log("success inject profiler code");
        //    EditorApplication.update -= callback;
        //}

         private static void AppendMacro(string macro)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
            System.Diagnostics.StackFrame sf = st.GetFrame(0);
            string path = sf.GetFileName();
            string selfPath = path;

#if UNITY_EDITOR_WIN
            path = path.Replace("Editor\\StartUp.cs", "Core\\LuaHookSetup.cs");
#else
            path = path.Replace("Editor/StartUp.cs", "Core/LuaHookSetup.cs");
#endif
            AppendText(macro, selfPath);
            AppendText(macro, path);
        }

        private static void AppendText(string macro, string path)
        {
            string text = File.ReadAllText(path);
            string text2 = new StringReader(text).ReadLine();
            if (text2.Contains("#define"))
            {
                text = text.Substring(text2.Length, text.Length - text2.Length);
            }
            else
            {
                macro += "\r\n";
            }
            text = text.Insert(0, macro);
            File.WriteAllText(path, text);
        }
    }

    public static class InjectMethods
    {
        private static MethodDefinition m_beginSampleMethod;
        private static MethodDefinition m_endSampleMethod;
        private static MethodDefinition m_setMainL;
        private static MethodDefinition m_getMainL;
        private static MethodDefinition m_registerLua;
        private static MethodDefinition m_hookloadbuffer;
        private static MethodDefinition m_tolstring;
        private static MethodDefinition m_trygetluastring;
        private static MethodDefinition m_refstring;
        public delegate void InjectMethodAction(MethodDefinition method, ModuleDefinition module, MethodDefinition newMethod);

#if XLUA
        private const string LUA_FULL_NAME = "XLua.LuaDLL.Lua";
        private const string LUA_NEW_STATE = "luaL_newstate";
        private const string LUA_CLOSE = "lua_close";
        private const string LUA_TOSTRING = "lua_tostring";
        private const string LUA_LOAD_BUFFER = "xluaL_loadbuffer";
        private const string LUA_NATIVE_TOSTRING = "lua_tolstring";
#elif SLUA
        private const string LUA_FULL_NAME = "SLua.LuaDLL";
        private const string LUA_NEW_STATE = "luaL_newstate";
        private const string LUA_CLOSE = "lua_close";
        private const string LUA_TOSTRING = "lua_tostring";
        private const string LUA_LOAD_BUFFER = "luaLS_loadbuffer";
        private const string LUA_NATIVE_TOSTRING = "luaS_tolstring32";
#endif

        #region try finally
        public static void InjectAllMethods()
        {
            if (EditorApplication.isCompiling)
            {
                Debug.LogError("is compiling");
                return;
            }

            var projectPath = System.Reflection.Assembly.Load("Assembly-CSharp").ManifestModule.FullyQualifiedName;
            var profilerPath = (typeof(LuaProfiler).Assembly).ManifestModule.FullyQualifiedName;

            InjectAllMethods(projectPath, profilerPath, true);
        }

        private static void InjectAllMethods(string injectPath, string profilerPath, bool needMdb)
        {
            string md5 = null;
            md5 = GetMD5HashFromFile(injectPath);
            if (md5 == LuaDeepProfilerSetting.Instance.assMd5) return;

            AssemblyDefinition injectAss = LoadAssembly(injectPath, needMdb);
            AssemblyDefinition profilerAss = null;
            if (injectPath == profilerPath)
            {
                profilerAss = injectAss;
            }
            else
            {
                profilerAss = LoadAssembly(profilerPath, needMdb);
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
                        var t = item.DeclaringType.GetMonoType();
                        bool isMonoBehaviour = typeof(MonoBehaviour).IsAssignableFrom(t);
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
            LuaDeepProfilerSetting.Instance.assMd5 = GetMD5HashFromFile(injectPath);
        }

        public static Type GetMonoType(this TypeReference type)
        {
            return System.Reflection.Assembly.Load(type.Module.Assembly.Name.Name).GetType(type.GetReflectionName());
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
        public static AssemblyDefinition LoadAssembly(string path, bool needRead)
        {
            AssemblyDefinition result = null;
            bool flag = needRead;
            if (needRead)
            {
                flag = File.Exists(path + ".mdb");
            }


            if (flag)
            {
                ReaderParameters readerParameters = new ReaderParameters
                {
                    ReadSymbols = true
                };
                result = AssemblyDefinition.ReadAssembly(path, readerParameters);
            }
            else
            {
                result = AssemblyDefinition.ReadAssembly(path);
            }
            AddResolver(result);
            return result;
        }

        public static void WriteAssembly(string path, AssemblyDefinition ass)
        {
            bool flag = File.Exists(path + ".mdb");

            if (flag)
            {
                WriterParameters writerParameters = new WriterParameters
                {
                    WriteSymbols = true
                };
                ass.Write(path, writerParameters);
            }
            else
            {
                ass.Write(path);
            }
        }

        public static void Recompile()
        {
            BuildTargetGroup bg = BuildTargetGroup.Standalone;
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.Android:
                    bg = BuildTargetGroup.Android;
                    break;
                case BuildTarget.iOS:
                    bg = BuildTargetGroup.iOS;
                    break;
            }
            string path = PlayerSettings.GetScriptingDefineSymbolsForGroup(bg);
            bool hasRecompile = false;

            string[] heads = path.Split(';');
            path = "";
            foreach (var item in heads)
            {
                if (item == "MIKU_RECOMPILE")
                {
                    hasRecompile = true;
                    continue;
                }
                path += item + ";";
            }

            if (!hasRecompile)
            {
                path += "MIKU_RECOMPILE;";
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(bg, path);
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

        private static void AddResolver(AssemblyDefinition assembly)
        {
            var assemblyResolver = assembly.MainModule.AssemblyResolver as DefaultAssemblyResolver;
            HashSet<string> paths = new HashSet<string>();
            paths.Add("./Library/ScriptAssemblies/");
            foreach (string path in (from asm in System.AppDomain.CurrentDomain.GetAssemblies()
                                     select asm.ManifestModule.FullyQualifiedName).Distinct<string>())
            {
                try
                {
                    string dir = Path.GetDirectoryName(path);
                    if (!paths.Contains(dir))
                    {
                        paths.Add(dir);
                    }
                }
                catch
                {
                }
            }

            foreach (var item in paths)
            {
                assemblyResolver.AddSearchDirectory(item);
            }
        }
        #endregion

        #region hook
        public static void HookLuaFun()
        {
            string profilerPath = (typeof(LuaProfiler).Assembly).ManifestModule.FullyQualifiedName;
            string luaPath = (typeof(LuaDLL).Assembly).ManifestModule.FullyQualifiedName;
            DoHookLuaFun(luaPath, profilerPath);
        }

        private static void DoHookLuaFun(string luaPath, string profilerPath)
        {
            AssemblyDefinition luaAssembly = LoadAssembly(luaPath, false);
            AssemblyDefinition profilerAssembly = null;
            if (luaPath == profilerPath)
            {
                profilerAssembly = luaAssembly;
            }
            else
            {
                profilerAssembly = LoadAssembly(profilerPath, false);
            }
            

            #region find lua method
            var profilerType = luaAssembly.MainModule.GetType(LUA_FULL_NAME);
            if (profilerType == null) return;
            foreach (var m in profilerType.Methods)
            {
                //已经注入了就不注入了
                if (m.Name == LUA_NEW_STATE + "_profiler")
                {
                    return;
                }
                if (m.Name == LUA_NATIVE_TOSTRING)
                {
                    m_tolstring = m;
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
                else if (m.Name == "TryGetLuaString")
                {
                    m_trygetluastring = m;
                }
                else if (m.Name == "RefString")
                {
                    m_refstring = m;
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
                { LUA_LOAD_BUFFER, InjectLoaderMethod }
            };

            HookDllFun(profilerType, hookExternfunDict, luaAssembly);
#if SLUA
            profilerType = luaAssembly.MainModule.GetType("SLua.LuaDLLWrapper");
            HookDllFun(profilerType, hookExternfunDict, luaAssembly);
#endif

            luaAssembly.Write(luaPath);
        }

        private static void HookDllFun(TypeDefinition profilerType, Dictionary<string, InjectMethodAction> hookExternfunDict, AssemblyDefinition assembly)
        {
            var methods = new List<MethodDefinition>(profilerType.Methods);
            foreach (var m in methods)
            {
                InjectMethodAction action;
                if (hookExternfunDict.TryGetValue(m.Name, out action))
                {
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
                else if (m.Name == LUA_TOSTRING)
                {
                    var method = new MethodDefinition(m.Name + "_profiler", m.Attributes, m.ReturnType);
                    profilerType.Methods.Add(method);
                    CopyMethod(m, method);
                    ModifyTostring(m, assembly.MainModule, method);
                }
            }
        }

        private static void InjectNewStateMethod(MethodDefinition method, ModuleDefinition module, MethodDefinition newMethod)
        {
            if (method.Body == null) return;
            VariableDefinition injection = null;
            injection = new VariableDefinition(newMethod.ReturnType);
            method.Body.Variables.Add(injection);
            injection = new VariableDefinition(newMethod.ReturnType);
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
            injection = new VariableDefinition(module.TypeSystem.Int32);
            method.Body.Variables.Add(injection);
            injection = new VariableDefinition(module.TypeSystem.Int32);
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

        private static void ModifyTostring(MethodDefinition tostring, ModuleDefinition module, MethodDefinition newMethod)
        {
            if (tostring.Body == null) return;
            VariableDefinition injection = null;
            tostring.Body.Variables.Clear();
#if XLUA
            injection = new VariableDefinition(module.TypeSystem.IntPtr);
#elif SLUA
            injection = new VariableDefinition(module.TypeSystem.Int32);
#endif
            tostring.Body.Variables.Add(injection);
            injection = new VariableDefinition(module.TypeSystem.IntPtr);
            tostring.Body.Variables.Add(injection);
            injection = new VariableDefinition(module.TypeSystem.String);
            tostring.Body.Variables.Add(injection);
            injection = new VariableDefinition(module.TypeSystem.String);
            tostring.Body.Variables.Add(injection);

            var isNullMethod = typeof(string).GetMethod("IsNullOrEmpty");
            var intern = typeof(string).GetMethod("Intern");

            var ils = tostring.Body.Instructions;
            ils.Clear();
            var il = tostring.Body.GetILProcessor();

            var il8 = il.Create(OpCodes.Call, module.ImportReference(m_trygetluastring));
            var il16 = il.Create(OpCodes.Call, module.ImportReference(isNullMethod));
            var il23 = il.Create(OpCodes.Ldloc_1);
            var il29 = il.Create(OpCodes.Ldloc_2);
            var il30 = il.Create(OpCodes.Stloc_3);
            var il32 = il.Create(OpCodes.Ldloc_3);

            il.Append(il.Create(OpCodes.Nop));                                                  //0
            il.Append(il.Create(OpCodes.Ldarg_0));                                              //1
            il.Append(il.Create(OpCodes.Ldarg_1));                                              //2
            il.Append(il.Create(OpCodes.Ldloca_S, tostring.Body.Variables[0]));                 //3
            il.Append(il.Create(OpCodes.Call, module.ImportReference(m_tolstring)));            //4
            il.Append(il.Create(OpCodes.Stloc_1));                                              //5
            il.Append(il.Create(OpCodes.Ldloc_1));                                              //6
            il.Append(il.Create(OpCodes.Ldloca_S, tostring.Body.Variables[2]));                 //7
            il.Append(il8);                                                                     //8
            il.InsertAfter(il8, il.Create(OpCodes.Brtrue, il29));                               //9
            il.Append(il.Create(OpCodes.Nop));                                                  //10
            il.Append(il.Create(OpCodes.Ldarg_0));                                              //11
            il.Append(il.Create(OpCodes.Ldarg_1));                                              //12
            il.Append(il.Create(OpCodes.Call, module.ImportReference(newMethod)));              //13
            il.Append(il.Create(OpCodes.Stloc_2));                                              //14
            il.Append(il.Create(OpCodes.Ldloc_2));                                              //15
            il.Append(il16);                                                                    //16
            il.InsertAfter(il16, il.Create(OpCodes.Brtrue, il23));                              //17
            il.Append(il.Create(OpCodes.Nop));                                                  //18
            il.Append(il.Create(OpCodes.Ldloc_2));                                              //19
            il.Append(il.Create(OpCodes.Call, module.ImportReference(intern)));                 //20
            il.Append(il.Create(OpCodes.Stloc_2));                                              //21
            il.Append(il.Create(OpCodes.Nop));                                                  //22
            il.Append(il23);                                                                    //23
            il.Append(il.Create(OpCodes.Ldarg_1));                                              //24
            il.Append(il.Create(OpCodes.Ldloc_2));                                              //25
            il.Append(il.Create(OpCodes.Ldarg_0));                                              //26
            il.Append(il.Create(OpCodes.Call, module.ImportReference(m_refstring)));            //27
            il.Append(il.Create(OpCodes.Nop));                                                  //28
            il.Append(il29);                                                                    //29
            il.Append(il30);                                                                    //30
            il.InsertAfter(il30, il.Create(OpCodes.Br, il32));                                  //31
            il.Append(il32);                                                                    //32
            il.Append(il.Create(OpCodes.Ret));                                                  //33

        }
        #endregion

#if USE_LUA_PROFILER
        [PostProcessScene]
        private static void OnPostprocessScene()
        {
            var luaPath = (typeof(LuaDLL).Assembly).ManifestModule.FullyQualifiedName;
            var projectPath = System.Reflection.Assembly.Load("Assembly-CSharp").ManifestModule.FullyQualifiedName;
            var profilerPath = (typeof(LuaProfiler).Assembly).ManifestModule.FullyQualifiedName;
            if (LuaDeepProfilerSetting.Instance.isDeepLuaProfiler)
            {
                DoHookLuaFun(luaPath, profilerPath);
            }
            if (LuaDeepProfilerSetting.Instance.isDeepMonoProfiler)
            {
                InjectAllMethods(projectPath, profilerPath, false);
            }
        }
#endif
    }
}