using AdvancedDataGridView;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using EasyHook;
using System.Runtime.Remoting;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace MikuLuaProfiler
{
    public partial class ProfilerForm : Form
    {
        public ProfilerForm()
        {
            InitializeComponent();
            button1.Enabled = true;
            attachmentColumn.DefaultCellStyle.NullValue = null;

            NetWorkServer.RegisterOnReceiveSample(OnReceiveSample);
            NetWorkServer.BeginListen("0.0.0.0", 2333);
            timer1.Enabled = true;
            boldFont = new Font(tvTaskList.DefaultCellStyle.Font, FontStyle.Bold);
        }

        protected override void OnClosed(EventArgs e)
        {
            Thread.Sleep(1000);
            base.OnClosed(e);
        }

        #region refresh
        Queue<Sample> queue = new Queue<Sample>(32);
        Dictionary<string, Sample> dict = new Dictionary<string, Sample>();
        Dictionary<string, TreeGridNode> nodeDict = new Dictionary<string, TreeGridNode>();
        List<Sample> roots = new List<Sample>();
        private Font boldFont;
        private void OnReceiveSample(Sample sample)
        {
            lock (queue)
            {
                queue.Enqueue(sample);
                Console.WriteLine(sample.name);
            }
        }

        private void FillFormInfo()
        {
            foreach (var item in roots)
            {
                TreeGridNode treeNode;
                if (!nodeDict.TryGetValue(item.fullName, out treeNode))
                {
                    treeNode = tvTaskList.Nodes.Add();
                    nodeDict.Add(item.fullName, treeNode);
                }
                DoFillChildFormInfo(item, treeNode);
            }
            tvTaskList.Refresh();
        }

        const long MaxB = 1024;
        const long MaxK = MaxB * 1024;
        const long MaxM = MaxK * 1024;
        const long MaxG = MaxM * 1024;

        public static string GetMemoryString(long value, string unit = "B")
        {
            string result = null;
            int sign = Math.Sign(value);

            value = Math.Abs(value);
            if (value < MaxB)
            {
                result = string.Format("{0}{1}", value, unit);
            }
            else if (value < MaxK)
            {
                result = string.Format("{0:N2}K{1}", (float)value / MaxB, unit);
            }
            else if (value < MaxM)
            {
                result = string.Format("{0:N2}M{1}", (float)value / MaxK, unit);
            }
            else if (value < MaxG)
            {
                result = string.Format("{0:N2}G{1}", (float)value / MaxM, unit);
            }
            if (sign < 0)
            {
                result = "-" + result;
            }
            return result;
        }

        private void DoFillChildFormInfo(Sample sampleNood, TreeGridNode treeNode)
        {
            treeNode.DefaultCellStyle.Font = boldFont;
            float totoalTime = (float)sampleNood.currentTime / 10000;
            treeNode.SetValues(null, sampleNood.name, GetMemoryString(sampleNood.costLuaGC), GetMemoryString(sampleNood.selfLuaGC), GetMemoryString(sampleNood.luaGC), (totoalTime / (float)sampleNood.calls).ToString("f2") + "ms", totoalTime.ToString("f2") + "ms", GetMemoryString(sampleNood.calls, ""));
            sampleNood.luaGC = 0;
            for (int i = 0, imax = sampleNood.childs.Count; i < imax; i++)
            {
                var item = sampleNood.childs[i];
                TreeGridNode node;
                if (!nodeDict.TryGetValue(item.fullName, out node))
                {
                    node = treeNode.Nodes.Add();
                    nodeDict.Add(item.fullName, node);
                }
                DoFillChildFormInfo(item, node);
            }
        }
        #endregion

        #region click
        public void OnProcessTextChange(object sender, EventArgs e)
        {
            string origin = processCom.Text;
            string text = processCom.Text.ToLower();
            processCom.Items.Clear();
            var pArray = Process.GetProcesses();
            for (int i = 0, imax = pArray.Length; i < imax; i++)
            {
                if (pArray[i].ProcessName.ToLower().Contains(text))
                {
                    processCom.Items.Add(pArray[i].ProcessName);
                }
            }
            processCom.DroppedDown = true;
            processCom.Text = origin;
            processCom.SelectionStart = processCom.Text.Length;
            //processCom.Show();
        }

        private void injectButton_Click(object sender, EventArgs e)
        {
            Process[] process = Process.GetProcessesByName(processCom.Text);
            if (process.Length > 0)
            {
                var p = Process.GetProcessById(process.FirstOrDefault().Id);
                if (p == null)
                {
                    MessageBox.Show("指定的进程不存在!");
                    return;
                }

                if (IsWin64Emulator(p.Id) != IsWin64Emulator(Process.GetCurrentProcess().Id))
                {
                    var currentPlat = IsWin64Emulator(Process.GetCurrentProcess().Id) ? 64 : 32;
                    var targetPlat = IsWin64Emulator(p.Id) ? 64 : 32;
                    MessageBox.Show(string.Format("当前程序是{0}位程序，目标进程是{1}位程序，请调整编译选项重新编译后重试！", currentPlat, targetPlat));
                    return;
                }

                if (!RegGACAssembly())
                {
                    return;
                }
                InstallHookInternal(p.Id);
            }
            else
            {
                MessageBox.Show("该进程不存在！");
            }
        }

        private void deattachBtn_Click(object sender, EventArgs e)
        {
            Thread.Sleep(1000);

            NativeAPI.GacUninstallAssemblies
            (
                new string[] { "HookLib.dll" }
                , "A simple ProcessMonitor based on EasyHook!",
                base64Str
            );
            Thread.Sleep(100);

            MessageBox.Show("已解除");
            injectButton.Enabled = true;
            deattachBtn.Enabled = false;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            ClearForm();
        }
        private void ClearForm()
        {
            queue.Clear();
            dict.Clear();
            nodeDict.Clear();
            roots.Clear();
            tvTaskList.Nodes.Clear();
        }
        #endregion

        #region GAC
        private static string dictPath = AppDomain.CurrentDomain.BaseDirectory;
        private string base64Str = GenBase64Str();

        public static string GenBase64Str()
        {
            Byte[] IdentData = new Byte[30];
            new RNGCryptoServiceProvider().GetBytes(IdentData);
            return Convert.ToBase64String(IdentData);
        }

        private bool RegGACAssembly()
        {
            if (!NativeAPI.RhIsAdministrator())
            {
                return false;
            }

            try
            {
                try
                {
                    NativeAPI.GacUninstallAssemblies
                    (
                        new string[] { "HookLib.dll" }
                        , "A simple ProcessMonitor based on EasyHook!",
                        base64Str
                    );
                    Thread.Sleep(100);
                }
                catch { }

                NativeAPI.GacInstallAssemblies
                (
                    new string[] { Path.Combine(dictPath, "EasyHook.dll"), Path.Combine(dictPath, "HookLib.dll") }
                    , "A simple ProcessMonitor based on EasyHook!",
                    base64Str
                );
                Thread.Sleep(100);
            }
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
                return false;
            }

            return true;
        }

        private bool InstallHookInternal(int processId)
        {
            try
            {
                var parameter = new HookParameter
                {
                    Msg = "已经成功注入目标进程",
                    HostProcessId = RemoteHooking.GetCurrentProcessId()
                };
                RemoteHooking.Inject(
                    processId,
                    InjectionOptions.Default,
                    typeof(HookParameter).Assembly.Location,
                    typeof(HookParameter).Assembly.Location,
                    string.Empty,
                    parameter
                );
            }
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
                return false;
            }
            injectButton.Enabled = false;
            deattachBtn.Enabled = true;
            return true;
        }

        private static bool IsWin64Emulator(int processId)
        {
            var process = Process.GetProcessById(processId);
            if (process == null)
                return false;

            if ((Environment.OSVersion.Version.Major > 5)
                || ((Environment.OSVersion.Version.Major == 5) && (Environment.OSVersion.Version.Minor >= 1)))
            {
                bool retVal;

                return !(IsWow64Process(process.Handle, out retVal) && retVal);
            }

            return false; // not on 64-bit Windows Emulator
        }

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsWow64Process([In] IntPtr process, [Out] out bool wow64Process);

        #endregion

        private void Timer1_Tick(object sender, EventArgs e)
        {
            lock (queue)
            {
                while (queue.Count > 0)
                {
                    Sample sample = queue.Dequeue();
                    Sample s;
                    if (dict.TryGetValue(sample.name, out s))
                    {
                        s.AddSample(sample);
                    }
                    else
                    {
                        dict.Add(sample.name, sample);
                        roots.Add(sample);
                    }
                }
                FillFormInfo();
            }
        }
    }
}