using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MikuLuaProfiler
{
    public class NativeUtility
    {
        public static unsafe IntPtr ConvertByteArrayToPtr(byte[] buff)
        {
            IntPtr intPtr;
            fixed (byte* b = buff)
            {
                intPtr = (IntPtr)b;
            }
            return intPtr;
        }
    }
}
