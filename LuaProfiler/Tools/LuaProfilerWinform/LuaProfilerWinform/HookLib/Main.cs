using System;
using System.Text;
using System.Runtime.InteropServices;
using EasyHook;
using System.Threading;
using System.Windows.Forms;

namespace HookLib
{
    [Serializable]
    public class HookParameter
    {
        public string Msg { get; set; }
        public int HostProcessId { get; set; }
    }

    public class Main : IEntryPoint
    {
        #region field
        HookServer _server = null;
        public LocalHook MessageBoxWHook = null;
        public LocalHook MessageBoxAHook = null;
        #endregion

        #region instance
        private static Main _instance;
        public static Main GetInstance()
        {
            return _instance;
        }
        #endregion

        public void Uninstall()
        {
            if (MessageBoxWHook != null)
            {
                MessageBoxWHook.Dispose();
                MessageBoxWHook = null;
            }
            if (MessageBoxAHook != null)
            {
                MessageBoxAHook.Dispose();
                MessageBoxAHook = null;
            }
        }

        public Main(
            RemoteHooking.IContext context,
            string channelName
            , HookParameter parameter
            )
        {
            _server = RemoteHooking.IpcConnectClient<HookServer>(channelName);
            _instance = this;
            _server.isHook = false;
        }

        public void Run(
            RemoteHooking.IContext context,
            string channelName
            , HookParameter parameter
            )
        {
            try
            {
                MessageBox.Show(parameter.Msg);
                MessageBoxWHook = LocalHook.Create(
                    LocalHook.GetProcAddress("user32.dll", "MessageBoxW"),
                    new DMessageBoxW(MessageBoxW_Hooked),
                    this);
                MessageBoxWHook.ThreadACL.SetExclusiveACL(new int[1]);

                MessageBoxAHook = LocalHook.Create(
                    LocalHook.GetProcAddress("user32.dll", "MessageBoxA"),
                    new DMessageBoxW(MessageBoxA_Hooked),
                    this);
                MessageBoxAHook.ThreadACL.SetExclusiveACL(new int[1]);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            _server.isHook = true;
            try
            {
                while (_server.isHook)
                {
                    Thread.Sleep(10);
                }
            }
            catch
            {
            }
            Uninstall();
        }

        #region MessageBoxW

        [DllImport("user32.dll", EntryPoint = "MessageBoxW", CharSet = CharSet.Unicode)]
        public static extern IntPtr MessageBoxW(int hWnd, string text, string caption, uint type);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        delegate IntPtr DMessageBoxW(int hWnd, string text, string caption, uint type);

        static IntPtr MessageBoxW_Hooked(int hWnd, string text, string caption, uint type)
        {
            return MessageBoxW(hWnd, "已注入-" + text, "已注入-" + caption, type);
        }

        #endregion
        
        #region MessageBoxA

        [DllImport("user32.dll", EntryPoint = "MessageBoxA", CharSet = CharSet.Ansi)]
        public static extern IntPtr MessageBoxA(int hWnd, string text, string caption, uint type);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        delegate IntPtr DMessageBoxA(int hWnd, string text, string caption, uint type);

        static IntPtr MessageBoxA_Hooked(int hWnd, string text, string caption, uint type)
        {
            return MessageBoxA(hWnd, "已注入-" + text, "已注入-" + caption, type);
        }

        #endregion
    }
}
