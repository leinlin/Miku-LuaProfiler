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
    using UnityEditor;
    using System.Text;
    using System.IO;

    [InitializeOnLoad]
    public static class StartUp
    {
        static bool isPlaying = false;

        static StartUp()
        {
            EditorApplication.playmodeStateChanged += () =>
            {
            if (isPlaying == true && EditorApplication.isPlaying == false)
                {
                    NetWorkServer.Close();
                }

                isPlaying = EditorApplication.isPlaying;
            };
        }

    }

    public static class NetWorkServer
    {
        private static Socket sock;
        private static IPEndPoint MyServer;                             //定义主机
        private static Thread thread;
        private static Socket socklin;                                  //临时套接字,接受客户端连接请求
        private const int BUFF_LEN = 1024 * 1024 * 100;
        private static byte[] bytes;
        private static MemoryStream ms;
        private static BinaryReader br;

        private const int PACK_HEAD = 0x23333333;
        private static Action<Sample> m_onReceive;


        public static bool CheckIsReceiving()
        {
            return socklin != null;
        }

        public static void RegisterOnReceive(Action<Sample> onReceive)
        {
            m_onReceive = onReceive;
        }

        public static void BeginListen(string ip, int port)
        {
            if (sock != null) return;
            m_strCacheDict.Clear();

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

                UnityEngine.Debug.Log(string.Format("<color=#00ff00>listen to port: {1}</color>", ip, port));

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
            try
            {
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

            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e);
            }

            bytes = null;
            if (ms != null)
            {
                ms.Dispose();
                UnityEngine.Debug.Log("<color=#00ff00>close server</color>");
            }
            ms = null;
            br = null;
            GC.Collect();
            if (thread != null)
            {
                if (thread.ThreadState != ThreadState.Running)
                {
                    var tmp = thread;
                    thread = null;
                    tmp.Abort();
                }
                else
                {
                    thread = null;
                }
            }
        }

        private static void DoReceive()
        {
            int notAvailable = 0;

            if (socklin == null)
            {
                try
                {
                    socklin = sock.Accept();   //接受连接请求
                }
                catch
                {
                    socklin = null;
                    return;
                }
                socklin.ReceiveBufferSize = BUFF_LEN;
                socklin.ReceiveTimeout = 30000;
                //连接
                if (!socklin.Connected)
                {
                    UnityEngine.Debug.Log("<color=#ff0000>fail</color>");
                    return;
                }
                else
                {
                    bytes = new byte[BUFF_LEN];
                    ms = new MemoryStream(bytes);
                    br = new BinaryReader(ms);
                    m_strCacheDict.Clear();
                    UnityEngine.Debug.Log("<color=#00ff00>connect success</color>");
                }
            }

            //sign为true 循环接受数据
            while (true)
            {
                try
                {
                    if (thread == null)
                    {
                        return;
                    }

                    if (socklin == null)
                    {
                        return;
                    }

                    int rlen = socklin.Receive(bytes, SocketFlags.None);   //接受数据
                    if (rlen == 0)
                    {
                        notAvailable++;
                        if (notAvailable >= 100)
                        {
                            Close();
                        }

                        continue;
                    }

                    ms.Seek(0, SeekOrigin.Begin);
                    //处理粘包
                    while (br.ReadInt32() == PACK_HEAD)
                    {
                        Sample s = Deserialize(br);
                        if (m_onReceive != null)
                        {
                            m_onReceive(s);
                        }
                    }
                    notAvailable = 0;
                }
#pragma warning disable 0168
                catch (Exception e)
                {
                    UnityEngine.Debug.Log(e);
                    Close();
                }
#pragma warning restore 0168
                Thread.Sleep(50);
            }
        }
        private static Dictionary<int, string> m_strCacheDict = new Dictionary<int, string>(4096);
        public static Sample Deserialize(BinaryReader br)
        {
            Sample s = Sample.Create();

            s.calls = br.ReadInt32();
            s.frameCount = br.ReadInt32();
            s.costLuaGC = br.ReadInt64();
            s.costMonoGC = br.ReadInt64();

            int len = br.ReadInt32();
            byte[] datas = br.ReadBytes(len);
            s.name = string.Intern(Encoding.UTF8.GetString(datas));

            s.costTime = br.ReadInt64();
            s.currentLuaMemory = br.ReadInt64();
            s.currentMonoMemory = br.ReadInt64();
            int count = br.ReadInt32();

            for (int i = 0, imax = count; i < imax; i++)
            {
                Deserialize(br).fahter = s;
            }
            return s;
        }

    }
}
