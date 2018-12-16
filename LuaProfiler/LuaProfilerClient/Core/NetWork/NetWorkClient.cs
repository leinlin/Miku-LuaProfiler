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
    using System.Net.Sockets;
    using System.Threading;

    public static class NetWorkClient
    {
        private static TcpClient m_client = null;
        private static Thread m_sendThread;
        private static ByteBuf m_buf;
        private static Queue<Sample> m_sampleQueue = new Queue<Sample>(256);
        private const int PACK_HEAD = 0x23333333;

        #region public
        public static void ConnectServer(string host, int port)
        {
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
                m_sampleQueue.Clear();
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
            if (m_client != null)
            {
                if (m_client.Connected) m_client.Close();
                m_client = null;
            }

            if (m_sendThread != null)
            {
                m_sendThread.Abort();
                m_sendThread = null;
            }
            m_sampleQueue.Clear();
            UnityEngine.Debug.Log("close client");
        }

        public static void SendMessage(Sample sample)
        {
            lock (m_sampleQueue)
            {
                m_sampleQueue.Enqueue(sample);
            }
        }
        #endregion

        #region private
        private static void DoSendMessage()
        {

            if (m_buf == null)
            {
                m_buf = ByteBuf.Allocate(4096);
            }

            while (true)
            {
                try
                {
                    lock (m_sampleQueue)
                    {
                        while (m_sampleQueue.Count > 0)
                        {
                            Sample s = m_sampleQueue.Dequeue();
                            m_buf.Clear();

                            m_buf.Write(PACK_HEAD);
                            Serialize(s);
                            s.Restore();
                            int length = 0;
                            byte[] datas = m_buf.GetOrginArray(out length);
                            m_client.Client.Send(datas, length, SocketFlags.None);
                        }
                    }
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

        private static void Serialize(Sample s)
        {
            m_buf.Write(s.calls);
            m_buf.Write(s.frameCount);
            m_buf.Write(s.costLuaGC);
            m_buf.Write(s.costMonoGC);
            m_buf.Write(s.name);
            m_buf.Write(s.costTime);
            m_buf.Write(s.currentLuaMemory);
            m_buf.Write(s.currentMonoMemory);
            m_buf.Write(s.childs.Count);
            for (int i = 0, imax = s.childs.Count; i < imax; i++)
            {
                Serialize(s.childs[i]);
            }
        }

        private static void OnConnect(IAsyncResult asr)
        {
            m_sendThread = new Thread(new ThreadStart(DoSendMessage));
            m_sendThread.Start();
        }
        #endregion

    }
}

