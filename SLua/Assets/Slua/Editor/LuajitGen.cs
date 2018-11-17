// The MIT License (MIT)

// Copyright 2015 Siney/Pangweiwei siney@yeah.net / chenjian  2743182@qq.com
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;
using System;

namespace SLua
{
    public class LuajitGen
    {
        //---------------------------------------------
        // process 
        public static Process StartProcess(string command, string param, string workDir = "")
        {
            return StartProcess(command, param, workDir, DataReceived, ErrorReceived);
        }

        public static Process StartProcess(
            string command,
            string param,
            string workDir,
            DataReceivedEventHandler dataReceived,
            DataReceivedEventHandler errorReceived
            )
        {

            Process ps = new Process
            {
                StartInfo =
            {
                FileName = command,
                Arguments = param,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = workDir,

    }
            };
            ps.OutputDataReceived += dataReceived;
            ps.ErrorDataReceived += errorReceived;
            ps.Start();
            ps.BeginOutputReadLine();
            ps.BeginErrorReadLine();

            return ps;
        }

        private static void DataReceived(object sender, DataReceivedEventArgs eventArgs)
        {
            if (eventArgs.Data != null) UnityEngine.Debug.Log(eventArgs.Data);
        }

        private static void ErrorReceived(object sender, DataReceivedEventArgs eventArgs)
        {
            if (eventArgs.Data != null) UnityEngine.Debug.LogError(eventArgs.Data);
        }



        public static void compileLuaJit(string[] src, string[] dst, JITBUILDTYPE buildType)
        {
            UnityEngine.Debug.Log("compileLuajit");
            string workDir = Application.dataPath + "/../jit/";
#if !UNITY_EDITOR_OSX
            Dictionary<JITBUILDTYPE, string> build = new Dictionary<JITBUILDTYPE, string>
            {
                { JITBUILDTYPE.X86, Application.dataPath + "/../jit/win/x86/luajit.exe" },
                { JITBUILDTYPE.X64, Application.dataPath + "/../jit/win/x64/luajit.exe" },
                { JITBUILDTYPE.GC64, Application.dataPath + "/../jit/win/gc64/luajit.exe" },
            };

#else
            Dictionary<JITBUILDTYPE, string> build = new Dictionary<JITBUILDTYPE, string>
            {
                { JITBUILDTYPE.X86, Application.dataPath + "/../jit/mac/x86/luajit" },
                { JITBUILDTYPE.X64, Application.dataPath + "/../jit/mac/x64/luajit" },
                { JITBUILDTYPE.GC64, Application.dataPath + "/../jit/mac/gc64/luajit" },
            };
#endif
            string exePath = build[buildType];
            Process[] psList = new Process[src.Length];

            for (int i = 0; i < src.Length; i++)
            {
                string srcLua = Application.dataPath + "/../" + src[i];
                string dstLua = Application.dataPath + "/../" + dst[i];
                string cmd = " -b " + srcLua + " " + dstLua;

                ProcessStartInfo StartInfo = new ProcessStartInfo();
                StartInfo.FileName = exePath;
                StartInfo.Arguments = cmd;
                StartInfo.CreateNoWindow = true;
                StartInfo.UseShellExecute = false;
                StartInfo.WorkingDirectory = workDir;
                Process ps = new Process();
                ps.StartInfo = StartInfo;
                psList[i] = ps;
            }

            try
            {
                int totalSize = src.Length;
                int workThreadCount = Environment.ProcessorCount * 2 + 2;
                int batchCount = (int)Math.Ceiling(totalSize / (float)workThreadCount);
                for (int batchIndex = 0; batchIndex < batchCount; ++batchIndex)
                {
                    int processIndex;
                    int offset = batchIndex * workThreadCount;
                    for (processIndex = 0; processIndex < workThreadCount; ++processIndex)
                    {
                        int fileIndex = offset + processIndex;
                        if (fileIndex >= totalSize)
                            break;
                        var ps = psList[fileIndex];
                        ps.Start();
                    }

                    bool fail = false;
                    string fileName = null;
                    string arguments = null;
                    for (int i = offset; i < offset + processIndex; ++i)
                    {
                        var ps = psList[i];
                        ps.WaitForExit();
                        if (ps.ExitCode != 0 && !fail)
                        {
                            fail = true;
                            fileName = ps.StartInfo.FileName;
                            arguments = ps.StartInfo.Arguments;
                        }
                        ps.Dispose();
                    }

                    if (fail)
                    {
                        throw new Exception(string.Format("Luajit Compile Fail.FileName={0},Arg={1}", fileName, arguments));
                    }
                }
            }
            finally
            {
                foreach (var ps in psList)
                {
                    ps.Dispose();
                }
            }
        }

        public static void exportLuajit(string res, string ext, string jitluadir, JITBUILDTYPE buildType)
        {
            // delete
            AssetDatabase.DeleteAsset(jitluadir);

            var files = Directory.GetFiles(res, ext, SearchOption.AllDirectories);
            var dests = new string[files.Length];

            for (int i = 0; i < files.Length; i++)
            {
                string xfile = files[i].Remove(0, res.Length);
                xfile = xfile.Replace("\\", "/");
                string file = files[i].Replace("\\", "/");

                string dest = jitluadir + "/" + xfile;
                string destName = dest.Substring(0, dest.Length - 3) + "bytes";

                string destDir = Path.GetDirectoryName(destName);

                if (!Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }

                files[i] = file;
                dests[i] = destName;
                // Debug.Log(file + ":" + destName);
            }


            compileLuaJit(files, dests, buildType);
            AssetDatabase.Refresh();
        }

        [MenuItem("SLua/Compile Bytecode/luajitx86 for WIN32&Android ARMV7")]
        static void exportLuajitx86()
        {
            exportLuajit("Assets/Slua/Resources/", "*.txt", "Assets/Slua/jit/jitx86", JITBUILDTYPE.X86);
        }

        [MenuItem("SLua/Compile Bytecode/luajitx64 for WIN64")]
        static void exportLuajitx64()
        {
            exportLuajit("Assets/Slua/Resources/", "*.txt", "Assets/Slua/jit/jitx64", JITBUILDTYPE.X64);
        }

        [MenuItem("SLua/Compile Bytecode/luajitgc64 for MAC&ARM64")]
        static void exportLuajitgc64()
        {
            exportLuajit("Assets/Slua/Resources/", "*.txt", "Assets/Slua/jit/jitgc64", JITBUILDTYPE.GC64);
        }

    }
}
