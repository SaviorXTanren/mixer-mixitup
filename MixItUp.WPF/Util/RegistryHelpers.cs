using Microsoft.Win32;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Commands;
using StreamingClient.Base.Util;
using System;
using System.Runtime.InteropServices;

namespace MixItUp.WPF.Util
{
    public static class RegistryHelpers
    {
        private const string SoftwareClassesRegistryPathPrefx = "SOFTWARE\\Classes\\";

        private const long SHCNE_ASSOCCHANGED = 0x08000000L;
        private const uint SHCNF_IDLIST = 0x0000;

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern void SHChangeNotify(int wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

        public static void RegisterFileAssociation()
        {
            try
            {
                if (!RegistryHelpers.KeyExists(SoftwareClassesRegistryPathPrefx + CommandEditorWindowViewModelBase.MixItUpCommandFileExtension))
                {
                    using (var key = Registry.CurrentUser.CreateSubKey(SoftwareClassesRegistryPathPrefx + ActivationProtocolHandler.FileAssociationProgramID))
                    {
                        string applicationLocation = typeof(App).Assembly.Location;

                        key.SetValue("", "Mix It Up");

                        using (var currentVersion = key.CreateSubKey("CurVer"))
                        {
                            currentVersion.SetValue("", ActivationProtocolHandler.FileAssociationProgramID);
                        }

                        using (var defaultIcon = key.CreateSubKey("DefaultIcon"))
                        {
                            defaultIcon.SetValue("", $"{applicationLocation},0");
                        }

                        using (var commandKey = key.CreateSubKey(@"shell\open\command"))
                        {
                            commandKey.SetValue("", "\"" + applicationLocation + "\" \"%1\"");
                        }
                    }

                    using (var key = Registry.CurrentUser.CreateSubKey(SoftwareClassesRegistryPathPrefx + CommandEditorWindowViewModelBase.MixItUpCommandFileExtension))
                    {
                        key.SetValue("", ActivationProtocolHandler.FileAssociationProgramID);
                    }

                    SHChangeNotify((int)SHCNE_ASSOCCHANGED, (int)SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        public static void RegisterURIActivationProtocol()
        {
            try
            {
                if (!RegistryHelpers.KeyExists(SoftwareClassesRegistryPathPrefx + ActivationProtocolHandler.URIProtocolActivationHeader))
                {
                    using (var key = Registry.CurrentUser.CreateSubKey(SoftwareClassesRegistryPathPrefx + ActivationProtocolHandler.URIProtocolActivationHeader))
                    {
                        string applicationLocation = typeof(App).Assembly.Location;

                        key.SetValue("", "URL:Mix It Up");
                        key.SetValue("URL Protocol", "");

                        using (var defaultIcon = key.CreateSubKey("DefaultIcon"))
                        {
                            defaultIcon.SetValue("", $"{applicationLocation},0");
                        }

                        using (var commandKey = key.CreateSubKey(@"shell\open\command"))
                        {
                            commandKey.SetValue("", "\"" + applicationLocation + "\" \"%1\"");
                        }
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        public static bool KeyExists(string path) { return Registry.CurrentUser.OpenSubKey(path) != null; }
    }
}
