#if UNITY_EDITOR_WIN || USE_LUA_PROFILER
using System;
using System.Collections.Generic;
using System.IO;

namespace MikuLuaProfiler
{

    public enum MsgHead
    {
        HeartBeat = 0,
        DisConnect,
        ProfileSampleData,
        LuaRefInfo,
    }

    public class PacketMsgAttribute : Attribute
    {
        public MsgHead msgHead;

        public PacketMsgAttribute(MsgHead msgHead)
        {
            this.msgHead = msgHead;
        }
    }

    public static class PacketFactory<T>  where T: new()
    {
        private static Stack<T> pool = new Stack<T>(1024);
        public static T GetPacket()
        {
            if (pool.Count > 0)
            {
                lock (pool)
                {
                    return pool.Pop();
                }
            }
            return new T();
        }

        public static void Release(T p)
        {
            lock (pool)
            {
                pool.Push(p);
            }
        }
    }

    public abstract class PacketBase
    {
        public abstract MsgHead MsgHead { get; }
        public object Context { get; set; }
        
        public PacketBase()
        {
        }

        public virtual void Read(BinaryReader br)
        {
        }

        public virtual void Write(BinaryWriter bw)
        {
        }

        public virtual void OnRun()
        {
        }

        public virtual void OnRelease()
        {
        }

        public abstract void WriteOver();
    }
    
    public abstract class PacketBase<T> : PacketBase where  T : PacketBase<T>, new()
    {
        public override void WriteOver()
        {
            T obj = (T)this;
            obj.OnRelease();
            PacketFactory<T>.Release(obj);
        }
    }
}
#endif