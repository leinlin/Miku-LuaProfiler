using System;

namespace MikuLuaProfiler_Winform
{
    public class HookServer : MarshalByRefObject
    {
        public bool isHook = false;
        public bool newstate = false;
        public bool discardInvalid = true;
        public bool IsInstalled()
        {
            return isHook;
        }

        public void Deattach()
        {
            isHook = false;
        }
        public void ReportMessage(string message)
        {
            Console.WriteLine(message);
        }
    }
}
