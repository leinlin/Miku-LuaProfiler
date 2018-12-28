#if UNITY_EDITOR || USE_LUA_PROFILER
namespace MikuLuaProfiler
{
    public static class LuaConf
    {
        public const int LUAI_BITSINT = 32;

#pragma warning disable 0429
        public const int LUAI_MAXSTACK = (LUAI_BITSINT >= 32)
            ? 1000000
            : 15000
            ;
#pragma warning restore 0429

        // reserve some space for error handling
        public const int LUAI_FIRSTPSEUDOIDX = (-LUAI_MAXSTACK - 1000);

        public const string LUA_SIGNATURE = "\u001bLua";
        public static string LUA_DIRSEP
        {
            get { return System.IO.Path.DirectorySeparatorChar.ToString(); }
        }
    }

}
#endif
