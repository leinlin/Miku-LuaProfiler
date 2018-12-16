/*
* ==============================================================================
* Filename: LuaExport
* Created:  2018/7/13 14:29:22
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace MikuLuaProfiler
{
    public class Sample
    {
        public long currentLuaMemory;
        public long currentMonoMemory;
        private string _fullName = null;
        public long currentTime;

        public int calls;
        public int frameCount;
        public long costLuaGC;
        public long costMonoGC;
        public string name;
        public long costTime;
        public Sample _father;
        public List<Sample> childs = new List<Sample>(256);
        public string captureUrl = null;

        public long selfLuaGC
        {
            get
            {
                long result = costLuaGC;

                foreach (var item in childs)
                {
                    result -= item.costLuaGC;
                }

                return result;
            }
        }

        public long selfMonoGC
        {
            get
            {
                long result = costMonoGC;

                foreach (var item in childs)
                {
                    result -= item.costMonoGC;
                }

                return result;
            }
        }
        #region property
        public string fullName
        {
            get
            {
                if (_father == null) return name;

                if (_fullName == null)
                {
                    Dictionary<string, string> childDict;
                    if (!m_fullNamePool.TryGetValue(_father.fullName, out childDict))
                    {
                        childDict = new Dictionary<string, string>();
                        m_fullNamePool.Add(_father.fullName, childDict);
                    }

                    if (!childDict.TryGetValue(name, out _fullName))
                    {
                        string value = name;
                        var f = _father;
                        while (f != null)
                        {
                            value = f.name + value;
                            f = f.fahter;
                        }
                        _fullName = value;
                        childDict[name] = _fullName;
                    }

                    return _fullName;
                }
                else
                {
                    return _fullName;
                }
            }
        }
        public Sample fahter
        {
            set
            {
                if (value != null)
                {
                    bool needAdd = true;
                    foreach (var item in value.childs)
                    {
                        if (item.name == name)
                        {
                            needAdd = false;
                            item.AddSample(this);
                            break;
                        }
                    }
                    if (needAdd)
                    {
                        value.childs.Add(this);
                        _father = value;
                    }
                }
                else
                {
                    _father = null;
                }
            }
            get
            {
                return _father;
            }
        }
        #endregion

        #region pool
        private static string capturePath = "";
        private static Dictionary<string, Dictionary<string, string>> m_fullNamePool = new Dictionary<string, Dictionary<string, string>>();
        private static ObjectPool<Sample> samplePool = new ObjectPool<Sample>(250);
        public static Sample Create()
        {
            return samplePool.GetObject();
        }

        public void Restore()
        {
            for (int i = 0, imax = childs.Count; i < imax; i++)
            {
                childs[i].Restore();
            }
            childs.Clear();
            samplePool.Store(this);
        }
        #endregion

        #region method
        public void AddSample(Sample s)
        {
            calls += s.calls;
            costLuaGC += s.costLuaGC;
            costMonoGC += s.costMonoGC;
            costTime += s.costTime;
            for (int i =s.childs.Count -1;i>=0;i--)
            {
                var item = s.childs[i];
                item.fahter = this;
                if (item.fahter != s)
                {
                    s.childs.RemoveAt(i);
                }
            }
        }
        public static string Capture()
        {
            if (string.IsNullOrEmpty(capturePath)) capturePath = "capture" + DateTime.Now.Ticks.ToString();

            Directory.CreateDirectory(capturePath);

            string result = capturePath + "/" + UnityEngine.Time.frameCount.ToString() + ".png";
#if UNITY_2017_1_OR_NEWER
            ScreenCapture.CaptureScreenshot(result, 0);
#else
            Application.CaptureScreenshot(result, 0);
#endif
            return result;
        }

        public Sample Clone()
        {
            Sample s = new Sample();

            s.calls = calls;
            s.frameCount = frameCount;
            s.costLuaGC = costLuaGC;
            s.name = name;
            s.costTime = costTime;

            int childCount = childs.Count;
            for (int i = 0; i < childCount; i++)
            {
                Sample child = childs[i].Clone();
                child.fahter = s;
            }
            s.captureUrl = captureUrl;
            s.currentLuaMemory = currentLuaMemory;

            return s;
        }
#endregion

        #region 序列化
        public static void SerializeList(List<Sample> samples, string path)
        {
            FileStream fs = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
            BinaryWriter b = new BinaryWriter(fs);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.ClearProgressBar();
#endif
            b.Write(samples.Count);
            for (int i = 0, imax = samples.Count; i < imax; i++)
            {
                Sample s = samples[i];
#if UNITY_EDITOR
                UnityEditor.EditorUtility.DisplayProgressBar("serialize profiler data", "serialize " + s.name, (float)i / (float)imax);
#endif
                byte[] datas = s.Serialize();
                b.Write(datas.Length);
                b.Write(datas);
            }
            b.Close();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.ClearProgressBar();
#endif
        }

        public static List<Sample> DeserializeList(string path)
        {
            FileStream ms = new FileStream(path, FileMode.Open, FileAccess.Read);
            BinaryReader b = new BinaryReader(ms);

            int count = b.ReadInt32();
            List<Sample> result = new List<Sample>(count);

            for (int i = 0, imax = count; i < imax; i++)
            {
                int len = b.ReadInt32();
                Sample s = Deserialize(b.ReadBytes(len));
                result.Add(s);
            }
            b.Close();

            return result;
        }

        public byte[] Serialize()
        {
            byte[] result = null;
            MemoryStream ms = new MemoryStream();
            BinaryWriter b = new BinaryWriter(ms);

            b.Write(calls);
            b.Write(frameCount);
            b.Write(costLuaGC);
            b.Write(costMonoGC);

            byte[] datas = Encoding.UTF8.GetBytes(name);
            b.Write(datas.Length);
            b.Write(datas);

            b.Write(costTime);

            b.Write(childs.Count);
            for (int i = 0, imax = childs.Count; i < imax; i++)
            {
                datas = childs[i].Serialize();
                b.Write(datas.Length);
                b.Write(datas);
            }

            if (string.IsNullOrEmpty(captureUrl) || !File.Exists(captureUrl))
            {
                b.Write(false);
            }
            else
            {
                b.Write(true);
                datas = Encoding.UTF8.GetBytes(captureUrl);
                b.Write(datas.Length);
                b.Write(datas);

                //写入图片数据
                datas = File.ReadAllBytes(captureUrl);
                b.Write(datas.Length);
                b.Write(datas);
            }
            b.Write(currentLuaMemory);
            b.Write(currentMonoMemory);

            result = ms.ToArray();
            b.Close();

            return result;
        }

        public static Sample Deserialize(byte[] data)
        {
            Sample s = new Sample();
            MemoryStream ms = new MemoryStream(data);
            BinaryReader b = new BinaryReader(ms);

            s.calls = b.ReadInt32();
            s.frameCount = b.ReadInt32();
            s.costLuaGC = b.ReadInt64();
            s.costMonoGC = b.ReadInt64();

            int len = b.ReadInt32();
            byte[] datas = b.ReadBytes(len);
            s.name = Encoding.UTF8.GetString(datas);

            s.costTime = b.ReadInt64();

            int childCount = b.ReadInt32();
            for (int i = 0; i < childCount; i++)
            {
                len = b.ReadInt32();
                datas = b.ReadBytes(len);
                Sample child = Deserialize(datas);
                child.fahter = s;
            }

            bool hasCapture = b.ReadBoolean();
            if (hasCapture)
            {
                len = b.ReadInt32();
                datas = b.ReadBytes(len);
                s.captureUrl = Encoding.UTF8.GetString(datas);

                if (!File.Exists(s.captureUrl))
                {
                    string dir = Path.GetDirectoryName(s.captureUrl);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    len = b.ReadInt32();
                    datas = b.ReadBytes(len);
                    //写入图片数据
                    File.WriteAllBytes(s.captureUrl, datas);
                }

            }
            s.currentLuaMemory = b.ReadInt64();
            s.currentMonoMemory = b.ReadInt64();

            b.Close();

            return s;
        }

        public static void DeleteFiles(string str)
        {
            DirectoryInfo fatherFolder = new DirectoryInfo(str);
            //删除当前文件夹内文件
            FileInfo[] files = fatherFolder.GetFiles();
            foreach (FileInfo file in files)
            {
                string fileName = file.Name;
                try
                {
                    File.Delete(file.FullName);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                }
            }
            //递归删除子文件夹内文件
            foreach (DirectoryInfo childFolder in fatherFolder.GetDirectories())
            {
                DeleteFiles(childFolder.FullName);
            }
            Directory.Delete(str, true);
        }
        #endregion

    }

}
#endif

