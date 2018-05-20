using System.IO;
using System.Windows;

namespace MixItUp.Reporter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static uint MixerUserID { get; private set; }
        public static string LogFilePath { get; private set; }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length >= 1)
            {
                if (uint.TryParse(e.Args[0], out uint id))
                {
                    App.MixerUserID = id;
                }
            }

            if (e.Args.Length >= 2)
            {
                App.LogFilePath = e.Args[1];
            }
        }
    }
}
