/*
* ==============================================================================
* Filename: NetWorkClient
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
    using System.Net.Sockets;
    using System.Threading;
    using System.Text;

    public static class NetWorkClient
    {
        private static TcpClient m_client = null;
        private static Thread m_sendThread;
        private static Queue<Sample> m_sampleQueue = new Queue<Sample>(256);
        private const int PACK_HEAD = 0x23333333;
        private static SocketError errorCode;
        #region public
        public static void ConnectServer(string host, int port)
        {
            if (m_client != null) return;

            m_client = new TcpClient();
            m_client.NoDelay = true;
            try
            {
                IAsyncResult result = m_client.BeginConnect(
                    host,
                    port,
                    new AsyncCallback(OnConnect),
                    null
                );

                bool success = result.AsyncWaitHandle.WaitOne(5000, true);
                if (!success)
                {
                    //超时
                    Close();
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e);
                Close();
            }
        }

        public static void Close()
        {
            try
            {
                if (m_client != null)
                {
                    if (m_client.Connected)
                    {
                        m_client.Close();
                    }
                    m_client = null;
                }
                m_sampleQueue.Clear();
                if (m_sendThread != null)
                {
                    m_sendThread.Abort();
                    m_sendThread = null;
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e);
            }
            UnityEngine.Debug.Log("close client");
        }

        static Dictionary<string, Sample> m_dict = new Dictionary<string, Sample>(1024);
        public static void SendMessage(Sample sample)
        {
            lock (m_sampleQueue)
            {
                Sample s;
                if (m_dict.TryGetValue(sample.name, out s))
                {
                    s.AddSample(sample);
                    sample.Restore();
                }
                else
                {
                    m_sampleQueue.Enqueue(sample);
                    m_dict.Add(sample.name, sample);
                }
            }
        }
        #endregion

        #region private
        private static void DoSendMessage()
        {
            while (true)
            {
                try
                {
                    if (m_sendThread == null) return;
                    byte[] datas = null;
                    lock (m_sampleQueue)
                    {
                        while (m_sampleQueue.Count > 0)
                        {
                            Sample s = m_sampleQueue.Dequeue();
                            MemoryStream ms = new MemoryStream();
                            BinaryWriter bw = new BinaryWriter(ms);
                            bw.Write(PACK_HEAD);
                            Serialize(s, bw);
                            datas = ms.ToArray();
                            bw.Close();
                            m_client.Client.Send(datas, datas.Length, SocketFlags.None);
                            s.Restore();
                        }
                        m_dict.Clear();
                    }

                    Thread.Sleep(50);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log(e);
                    Close();
                }
                if (!HookLuaUtil.isPlaying)
                {
                    throw new Exception();
                }
            }

        }

        private static void Serialize(Sample s, BinaryWriter bw)
        {
            bw.Write(s.calls);
            bw.Write(s.frameCount);
            bw.Write(s.costLuaGC);
            bw.Write(s.costMonoGC);
            byte[] datas = Encoding.UTF8.GetBytes(s.name);
            bw.Write(datas.Length);
            bw.Write(datas);
            bw.Write(s.costTime);
            bw.Write(s.currentLuaMemory);
            bw.Write(s.currentMonoMemory);
            bw.Write(s.childs.Count);
            for (int i = 0, imax = s.childs.Count; i < imax; i++)
            {
                Serialize(s.childs[i], bw);
            }

        }

        private static void OnConnect(IAsyncResult asr)
        {
            m_client.Client.SendTimeout = 30000;
            m_sendThread = new Thread(new ThreadStart(DoSendMessage));
            m_sendThread.Start();
        }
        #endregion

    }
}

