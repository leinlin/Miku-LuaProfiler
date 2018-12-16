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
            EditorApplication.playModeStateChanged += (state) =>
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
        private static byte[] bytes = new byte[0xFFFFFF];
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

                if (thread != null)
                {
                    lock (thread)
                    {
                        thread.Abort();
                        thread = null;
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e);
            }
            UnityEngine.Debug.Log("close server");
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
                socklin.ReceiveTimeout = 30000;
                //连接
                if (!socklin.Connected)
                {
                    UnityEngine.Debug.Log("fail");
                    return;
                }
                else
                {
                    UnityEngine.Debug.Log("succ");
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

                    int rlen = socklin.Receive(bytes, SocketFlags.None);   //接受数据
                    if (rlen == 0)
                    {
                        notAvailable++;
                        try
                        {
                            if (notAvailable >= 100)
                            {
                                socklin.Close();
                                socklin = null;
                            }

                        }
                        catch(Exception e)
                        {
                            UnityEngine.Debug.Log(e);
                            Close();
                        }

                        continue;
                    }
                    MemoryStream ms = new MemoryStream(bytes);
                    BinaryReader br = new BinaryReader(ms);
                    //处理粘包
                    while (br.ReadInt32() == PACK_HEAD)
                    {
                        Sample s = Deserialize(br);
                        if (m_onReceive != null)
                        {
                            m_onReceive(s);
                        }
                    }
                    br.Close();
                    notAvailable = 0;
                }
                catch (Exception e)
                {
                    Close();
                }

                Thread.Sleep(50);
            }
        }

        public static Sample Deserialize(BinaryReader br)
        {
            Sample s = new Sample();

            s.calls = br.ReadInt32();
            s.frameCount = br.ReadInt32();
            s.costLuaGC = br.ReadInt64();
            s.costMonoGC = br.ReadInt64();
            int len = br.ReadInt32();
            byte[] datas = br.ReadBytes(len);
            s.name = string.Intern(Encoding.UTF8.GetString(datas).Trim());
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
