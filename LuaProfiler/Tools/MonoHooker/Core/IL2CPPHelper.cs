using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public static class IL2CPPHelper
{
    public enum Protection
    {
        PAGE_NOACCESS           = 0x01,
        PAGE_READONLY           = 0x02,
        PAGE_READWRITE          = 0x04,
        PAGE_WRITECOPY          = 0x08,
        PAGE_EXECUTE            = 0x10,
        PAGE_EXECUTE_READ       = 0x20,
        PAGE_EXECUTE_READWRITE  = 0x40,
        PAGE_EXECUTE_WRITECOPY  = 0x80,
        PAGE_GUARD              = 0x100,
        PAGE_NOCACHE            = 0x200,
        PAGE_WRITECOMBINE       = 0x400
    }

#if UNITY_STANDALONE_WIN

    [DllImport("kernel32")]
    public static extern bool VirtualProtect(IntPtr lpAddress, uint dwSize, Protection flNewProtect, out uint lpflOldProtect);
#else
    // mprotect
    public static bool VirtualProtect(IntPtr lpAddress, uint dwSize, Protection flNewProtect, out uint lpflOldProtect)
    {
        lpflOldProtect = 0;
        return false;
    }
#endif
}
