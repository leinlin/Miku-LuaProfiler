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
    public enum LuaTypes
    {
        LUA_TNONE = -1,
        LUA_TNIL = 0,
        LUA_TNUMBER = 3,
        LUA_TSTRING = 4,
        LUA_TBOOLEAN = 1,
        LUA_TTABLE = 5,
        LUA_TFUNCTION = 6,
        LUA_TUSERDATA = 7,
        LUA_TTHREAD = 8,
        LUA_TLIGHTUSERDATA = 2
    }
    public abstract class NetBase
    {
        public abstract void Restore();
    }

    public class LuaRefInfo : NetBase
    {
        #region field
        public byte cmd; //1添加、0移除
        public int frameCount;
        public string name;
        public string addr;
        public byte type; //1 function 2 table
        #endregion

        #region pool
        private static ObjectPool<LuaRefInfo> m_pool = new ObjectPool<LuaRefInfo>(32);
        public static LuaRefInfo Create()
        {
            LuaRefInfo r = m_pool.GetObject();
            return r;
        }

        public override void Restore()
        {
            m_pool.Store(this);
        }

        public LuaRefInfo Clone()
        {
            LuaRefInfo result = new LuaRefInfo();

            result.cmd = this.cmd;
            result.frameCount = this.frameCount;
            result.name = this.name;
            result.addr = this.addr;
            result.type = this.type;

            return result;
        }
        #endregion
    }

    public class LuaDiffInfo : NetBase
    {
        #region field
        public Dictionary<string, LuaTypes> addRef = new Dictionary<string, LuaTypes>(); //1添加、0移除
        public Dictionary<string, List<string>> addDetail = new Dictionary<string, List<string>>();
        public Dictionary<string, LuaTypes> rmRef = new Dictionary<string, LuaTypes>();
        public Dictionary<string, List<string>> rmDetail = new Dictionary<string, List<string>>();
        public List<string> nullRef = new List<string>();
        #endregion

        #region api
        public void PushAddRef(string addKey, int addType)
        {
            addRef.Add(addKey, (LuaTypes)addType);
        }

        public void PushAddDetail(string addKey, string value)
        {
            List<string> list;
            if (!addDetail.TryGetValue(addKey, out list))
            {
                list = new List<string>();
                addDetail[addKey] = list;
            }
            list.Add(value);
        }

        public void PushRmRef(string addKey, int addType)
        {
            rmRef.Add(addKey, (LuaTypes)addType);
        }

        public void PushRmDetail(string key, string value)
        {
            List<string> list;
            if (!rmDetail.TryGetValue(key, out list))
            {
                list = new List<string>();
                rmDetail[key] = list;
            }
            list.Add(value);
        }

        public void PushNullRef(string value)
        {
            nullRef.Add(value);
        }
        #endregion

        #region pool
        private static ObjectPool<LuaDiffInfo> m_pool = new ObjectPool<LuaDiffInfo>(32);
        public static LuaDiffInfo Create()
        {
            LuaDiffInfo r = m_pool.GetObject();
            r.addRef.Clear();
            r.addDetail.Clear();
            r.rmRef.Clear();
            r.rmDetail.Clear();
            r.nullRef.Clear();
            return r;
        }
        public override void Restore()
        {
            m_pool.Store(this);
        }
        #endregion
    }

    public class Sample : NetBase
    {
        public int currentLuaMemory;
        public int currentMonoMemory;
        public long currentTime;
        public float fps;
        public int pss;
        public float power;

        public int calls;
        public int frameCount;

        public int costLuaGC;
        public int costMonoGC;
        public string name;
        public int costTime;
        public Sample _father;
        public List<Sample> childs = new List<Sample>(16);
        public string captureUrl = null;
        private string _fullName;

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
                        childDict[name] = string.Intern(_fullName);
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
                    value.childs.Add(this);
                }
                _father = value;
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
        private static ObjectPool<Sample> samplePool = new ObjectPool<Sample>(256);
        public static Sample Create()
        {
            Sample s = samplePool.GetObject();

            return s;
        }

        public override void Restore()
        {
            for (int i = 0, imax = childs.Count; i < imax; i++)
            {
                childs[i].Restore();
            }
            _fullName = null;
            _father = null;
            captureUrl = null;
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
            s.fps = fps;
            s.pss = pss;
            s.power = power;
            s.costMonoGC = costMonoGC;
            s.costLuaGC = costLuaGC;
            s.name = name;
            s.costTime = costTime;

            int childCount = childs.Count;
            for (int i = 0; i < childCount; i++)
            {
                Sample child = childs[i].Clone();
                child.fahter = s;
            }

            s.currentLuaMemory = currentLuaMemory;
            s.currentMonoMemory = currentMonoMemory;
            s.currentTime = currentTime;
            s._fullName = _fullName;
            s.captureUrl = captureUrl;
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
            b.Write(fps);
            b.Write(pss);
            b.Write(power);
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
            s.fps = b.ReadSingle();
            s.pss = b.ReadInt32();
            s.costLuaGC = b.ReadInt32();
            s.costMonoGC = b.ReadInt32();

            int len = b.ReadInt32();
            byte[] datas = b.ReadBytes(len);
            s.name = Encoding.UTF8.GetString(datas);

            s.costTime = b.ReadInt32();

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
            s.currentLuaMemory = b.ReadInt32();
            s.currentMonoMemory = b.ReadInt32();

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

