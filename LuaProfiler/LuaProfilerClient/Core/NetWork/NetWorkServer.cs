/*
* ==============================================================================
* Filename: NetWorkServer
* Created:  2018/7/13 14:29:22
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/

namespace MikuLuaProfiler
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    public static class NetWorkServer
    {
        private static Socket sock;
        private static IPEndPoint MyServer;                             //定义主机
        private static Thread thread;
        private static Socket socklin;                                  //临时套接字,接受客户端连接请求
        private static byte[] bytes = new byte[65536];
        public static ByteBuf m_buf = ByteBuf.Allocate(bytes);
        private const int PACK_HEAD = 0x23333333;
        private static Action<Sample> m_onReceive;

        public static void RegisterOnReceive(Action<Sample> onReceive)
        {
            m_onReceive = onReceive;
        }

        public static void BeginListen(string ip, int port)
        {
            try
            {
                IPAddress myIP = IPAddress.Parse(ip);
                //定义主机
                MyServer = new IPEndPoint(myIP, port);
                //构造套接字
                sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //绑定端口
                sock.Bind(MyServer);
                //开始监听
                sock.Listen(0);

                UnityEngine.Debug.Log(string.Format("主机 {0} 端口 {1} 开始监听", ip, port));

                //构造线程
                thread = new Thread(new ThreadStart(DoReceive)); //targett自定义函数:接受客户端连接请求
                //启动线程用于接受连接和接受数据
                thread.Start();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e);
            }
        }

        public static void Close()
        {
            if (thread != null)
            {
                thread.Abort();
                thread = null;
            }
            if (socklin != null)
            {
                socklin.Close();
                socklin = null;
            }
            if (sock != null)
            {
                sock.Close();
                sock = null;
            }
            UnityEngine.Debug.Log("close server");
        }

        private static void DoReceive()
        {
            socklin = sock.Accept();   //接受连接请求
            UnityEngine.Debug.Log("xxx");
            socklin.ReceiveTimeout = 100000;
            //socklin.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.ReceiveTimeout, -300);
            //连接
            if (!socklin.Connected)
            {
                UnityEngine.Debug.Log("fail");
                return;
            }
            UnityEngine.Debug.Log("succ");
            //sign为true 循环接受数据
            while (true)
            {
                try
                {
                    m_buf.Clear();
                    if (socklin == null) return;
                    socklin.Receive(bytes, SocketFlags.None);   //接受数据
                    m_buf = ByteBuf.Allocate(bytes);

                    //处理粘包
                    while (m_buf.ReadInt() == PACK_HEAD)
                    {
                        Sample s = Deserialize();
                        if (m_onReceive != null)
                        {
                            m_onReceive(s);
                        }
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log(e);
                    Close();
                }
            }
        }

        public static Sample Deserialize()
        {
            Sample s = Sample.Create();

            s.calls = m_buf.ReadInt();
            s.frameCount = m_buf.ReadInt();
            s.costLuaGC = m_buf.ReadLong();
            s.costMonoGC = m_buf.ReadLong();
            s.name = m_buf.ReadString();
            s.costTime = m_buf.ReadLong();
            s.currentLuaMemory = m_buf.ReadLong();
            s.currentMonoMemory = m_buf.ReadLong();
            int count = m_buf.ReadInt();

            s.childs = new List<Sample>(count);
            for (int i = 0, imax = count; i < imax; i++)
            {
                s.childs.Add(Deserialize());
            }

            return s;
        }

    }
}
