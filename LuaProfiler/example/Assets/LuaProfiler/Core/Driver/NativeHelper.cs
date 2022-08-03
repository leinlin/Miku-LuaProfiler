/*
               #########                       
              ############                     
              #############                    
             ##  ###########                   
            ###  ###### #####                  
            ### #######   ####                 
           ###  ########## ####                
          ####  ########### ####               
         ####   ###########  #####             
        #####   ### ########   #####           
       #####   ###   ########   ######         
      ######   ###  ###########   ######       
     ######   #### ##############  ######      
    #######  #####################  ######     
    #######  ######################  ######    
   #######  ###### #################  ######   
   #######  ###### ###### #########   ######   
   #######    ##  ######   ######     ######   
   #######        ######    #####     #####    
    ######        #####     #####     ####     
     #####        ####      #####     ###      
      #####       ###        ###      #        
        ###       ###        ###               
         ##       ###        ###               
__________#_______####_______####______________
                我们的未来没有BUG                
* ==============================================================================
* Filename: NativeHelper
* Created:  2018/7/13 14:29:22
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/
#if UNITY_EDITOR_WIN || USE_LUA_PROFILER
namespace MikuLuaProfiler
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using UnityEngine;

#if UNITY_5_5_OR_NEWER
    using UnityEngine.Profiling;
#endif

	[StructLayout(LayoutKind.Sequential)]
	public struct SYSTEM_INFO
	{
		public int dwOemId;
		public uint dwPageSize;
		public IntPtr lpMinimumApplicationAddress;
		public IntPtr lpMaximumApplicationAddress;
		public IntPtr dwActiveProcessorMask;
		public int dwNumberOfProcessors;
		public int dwProcessorType;
		public int dwAllocationGranularity;
		public short wProcessorLevel;
		public short wProcessorRevision;
	}

	public static class NativeHelper
    {
		public const uint MEM_COMMIT = 0x00001000;
		public const uint MEM_RESERVE = 0x00002000;
		public const uint MEM_RELEASE = 0x00008000;
		public const uint PAGE_EXECUTE = 0x10;
		public const uint PAGE_EXECUTE_READ = 0x20;
		public const uint PAGE_EXECUTE_READWRITE = 0x40;
		public const uint PAGE_EXECUTE_WRITECOPY = 0x80;

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		public static extern IntPtr GetModuleHandle(string InPath);
		
		[DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
		public static extern IntPtr GetProcAddress(IntPtr InModule, string InProcName);

		public static IntPtr GetProcAddress(string InPath, string InProcName)
		{
			var ptr = GetModuleHandle(InPath);
			if (ptr != IntPtr.Zero)
			{
				return GetProcAddress(ptr, InProcName);
			}

			return IntPtr.Zero;
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern void GetSystemInfo(ref SYSTEM_INFO lpSystemInfo);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr VirtualAlloc(IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

		[DllImport("kernel32.dll")]
		public static extern bool VirtualProtect(IntPtr lpAddress, uint dwSize, uint flNewProtect, out uint lpflOldProtect);

		[DllImport("kernel32.dll")]
		public static extern bool VirtualFree(IntPtr lpAddress, uint dwSize, uint dwFreeType);
		public static unsafe IntPtr LhAllocateMemoryEx(IntPtr InEntryPoint)
		{
			IntPtr Res = IntPtr.Zero;
			ulong Base;
			ulong iStart;
			ulong iEnd;
			ulong Index;

			SYSTEM_INFO SysInfo = default(SYSTEM_INFO);
			uint PAGE_SIZE;

			GetSystemInfo(ref SysInfo);

			PAGE_SIZE = SysInfo.dwPageSize;

			iStart = ((ulong)InEntryPoint) - ((ulong)0x7FFFFF00);
			iEnd = ((ulong)InEntryPoint) + ((ulong)0x7FFFFF00);

			if (iStart < (ulong)SysInfo.lpMinimumApplicationAddress)
				iStart = (ulong)SysInfo.lpMinimumApplicationAddress; // shall not be null, because then VirtualAlloc() will not work as expected

			if (iEnd > (ulong)SysInfo.lpMaximumApplicationAddress)
				iEnd = (ulong)SysInfo.lpMaximumApplicationAddress;

			// we are trying to get memory as near as possible to relocate most RIP-relative instructions
			for (Base = (ulong)InEntryPoint, Index = 0; ; Index += PAGE_SIZE)
			{
				bool end = true;
				if (Base + Index < iEnd)
				{
					if ((Res = VirtualAlloc((IntPtr)(Base + Index), PAGE_SIZE, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE)) != IntPtr.Zero)
						break;
					end = false;
				}

				if (Base - Index > iStart)
				{
					if ((Res = VirtualAlloc((IntPtr)(Base - Index), PAGE_SIZE, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE)) != IntPtr.Zero)
						break;
					end = false;
				}

				if (end)
					break;
			}

			return Res;
		}

		public static unsafe void FixedProxyBuff(IntPtr InEntryPoint, int InEPSize, IntPtr Buffer, out int OutRelocSize)
		{
			OutRelocSize = InEPSize;

			byte b1 = 0;
			byte b2 = 0;
			bool a16 = false;
			long AbsAddr = 0;

			bool is64 = IntPtr.Size == 8;
			byte* pRes = (byte*)Buffer;
			byte* pOld = (byte*)InEntryPoint;
			uint OpcodeLen = 0;
			LDasm.ldasm_data data = new LDasm.ldasm_data();

			while ((long)pOld < ((long)InEntryPoint + InEPSize))
			{
				b1 = *(pOld);
				b2 = *(pOld + 1);

				if (b1 == 0x67)
				{
					a16 = true;
					pOld++;
					continue;
				}

				switch (b1)
				{
					case 0xE9:
					case 0xE8:
					{
						if (a16)
						{
							AbsAddr = *((short*)(pOld + 1));
							OpcodeLen = 3;
						}
						else
						{
							AbsAddr = *((int*)(pOld + 1));
							OpcodeLen = 5;
						}
					} break;
					case 0xEB: // jmp imm8
					{
						AbsAddr = *((byte*)(pOld + 1));
						OpcodeLen = 2;
					} break;
					case 0xE3: // jcxz imm8
						{
							throw new Exception("Hooking near (conditional) jumps is not supported.");
						}
					case 0x0F:
						{
							if ((b2 & 0xF0) == 0x80) // jcc imm16/imm32
								throw new Exception("Hooking far conditional jumps is not supported.");
						} break;
				}

				if ((b1 & 0xF0) == 0x70) // jcc imm8
					throw new Exception("Hooking near conditional jumps is not supported.");

				if (OpcodeLen > 0)
				{
					AbsAddr += (long)(pOld + OpcodeLen);

					if (IntPtr.Size == 8)
					{
						*(pRes++) = 0x48; // REX.W-Prefix
					}

					*(pRes++) = 0xB8;               // mov eax,
					*((long*)pRes) = AbsAddr;   //          address

					pRes += sizeof(void*);

					// points into entry point?
					if ((AbsAddr >= (long)InEntryPoint) && (AbsAddr < (long)InEntryPoint + (long)InEPSize))
						/* is not really unhookable but not worth the effort... */
						throw new Exception("Hooking jumps into the hooked entry point is not supported.");

					/////////////////////////////////////////////////////////
					// insert alternate code
					switch (b1)
					{
						case 0xE8: // call eax
							{
								*(pRes++) = 0xFF;
								*(pRes++) = 0xD0;
							}
							break;
						case 0xE9: // jmp eax
						case 0xEB: // jmp imm8
							{
								*(pRes++) = 0xFF;
								*(pRes++) = 0xE0;
							}
							break;
					}
				}

				uint length = LDasm.ldasm((void*)pOld, data, is64);
                pOld = (byte*)((ulong)pOld + length);
				if (OpcodeLen == 0)
				{
					pRes += length;
				}
			}

			OutRelocSize = (int)((ulong)pRes - (ulong)Buffer);

		}

		#region native
		public static uint GetPass()
		{
#if UNITY_5_5_OR_NEWER
			return (uint)Profiler.GetTotalAllocatedMemoryLong();
#else
            return (uint)Profiler.GetTotalAllocatedMemory();
#endif
		}
		public static float GetBatteryLevel()
        {
            float result = 100;
            return result;
        }
		#endregion

	}
}
#endif
