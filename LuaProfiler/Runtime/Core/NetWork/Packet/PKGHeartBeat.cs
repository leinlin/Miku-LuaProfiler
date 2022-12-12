#if UNITY_EDITOR_WIN || USE_LUA_PROFILER
namespace MikuLuaProfiler
{
    [PacketMsg(MsgHead.HeartBeat)]
    public class HeartBeatMsg : PacketBase<HeartBeatMsg>
    {
        public override MsgHead MsgHead => MsgHead.HeartBeat;
    }
}
#endif
