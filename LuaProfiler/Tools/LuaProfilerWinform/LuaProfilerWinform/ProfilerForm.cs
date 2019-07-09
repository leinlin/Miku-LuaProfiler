using AdvancedDataGridView;
using System;
using System.Drawing;
using System.Windows.Forms;
using HookLib;
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
            SetStyle();
        }

        protected override void OnClosed(EventArgs e)
        {
            if (serverInterface != null)
            {
                serverInterface.isHook = false;
            }
            Thread.Sleep(1000);
            base.OnClosed(e);
        }

        private void SetStyle()
        {
            attachmentColumn.DefaultCellStyle.NullValue = null;

            FillFormInfo();
        }

        private void FillFormInfo()
        {
            Font boldFont = new Font(tvTaskList.DefaultCellStyle.Font, FontStyle.Bold);

            TreeGridNode node = tvTaskList.Nodes.Add(null, "函数1", "100k", "100k", "0B", "0ms", "0ms", "0ms", 100, 1);
            node.DefaultCellStyle.Font = boldFont;
            node = node.Nodes.Add(null, "函数2", "100k", "100k", "0B", "0ms", "0ms", "0ms", 100, 1);
            node = node.Parent.Nodes.Add(null, "函数3", "100k", "100k", "0B", "0ms", "0ms", "0ms", 100, 1);
            node = node.Parent.Nodes.Add(null, "函数4", "100k", "100k", "0B", "0ms", "0ms", "0ms", 100, 1);
            var node5 = node.Parent.Nodes.Add(null, "函数5", "100k", "100k", "0B", "0ms", "0ms", "0ms", 100, 1);
            for (int i = 6, imax = 1000; i < imax; i++)
            {
                node = node5.Nodes.Add(null, "函数" + i, "100k", "100k", "0B", "0ms", "0ms", "0ms", 100, 1);
            }
        }

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

        #region inject
        #region filed
        private HookServer serverInterface;
        private static string dictPath = AppDomain.CurrentDomain.BaseDirectory;
        private string base64Str = GenBase64Str();
        #endregion

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
                        new string[] {  "HookLib.dll" }
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
            catch(Exception ex)
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

                serverInterface = new HookServer();
                string channelName = null;
                RemoteHooking.IpcCreateServer<HookServer>(ref channelName, System.Runtime.Remoting.WellKnownObjectMode.Singleton, serverInterface);

                RemoteHooking.Inject(
                    processId,
                    InjectionOptions.Default,
                    typeof(HookParameter).Assembly.Location,
                    typeof(HookParameter).Assembly.Location,
                    channelName,
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

        private void deattachBtn_Click(object sender, EventArgs e)
        {
            serverInterface.Deattach();
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
            if (serverInterface != null && serverInterface.newstate)
            {
                MessageBox.Show("已经注入到程序中了！");
            }
            else
            {
                MessageBox.Show("没有注入！");
            }
        }
    }
}