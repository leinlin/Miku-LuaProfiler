/*
 Desc: 一个可以运行时 Hook Mono 方法的工具，让你可以无需修改 UnityEditor.dll 等文件就可以重写其函数功能
 Author: Misaka Mikoto
 Github: https://github.com/easy66/MonoHooker
 */
using System;
using System.Collections.Generic;

namespace MonoHooker
{
    /// <summary>
    /// Hooker 池，防止重复 Hook
    /// </summary>
    public static class HookerPool
    {
        private static Dictionary<IntPtr, HookerBase> _hookers = new Dictionary<IntPtr, HookerBase>();

        public static void AddHooker(IntPtr targetAddr, HookerBase hooker)
        {
            HookerBase preHooker;
            if (_hookers.TryGetValue(targetAddr, out preHooker))
            {
                preHooker.Uninstall();
                _hookers[targetAddr] = hooker;
            }
            else
            {
                _hookers.Add(targetAddr, hooker);
            }
        }

        public static void RemoveHooker(IntPtr targetAddr)
        {
            _hookers.Remove(targetAddr);
        }
    }
}
