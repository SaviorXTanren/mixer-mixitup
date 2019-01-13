using MixItUp.Base;
using MixItUp.WPF.Util;
using System.Diagnostics;
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
            await this.groupBoxControl.window.RunAsyncOperation(() =>
            {
                Process.Start("com.mixitup.streamdeckplugin.streamDeckPlugin");
                return Task.FromResult(0);
            });
        }
    }
}
