using System;
using System.IO;

namespace MikuLuaProfiler
{
    public class MBinaryWriter : BinaryWriter
    {
        public MBinaryWriter(Stream output) : base(output)
        {
            this._buffer = new byte[8];
        }

        public unsafe override void Write(float value)
        {
            fixed (byte* ptr = _buffer)
            {
                *(float*)ptr = value;
            }
            this.OutStream.Write(this._buffer, 0, 4);
        }

        public unsafe override void Write(short value)
        {
            fixed (byte* ptr = _buffer)
            {
                *(short*)ptr = value;
            }
            this.OutStream.Write(this._buffer, 0, 2);
        }

        public unsafe override void Write(ushort value)
        {
            fixed (byte* ptr = _buffer)
            {
                *(ushort*)ptr = value;
            }
            this.OutStream.Write(this._buffer, 0, 2);
        }

        public unsafe override void Write(int value)
        {
            fixed (byte* ptr = _buffer)
            {
                *(int*)ptr = value;
            }
            this.OutStream.Write(this._buffer, 0, 4);
        }

        public unsafe override void Write(uint value)
        {
            fixed (byte* ptr = _buffer)
            {
                *(uint*)ptr = value;
            }
            this.OutStream.Write(this._buffer, 0, 4);
        }

        private byte[] _buffer;
    }
}
