using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MikuLuaProfiler
{
	public unsafe class NativeHooker
	{
		public bool isHooked { get; private set; }

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
		/// <summary>
		/// 创建一个 Hook
		/// </summary>
		/// <param name="targetMethod">需要替换的目标方法</param>
		/// <param name="replacementMethod">准备好的替换方法</param>
		/// <param name="proxyMethod">如果还需要调用原始目标方法，可以通过此参数的方法调用，如果不需要可以填 null</param>
		public NativeHooker(IntPtr targetPtr, IntPtr replacementPtr)
		{
			_targetPtr = targetPtr;
			_replacementPtr = replacementPtr;
		}

		static NativeHooker()
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
		~NativeHooker()
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
			long addrOffset = (_targetPtr.ToInt64() + requireSize) - (_proxyBuff.ToInt64() + jumpBufferSize + requireSize + 5);
			if (addrOffset != (int)addrOffset)
			{
				throw new Exception("no match address");
			}

			fixed (byte* p = &Jumper[1])
			{
				*((int*)p) = (int)addrOffset;
			}

			pTarget = (byte*)_proxyBuff.ToPointer() + jumpBufferSize + requireSize;
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
