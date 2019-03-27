using Mono.Cecil;
using System;
using System.IO;
using MikuLuaProfiler_Editor;
using System.Reflection;

namespace InjectToLua
{
    class Program
    {
        public static string filePath = "";
        static void Main(string[] args)
        {
            //用于加载引用的dll资源
            AppDomain.CurrentDomain.AssemblyResolve += (sender, arg) =>
            {
                String resourceName = "InjectToLua.Lib." + new AssemblyName(arg.Name).Name + ".dll";
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    Byte[] assemblyData = new Byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);
                    return Assembly.Load(assemblyData);
                }
            };

            if (args.Length == 0 || args[0] == "--help")
            {
                Console.Write(@"
us -l to inject Lua
us -m to inject Mono
us --unzip to unzip dll file
last param is path");
                return;
            }


            if (args[0] == "--unzip")
            {
                string apkPath = args[1];
                ZipTool.UnZipOneDir(apkPath, ".", "assets/bin/Data/Managed/");

            }
            else if (args[0] == "--copy")
            {
                if (Directory.Exists("lua"))
                {
                    DeleteFolder("lua");
                }
                Directory.CreateDirectory("lua");

                for (int i = 1, imax = args.Length; i < imax; i++)
                {
                    string[] files = Directory.GetFiles(args[i], "*.lua", SearchOption.AllDirectories);
                    foreach (var item in files)
                    {
                        Console.WriteLine(item);
                        File.Copy(item, "lua/" + Path.GetFileName(item) + ".bytes", true);
                    }
                }
                Console.WriteLine("complete");
            }
            else
            {
                bool isInjectLua = true;
                bool isInjectMono = false;
                foreach (var item in args)
                {
                    if (item == "-l")
                    {
                        isInjectLua = true;
                    }
                    else if (item == "-m")
                    {
                        isInjectMono = true;
                    }
                }
                string path = args[args.Length - 1];
                Console.WriteLine(path);
                filePath = path;
                if (File.Exists(path))
                {
                    LoadAssembly(path, isInjectLua, isInjectMono);
                }
            }

        }

        static public void LoadAssembly(string path, bool isInjectLua, bool isInjectMono)
        {
            try
            {
                File.Copy(path, path + ".bak", true);
                AssemblyDefinition assemblyDefinition = AssemblyDefinition.ReadAssembly(path);
                (assemblyDefinition.MainModule.AssemblyResolver as DefaultAssemblyResolver).AddSearchDirectory(Path.GetDirectoryName(path));
                AssemblyDefinition assemblyDefinition2 = AssemblyDefinition.ReadAssembly("LuaProfilerDLL.dll");
                assemblyDefinition.MainModule.AssemblyReferences.Add(assemblyDefinition2.Name);
                assemblyDefinition.Write(path);
                if (isInjectLua)
                {
                    InjectMethods.HookLuaFun(path, "LuaProfilerDLL.dll");
                }
                if (isInjectMono)
                {
                    InjectMethods.InjectAllMethods(path, "LuaProfilerDLL.dll");
                }

                string profilerDLLPath = Path.Combine(Path.GetDirectoryName(path), "LuaProfilerDLL.dll");
                Console.WriteLine(profilerDLLPath);
                File.Copy("LuaProfilerDLL.dll", profilerDLLPath, true);
                Console.WriteLine("Inject Success");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        /// <summary>
        /// 删除文件夹（及文件夹下所有子文件夹和文件）
        /// </summary>
        /// <param name="directoryPath"></param>
        public static void DeleteFolder(string directoryPath)
        {
            foreach (string d in Directory.GetFileSystemEntries(directoryPath))
            {
                if (File.Exists(d))
                {
                    FileInfo fi = new FileInfo(d);
                    if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                        fi.Attributes = FileAttributes.Normal;
                    File.Delete(d);     //删除文件   
                }
                else
                    DeleteFolder(d);    //删除文件夹
            }
            Directory.Delete(directoryPath);    //删除空文件夹
        }
    }
}
