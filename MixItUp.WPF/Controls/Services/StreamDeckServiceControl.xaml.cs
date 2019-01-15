using Microsoft.Win32;
using MixItUp.Base;
using MixItUp.WPF.Util;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace MixItUp.WPF.Controls.Services
{
    public partial class StreamDeckServiceControl : ServicesControlBase
    {
        public StreamDeckServiceControl()
        {
            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            this.SetHeaderText("Stream Deck");
            await base.OnLoaded();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start("https://gc-updates.elgato.com/windows/sd-update/final/download-website.php");
        }

        private async void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            await this.groupBoxControl.window.RunAsyncOperation(async () =>
            {
                string installDir = GetStreamDeckInstallationDirectory();
                if (string.IsNullOrEmpty(installDir))
                {
                    await MessageBoxHelper.ShowMessageDialog("Stream Deck doesn't appear to be installed correctly.");
                    return;
                }

                string streamDeckExe = Path.Combine(installDir, "StreamDeck.exe");
                if (!File.Exists(streamDeckExe))
                {
                    await MessageBoxHelper.ShowMessageDialog("Could not find the Stream Deck application in the installation directory.");
                    return;
                }

                FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(streamDeckExe);
                if (versionInfo.FileMajorPart < 4)
                {
                    await MessageBoxHelper.ShowMessageDialog("Stream Deck is installed, but is not v4. Please update Stream Deck to v4.");
                    return;
                }

                if (!ChannelSession.Settings.EnableDeveloperAPI)
                {
                    await MessageBoxHelper.ShowMessageDialog("Please enable the Developer APIs before installing the Stream Deck plug-in.");
                    return;
                }

                Process.Start("com.mixitup.streamdeckplugin.streamDeckPlugin");
            });
        }

        private string GetStreamDeckInstallationDirectory()
        {
            return Registry.GetValue(@"HKEY_CURRENT_USER\Software\Elgato Systems GmbH\StreamDeck", "InstallDir", null) as string;
        }
    }
}
