using MixItUp.Base.Util;
using System;
using System.Diagnostics;

namespace MixItUp.WPF
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                Process[] processes = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
                if (processes.Length > 1)
                {
                    ActivationProtocolHandler.SendRequest(args);
                    return;
                }
            }

            App application = new App();
            application.InitializeComponent();
            application.Run();
        }
    }
}
