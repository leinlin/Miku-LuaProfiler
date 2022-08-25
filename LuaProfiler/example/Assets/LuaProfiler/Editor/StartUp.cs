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

#if UNITY_EDITOR_WIN
using Miku.Cecil;
using Miku.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

namespace MikuLuaProfiler
{
    [InitializeOnLoad]
    public static class StartUp
    {
        static StartUp()
        {
            if (LuaDeepProfilerSetting.Instance.isDeepMonoProfiler)
            {
                InjectMethods.InjectAllMethods();
            }
        }
    }

    public static class InjectMethods
    {
        private static MethodDefinition m_beginSampleMethod;
        private static MethodDefinition m_endSampleMethod;
        public delegate void InjectMethodAction(MethodDefinition method, ModuleDefinition module, MethodDefinition newMethod);

        #region try finally
        public static void InjectAllMethods()
        {
            if (EditorApplication.isCompiling) 
            {
                Debug.LogError("is compiling");
                return;
            }

            //var projectPath = System.Reflection.Assembly.Load("Assembly-CSharp").ManifestModule.FullyQualifiedName;
            var ass = typeof(LuaProfiler).Assembly;
            var profilerPath = ass.ManifestModule.FullyQualifiedName;
            var paths = FindReferenceAssembly(ass.GetName().Name);
            if (!paths.Contains(profilerPath))
            {
                paths.Add(profilerPath);
            }
            foreach (var path in paths)
            {
                InjectAllMethods(path, profilerPath);
            }
        }

        public static List<string> FindReferenceAssembly(string name)
        {
            List<string> result = new List<string>();

            var asms = AppDomain.CurrentDomain.GetAssemblies(); 
            foreach (var item in asms)
            {
                var refs = item.GetReferencedAssemblies();
                foreach (var refAss in refs)
                {
                    if (refAss.FullName.Contains(name))
                    {
                        string path = item.ManifestModule.FullyQualifiedName;
                        if (!result.Contains(path) && !path.Contains("Editor"))
                        {
                            result.Add(path);
                        }
                        break;
                    }
                }
            }
            return result;
        }

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

        private static void InjectAllMethods(string injectPath, string profilerPath)
        {
            string md5 = null;
            md5 = new FileInfo(injectPath).LastWriteTimeUtc.Ticks.ToString();
            if (md5 == LuaDeepProfilerSetting.Instance.GetAssMD5ByPath(injectPath)) return;

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
                if (type.FullName.Contains("XLua."))
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

                    if (item.DeclaringType.Namespace == ("XLua") ||
                        item.DeclaringType.Namespace == ("XLua.LuaDLL")
                        )
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

                    InjectTryFinally(item, module);
                }
            }

            WriteAssembly(injectPath, injectAss);
            LuaDeepProfilerSetting.Instance.SetAssMD5ByPath(injectPath, new FileInfo(injectPath).LastWriteTimeUtc.Ticks.ToString());
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
            if (method.Body.Instructions.Count == 0) return;
            if (method.IsConstructor && !method.IsStatic && method.Body.Instructions.Count == 1) return;

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

#if USE_LUA_PROFILER
        [UnityEditor.Callbacks.PostProcessScene]
        private static void OnPostprocessScene()
        {
            if (LuaDeepProfilerSetting.Instance.isDeepMonoProfiler)
            {
                InjectAllMethods(projectPath, profilerPath);
            }
        }
#endif
    }
}
#endif