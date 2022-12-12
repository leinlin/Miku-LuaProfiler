#if UNITY_EDITOR_WIN || USE_LUA_PROFILER
using System;
using System.IO;

namespace MikuLuaProfiler
{
    [PacketMsg(MsgHead.LuaRefInfo)]
    public class LuaRefInfo: PacketBase<LuaRefInfo>
    {
        public override MsgHead MsgHead => MsgHead.LuaRefInfo;

        #region field
        public byte cmd; //1添加、0移除
        public int frameCount;
        public string name;
        public string addr;
        public byte type; //1 function 2 table
        #endregion
        
        #region pool
        public static LuaRefInfo Create()
        {
            LuaRefInfo r = PacketFactory<LuaRefInfo>.GetPacket();
            return r;
        }

        public static LuaRefInfo Create(byte cmd, string name, string addr, byte type)
        {
            LuaRefInfo r = PacketFactory<LuaRefInfo>.GetPacket();
            r.cmd = cmd;
            r.name = name;
            r.addr = addr;
            r.type = type;
            return r;
        }

        public void Restore()
        {
            PacketFactory<LuaRefInfo>.Release(this);
        }
        
        public LuaRefInfo Clone()
        {
            LuaRefInfo result = new LuaRefInfo();

            result.cmd = this.cmd;
            result.frameCount = this.frameCount;
            result.name = this.name;
            result.addr = this.addr;
            result.type = this.type;

            return result;
        }
        #endregion

        #region static

        private static Action<LuaRefInfo> OnReciveSample;

        public static void RegAction(Action<LuaRefInfo> action)
        {
            OnReciveSample = action;
        }

        public static void UnRegAction()
        {
            OnReciveSample = null;
        }
        #endregion

        public override void Read(BinaryReader br)
        {
           cmd = br.ReadByte();
           frameCount = br.ReadInt32();
           name = br.ReadString();
           addr = br.ReadString();
           type = br.ReadByte();
        }

        public override void Write(BinaryWriter bw)
        {
            bw.Write(cmd);
            bw.Write(SampleData.frameCount);
            bw.Write(name);
            bw.Write(addr);
            bw.Write(type);
        }

        public override void OnRun()
        {
            OnReciveSample?.Invoke(this);
        }

    }
}

#endif