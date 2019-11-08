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
        private static NetworkStream ns;
        public static MBinaryWriter bw;
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
                m_receiveThread.Priority = ThreadPriority.Lowest;
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
            if (m_client == null)
            {
                sample.Restore();
                return;
            }
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
                            else if (s is LuaDiffInfo)
                            {
                                bw.Write((int)2);
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
                                case 1:
                                    {
                                        HookLuaSetup.RegisterAction(LuaHook.Record);
                                    }
                                    break;
                                case 2:
                                    {
                                        HookLuaSetup.RegisterAction(LuaHook.DiffServer);
                                    }
                                    break;
                            }
                            head = br.ReadInt32();
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

                // 防止多线程 栈展开故意做的优化，先自己展开28层，后面的就递归
                var childs0 = s.childs;
                for (int i0 = 0, i0max = childs0.Count; i0 < i0max; i0++)
                {
                    Sample s0 = childs0[i0];
                    bw.Write(s0.calls);
                    bw.Write(s0.frameCount);
                    bw.Write(s0.fps);
                    bw.Write(s0.pss);
                    bw.Write(s0.power);
                    bw.Write(s0.costLuaGC);
                    bw.Write(s0.costMonoGC);
                    WriteString(bw, s0.name);

                    bw.Write(s0.costTime);
                    bw.Write(s0.currentLuaMemory);
                    bw.Write(s0.currentMonoMemory);
                    bw.Write((ushort)s0.childs.Count);
                    var childs1 = s0.childs;
                    for (int i1 = 0, i1max = childs1.Count; i1 < i1max; i1++)
                    {
                        Sample s1 = childs1[i1];
                        bw.Write(s1.calls);
                        bw.Write(s1.frameCount);
                        bw.Write(s1.fps);
                        bw.Write(s1.pss);
                        bw.Write(s1.power);
                        bw.Write(s1.costLuaGC);
                        bw.Write(s1.costMonoGC);
                        WriteString(bw, s1.name);

                        bw.Write(s1.costTime);
                        bw.Write(s1.currentLuaMemory);
                        bw.Write(s1.currentMonoMemory);
                        bw.Write((ushort)s1.childs.Count);
                        var childs2 = s1.childs;
                        for (int i2 = 0, i2max = childs2.Count; i2 < i2max; i2++)
                        {
                            Sample s2 = childs2[i2];
                            bw.Write(s2.calls);
                            bw.Write(s2.frameCount);
                            bw.Write(s2.fps);
                            bw.Write(s2.pss);
                            bw.Write(s2.power);
                            bw.Write(s2.costLuaGC);
                            bw.Write(s2.costMonoGC);
                            WriteString(bw, s2.name);

                            bw.Write(s2.costTime);
                            bw.Write(s2.currentLuaMemory);
                            bw.Write(s2.currentMonoMemory);
                            bw.Write((ushort)s2.childs.Count);
                            var childs3 = s2.childs;
                            for (int i3 = 0, i3max = childs3.Count; i3 < i3max; i3++)
                            {
                                Sample s3 = childs3[i3];
                                bw.Write(s3.calls);
                                bw.Write(s3.frameCount);
                                bw.Write(s3.fps);
                                bw.Write(s3.pss);
                                bw.Write(s3.power);
                                bw.Write(s3.costLuaGC);
                                bw.Write(s3.costMonoGC);
                                WriteString(bw, s3.name);

                                bw.Write(s3.costTime);
                                bw.Write(s3.currentLuaMemory);
                                bw.Write(s3.currentMonoMemory);
                                bw.Write((ushort)s3.childs.Count);
                                var childs4 = s3.childs;
                                for (int i4 = 0, i4max = childs4.Count; i4 < i4max; i4++)
                                {
                                    Sample s4 = childs4[i4];
                                    bw.Write(s4.calls);
                                    bw.Write(s4.frameCount);
                                    bw.Write(s4.fps);
                                    bw.Write(s4.pss);
                                    bw.Write(s4.power);
                                    bw.Write(s4.costLuaGC);
                                    bw.Write(s4.costMonoGC);
                                    WriteString(bw, s4.name);

                                    bw.Write(s4.costTime);
                                    bw.Write(s4.currentLuaMemory);
                                    bw.Write(s4.currentMonoMemory);
                                    bw.Write((ushort)s4.childs.Count);
                                    var childs5 = s4.childs;
                                    for (int i5 = 0, i5max = childs5.Count; i5 < i5max; i5++)
                                    {
                                        Sample s5 = childs5[i5];
                                        bw.Write(s5.calls);
                                        bw.Write(s5.frameCount);
                                        bw.Write(s5.fps);
                                        bw.Write(s5.pss);
                                        bw.Write(s5.power);
                                        bw.Write(s5.costLuaGC);
                                        bw.Write(s5.costMonoGC);
                                        WriteString(bw, s5.name);

                                        bw.Write(s5.costTime);
                                        bw.Write(s5.currentLuaMemory);
                                        bw.Write(s5.currentMonoMemory);
                                        bw.Write((ushort)s5.childs.Count);
                                        var childs6 = s5.childs;
                                        for (int i6 = 0, i6max = childs6.Count; i6 < i6max; i6++)
                                        {
                                            Sample s6 = childs6[i6];
                                            bw.Write(s6.calls);
                                            bw.Write(s6.frameCount);
                                            bw.Write(s6.fps);
                                            bw.Write(s6.pss);
                                            bw.Write(s6.power);
                                            bw.Write(s6.costLuaGC);
                                            bw.Write(s6.costMonoGC);
                                            WriteString(bw, s6.name);

                                            bw.Write(s6.costTime);
                                            bw.Write(s6.currentLuaMemory);
                                            bw.Write(s6.currentMonoMemory);
                                            bw.Write((ushort)s6.childs.Count);
                                            var childs7 = s6.childs;
                                            for (int i7 = 0, i7max = childs7.Count; i7 < i7max; i7++)
                                            {
                                                Sample s7 = childs7[i7];
                                                bw.Write(s7.calls);
                                                bw.Write(s7.frameCount);
                                                bw.Write(s7.fps);
                                                bw.Write(s7.pss);
                                                bw.Write(s7.power);
                                                bw.Write(s7.costLuaGC);
                                                bw.Write(s7.costMonoGC);
                                                WriteString(bw, s7.name);

                                                bw.Write(s7.costTime);
                                                bw.Write(s7.currentLuaMemory);
                                                bw.Write(s7.currentMonoMemory);
                                                bw.Write((ushort)s7.childs.Count);
                                                var childs8 = s7.childs;
                                                for (int i8 = 0, i8max = childs8.Count; i8 < i8max; i8++)
                                                {
                                                    Sample s8 = childs8[i8];
                                                    bw.Write(s8.calls);
                                                    bw.Write(s8.frameCount);
                                                    bw.Write(s8.fps);
                                                    bw.Write(s8.pss);
                                                    bw.Write(s8.power);
                                                    bw.Write(s8.costLuaGC);
                                                    bw.Write(s8.costMonoGC);
                                                    WriteString(bw, s8.name);

                                                    bw.Write(s8.costTime);
                                                    bw.Write(s8.currentLuaMemory);
                                                    bw.Write(s8.currentMonoMemory);
                                                    bw.Write((ushort)s8.childs.Count);
                                                    var childs9 = s8.childs;
                                                    for (int i9 = 0, i9max = childs9.Count; i9 < i9max; i9++)
                                                    {
                                                        Sample s9 = childs9[i9];
                                                        bw.Write(s9.calls);
                                                        bw.Write(s9.frameCount);
                                                        bw.Write(s9.fps);
                                                        bw.Write(s9.pss);
                                                        bw.Write(s9.power);
                                                        bw.Write(s9.costLuaGC);
                                                        bw.Write(s9.costMonoGC);
                                                        WriteString(bw, s9.name);

                                                        bw.Write(s9.costTime);
                                                        bw.Write(s9.currentLuaMemory);
                                                        bw.Write(s9.currentMonoMemory);
                                                        bw.Write((ushort)s9.childs.Count);
                                                        var childs10 = s9.childs;
                                                        for (int i10 = 0, i10max = childs10.Count; i10 < i10max; i10++)
                                                        {
                                                            Sample s10 = childs10[i10];
                                                            bw.Write(s10.calls);
                                                            bw.Write(s10.frameCount);
                                                            bw.Write(s10.fps);
                                                            bw.Write(s10.pss);
                                                            bw.Write(s10.power);
                                                            bw.Write(s10.costLuaGC);
                                                            bw.Write(s10.costMonoGC);
                                                            WriteString(bw, s10.name);

                                                            bw.Write(s10.costTime);
                                                            bw.Write(s10.currentLuaMemory);
                                                            bw.Write(s10.currentMonoMemory);
                                                            bw.Write((ushort)s10.childs.Count);
                                                            var childs11 = s10.childs;
                                                            for (int i11 = 0, i11max = childs11.Count; i11 < i11max; i11++)
                                                            {
                                                                Sample s11 = childs11[i11];
                                                                bw.Write(s11.calls);
                                                                bw.Write(s11.frameCount);
                                                                bw.Write(s11.fps);
                                                                bw.Write(s11.pss);
                                                                bw.Write(s11.power);
                                                                bw.Write(s11.costLuaGC);
                                                                bw.Write(s11.costMonoGC);
                                                                WriteString(bw, s11.name);

                                                                bw.Write(s11.costTime);
                                                                bw.Write(s11.currentLuaMemory);
                                                                bw.Write(s11.currentMonoMemory);
                                                                bw.Write((ushort)s11.childs.Count);
                                                                var childs12 = s11.childs;
                                                                for (int i12 = 0, i12max = childs12.Count; i12 < i12max; i12++)
                                                                {
                                                                    Sample s12 = childs12[i12];
                                                                    bw.Write(s12.calls);
                                                                    bw.Write(s12.frameCount);
                                                                    bw.Write(s12.fps);
                                                                    bw.Write(s12.pss);
                                                                    bw.Write(s12.power);
                                                                    bw.Write(s12.costLuaGC);
                                                                    bw.Write(s12.costMonoGC);
                                                                    WriteString(bw, s12.name);

                                                                    bw.Write(s12.costTime);
                                                                    bw.Write(s12.currentLuaMemory);
                                                                    bw.Write(s12.currentMonoMemory);
                                                                    bw.Write((ushort)s12.childs.Count);
                                                                    var childs13 = s12.childs;
                                                                    for (int i13 = 0, i13max = childs13.Count; i13 < i13max; i13++)
                                                                    {
                                                                        Sample s13 = childs13[i13];
                                                                        bw.Write(s13.calls);
                                                                        bw.Write(s13.frameCount);
                                                                        bw.Write(s13.fps);
                                                                        bw.Write(s13.pss);
                                                                        bw.Write(s13.power);
                                                                        bw.Write(s13.costLuaGC);
                                                                        bw.Write(s13.costMonoGC);
                                                                        WriteString(bw, s13.name);

                                                                        bw.Write(s13.costTime);
                                                                        bw.Write(s13.currentLuaMemory);
                                                                        bw.Write(s13.currentMonoMemory);
                                                                        bw.Write((ushort)s13.childs.Count);
                                                                        var childs14 = s13.childs;
                                                                        for (int i14 = 0, i14max = childs14.Count; i14 < i14max; i14++)
                                                                        {
                                                                            Sample s14 = childs14[i14];
                                                                            bw.Write(s14.calls);
                                                                            bw.Write(s14.frameCount);
                                                                            bw.Write(s14.fps);
                                                                            bw.Write(s14.pss);
                                                                            bw.Write(s14.power);
                                                                            bw.Write(s14.costLuaGC);
                                                                            bw.Write(s14.costMonoGC);
                                                                            WriteString(bw, s14.name);

                                                                            bw.Write(s14.costTime);
                                                                            bw.Write(s14.currentLuaMemory);
                                                                            bw.Write(s14.currentMonoMemory);
                                                                            bw.Write((ushort)s14.childs.Count);
                                                                            var childs15 = s14.childs;
                                                                            for (int i15 = 0, i15max = childs15.Count; i15 < i15max; i15++)
                                                                            {
                                                                                Sample s15 = childs15[i15];
                                                                                bw.Write(s15.calls);
                                                                                bw.Write(s15.frameCount);
                                                                                bw.Write(s15.fps);
                                                                                bw.Write(s15.pss);
                                                                                bw.Write(s15.power);
                                                                                bw.Write(s15.costLuaGC);
                                                                                bw.Write(s15.costMonoGC);
                                                                                WriteString(bw, s15.name);

                                                                                bw.Write(s15.costTime);
                                                                                bw.Write(s15.currentLuaMemory);
                                                                                bw.Write(s15.currentMonoMemory);
                                                                                bw.Write((ushort)s15.childs.Count);
                                                                                var childs16 = s15.childs;
                                                                                for (int i16 = 0, i16max = childs16.Count; i16 < i16max; i16++)
                                                                                {
                                                                                    Sample s16 = childs16[i16];
                                                                                    bw.Write(s16.calls);
                                                                                    bw.Write(s16.frameCount);
                                                                                    bw.Write(s16.fps);
                                                                                    bw.Write(s16.pss);
                                                                                    bw.Write(s16.power);
                                                                                    bw.Write(s16.costLuaGC);
                                                                                    bw.Write(s16.costMonoGC);
                                                                                    WriteString(bw, s16.name);

                                                                                    bw.Write(s16.costTime);
                                                                                    bw.Write(s16.currentLuaMemory);
                                                                                    bw.Write(s16.currentMonoMemory);
                                                                                    bw.Write((ushort)s16.childs.Count);
                                                                                    var childs17 = s16.childs;
                                                                                    for (int i17 = 0, i17max = childs17.Count; i17 < i17max; i17++)
                                                                                    {
                                                                                        Sample s17 = childs17[i17];
                                                                                        bw.Write(s17.calls);
                                                                                        bw.Write(s17.frameCount);
                                                                                        bw.Write(s17.fps);
                                                                                        bw.Write(s17.pss);
                                                                                        bw.Write(s17.power);
                                                                                        bw.Write(s17.costLuaGC);
                                                                                        bw.Write(s17.costMonoGC);
                                                                                        WriteString(bw, s17.name);

                                                                                        bw.Write(s17.costTime);
                                                                                        bw.Write(s17.currentLuaMemory);
                                                                                        bw.Write(s17.currentMonoMemory);
                                                                                        bw.Write((ushort)s17.childs.Count);
                                                                                        var childs18 = s17.childs;
                                                                                        for (int i18 = 0, i18max = childs18.Count; i18 < i18max; i18++)
                                                                                        {
                                                                                            Sample s18 = childs18[i18];
                                                                                            bw.Write(s18.calls);
                                                                                            bw.Write(s18.frameCount);
                                                                                            bw.Write(s18.fps);
                                                                                            bw.Write(s18.pss);
                                                                                            bw.Write(s18.power);
                                                                                            bw.Write(s18.costLuaGC);
                                                                                            bw.Write(s18.costMonoGC);
                                                                                            WriteString(bw, s18.name);

                                                                                            bw.Write(s18.costTime);
                                                                                            bw.Write(s18.currentLuaMemory);
                                                                                            bw.Write(s18.currentMonoMemory);
                                                                                            bw.Write((ushort)s18.childs.Count);
                                                                                            var childs19 = s18.childs;
                                                                                            for (int i19 = 0, i19max = childs19.Count; i19 < i19max; i19++)
                                                                                            {
                                                                                                Sample s19 = childs19[i19];
                                                                                                bw.Write(s19.calls);
                                                                                                bw.Write(s19.frameCount);
                                                                                                bw.Write(s19.fps);
                                                                                                bw.Write(s19.pss);
                                                                                                bw.Write(s19.power);
                                                                                                bw.Write(s19.costLuaGC);
                                                                                                bw.Write(s19.costMonoGC);
                                                                                                WriteString(bw, s19.name);

                                                                                                bw.Write(s19.costTime);
                                                                                                bw.Write(s19.currentLuaMemory);
                                                                                                bw.Write(s19.currentMonoMemory);
                                                                                                bw.Write((ushort)s19.childs.Count);
                                                                                                var childs20 = s19.childs;
                                                                                                for (int i20 = 0, i20max = childs20.Count; i20 < i20max; i20++)
                                                                                                {
                                                                                                    Sample s20 = childs20[i20];
                                                                                                    bw.Write(s20.calls);
                                                                                                    bw.Write(s20.frameCount);
                                                                                                    bw.Write(s20.fps);
                                                                                                    bw.Write(s20.pss);
                                                                                                    bw.Write(s20.power);
                                                                                                    bw.Write(s20.costLuaGC);
                                                                                                    bw.Write(s20.costMonoGC);
                                                                                                    WriteString(bw, s20.name);

                                                                                                    bw.Write(s20.costTime);
                                                                                                    bw.Write(s20.currentLuaMemory);
                                                                                                    bw.Write(s20.currentMonoMemory);
                                                                                                    bw.Write((ushort)s20.childs.Count);
                                                                                                    var childs21 = s20.childs;
                                                                                                    for (int i21 = 0, i21max = childs21.Count; i21 < i21max; i21++)
                                                                                                    {
                                                                                                        Sample s21 = childs21[i21];
                                                                                                        bw.Write(s21.calls);
                                                                                                        bw.Write(s21.frameCount);
                                                                                                        bw.Write(s21.fps);
                                                                                                        bw.Write(s21.pss);
                                                                                                        bw.Write(s21.power);
                                                                                                        bw.Write(s21.costLuaGC);
                                                                                                        bw.Write(s21.costMonoGC);
                                                                                                        WriteString(bw, s21.name);

                                                                                                        bw.Write(s21.costTime);
                                                                                                        bw.Write(s21.currentLuaMemory);
                                                                                                        bw.Write(s21.currentMonoMemory);
                                                                                                        bw.Write((ushort)s21.childs.Count);
                                                                                                        var childs22 = s21.childs;
                                                                                                        for (int i22 = 0, i22max = childs22.Count; i22 < i22max; i22++)
                                                                                                        {
                                                                                                            Sample s22 = childs22[i22];
                                                                                                            bw.Write(s22.calls);
                                                                                                            bw.Write(s22.frameCount);
                                                                                                            bw.Write(s22.fps);
                                                                                                            bw.Write(s22.pss);
                                                                                                            bw.Write(s22.power);
                                                                                                            bw.Write(s22.costLuaGC);
                                                                                                            bw.Write(s22.costMonoGC);
                                                                                                            WriteString(bw, s22.name);

                                                                                                            bw.Write(s22.costTime);
                                                                                                            bw.Write(s22.currentLuaMemory);
                                                                                                            bw.Write(s22.currentMonoMemory);
                                                                                                            bw.Write((ushort)s22.childs.Count);
                                                                                                            var childs23 = s22.childs;
                                                                                                            for (int i23 = 0, i23max = childs23.Count; i23 < i23max; i23++)
                                                                                                            {
                                                                                                                Sample s23 = childs23[i23];
                                                                                                                bw.Write(s23.calls);
                                                                                                                bw.Write(s23.frameCount);
                                                                                                                bw.Write(s23.fps);
                                                                                                                bw.Write(s23.pss);
                                                                                                                bw.Write(s23.power);
                                                                                                                bw.Write(s23.costLuaGC);
                                                                                                                bw.Write(s23.costMonoGC);
                                                                                                                WriteString(bw, s23.name);

                                                                                                                bw.Write(s23.costTime);
                                                                                                                bw.Write(s23.currentLuaMemory);
                                                                                                                bw.Write(s23.currentMonoMemory);
                                                                                                                bw.Write((ushort)s23.childs.Count);
                                                                                                                var childs24 = s23.childs;
                                                                                                                for (int i24 = 0, i24max = childs24.Count; i24 < i24max; i24++)
                                                                                                                {
                                                                                                                    Sample s24 = childs24[i24];
                                                                                                                    bw.Write(s24.calls);
                                                                                                                    bw.Write(s24.frameCount);
                                                                                                                    bw.Write(s24.fps);
                                                                                                                    bw.Write(s24.pss);
                                                                                                                    bw.Write(s24.power);
                                                                                                                    bw.Write(s24.costLuaGC);
                                                                                                                    bw.Write(s24.costMonoGC);
                                                                                                                    WriteString(bw, s24.name);

                                                                                                                    bw.Write(s24.costTime);
                                                                                                                    bw.Write(s24.currentLuaMemory);
                                                                                                                    bw.Write(s24.currentMonoMemory);
                                                                                                                    bw.Write((ushort)s24.childs.Count);
                                                                                                                    var childs25 = s24.childs;
                                                                                                                    for (int i25 = 0, i25max = childs25.Count; i25 < i25max; i25++)
                                                                                                                    {
                                                                                                                        Sample s25 = childs25[i25];
                                                                                                                        bw.Write(s25.calls);
                                                                                                                        bw.Write(s25.frameCount);
                                                                                                                        bw.Write(s25.fps);
                                                                                                                        bw.Write(s25.pss);
                                                                                                                        bw.Write(s25.power);
                                                                                                                        bw.Write(s25.costLuaGC);
                                                                                                                        bw.Write(s25.costMonoGC);
                                                                                                                        WriteString(bw, s25.name);

                                                                                                                        bw.Write(s25.costTime);
                                                                                                                        bw.Write(s25.currentLuaMemory);
                                                                                                                        bw.Write(s25.currentMonoMemory);
                                                                                                                        bw.Write((ushort)s25.childs.Count);
                                                                                                                        var childs26 = s25.childs;
                                                                                                                        for (int i26 = 0, i26max = childs26.Count; i26 < i26max; i26++)
                                                                                                                        {
                                                                                                                            Sample s26 = childs26[i26];
                                                                                                                            bw.Write(s26.calls);
                                                                                                                            bw.Write(s26.frameCount);
                                                                                                                            bw.Write(s26.fps);
                                                                                                                            bw.Write(s26.pss);
                                                                                                                            bw.Write(s26.power);
                                                                                                                            bw.Write(s26.costLuaGC);
                                                                                                                            bw.Write(s26.costMonoGC);
                                                                                                                            WriteString(bw, s26.name);

                                                                                                                            bw.Write(s26.costTime);
                                                                                                                            bw.Write(s26.currentLuaMemory);
                                                                                                                            bw.Write(s26.currentMonoMemory);
                                                                                                                            bw.Write((ushort)s26.childs.Count);
                                                                                                                            var childs27 = s26.childs;
                                                                                                                            for (int i27 = 0, i27max = childs27.Count; i27 < i27max; i27++)
                                                                                                                            {
                                                                                                                                Sample s27 = childs27[i27];
                                                                                                                                bw.Write(s27.calls);
                                                                                                                                bw.Write(s27.frameCount);
                                                                                                                                bw.Write(s27.fps);
                                                                                                                                bw.Write(s27.pss);
                                                                                                                                bw.Write(s27.power);
                                                                                                                                bw.Write(s27.costLuaGC);
                                                                                                                                bw.Write(s27.costMonoGC);
                                                                                                                                WriteString(bw, s27.name);

                                                                                                                                bw.Write(s27.costTime);
                                                                                                                                bw.Write(s27.currentLuaMemory);
                                                                                                                                bw.Write(s27.currentMonoMemory);
                                                                                                                                bw.Write((ushort)s27.childs.Count);
                                                                                                                                var childs28 = s27.childs;
                                                                                                                                for (int i28 = 0, i28max = childs28.Count; i28 < i28max; i28++)
                                                                                                                                {
                                                                                                                                    Sample s28 = childs28[i28];
                                                                                                                                    Serialize(s28, bw);
                                                                                                                                }
                                                                                                                            }
                                                                                                                        }
                                                                                                                    }
                                                                                                                }
                                                                                                            }
                                                                                                        }
                                                                                                    }
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
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
            }
            else if (o is LuaDiffInfo)
            {
                LuaDiffInfo ld = (LuaDiffInfo)o;
                // add
                var addDict = ld.addRef;
                bw.Write(addDict.Count);
                foreach (var item in addDict)
                {
                    WriteString(bw, item.Key);
                    bw.Write((int)item.Value);
                }
                var addDetail = ld.addDetail;
                bw.Write(addDetail.Count);
                foreach (var item in addDetail)
                {
                    WriteString(bw, item.Key);
                    var list = item.Value;
                    bw.Write(list.Count);
                    foreach (var listItem in list)
                    {
                        WriteString(bw, listItem);
                    }
                }
                // rm
                var rmDict = ld.rmRef;
                bw.Write(rmDict.Count);
                foreach (var item in rmDict)
                {
                    WriteString(bw, item.Key);
                    bw.Write((int)item.Value);
                }
                var rmDetail = ld.rmDetail;
                bw.Write(rmDetail.Count);
                foreach (var item in rmDetail)
                {
                    WriteString(bw, item.Key);
                    var list = item.Value;
                    bw.Write(list.Count);
                    foreach (var listItem in list)
                    {
                        WriteString(bw, listItem);
                    }
                }

                // null
                var nullDict = ld.nullRef;
                bw.Write(nullDict.Count);
                foreach (var item in nullDict)
                {
                    WriteString(bw, item.Key);
                    bw.Write((int)item.Value);
                }
                var nullDetail = ld.nullDetail;
                bw.Write(nullDetail.Count);
                foreach (var item in nullDetail)
                {
                    WriteString(bw, item.Key);
                    var list = item.Value;
                    bw.Write(list.Count);
                    foreach (var listItem in list)
                    {
                        WriteString(bw, listItem);
                    }
                }
            }
        }

        public static void WriteString(BinaryWriter bw, string name)
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