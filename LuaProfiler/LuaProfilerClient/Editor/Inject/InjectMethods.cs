/*
* ==============================================================================
* Filename: InjectMethods
* Created:  2018/7/13 14:29:22
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
using System.Runtime;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace MikuLuaProfiler
{
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

        #region try finally
        public static void InjectAllMethods()
        {
            if (EditorApplication.isCompiling)
            {
                Debug.LogError("is compiling");
                return;
            }

            string assemblyPath = "./Library/ScriptAssemblies/Assembly-CSharp.dll";
            InjectAllMethods(assemblyPath);
        }

        private static void InjectAllMethods(string assemblyPath, bool needMd5 = true)
        {
            string md5 = null;
            if (needMd5)
            {
                md5 = GetMD5HashFromFile(assemblyPath);
                if (md5 == LuaDeepProfilerSetting.Instance.assMd5) return;
            }

            bool flag = File.Exists(assemblyPath + ".mdb");
            AssemblyDefinition assembly;

            if (flag)
            {
                ReaderParameters readerParameters = new ReaderParameters
                {
                    ReadSymbols = true
                };
                assembly = AssemblyDefinition.ReadAssembly(assemblyPath, readerParameters);
            }
            else
            {
                assembly = AssemblyDefinition.ReadAssembly(assemblyPath);
            }
            AddResolver(assembly);

            //m_getCurrentMethod = typeof(MethodBase).GetMethod("GetCurrentMethod");

            var profilerType = assembly.MainModule.GetType("MikuLuaProfiler.LuaProfiler");
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
                //if (m.Name == "GetMethodLineString")
                //{
                //    m_getMethodString = m; 
                //}
            }

            var module = assembly.MainModule;
            foreach (var type in assembly.MainModule.Types)
            {

                if (type.FullName.Contains("`"))
                {
                    continue;
                }

                if (type.FullName.Contains("MikuLuaProfiler"))
                {
                    continue;
                }

                foreach (var item in type.Methods)
                {
                    //if (item.IsConstructor) continue;
                    //丢弃协同 

                    if (item.IsConstructor)
                    {
                        continue;
                        //var declaringType = item.DeclaringType;
                        //Type monoType = declaringType.GetMonoType();
                        //Type corType = typeof(MonoBehaviour);
                        //if (corType.IsAssignableFrom(monoType))
                        //{
                        //    continue;
                        //}
                    }
                    if (item.Name == ".cctor")
                    {
                        continue;
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

                    if (item.Name.Contains("Equals"))
                    {
                        continue;
                    }
                    if (item.Name.Contains("GetType"))
                    {
                        continue;
                    }
                    if (item.Name.Contains("GetHashCode"))
                    {
                        continue;
                    }
                    InjectTryFinally(item, module);
                }
            }

            if (flag)
            {
                WriterParameters writerParameters = new WriterParameters
                {
                    WriteSymbols = true
                };
                assembly.Write(assemblyPath, writerParameters);
            }
            else
            {
                assembly.Write(assemblyPath);
            }
            if (needMd5)
            {
                LuaDeepProfilerSetting.Instance.assMd5 = GetMD5HashFromFile(assemblyPath);
            }
            GCSettings.LatencyMode = GCLatencyMode.LowLatency;
        }

        //public static Type GetMonoType(this TypeReference type)
        //{
        //    return Type.GetType(type.GetReflectionName());
        //}

        //private static string GetReflectionName(this TypeReference type)
        //{
        //    if (type.IsGenericInstance)
        //    {
        //        var genericInstance = (GenericInstanceType)type;
        //        return string.Format("{0}.{1}[{2}]", genericInstance.Namespace, type.Name, String.Join(",", genericInstance.GenericArguments.Select(p => p.GetReflectionName()).ToArray()));
        //    }
        //    return type.FullName;
        //}

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

        private static Instruction FixReturns(MethodDefinition Method)
        {
            var body = Method.Body;
            if (Method.ReturnType.FullName == "System.Void")
            {
                var instructions = body.Instructions;
                var lastRet = Instruction.Create(OpCodes.Ret);
                instructions.Add(lastRet);

                for (var index = 0; index < instructions.Count - 1; index++)
                {
                    var instruction = instructions[index];

                    if (instruction.OpCode == OpCodes.Ret)
                    {
                        instruction.OpCode = OpCodes.Leave;
                        instruction.Operand = lastRet;
                    }
                }
                return lastRet;
            }
            else
            {
                var instructions = body.Instructions;
                VariableDefinition returnVariable = null;
                returnVariable = new VariableDefinition("methodTimerReturn", Method.ReturnType);
                body.Variables.Add(returnVariable);
                //if (body.Variables.Count > 0 && body.Variables[0].VariableType == Method.ReturnType)
                //{
                //    foreach (var v in body.Variables)
                //    {
                //        if (v.VariableType == Method.ReturnType)
                //        {
                //            returnVariable = v;
                //        }
                //    }
                //}
                //if(returnVariable == null)
                //{
                //    returnVariable = new VariableDefinition("methodTimerReturn", Method.ReturnType);
                //    body.Variables.Add(returnVariable);
                //}
                var lastLd = Instruction.Create(OpCodes.Ldloc, returnVariable);
                instructions.Add(lastLd);
                instructions.Add(Instruction.Create(OpCodes.Ret));

                for (var index = 0; index < instructions.Count - 2; index++)
                {
                    var instruction = instructions[index];
                    if (instruction.OpCode == OpCodes.Ret)
                    {
                        instruction.OpCode = OpCodes.Leave;
                        instruction.Operand = lastLd;
                        instructions.Insert(index, Instruction.Create(OpCodes.Stloc, returnVariable));
                        index++;
                    }
                }
                return lastLd;
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

            var returnInstruction = FixReturns(method);
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

        #region hook
        public static void HookLuaFun()
        {
            string assemblyPath = "./Library/ScriptAssemblies/Assembly-CSharp.dll";
            DoHookLuaFun(assemblyPath);
        }

        private static void DoHookLuaFun(string assemblyPath)
        {
            AssemblyDefinition assembly;

            assembly = AssemblyDefinition.ReadAssembly(assemblyPath);
            AddResolver(assembly);

            var profilerType = assembly.MainModule.GetType("XLua.LuaDLL.Lua");
            foreach (var m in profilerType.Methods)
            {
                //已经注入了就不注入了
                if (m.Name == "luaL_newstate_profiler")
                {
                    return;
                }
                if (m.Name == "lua_tolstring")
                {
                    m_tolstring = m;
                }
            }

            var luaProfiler = assembly.MainModule.GetType("MikuLuaProfiler.LuaProfiler");
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

            var luaHookType = assembly.MainModule.GetType("MikuLuaProfiler.LuaHook");
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

            var luaRegister = assembly.MainModule.GetType("MikuLuaProfiler.MikuLuaProfilerLuaProfilerWrap");
            foreach (var m in luaRegister.Methods)
            {
                if (m.Name == "__Register")
                {
                    m_registerLua = m;
                    break;
                }
            }

            Dictionary<string, InjectMethodAction> m_hookExternfunDict = new Dictionary<string, InjectMethodAction>
            {
                { "luaL_newstate", InjectNewStateMethod},
                { "lua_close", InjectCloseMethod },
                { "xluaL_loadbuffer", InjectLoaderMethod }
            };

            var methods = new List<MethodDefinition>(profilerType.Methods);
            foreach (var m in methods)
            {
                InjectMethodAction action;
                if (m_hookExternfunDict.TryGetValue(m.Name, out action))
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
                    foreach (var item in m.Body.Variables)
                    {
                        method.Body.Variables.Add(item);
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
                else if (m.Name == "lua_tostring")
                {
                    var method = new MethodDefinition(m.Name + "_profiler", m.Attributes, m.ReturnType);
                    profilerType.Methods.Add(method);
                    CopyMethod(m, method);
                    ModifyTostring(m, assembly.MainModule, method);
                }
            }

            assembly.Write(assemblyPath);
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
            injection = new VariableDefinition(module.TypeSystem.IntPtr);
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

        public static void GetHotfixAssembly()
        {
            var projectPath = System.Reflection.Assembly.Load("Assembly-CSharp").ManifestModule.FullyQualifiedName;
            Debug.Log(projectPath);
        }

        [PostProcessBuild(1000)]
        private static void OnPostprocessBuildPlayer(BuildTarget buildTarget, string buildPath)
        {
            if (buildTarget != BuildTarget.Android && buildTarget != BuildTarget.iOS)
            {
                string buildDir = buildPath.Replace("/" + Path.GetFileName(buildPath), "");
#if UNITY_EDITOR_WIN
                string assPath = buildDir + "/" + Path.GetFileName(buildDir) + "_Data" + "/Managed/Assembly-CSharp.dll";
#else
                assPath = buildPath + "/" + Path.GetFileNameWithoutExtension(buildPath) + "_Data" + "/Managed/Assembly-CSharp.dll";
#endif
                if (LuaDeepProfilerSetting.Instance.isDeepLuaProfiler)
                {
                    DoHookLuaFun(assPath);
                }
                if (LuaDeepProfilerSetting.Instance.isDeepMonoProfiler)
                {
                    InjectAllMethods(assPath);
                }
            }
            else
            {
                //string path = Environment.CurrentDirectory;
                //string[] paths = Directory.GetFiles(path, "Assembly-CSharp.dll", SearchOption.AllDirectories);
                //foreach (var assPath in paths)
                //{
                //    if (LuaDeepProfilerSetting.Instance.isDeepLuaProfiler)
                //    {
                //        DoHookLuaFun(assPath);
                //    }
                //    if (LuaDeepProfilerSetting.Instance.isDeepMonoProfiler)
                //    {
                //        InjectAllMethods(assPath);
                //    }
                //}
            }
        }

        [PostProcessScene]
        private static void OnPostprocessScene()
        {
            var projectPath = System.Reflection.Assembly.Load("Assembly-CSharp").ManifestModule.FullyQualifiedName;
            if (LuaDeepProfilerSetting.Instance.isDeepLuaProfiler)
            {
                DoHookLuaFun(projectPath);
            }
            if (LuaDeepProfilerSetting.Instance.isDeepMonoProfiler)
            {
                InjectAllMethods(projectPath, false);
            }

            //string path = Environment.CurrentDirectory;
            //string[] paths = Directory.GetFiles(path, "Assembly-CSharp.dll", SearchOption.AllDirectories);
            //Debug.Log(path);
            //foreach (var assPath in paths)
            //{

            //}
        }
    }

}