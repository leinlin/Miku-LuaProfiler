using System;
using System.IO;

namespace InjectLua
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                string[] paths = Directory.GetFiles(System.Environment.CurrentDirectory, "*.lua", SearchOption.AllDirectories);
                foreach (var path in paths)
                {
                    string the_code = File.ReadAllText(path);
                    the_code = MikuLuaProfiler.Parse.InsertSample(the_code, Path.GetFileName(path));
                    File.WriteAllText(path, the_code);
                }
            }
            else
            {
                string inPath = null;
                string outPath = null;
                inPath = args[0];
                outPath = args[1];
                if (!File.Exists(inPath))
                {
                    Console.WriteLine(inPath + " not exit");
                    return;
                }
                string code = File.ReadAllText(inPath);
                code = MikuLuaProfiler.Parse.InsertSample(code, Path.GetFileName(inPath));

                File.WriteAllText(outPath, code);
            }

        }
    }
}
