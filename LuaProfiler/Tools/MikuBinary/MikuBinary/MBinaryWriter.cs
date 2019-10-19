using System;
using System.IO;

namespace MikuLuaProfiler
{
    // Token: 0x02000002 RID: 2
    public class MBinaryWriter : BinaryWriter
    {
        // Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
        public MBinaryWriter(Stream output) : base(output)
        {
            this._buffer = new byte[8];
        }

        // Token: 0x06000002 RID: 2 RVA: 0x00002068 File Offset: 0x00000268
        public unsafe override void Write(float value)
        {
            fixed (byte* ptr = _buffer)
            {
                *(float*)ptr = value;
            }
            this.OutStream.Write(this._buffer, 0, 4);
        }

        // Token: 0x06000003 RID: 3 RVA: 0x000020AC File Offset: 0x000002AC
        public unsafe override void Write(short value)
        {
            fixed (byte* ptr = _buffer)
            {
                *(short*)ptr = value;
            }
            this.OutStream.Write(this._buffer, 0, 2);
        }

        // Token: 0x06000004 RID: 4 RVA: 0x000020F0 File Offset: 0x000002F0
        public unsafe override void Write(ushort value)
        {
            fixed (byte* ptr = _buffer)
            {
                *(ushort*)ptr = value;
            }
            this.OutStream.Write(this._buffer, 0, 2);
        }

        // Token: 0x06000005 RID: 5 RVA: 0x00002134 File Offset: 0x00000334
        public unsafe override void Write(int value)
        {
            fixed (byte* ptr = _buffer)
            {
                *(int*)ptr = value;
            }
            this.OutStream.Write(this._buffer, 0, 4);
        }

        // Token: 0x06000006 RID: 6 RVA: 0x00002178 File Offset: 0x00000378
        public unsafe override void Write(uint value)
        {
            fixed (byte* ptr = _buffer)
            {
                *(uint*)ptr = value;
            }
            this.OutStream.Write(this._buffer, 0, 4);
        }

        // Token: 0x04000001 RID: 1
        private byte[] _buffer;
    }
}
