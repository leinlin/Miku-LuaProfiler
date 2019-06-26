using System;

namespace HookLib
{
    public class HookServer : MarshalByRefObject
    {
        public bool isHook = false;
        public bool IsInstalled()
        {
            return isHook;
        }
        public void ReportMessage(string message)
        {
            Console.WriteLine(message);
        }
    }
}
