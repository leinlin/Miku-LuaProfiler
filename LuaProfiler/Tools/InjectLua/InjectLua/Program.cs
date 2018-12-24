using System;
using System.IO;

namespace InjectLua
{
    class Program
    {
        static void Main(string[] args)
        {
            string inPath = null;
            string outPath = null;
            if (args.Length < 2)
            {
                inPath = "Template.lua";
                outPath = "Template.bytes";
            }
            else
            {
                inPath = args[0];
                outPath = args[1];
            }

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
