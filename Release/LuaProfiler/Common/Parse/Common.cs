#if UNITY_EDITOR
namespace UniLua
{
	public static class LuaConf
	{
		public const int LUAI_BITSINT			= 32;

#pragma warning disable 0429
		public const int LUAI_MAXSTACK = (LUAI_BITSINT >= 32)
			? 1000000
			: 15000
			;
#pragma warning restore 0429

		// reserve some space for error handling
		public const int LUAI_FIRSTPSEUDOIDX	= (-LUAI_MAXSTACK-1000);

		public const string LUA_SIGNATURE = "\u001bLua";
		public static string LUA_DIRSEP {
			get { return System.IO.Path.DirectorySeparatorChar.ToString(); }
		}
	}

	public static class LuaLimits
	{
		public const int MAX_INT 	= System.Int32.MaxValue - 2;
		public const int MAXUPVAL 	= System.Byte.MaxValue;
		public const int LUAI_MAXCCALLS = 200;
		public const int MAXSTACK	= 250;
	}

	public static class LuaDef
	{
		public const int LUA_MINSTACK 			= 20;
		public const int BASIC_STACK_SIZE		= LUA_MINSTACK * 2;
		public const int EXTRA_STACK			= 5;

		public const int LUA_RIDX_MAINTHREAD 	= 1;
		public const int LUA_RIDX_GLOBALS 		= 2;
		public const int LUA_RIDX_LAST 			= LUA_RIDX_GLOBALS;

		public const int LUA_MULTRET			= -1;

		public const int LUA_REGISTRYINDEX		= LuaConf.LUAI_FIRSTPSEUDOIDX;

		// number of list items accumulate before a SETLIST instruction
		public const int LFIELDS_PER_FLUSH 		= 50;

		public const int LUA_IDSIZE				= 60;

		public const string LUA_VERSION_MAJOR	= "5";
		public const string LUA_VERSION_MINOR	= "2";
		public const string LUA_VERSION = "Lua " + LUA_VERSION_MAJOR + "." + LUA_VERSION_MINOR;

		public const string LUA_ENV = "_ENV";

		public const int BASE_CI_SIZE = 8;
	}

	public static class LuaConstants
	{
		public const int LUA_NOREF = -2;
		public const int LUA_REFNIL = -1;
	}

	public enum LuaType
	{
		LUA_TNONE = -1,
		LUA_TNIL = 0,
		LUA_TBOOLEAN = 1,
		LUA_TLIGHTUSERDATA = 2,
		LUA_TNUMBER = 3,
		LUA_TSTRING = 4,
		LUA_TTABLE = 5,
		LUA_TFUNCTION = 6,
		LUA_TUSERDATA = 7,
		LUA_TTHREAD = 8,

		LUA_TUINT64 = 9,

		LUA_NUMTAGS = 10,

		LUA_TPROTO,
		LUA_TUPVAL,
		LUA_TDEADKEY,
	}

	public enum ClosureType
	{
		LUA,
		CSHARP,
	}

	public enum ThreadStatus
	{
		LUA_RESUME_ERROR = -1,
		LUA_OK			 = 0,
		LUA_YIELD		 = 1,
		LUA_ERRRUN		 = 2,
		LUA_ERRSYNTAX	 = 3,
		LUA_ERRMEM		 = 4,
		LUA_ERRGCMM		 = 5,
		LUA_ERRERR		 = 6,

		LUA_ERRFILE		 = 7,
	}

	/* ORDER TM */
	internal enum LuaOp
	{
		LUA_OPADD	= 0,
		LUA_OPSUB	= 1,
		LUA_OPMUL	= 2,
		LUA_OPDIV	= 3,
		LUA_OPMOD	= 4,
		LUA_OPPOW	= 5,
		LUA_OPUNM	= 6,
	}

	public enum LuaEq
	{
		LUA_OPEQ	= 0,
		LUA_OPLT	= 1,
		LUA_OPLE	= 2,
	}

}
#endif
