#if UNITY_EDITOR || USE_LUA_PROFILER
namespace MikuLuaProfiler
{
    [PacketMsg(MsgHead.DisConnect)]
    public class PKGDisconnect : PacketBase<PKGDisconnect>
    {
        public override MsgHead MsgHead
        {
            get
            {
                return MsgHead.DisConnect;
                
            }
        }
    }
}
#endif
