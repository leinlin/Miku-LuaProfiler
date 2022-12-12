#if UNITY_EDITOR_WIN || USE_LUA_PROFILER

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Debug = UnityEngine.Debug;

namespace MikuLuaProfiler
{
    public static partial class NetWorkMgr
    {
        private static TcpListener tcpLister;
        private static TcpClient tcpClient = null;
        private static Thread acceptThread;

        public static event Action<TcpClient> OnConnected;
        public static event Action<TcpClient> OnDisconnect;

        #region public
        public static void BeginListen(string ip, int port)
        {
            if (tcpLister != null) return;
            
            AddListener<PKGDisconnect>(OnRecvDisconnect);

            IPAddress myIP = IPAddress.Parse(ip);
            tcpLister = new TcpListener(myIP, port);
            tcpLister.Start();
            acceptThread = new Thread(AcceptThread);
            acceptThread.Name = "ProfilerAcceptThread";
            acceptThread.Start();
        }

        public static void Close()
        {
            RemoveAllListeners();
            isClose = true;
            if (tcpLister != null)
            {
                tcpLister.Stop();
                tcpLister = null;
            }

            if (acceptThread != null)
            {
                acceptThread.Abort();
                acceptThread = null;
            }
        }

        #endregion

        #region private
        private static void AcceptThread()
        {
            Debug.Log("<color=#00ff00>begin listerner</color>");
            tcpClient = null;

            while (true)
            {
                try
                {
                    if (tcpLister == null) break;
                    AcceptAClient();
                    Thread.Sleep(100);
                }
                catch
                {
                    break;
                }
            }
        }
        
        private static void AcceptAClient()
        {
            var newTcpClient = tcpLister.AcceptTcpClient();
            Debug.Log("<color=#00ff00>link start</color>");
            
            // 有新的连接，强制踢掉旧连接（不然偶尔会出现莫名挂掉无法连接） TODO: 支持多客户端并发连接
            _Close();
            tcpClient?.Close();
            receiveThread?.Join();
            sendThread?.Join();

            tcpClient = newTcpClient;
            lastBeatTime = curTime;
            isClose = false;
            
            NetworkStream ns = tcpClient.GetStream();
            // 启动一个线程来接受请求
            receiveThread = new Thread(()=>DoReceiveMessage(ns));
            receiveThread.Name = "ProfilerReceiveMessage";
            receiveThread.Start();

            // 启动一个线程来发送请求
            sendThread = new Thread(()=>DoSendMessage(ns));
            sendThread.Name = "ProfilerSendMessage";
            sendThread.Start();

            if (OnConnected != null)
            {
                var clientCtx = tcpClient;
                Action actionOnConnected = ()=> OnConnected(clientCtx);
                lock(m_eventQueue)
                    m_eventQueue.Enqueue(actionOnConnected);
            }
        }
        #endregion
    }
}
#endif
