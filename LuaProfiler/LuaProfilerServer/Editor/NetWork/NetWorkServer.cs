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
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using UnityEditor;

    [InitializeOnLoad]
    public static class StartUp
    {
        static bool isPlaying = false;

        static StartUp()
        {
#if UNITY_2017_1_OR_NEWER
            EditorApplication.playModeStateChanged += (state) =>
            {
                if (isPlaying == true && EditorApplication.isPlaying == false)
                {
                    NetWorkServer.Close();
                }

                isPlaying = EditorApplication.isPlaying;
            };
#else
            EditorApplication.playmodeStateChanged += () =>
            {
                if (isPlaying == true && EditorApplication.isPlaying == false)
                {
                    NetWorkServer.Close();
                }

                isPlaying = EditorApplication.isPlaying;
            };
#endif
        }
    }

    public static class NetWorkServer
    {
        private static TcpListener tcpLister;
        private static TcpClient tcpClient = null;
        private static Thread receiveThread;
        private static Thread sendThread;
        private static Thread acceptThread;
        private static NetworkStream ns;
        private static BinaryReader br;
        private static BinaryWriter bw;

        private const int PACK_HEAD = 0x23333333;
        private static Action<Sample> m_onReceiveSample;
        private static Action<LuaRefInfo> m_onReceiveRef;
        private static Queue<int> m_cmdQueue = new Queue<int>(32);

        public static bool CheckIsReceiving()
        {
            return tcpClient != null;
        }

        public static void RegisterOnReceiveSample(Action<Sample> onReceive)
        {
            m_onReceiveSample = onReceive;
        }

        public static void RegisterOnReceiveRefInfo(Action<LuaRefInfo> onReceive)
        {
            m_onReceiveRef = onReceive;
        }

        public static void BeginListen(string ip, int port)
        {
            if (tcpLister != null) return;

            m_strCacheDict.Clear();

            IPAddress myIP = IPAddress.Parse(ip);
            tcpLister = new TcpListener(myIP, port);
            tcpLister.Start();
            acceptThread = new Thread(AcceptThread);
            acceptThread.Start();
        }

        private static void AcceptThread()
        {
            UnityEngine.Debug.Log("<color=#00ff00>begin listerner</color>");
            tcpClient = null;
            try
            {
                if (tcpClient == null)
                {
                    tcpClient = tcpLister.AcceptTcpClient();
                }
             }
            catch
            {
                UnityEngine.Debug.Log("<color=#ff0000>start fail</color>");
                Close();
            }

            UnityEngine.Debug.Log("<color=#00ff00>link start</color>");
            tcpClient.ReceiveTimeout = 1000000;
            ns = tcpClient.GetStream();
            br = new BinaryReader(ns);
            bw = new BinaryWriter(ns);
            ns.ReadTimeout = 600000;

            // 启动一个线程来接受请求
            receiveThread = new Thread(DoReceiveMessage);
            receiveThread.Start();

            // 启动一个线程来发送请求
            sendThread = new Thread(DoSendMessage);
            sendThread.Start();
            acceptThread = null;
        }

        public static void SendCmd(int cmd)
        {
            lock (m_cmdQueue)
            {
                m_cmdQueue.Enqueue(cmd);
            }
        }

        // 接受请求
        private static void DoReceiveMessage()
        {
            UnityEngine.Debug.Log("<color=#00ff00>begin to listener</color>");

            //sign为true 循环接受数据
            while (true)
            {
                try
                {
                    if (tcpClient == null)
                    {
                        Close();
                        return;
                    }

                    if (ns.CanRead && ns.DataAvailable)
                    {
                        try
                        {
                            int head = br.ReadInt32();
                            //处理粘包
                            while (head == PACK_HEAD)
                            {
                                int messageId = br.ReadInt32();
                                switch (messageId)
                                {
                                    case 0:
                                        {
                                            Sample s = Deserialize(br);
                                            if (m_onReceiveSample != null)
                                            {
                                                m_onReceiveSample(s);
                                            }
                                        }
                                        break;
                                    case 1:
                                        {
                                            var r = DeserializeRef(br);
                                            if (m_onReceiveRef != null)
                                            {
                                                m_onReceiveRef(r);
                                            }
                                        }
                                        break;
                                }

                            }
                        }
#pragma warning disable 0168
                        catch (EndOfStreamException ex)
                        {
                            Close();
                            return;
                        }
#pragma warning restore 0168
                    }

                }
#pragma warning disable 0168
                catch (ThreadAbortException e) { }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log(e);
                    Close();
                }
#pragma warning restore 0168
                Thread.Sleep(10);
            }
        }

        private static void DoSendMessage()
        {
            while (true)
            {
                try
                {
                    if (ns.CanWrite)
                    {
                        while (m_cmdQueue.Count > 0)
                        {
                            int msgId = -1;
                            lock (m_cmdQueue)
                            {
                                msgId = m_cmdQueue.Dequeue();
                            }
                            bw.Write(PACK_HEAD);
                            bw.Write(msgId);
                        }
                    }
                }
#pragma warning disable 0168
                catch (ThreadAbortException e) { }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log(e);
                    Close();
                }
#pragma warning restore 0168
                Thread.Sleep(10);
            }
        }

        public static void Close()
        {
            try
            {
                if (tcpLister != null)
                {
                    tcpLister.Stop();
                    tcpLister = null;
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e);
            }
            UnityEngine.Debug.Log("<color=#ff0000>disconnect</color>");

            if (acceptThread != null)
            {
                try
                {
                    acceptThread.Abort();
                }
                catch { }
                receiveThread = null;
            }

            if (receiveThread != null)
            {
                try
                {
                    receiveThread.Abort();
                }
                catch { }
                receiveThread = null;
            }
            if (sendThread != null)
            {
                try
                {
                    sendThread.Abort();
                }
                catch { }
                sendThread = null;
            }
        }

        private static Dictionary<int, string> m_strCacheDict = new Dictionary<int, string>(4096);
        public static Sample Deserialize(BinaryReader br)
        {
            Sample s = Sample.Create();

            s.calls = br.ReadInt32();
            s.frameCount = br.ReadInt32();
            s.fps = br.ReadSingle();
            s.pss = br.ReadInt32();
            s.power = br.ReadSingle();
            s.costLuaGC = br.ReadInt32();
            s.costMonoGC = br.ReadInt32();
            s.name = ReadString(br);

            s.costTime = br.ReadInt32();
            s.currentLuaMemory = br.ReadInt32();
            s.currentMonoMemory = br.ReadInt32();
            int count = br.ReadUInt16();
            for (int i = 0, imax = count; i < imax; i++)
            {
                Deserialize(br).fahter = s;
            }
            return s;
        }

        public static LuaRefInfo DeserializeRef(BinaryReader br)
        {
            LuaRefInfo refInfo = LuaRefInfo.Create();
            refInfo.cmd = br.ReadByte();
            refInfo.frameCount = br.ReadInt32();
            refInfo.name = ReadString(br);
            refInfo.addr = ReadString(br);
            refInfo.type = br.ReadByte();

            return refInfo;
        }

        private static string ReadString(BinaryReader br)
        {
            string result = null;

            bool isRef = br.ReadBoolean();
            int index = br.ReadInt32();
            if (!isRef)
            {
                int len = br.ReadInt32();
                byte[] datas = br.ReadBytes(len);
                result = string.Intern(Encoding.UTF8.GetString(datas));
                m_strCacheDict[index] = result;
            }
            else
            {
                result = m_strCacheDict[index];
            }

            return result;
        }
    }

}
