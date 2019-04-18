/*
 Desc: 一个可以运行时 Hook Mono 方法的工具，让你可以无需修改 UnityEditor.dll 等文件就可以重写其函数功能
 Author: Misaka Mikoto
 Github: https://github.com/easy66/MonoHooker
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MonoHooker
{
    // 在library 的指针附近
    public unsafe class NativeLibraryPool
    {
        private static Dictionary<string, NativeLibraryPool> s_dict = new Dictionary<string, NativeLibraryPool>();
        public static NativeLibraryPool GetLibraryPool(string libraryName)
        {
            NativeLibraryPool result = null;
            if (!s_dict.TryGetValue(libraryName, out result))
            {
                result = new NativeLibraryPool(libraryName);
            }
            return result;
        }

        private NativeLibraryPool(string libraryName)
        {
            IntPtr targetPtr = NativeAPI.mono_hooker_get_library_addr(libraryName);
            UnityEngine.Debug.Log("open dll:" + targetPtr);
            // B 跳转范围为 -0x7FFFFF ~ 0x7FFFFF
            byte* allocAddr = (byte*)targetPtr.ToPointer() - 0x7FFFFF;
            byte* endAddr = (byte*)targetPtr.ToPointer() + 0x7FFFFF;
            while (_proxyPoolHead == null && allocAddr < endAddr)
            {
                _proxyPoolHead = (byte*)NativeAPI.mono_hooker_jit_alloc((IntPtr)allocAddr, POOL_BUFF_SIZE);
                allocAddr += POOL_BUFF_SIZE;
            }
            Debug.Assert(_proxyPoolHead != null);
            UnityEngine.Debug.Log("_proxyPoolHead:" + (IntPtr)_proxyPoolHead);
            for (int i = 0; i < POOL_LENGTH; i++)
            {
                _indexPool.Push(i);
            }
        }

        private void StoreIndex(int index)
        {
            _indexPool.Push(index);
        }

        private int GetIndex()
        {
            Debug.Assert(_indexPool.Count > 0);
            return _indexPool.Pop();
        }

        public byte* Alloc()
        {
            int index = GetIndex();
            return _proxyPoolHead + index * BUFF_SIZE;
        }

        public void Free(byte* ptr)
        {
            int index = (int)((long)_proxyPoolHead - (long)ptr) / BUFF_SIZE;
            StoreIndex(index);
        }

        ~NativeLibraryPool()
        {
            NativeAPI.mono_hooker_free(_proxyPoolHead, POOL_BUFF_SIZE);
        }

        // 申请1024段HOOK proxy，每个proxy 占用20字节
        private const int POOL_LENGTH = 256;
        private const int POOL_BUFF_SIZE = POOL_LENGTH * BUFF_SIZE;
        private const int BUFF_SIZE = 20;
        private byte* _proxyPoolHead = null;
        private Stack<int> _indexPool = new Stack<int>(POOL_LENGTH);
    }

}
