
namespace MikuLuaProfiler
{
    [PacketMsg(MsgHead.HeartBeat)]
    public class HeartBeatMsg : PacketBase<HeartBeatMsg>
    {
        public override MsgHead MsgHead => MsgHead.HeartBeat;
    }
}
