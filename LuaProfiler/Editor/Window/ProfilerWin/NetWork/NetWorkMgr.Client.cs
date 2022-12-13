using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using UnityEngine;
#if UNITY_5_6_OR_NEWER && UNITY_EDITOR_WIN
namespace MikuLuaProfiler
{
    public static class NetWorkMgrClient
    {
        private static NetworkStream ns;
        private static MBinaryReader br;
        private static MBinaryWriter bw;
        private const int PACK_HEAD = 0x114514;

        private static Dictionary<int, Type> receiveMsgDict = new Dictionary<int, Type>();
        private static Dictionary<Type, int> receiveMsgTypeDict = new Dictionary<Type, int>();
        private static Thread receiveThread;
        private static TcpClient tcpClient;
        private static bool _isConnected = false;

        static NetWorkMgrClient()
        {
            var types = typeof(NetWorkMgr).Assembly.GetTypes();
            foreach (var t in types)
            {
                if (t.Namespace == "MikuLuaProfiler"
                    && t.IsSubclassOf(typeof(PacketBase)))
                {
                    var attr = t.GetCustomAttribute<PacketMsgAttribute>();
                    if (attr != null)
                    {
                        receiveMsgDict[(int)attr.msgHead] = t;
                        receiveMsgTypeDict[t] = (int)attr.msgHead;
                    }
                }
            }
        }

        public static void Disconnect()
        {
            if (tcpClient != null)
            {
                tcpClient.Close();
                tcpClient.Dispose();
                tcpClient = null;
                if (bw != null)
                {
                    bw.Dispose();
                    bw = null;
                }

                if (br != null)
                {
                    br.Dispose();
                    br = null;
                }

                _isConnected = false;
            }
        }

        public static void Connect(string ip, int port)
        {
            tcpClient = new TcpClient();
            tcpClient.Connect(ip, port);

            ns = tcpClient.GetStream();
            br = new MBinaryReader(ns);
            bw = new MBinaryWriter(ns);
            if (receiveThread != null)
            {
                receiveThread.Abort();
                receiveThread = null;
            }
            receiveThread = new Thread(DoReceiveMessage);
            receiveThread.Name = "Client ReceiveThread";
            receiveThread.Start();
            _isConnected = true;
        }

        public static bool GetIsConnect()
        {
            return _isConnected;
        }

        private static void DoReceiveMessage()
        {
            while (true)
            {
                try
                {
                    if (tcpClient == null)
                    {
                        break;
                    }

                    if (ns.CanRead && ns.DataAvailable)
                    {
                        int head = br.ReadInt32();
                        //处理粘包
                        while (head == PACK_HEAD)
                        {
                            int messageId = br.ReadInt32();
                            Type t;
                            if (receiveMsgDict.TryGetValue(messageId, out t))
                            {
                                var msg = Activator.CreateInstance(t) as PacketBase;
                                msg.Read(br);
                                msg.OnRun();
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e.ToString());
                    break;
                }
                Thread.Sleep(10);
            }


            if (receiveThread != null)
            {
                receiveThread.Abort();
                receiveThread = null;
            }
        }

        public static void SendMessage(PacketBase msg)
        {
            if(bw == null) return;

            bw.Write(PACK_HEAD);
            int msgId = 0;
            receiveMsgTypeDict.TryGetValue(msg.GetType(), out msgId);
            bw.Write(msgId);
            msg.Write(bw);
            msg.WriteOver();
        }

    }
}
#endif