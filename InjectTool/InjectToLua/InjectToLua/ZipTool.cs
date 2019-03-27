using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;

namespace InjectToLua
{
    class ZipTool
    {
        public static void UnZipOneDir(string zipedFile, string destPath, string zipDir)
        {

            if (!destPath.EndsWith("/"))
            {
                destPath = destPath + "/";
            }

            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }

            using (ZipInputStream s = new ZipInputStream(File.OpenRead(zipedFile)))
            {
                ZipEntry theEntry;
                while ((theEntry = s.GetNextEntry()) != null)
                {
                    if (theEntry.Name.Contains(zipDir) && theEntry.Name.EndsWith(".dll") && theEntry.IsFile)
                    {
                        Console.WriteLine("unzip:" + theEntry.Name);
                        UnzipOneFile(theEntry, s, destPath);
                    }
                }
                s.Close();
            }
        }

        private static void UnzipOneFile(ZipEntry theEntry, ZipInputStream s, string strDirectory)
        {
            string directoryName = "";
            string pathToZip = "";
            pathToZip = theEntry.Name;
            if (pathToZip != "")
                directoryName = Path.GetDirectoryName(pathToZip) + "/";

            string fileName = Path.GetFileName(pathToZip);

            Directory.CreateDirectory(strDirectory + directoryName);
            float rate = (float)theEntry.CompressedSize / (float)theEntry.Size;
            if (fileName != "")
            {
                if ((File.Exists(strDirectory + directoryName + fileName))
                    || (!File.Exists(strDirectory + directoryName + fileName)))
                {
                    using (FileStream streamWriter = File.Create(strDirectory + directoryName + fileName))
                    {
                        int size = 2048;
                        byte[] data = new byte[size];
                        while (true)
                        {
                            size = s.Read(data, 0, data.Length);

                            if (size > 0)
                            {
                                streamWriter.Write(data, 0, size);
                            }
                            else
                            {
                                break;
                            }
                        }
                        streamWriter.Close();
                    }
                }
            }

        }

    }
}
