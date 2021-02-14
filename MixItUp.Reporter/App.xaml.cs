using MixItUp.Base.Model;
using System.Windows;

namespace MixItUp.Reporter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string LogFilePath { get; private set; }

        public static StreamingPlatformTypeEnum PlatformType { get; private set; }
        public static string UserID { get; private set; }
        public static string Username { get; private set; }

        public App()
        {
            // NOTE: Uncomment the lines below to test other cultures
            //System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("de-DE");
            //System.Threading.Thread.CurrentThread.CurrentCulture = ci;
            //System.Threading.Thread.CurrentThread.CurrentUICulture = ci;
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length >= 1)
            {
                App.LogFilePath = e.Args[0];
            }

            if (e.Args.Length >= 2 && int.TryParse(e.Args[1], out int platformID))
            {
                App.PlatformType = (StreamingPlatformTypeEnum)platformID;
            }

            if (e.Args.Length >= 3)
            {
                App.UserID = e.Args[2];
            }

            if (e.Args.Length >= 4)
            {
                App.Username = e.Args[3];
            }
        }
    }
}
