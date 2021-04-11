using Microsoft.Win32;
using MixItUp.Base.Util;
using System.IO;

namespace MixItUp.WPF.Util
{
    public static class RegistryHelpers
    {
        public static void RegisterURIActivationProtocol()
        {
            using (var key = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Classes\\" + ActivationProtocolHandler.URIProtocolActivationHeader))
            {
                // Replace typeof(App) by the class that contains the Main method or any class located in the project that produces the exe.
                // or replace typeof(App).Assembly.Location by anything that gives the full path to the exe
                string applicationLocation = typeof(App).Assembly.Location;
                string applicationFolderLocation = Path.GetDirectoryName(applicationLocation);

                key.SetValue("", "URL:Mix It Up");
                key.SetValue("URL Protocol", "");

                using (var defaultIcon = key.CreateSubKey("DefaultIcon"))
                {
                    defaultIcon.SetValue("", Path.Combine(applicationFolderLocation, "Logo.ico"));
                }

                using (var commandKey = key.CreateSubKey(@"shell\open\command"))
                {
                    commandKey.SetValue("", "\"" + applicationLocation + "\" \"%1\"");
                }
            }
        }
    }
}
