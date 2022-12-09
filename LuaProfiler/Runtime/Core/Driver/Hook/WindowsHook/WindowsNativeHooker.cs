#if UNITY_EDITOR_WIN || (USE_LUA_PROFILER && UNITY_STANDALONE_WIN)
using System;
using System.Runtime.InteropServices;

namespace MikuLuaProfiler
{
	public class WindowsNativeUtil : NativeUtilInterface
	{
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		private static extern IntPtr GetModuleHandle(string InPath);
		
		[DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
		private static extern IntPtr GetProcAddress(IntPtr InModule, string InProcName);

		public IntPtr GetProcAddress(string InPath, string InProcName)
		{
			var ptr = GetModuleHandle(InPath);
			if (ptr != IntPtr.Zero)
			{
				return GetProcAddress(ptr, InProcName);
			}
			return IntPtr.Zero;
		}

		public IntPtr GetProcAddressByHandle(IntPtr InModule, string InProcName)
		{
			return GetProcAddress(InModule, InProcName);
		}

		public INativeHooker CreateHook()
		{
			return new WindowsNativeHooker();
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate IntPtr LoadLibraryExW_t(IntPtr lpFileName, IntPtr hFile, int dwFlags);
		public static LoadLibraryExW_t LoadLibraryExW_dll;

		private static WindowsNativeHooker hooker = null;
		private static LoadLibraryExW_t fun;
		public void HookLoadLibrary(Action<IntPtr> callBack)
		{
			IntPtr handle = GetProcAddress("KernelBase.dll", "LoadLibraryExW");
			if (handle == IntPtr.Zero)
				handle = GetProcAddress("kernel32.dll", "LoadLibraryExW");
			if (handle != IntPtr.Zero)
			{
				// LoadLibraryExW is called by the other LoadLibrary functions, so we
				// only need to hook it.
				fun = (IntPtr lpFileName, IntPtr hFile, int dwFlags) =>
				{
					var ret = LoadLibraryExW_dll(lpFileName, hFile, dwFlags);
					if (GetProcAddressByHandle(ret, "luaL_newstate") != IntPtr.Zero)
					{
						callBack(ret);
						hooker.Uninstall();
					}
					return ret;
				};
				hooker = new WindowsNativeHooker();
				hooker.Init(handle, Marshal.GetFunctionPointerForDelegate(fun));
				hooker.Install();
				
				LoadLibraryExW_dll = (LoadLibraryExW_t)hooker.GetProxyFun(typeof(LoadLibraryExW_t));
			}
		}

	}

	public unsafe class WindowsNativeHooker : INativeHooker
	{
		public  bool isHooked { get; set; }

		private IntPtr _targetPtr;          // 目标方法被 jit 后的地址指针
		private IntPtr _replacementPtr;
		private IntPtr _allocProxyBuff;
		private IntPtr _proxyBuff;
		private byte[] _backupBuff;

		static byte[] Jumper = { 0xE9, 0x00, 0x00, 0x00, 0x00};

		private static readonly byte[] s_jmpBuff;
		private static readonly byte[] s_jmpBuff_32 = new byte[] // 6 bytes
		{
			0x68, 0x00, 0x00, 0x00, 0x00,                       // push $val
			0xC3                                                // ret
		};
		private static readonly byte[] s_jmpBuff_64 = new byte[] // 14 bytes
		{
			0xFF, 0x25, 0x00, 0x00, 0x00, 0x00,                 // jmp [rip]
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,     // $val
		};
		private static readonly int s_addrOffset = 1;

		public void Init(IntPtr targetPtr, IntPtr replacementPtr)
		{
			_targetPtr = targetPtr;
			_replacementPtr = replacementPtr;
		}

		static WindowsNativeHooker()
		{
			if (IntPtr.Size == 4)
			{
				s_jmpBuff = s_jmpBuff_32;
				s_addrOffset = 1;
			}
			else
			{
				s_jmpBuff = s_jmpBuff_64;
				s_addrOffset = 6;
			}

		}

		~WindowsNativeHooker()
		{
			Uninstall();
		}
		public Delegate GetProxyFun(Type t)
		{
			return Marshal.GetDelegateForFunctionPointer(_proxyBuff, t);
		}
		public void Install()
		{
			if (LDasm.IsiOS()) // iOS 不支持修改 code 所在区域 page
				return;

			if (isHooked)
				return;

			int requireSize = InitProxyBuff();
			BackupHeader(requireSize);
			PatchTargetMethod();

			isHooked = true;
		}
		public void Uninstall()
		{
			if (!isHooked)
				return;

			byte* pTarget = (byte*)_targetPtr.ToPointer();
			for (int i = 0; i < _backupBuff.Length; i++)
				*pTarget++ = _backupBuff[i];

			NativeHelper.VirtualFree(_allocProxyBuff, 0, NativeHelper.MEM_RELEASE);
			isHooked = false;
		}

		#region 

		// 将原始方法跳转到我们的方法
		private void PatchTargetMethod()
		{
			long addrOffset = _allocProxyBuff.ToInt64() - _targetPtr.ToInt64() - 5;
			if (addrOffset != (int)addrOffset)
			{
				throw new Exception("no match address"); 
			}

			fixed (byte* p = &Jumper[1])
			{
				*((int*)p) = (int)addrOffset;
			}

			byte* pTarget = (byte*)_targetPtr.ToPointer();

			if (pTarget != null)
			{
				for (int i = 0, imax = Jumper.Length; i < imax; i++)
					pTarget[i] = Jumper[i];
			}

		}

		/// <summary>
		/// 备份原始方法头
		/// </summary>
		private void BackupHeader(int requireSize)
		{
			_proxyBuff = NativeHelper.LhAllocateMemoryEx(_targetPtr);
			_allocProxyBuff = _proxyBuff;
			_backupBuff = new byte[requireSize];
			byte* pTarget = (byte*)_proxyBuff.ToPointer();

			fixed (byte* p = &s_jmpBuff[s_addrOffset])
			{
				*((IntPtr*)p) = (IntPtr)_replacementPtr;
			}

			for (int i = 0, imax = s_jmpBuff.Length; i < imax; i++)
				*pTarget++ = s_jmpBuff[i];

			_proxyBuff = (IntPtr)pTarget;

			pTarget = (byte*)_targetPtr.ToPointer();
			byte* pProxy = (byte*)_proxyBuff.ToPointer();

			for (int i = 0; i < requireSize; i++)
			{
				byte data = *pTarget++;
				*pProxy++ = data;
				_backupBuff[i] = data;
			}

			int jumpBufferSize = 0;
			NativeHelper.FixedProxyBuff(_targetPtr, requireSize, _proxyBuff, out jumpBufferSize);
			long addrOffset = (_targetPtr.ToInt64() + requireSize) - (_proxyBuff.ToInt64() + jumpBufferSize + 5);
			if (addrOffset != (int)addrOffset)
			{
				throw new Exception("no match address");
			}

			fixed (byte* p = &Jumper[1])
			{
				*((int*)p) = (int)addrOffset;
			}

			pTarget = (byte*)_proxyBuff.ToPointer() + jumpBufferSize;
			if (pTarget != null)
			{
				for (int i = 0, imax = Jumper.Length; i < imax; i++)
					*pTarget++ = Jumper[i];
			}
		}
		private int InitProxyBuff()
		{
			int requireSize = (int)LDasm.SizeofMinNumByte(_targetPtr, Jumper.Length);


			EnableAddrModifiable(_targetPtr, (uint)requireSize);

			return requireSize;
		}

		private void EnableAddrModifiable(IntPtr ptr, uint size)
		{
			uint oldProtect;
			NativeHelper.VirtualProtect(ptr, size * 4, NativeHelper.PAGE_EXECUTE_READWRITE, out oldProtect);
		}

		#endregion

	}
}

#endif