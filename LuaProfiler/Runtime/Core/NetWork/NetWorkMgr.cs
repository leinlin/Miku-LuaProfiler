#if UNITY_EDITOR_WIN || USE_LUA_PROFILER

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Debug = UnityEngine.Debug;

namespace MikuLuaProfiler
{
    public static partial class NetWorkMgr
    {
        private static Thread receiveThread;
        private static Thread sendThread;
        private const int PACK_HEAD = 0x114514;
        private const float HEART_BEAT_DELTA = float.MaxValue; // 永不超时 (原来是10分钟)
        public static float lastBeatTime = 0;
        public static float curTime = 0;
        public static bool isClose = false;
        private static Queue<PacketBase> m_cmdQueue = new Queue<PacketBase>(32);
        private static Queue<PacketBase> m_msgQueue = new Queue<PacketBase>(32);
        private static Queue<Action> m_eventQueue = new Queue<Action>();
        private static object m_CloseLock = 1;

        private static Dictionary<int, Type> receiveMsgDict = new Dictionary<int, Type>();

        static NetWorkMgr()
        {
            var types = typeof(NetWorkMgr).Assembly.GetTypes();
            foreach (var t in types)
            {
                if (t.IsSubclassOf(typeof(PacketBase)) && !t.IsAbstract)
                {
                    var attr = (PacketMsgAttribute)Attribute.GetCustomAttribute(t, typeof(PacketMsgAttribute));
                    if (attr != null)
                    {
                        receiveMsgDict[(int)attr.msgHead] = t;
                        // PaperU3dProfiler.Log(t + "=" + attr.msgHead);
                    }
                }
            }
        }

        public static void Update()
        {
            if (m_eventQueue.Count > 0)
            {
                lock (m_eventQueue)
                {
                    while (m_eventQueue.Count > 0)
                    {
                        var msg = m_eventQueue.Dequeue();
                        msg();
                    }
                }
            }

            if (m_msgQueue.Count > 0)
            {
                lock (m_msgQueue)
                {
                    while (m_msgQueue.Count > 0)
                    {
                        var msg = m_msgQueue.Dequeue();
                        msg.OnRun();
                    }
                }
            }
        }

        #region public
        
        static void OnRecvDisconnect(PKGDisconnect pkg)
        {
            NetWorkMgr.Disconnect();
        }

        private static void _Close()
        {
            isClose = true;
            if (tcpClient != null)
            {
                if (OnDisconnect != null)
                {
                    var clientCtx = tcpClient;
                    tcpClient = null;
                    Action actionDisconnect = () => OnDisconnect(clientCtx);
                    lock(m_eventQueue)
                        m_eventQueue.Enqueue(actionDisconnect);
                }
            }
        }

        public static void Disconnect()
        {
            isClose = true;
        }

        public static bool CheckIsConnected()
        {
            return tcpClient != null;
        }

        public static void SendCmd(PacketBase cmd)
        {
            lock (m_cmdQueue)
            {
                m_cmdQueue.Enqueue(cmd);
            }
        }
        #endregion

        #region private
        
        // 接受请求
        private static void DoReceiveMessage(NetworkStream ns)
        {
            UnityEngine.Debug.Log("<color=#00ff00>begin to receive</color>");

            MBinaryReader br = new MBinaryReader(ns);
            ;
            //sign为true 循环接受数据
            while (true)
            {
                try
                {
                    if (isClose || tcpClient == null)
                    {
                        _Close();
                        break;
                    }

                    while (ns.CanRead && ns.DataAvailable)
                    {
                        int head = br.ReadInt32();
                        //处理粘包
                        if (head == PACK_HEAD)
                        {
                            int messageId = br.ReadInt32();
                            
                            Type t;
                            if (receiveMsgDict.TryGetValue(messageId, out t))
                            {
                                var msg = (PacketBase) Activator.CreateInstance(t);
                                msg.Context = tcpClient;
                                msg.Read(br);

                                if (messageId != (int) MsgHead.HeartBeat)
                                {
                                    lock (m_msgQueue)
                                    {
                                        m_msgQueue.Enqueue(msg);
                                    }
                                }
                            }
                        }
                        lastBeatTime = curTime;
                    }
                    
                    // 处理超时的时候 要断开链接
                    if (curTime - lastBeatTime > HEART_BEAT_DELTA)
                    {
                        _Close();
                        break;
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log(e);
                    _Close();
                    break;
                }
                Thread.Sleep(10);
            }
        }
        private static void DoSendMessage(NetworkStream ns)
        {
            MBinaryWriter bw = new MBinaryWriter(ns);

            while (true)
            {
                try
                {
                    if (isClose)
                    {
                        break;
                    }

                    if (ns.CanWrite)
                    {
                        lock (m_cmdQueue)
                        {
                            while (m_cmdQueue.Count > 0)
                            {
                                var msg = m_cmdQueue.Dequeue();
                                bw.Write(PACK_HEAD);
                                bw.Write((int)msg.MsgHead);
                                msg.Write(bw);
                                msg.WriteOver();
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    if (tcpClient.Connected)
                    {
                        UnityEngine.Debug.Log(e);
                    }
                    _Close();
                    break;
                }
                Thread.Sleep(10);
            }
        }
        #endregion
        
        #region dispatch
        
        private static Dictionary<Type, Delegate> _msgHandlers = new Dictionary<Type, Delegate>();

        public delegate void MsgHandlerDelegate<TPacket>(TPacket pkg) where TPacket : PacketBase;

        public static void AddListener<TPacket>(MsgHandlerDelegate<TPacket> handler)
            where TPacket : PacketBase
        {
            // Debug.Log("AddListener: " + typeof(TPacket));
            Delegate oldHandler;
            var msgType = typeof(TPacket);
            if (_msgHandlers.TryGetValue(msgType, out oldHandler))
            {
                _msgHandlers[msgType] = Delegate.Combine(oldHandler, handler);
            }
            else
            {
                _msgHandlers[msgType] = handler;
            }
        }
        
        public static void RemoveListener<TPacket>(MsgHandlerDelegate<TPacket> handler)
            where TPacket : PacketBase
        {
            // Debug.Log("RemoveListener: " + typeof(TPacket));
            Delegate @delegate;
            var msgType = typeof(TPacket);
            if (_msgHandlers.TryGetValue(msgType, out @delegate))
            {
                Delegate currentDel = Delegate.Remove(@delegate, handler);

                if (currentDel == null)
                {
                    _msgHandlers.Remove(msgType);
                }
                else
                {
                    _msgHandlers[msgType] = currentDel;
                }
            }
        }

        public static void Dispatch(PacketBase pkg)
        {
            if (pkg == null) throw new ArgumentNullException("pkg");
            if (_msgHandlers == null) throw new ObjectDisposedException("Cannot dispatch and event when disposed! ");

            Delegate handler;
            var msgType = pkg.GetType();
            if (_msgHandlers.TryGetValue(msgType, out handler))
            {
                handler.DynamicInvoke(pkg);
            }
            else
            {
                Debug.LogError("Invalid handler for msg: " + pkg.ToString());
            }
        }
        
        internal static void RemoveAllListeners()
        {
            // Debug.Log("RemoveAllListeners: ");
            var handlerTypes = new Type[_msgHandlers.Keys.Count];
            _msgHandlers.Keys.CopyTo(handlerTypes, 0);

            foreach (var handlerType in handlerTypes)
            {
                Delegate[] delegates = _msgHandlers[handlerType].GetInvocationList();
                foreach (Delegate @delegate1 in delegates)
                {
                    var handlerToRemove = Delegate.Remove(_msgHandlers[handlerType], @delegate1);
                    if (handlerToRemove == null)
                    {
                        _msgHandlers.Remove(handlerType);
                    }
                    else
                    {
                        _msgHandlers[handlerType] = handlerToRemove;
                    }
                }
            }
        }
        #endregion
    }
}
#endif
