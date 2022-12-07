
namespace MikuLuaProfiler
{
    [PacketMsg(MsgHead.DisConnect)]
    public class PKGDisconnect : PacketBase<PKGDisconnect>
    {
        public override MsgHead MsgHead => MsgHead.DisConnect;
    }
}
