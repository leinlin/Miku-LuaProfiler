using System.Collections.Generic;
using System.IO;
using System.Text;

#if UNITY_EDITOR_WIN || USE_LUA_PROFILER
namespace MikuLuaProfiler
{
    public class MBinaryReader : BinaryReader
    {
        public MBinaryReader(Stream input) : base(input)
        {
        }

        public MBinaryReader(Stream input, Encoding encoding) : base(input, encoding)
        {
        }

        public MBinaryReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
        {
        }
        
        private Dictionary<int, string> m_strCacheDict = new Dictionary<int, string>(4096);
        public override string ReadString()
        {
            string result = null;

            bool isRef = ReadBoolean();
            int index = ReadInt32();
            if (!isRef)
            {
                int len = ReadInt32();
                byte[] datas = ReadBytes(len);
                result = string.Intern(Encoding.UTF8.GetString(datas));
                m_strCacheDict[index] = result;
            }
            else
            {
                result = m_strCacheDict[index];
            }
            return result;
        }
    }
}
#endif