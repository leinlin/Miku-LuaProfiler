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
        private const int BUFF_LEN = 1024 * 1024 * 100;
        private static MemoryStream ms;
        private static BinaryWriter bw;

        #region public
        public static void ConnectServer(string host, int port)
        {
            if (m_client != null) return;
            m_client = new TcpClient();

            m_client.SendBufferSize = BUFF_LEN;
            //m_client.NoDelay = true;
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
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e);
            }
            finally
            {
                if (ms != null)
                {
                    ms.Close();
                    ms = null;
                    bw = null;
                    GC.Collect();
                }
                m_strDict.Clear();
                UnityEngine.Debug.Log("<color=#00ff00>close client</color>");
            }

            if (m_sendThread != null)
            {
                var tmp = m_sendThread;
                m_sendThread = null;
                tmp.Abort();
            }
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
                        if (m_sampleQueue.Count > 0)
                        {
                            while (m_sampleQueue.Count > 0)
                            {
                                Sample s = m_sampleQueue.Dequeue();
                                ms.Seek(0, SeekOrigin.Begin);
                                bw.Write(PACK_HEAD);
                                Serialize(s, bw);
                                int len = (int)ms.Position;
                                datas = ms.GetBuffer();
                                m_client.Client.Send(datas, len, SocketFlags.None);
                                s.Restore();
                            }
                            m_dict.Clear();
                        }
                        else
                        {
                            //发点空包过去，别让服务器觉得客户端死掉了
                            ms.Seek(0, SeekOrigin.Begin);
                            bw.Write((int)0);
                            int len = (int)ms.Position;
                            datas = ms.GetBuffer();
                            m_client.Client.Send(datas, len, SocketFlags.None);
                        }
                    }

                    Thread.Sleep(50);
                }
#pragma warning disable 0168
                catch (Exception e)
                {
                    Close();
                }
#pragma warning restore 0168
                if (!HookLuaUtil.isPlaying)
                {
                    throw new Exception();
                }
            }

        }

        private static int m_key = 0;

        public static int GetUniqueKey()
        {
            return m_key++;
        }

        private static Dictionary<string, KeyValuePair<int, byte[]>> m_strDict = new Dictionary<string, KeyValuePair<int, byte[]>>(4096);
        private static bool GetBytes(string s, out byte[] result, out int index)
        {
            bool ret = true;
            KeyValuePair<int, byte[]> kp;
            if (!m_strDict.TryGetValue(s, out kp))
            {
                result = Encoding.UTF8.GetBytes(s);
                index = GetUniqueKey();
                kp = new KeyValuePair<int, byte[]>(index, result);
                m_strDict.Add(s, kp);
                ret = false;
            }
            else
            {
                index = kp.Key;
                result = kp.Value;
            }

            return ret;
        }
        private static void Serialize(Sample s, BinaryWriter bw)
        {
            bw.Write(s.calls);
            bw.Write(s.frameCount);
            bw.Write(s.costLuaGC);
            bw.Write(s.costMonoGC);
            byte[] datas;
            int key;
            bool onlyKey = GetBytes(s.name, out datas, out key);
            byte r = (byte)(onlyKey ? 1 :0);
            bw.Write(r);
            bw.Write(key);
            if (!onlyKey)
            {
                bw.Write(datas.Length);
                bw.Write(datas);
            }

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
            UnityEngine.Debug.Log("<color=#00ff00>connect success</color>");
            m_client.Client.SendTimeout = 30000;
            m_strDict.Clear();
            m_key = 0;
            ms = new MemoryStream(BUFF_LEN);
            bw = new BinaryWriter(ms);

            m_sendThread = new Thread(new ThreadStart(DoSendMessage));
            m_sendThread.Start();
        }
        #endregion

    }
}

