/*
* ==============================================================================
* Filename: ByteBuf
* Created:  2018/7/13 14:29:22
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/

namespace MikuLuaProfiler
{
    using System;
    using System.Text;

    public class ByteBuf
    {
        //字节缓存区
        private byte[] m_buf;
        //读取索引
        private int m_readIndex = 0;
        //写入索引
        private int m_writeIndex = 0;
        //缓存区字节数组的长度
        private int m_capacity;

        /**
         * 构造方法
         */
        private ByteBuf(int capacity)
        {
            m_buf = new byte[capacity];
            this.m_capacity = capacity;
        }

        /**
         * 构造方法
         */
        private ByteBuf(byte[] bytes)
        {
            m_buf = bytes;
            this.m_capacity = bytes.Length;
        }
        /**
         * 构建一个capacity长度的字节缓存区ByteBuf对象
         */
        public static ByteBuf Allocate(int capacity)
        {
            return new ByteBuf(capacity);
        }

        /**
         * 构建一个以bytes为字节缓存区的ByteBuf对象，一般不推荐使用
         */
        public static ByteBuf Allocate(byte[] bytes)
        {
            return new ByteBuf(bytes);
        }
        /**
         * 根据length长度，确定大于此leng的最近的2次方数，如length=7，则返回值为8
         */
        private int FixLength(int length)
        {
            int n = 2;
            int b = 2;
            while (b < length)
            {
                b = 2 << n;
                n++;
            }
            return b;
        }

        /**
         * 确定内部字节缓存数组的大小
         */
        private int FixSizeAndReset(int currLen, int futureLen)
        {
            if (futureLen > currLen)
            {
                //以原大小的2次方数的两倍确定内部字节缓存区大小
                int size = FixLength(currLen) * 2;
                if (futureLen > size)
                {
                    //以将来的大小的2次方的两倍确定内部字节缓存区大小
                    size = FixLength(futureLen) * 2;
                }
                byte[] newbuf = new byte[size];
                Array.Copy(m_buf, 0, newbuf, 0, currLen);
                m_buf = newbuf;
                m_capacity = newbuf.Length;
            }
            return futureLen;
        }

        /**
         * 写入一个int16数据
         */
        public unsafe void Write(short value)
        {
            lock (this)
            {
                int total = 2 + m_writeIndex;
                int len = m_buf.Length;
                FixSizeAndReset(len, total);

                fixed (byte* ptr = m_buf)
                {
                    *(short*)(ptr + m_writeIndex) = value;
                }
                m_writeIndex = total;
            }
        }

        /**
         * 写入一个uint16数据
         */
        public unsafe void Write(ushort value)
        {
            lock (this)
            {
                int total = 2 + m_writeIndex;
                int len = m_buf.Length;
                FixSizeAndReset(len, total);

                fixed (byte* ptr = m_buf)
                {
                    *(ushort*)(ptr + m_writeIndex) = value;
                }
                m_writeIndex = total;
            }
        }

        /**
         * 写入一个int32数据
         */
        public unsafe void Write(int value)
        {
            lock (this)
            {
                int total = 4 + m_writeIndex;
                int len = m_buf.Length;
                FixSizeAndReset(len, total);

                fixed (byte* ptr = m_buf)
                {
                    *(int*)(ptr + m_writeIndex) = value;
                }
                m_writeIndex = total;
            }
        }

        /**
         * 写入一个uint32数据
         */
        public unsafe void Write(uint value)
        {
            lock (this)
            {
                int total = 4 + m_writeIndex;
                int len = m_buf.Length;
                FixSizeAndReset(len, total);

                fixed (byte* ptr = m_buf)
                {
                    *(uint*)(ptr + m_writeIndex) = value;
                }
                m_writeIndex = total;
            }
        }

        /**
         * 写入一个int64数据
         */
        public unsafe void Write(long value)
        {
            lock (this)
            {
                int total = 8 + m_writeIndex;
                int len = m_buf.Length;
                FixSizeAndReset(len, total);

                fixed (byte* ptr = m_buf)
                {
                    *(long*)(ptr + m_writeIndex) = value;
                }
                m_writeIndex = total;
            }
        }

        /**
         * 写入一个uint64数据
         */
        public unsafe void Write(ulong value)
        {
            lock (this)
            {
                int total = 8 + m_writeIndex;
                int len = m_buf.Length;
                FixSizeAndReset(len, total);

                fixed (byte* ptr = m_buf)
                {
                    *(ulong*)(ptr + m_writeIndex) = value;
                }
                m_writeIndex = total;
            }
        }

        /**
         * 写入一个float数据
         */
        public unsafe void Write(float value)
        {
            lock (this)
            {
                int total = 4 + m_writeIndex;
                int len = m_buf.Length;
                FixSizeAndReset(len, total);

                fixed (byte* ptr = m_buf)
                {
                    *(float*)(ptr + m_writeIndex) = value;
                }
                m_writeIndex = total;
            }
        }

        /**
         * 写入一个byte数据
         */
        public unsafe void Write(byte value)
        {
            lock (this)
            {
                int total = 1 + m_writeIndex;
                int len = m_buf.Length;
                FixSizeAndReset(len, total);

                fixed (byte* ptr = m_buf)
                {
                    *(ptr + m_writeIndex) = value;
                }
                m_writeIndex = total;
            }
        }

        /**
         * 写入一个double类型数据
         */
        public unsafe void Write(double value)
        {
            lock (this)
            {
                int total = 8 + m_writeIndex;
                int len = m_buf.Length;
                FixSizeAndReset(len, total);

                fixed (byte* ptr = m_buf)
                {
                    *(double*)(ptr + m_writeIndex) = value;
                }
                m_writeIndex = total;
            }
        }

        public void Write(byte[] bytes)
        {
            lock (this)
            {
                int total = bytes.Length + m_writeIndex;
                int len = m_buf.Length;
                FixSizeAndReset(len, total);

                Array.Copy(bytes, 0, m_buf, m_writeIndex, bytes.Length);

                m_writeIndex = total;
            }
        }

        public unsafe void Write(string v)
        {

            if (v == null)
            {
                v = string.Empty;
            }

            lock (this)
            {
                int length = v.Length * 2;
                Write(length);

                int total = length + m_writeIndex;
                int len = m_buf.Length;
                FixSizeAndReset(len, total);

                fixed (char* cptr = v)
                {
                    fixed (byte* ptr = m_buf)
                    {
                        StringCopy((byte*)cptr, ptr + m_writeIndex, len);
                    }
                }
                m_writeIndex = total;
                //byte[] bytes = Encoding.UTF8.GetBytes(v);
                //Write(bytes.Length);
                //Write(bytes);
            }


        }

        static unsafe void StringCopy(byte* src, byte* dest, int len)
        {
            if (len >= 0x10)
            {
                do
                {
                    *((int*)dest) = *((int*)src);
                    *((int*)(dest + 4)) = *((int*)(src + 4));
                    *((int*)(dest + 8)) = *((int*)(src + 8));
                    *((int*)(dest + 12)) = *((int*)(src + 12));
                    dest += 0x10;
                    src += 0x10;
                }
                while ((len -= 0x10) >= 0x10);
            }
            if (len > 0)
            {
                if ((len & 8) != 0)
                {
                    *((int*)dest) = *((int*)src);
                    *((int*)(dest + 4)) = *((int*)(src + 4));
                    dest += 8;
                    src += 8;
                }
                if ((len & 4) != 0)
                {
                    *((int*)dest) = *((int*)src);
                    dest += 4;
                    src += 4;
                }
                if ((len & 2) != 0)
                {
                    *((short*)dest) = *((short*)src);
                    dest += 2;
                    src += 2;
                }
                if ((len & 1) != 0)
                {
                    *dest = *src;
                }
            }
        }

        /**
         * 读取一个字节
         */
        public byte ReadByte()
        {
            byte b = m_buf[m_readIndex];
            m_readIndex++;
            return b;
        }

        /**
         * 从读取索引位置开始读取len长度的字节数组
         */
        private byte[] Read(int len)
        {
            byte[] bytes = new byte[len];
            Array.Copy(m_buf, m_readIndex, bytes, 0, len);
            m_readIndex += len;
            return bytes;
        }

        public byte[] ReadBytes(int len)
        {
            return Read(len);
        }

        /**
         * 读取一个uint16数据
         */
        public ushort ReadUshort()
        {
            ushort ret = BitConverter.ToUInt16(m_buf, m_readIndex);
            m_readIndex += 2;
            return ret;
        }
        /**
         * 是否读取到了byte数组的最后一位
         */
        public bool ReadEnd()
        {
            return m_readIndex >= m_capacity;
        }

        /**
         * 读取一个int16数据
         */
        public unsafe short ReadShort()
        {
            short ret = 0;
            lock (this)
            {
                fixed (byte* ptr = m_buf)
                {
                    ret = *((short*)(ptr + m_readIndex));
                }
            }
            m_readIndex += 2;
            return ret;
        }

        /**
         * 读取一个uint32数据
         */
        public unsafe uint ReadUint()
        {
            uint ret = 0;
            lock (this)
            {
                fixed (byte* ptr = m_buf)
                {
                    ret = *((uint*)(ptr + m_readIndex));
                }
            }
            m_readIndex += 4;
            return ret;
        }

        /**
         * 读取一个int32数据
         */
        public unsafe int ReadInt()
        {
            int ret = 0;
            lock (this)
            {
                fixed (byte* ptr = m_buf)
                {
                    ret = *((int*)(ptr + m_readIndex));
                }
            }
            m_readIndex += 4;
            return ret;
        }

        /**
         * 读取一个uint64数据
         */
        public unsafe ulong ReadUlong()
        {
            ulong ret = 0;
            lock (this)
            {
                fixed (byte* ptr = m_buf)
                {
                    ret = *((ulong*)(ptr + m_readIndex));
                }
            }
            m_readIndex += 8;
            return ret;
        }
        /**
         * 读取一个long数据
         */
        public unsafe long ReadLong()
        {
            long ret = 0;
            lock (this)
            {
                fixed (byte* ptr = m_buf)
                {
                    ret = *((long*)(ptr + m_readIndex));
                }
            }
            m_readIndex += 8;
            return ret;
        }

        /**
         * 读取一个float数据
         */
        public unsafe float ReadFloat()
        {
            float ret = 0;
            lock (this)
            {
                fixed (byte* ptr = m_buf)
                {
                    ret = *((float*)(ptr + m_readIndex));
                }
            }
            m_readIndex += 4;
            return ret;
        }

        /**
         * 读取一个double数据
         */
        public unsafe double ReadDouble()
        {
            double ret = 0;
            lock (this)
            {
                fixed (byte* ptr = m_buf)
                {
                    ret = *((double*)(ptr + m_readIndex));
                }
            }
            m_readIndex += 8;
            return ret;
        }

        /**
         * 读取 String 
         */
        public unsafe string ReadString()
        {
            int len = ReadInt();
            byte[] c = ReadBytes(len);
            string ret = new string(' ', len / 2);

            fixed (char* cptr = ret)
            {
                fixed (byte* ptr = c)
                {
                    StringCopy(ptr, (byte*)cptr, len);
                }
            }

            //fixed (byte* ptr = c)
            //{
            //    ret = new string((sbyte*)ptr);
            //}
            return ret;
        }

        /**
         * 清空此对象
         */
        public void Clear()
        {
            m_readIndex = 0;
            m_writeIndex = 0;
        }

        public byte[] GetOrginArray(out int length)
        {
            length = m_writeIndex;
            return m_buf;
        }
        /**
         * 获取可读的字节数组
         */
        public byte[] ToArray()
        {
            byte[] bytes = new byte[m_writeIndex];
            Array.Copy(m_buf, 0, bytes, 0, bytes.Length);
            return bytes;
        }
        public byte[] ToAllArray()
        {
            byte[] bytes = new byte[m_buf.Length];
            Array.Copy(m_buf, bytes, m_buf.Length);
            return bytes;
        }
        /**
         * 获取缓存区大小
         */
        public int GetCapacity()
        {
            return this.m_capacity;
        }

        public bool HasUnreadData()
        {
            return m_readIndex < m_buf.Length;
        }
    }
}