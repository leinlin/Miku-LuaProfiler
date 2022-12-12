#if UNITY_EDITOR_WIN || USE_LUA_PROFILER
namespace MikuLuaProfiler
{
    [PacketMsg(MsgHead.DisConnect)]
    public class PKGDisconnect : PacketBase<PKGDisconnect>
    {
        public override MsgHead MsgHead => MsgHead.DisConnect;
    }
}
#endif
