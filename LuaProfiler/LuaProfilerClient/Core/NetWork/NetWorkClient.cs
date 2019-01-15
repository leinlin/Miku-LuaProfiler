/*
               #########                       
              ############                     
              #############                    
             ##  ###########                   
            ###  ###### #####                  
            ### #######   ####                 
           ###  ########## ####                
          ####  ########### ####               
         ####   ###########  #####             
        #####   ### ########   #####           
       #####   ###   ########   ######         
      ######   ###  ###########   ######       
     ######   #### ##############  ######      
    #######  #####################  ######     
    #######  ######################  ######    
   #######  ###### #################  ######   
   #######  ###### ###### #########   ######   
   #######    ##  ######   ######     ######   
   #######        ######    #####     #####    
    ######        #####     #####     ####     
     #####        ####      #####     ###      
      #####       ###        ###      #        
        ###       ###        ###               
         ##       ###        ###               
__________#_______####_______####______________
                我们的未来没有BUG                
* ==============================================================================
* Filename: NetWorkClient
* Created:  2018/7/13 14:29:22
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/
#if UNITY_EDITOR || USE_LUA_PROFILER

namespace MikuLuaProfiler
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;

    public static class NetWorkClient
    {
        private static TcpClient m_client = null;
        private static Thread m_sendThread;
        private static Thread m_receiveThread;
        private static Queue<NetBase> m_sampleQueue = new Queue<NetBase>(256);
        private const int PACK_HEAD = 0x23333333;
        private static SocketError errorCode;
        private static NetworkStream ns;
        private static MBinaryWriter bw;
        private static BinaryReader br;
        private static int m_frameCount = 0;

        #region public
        public static void ConnectServer(string host, int port)
        {
            if (m_client != null) return;
            m_client = new TcpClient();

            m_client.NoDelay = true;
            try
            {
                m_client.Connect(host, port);

                UnityEngine.Debug.Log("<color=#00ff00>connect success</color>");
                m_client.Client.SendTimeout = 30000;
                //m_sampleDict.Clear();
                m_strDict.Clear();
                m_key = 0;
                ns = m_client.GetStream();
                bw = new MBinaryWriter(ns);
                br = new BinaryReader(ns);

                m_sendThread = new Thread(new ThreadStart(DoSendMessage));
                m_sendThread.Start();
                m_receiveThread = new Thread(new ThreadStart(DoRecieveMessage));
                m_receiveThread.Start();
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
                m_strDict.Clear();
            }

            if (m_receiveThread != null)
            {
                var tmp = m_receiveThread;
                m_receiveThread = null;
                tmp.Abort();
            }

            if (m_sendThread != null)
            {
                var tmp = m_sendThread;
                m_sendThread = null;
                tmp.Abort();
            }
        }

        //private static Dictionary<string, Sample> m_sampleDict = new Dictionary<string, Sample>(256);

        public static void SendMessage(NetBase sample)
        {
            if (m_client == null) return;
            lock (m_sampleQueue)
            {
                m_sampleQueue.Enqueue(sample);
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
                    if (m_sendThread == null)
                    {
                        UnityEngine.Debug.LogError("<color=#ff0000>m_sendThread null</color>");
                        return;
                    }
                    if (m_sampleQueue.Count > 0)
                    {
                        while (m_sampleQueue.Count > 0)
                        {
                            NetBase s = null;
                            lock (m_sampleQueue)
                            {
                                s = m_sampleQueue.Dequeue();
                            }
                            bw.Write(PACK_HEAD);
                            if (s is Sample)
                            {
                                bw.Write((int)0);
                            }
                            else if (s is LuaRefInfo)
                            {
                                bw.Write((int)1);
                            }
                            Serialize(s, bw);
                            s.Restore();
                        }
                    }
                    else if (m_frameCount != HookLuaSetup.frameCount)
                    {
                        bw.Write(PACK_HEAD);
                        //写入message 头编号
                        bw.Write((int)0);
                        Sample s = Sample.Create(0, (int)LuaLib.GetLuaMemory(LuaProfiler.mainL), "");
                        Serialize(s, bw);
                        s.Restore();
                        m_frameCount = HookLuaSetup.frameCount;
                    }
                    Thread.Sleep(10);
                }
#pragma warning disable 0168
                catch (ThreadAbortException e) { }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log(e);
                    Close();
                }
#pragma warning restore 0168
            }

        }

        private static void DoRecieveMessage()
        {
            while (true)
            {
                try
                {
                    if (m_receiveThread == null)
                    {
                        UnityEngine.Debug.LogError("<color=#ff0000>m_receiveThread null</color>");
                        return;
                    }
                    if (ns.CanRead && ns.DataAvailable)
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
                                        LuaProfiler.SendAllRef();
                                    }
                                    break;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log(e);
                }
                Thread.Sleep(10);
            }
        }

        private static int m_key = 0;
        public static int GetUniqueKey()
        {
            return m_key++;
        }
        private static Dictionary<string, KeyValuePair<int, byte[]>> m_strDict = new Dictionary<string, KeyValuePair<int, byte[]>>(8192);
        private static bool GetBytes(string s, out byte[] result, out int index)
        {
            bool ret = true;
            KeyValuePair<int, byte[]> keyValuePair;
            if (!m_strDict.TryGetValue(s, out keyValuePair))
            {
                result = Encoding.UTF8.GetBytes(s);
                index = GetUniqueKey();
                keyValuePair = new KeyValuePair<int, byte[]>(index, result);
                m_strDict.Add(s, keyValuePair);
                ret = false;
            }
            else
            {
                index = keyValuePair.Key;
                result = keyValuePair.Value;
            }

            return ret;
        }
        static int m_sleep_count = 0;
        private static void Serialize(NetBase o, BinaryWriter bw)
        {
            if (o is Sample)
            {
                Sample s = (Sample)o;

                bw.Write(s.calls);
                bw.Write(s.frameCount);
                bw.Write(s.fps);
                bw.Write(s.pss);
                bw.Write(s.power);
                bw.Write(s.costLuaGC);
                bw.Write(s.costMonoGC);
                WriteString(bw, s.name);

                bw.Write(s.costTime);
                bw.Write(s.currentLuaMemory);
                bw.Write(s.currentMonoMemory);
                bw.Write((ushort)s.childs.Count);
                m_sleep_count++;
                if (m_sleep_count >= 8)
                {
                    Thread.Sleep(0);
                    m_sleep_count = 0;
                }


                var childs = s.childs;
                for (int i = 0; i < childs.Count; i++)
                {
                    Serialize(childs[i], bw);
                }
            }
            else if (o is LuaRefInfo)
            {
                LuaRefInfo r = (LuaRefInfo)o;

                bw.Write(r.cmd);
                bw.Write(HookLuaSetup.frameCount);
                WriteString(bw, r.name);
                WriteString(bw, r.addr);
                bw.Write(r.type);
                Thread.Sleep(0);
            }
            m_sleep_count = 0;
        }

        private static void WriteString(BinaryWriter bw, string name)
        {
            byte[] datas;
            int index = 0;
            bool isRef = GetBytes(name, out datas, out index);
            bw.Write(isRef);
            bw.Write(index);
            if (!isRef)
            {
                bw.Write(datas.Length);
                bw.Write(datas);
            }
        }

        #endregion

    }

}

#endif