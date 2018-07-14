using MixItUp.Base;
using MixItUp.Desktop.Services;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.OAuth;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for TipeeeStreamServiceControl.xaml
    /// </summary>
    public partial class TipeeeStreamServiceControl : ServicesControlBase
    {
        private string authorizationToken = null;
        private bool windowClosed = false;

        public TipeeeStreamServiceControl()
        {
            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            this.SetHeaderText("Tipeee Stream");

            if (ChannelSession.Settings.TipeeeStreamOAuthToken != null)
            {
                this.ExistingAccountGrid.Visibility = Visibility.Visible;

                this.SetCompletedIcon(visible: true);
            }
            else
            {
                this.NewLoginGrid.Visibility = Visibility.Visible;
            }

            await base.OnLoaded();
        }

        private async void LogInButton_Click(object sender, RoutedEventArgs e)
        {
            authorizationToken = null;
            windowClosed = false;

            await this.groupBoxControl.window.RunAsyncOperation(async () =>
            {
                OAuthWebBrowserWindow oauthBrowser = new OAuthWebBrowserWindow(string.Format(TipeeeStreamService.AuthorizationURL, TipeeeStreamService.ClientID, TipeeeStreamService.ListeningURL), TipeeeStreamService.ListeningURL);
                oauthBrowser.Closed += OauthBrowser_Closed;
                oauthBrowser.OnTokenAcquired += OauthBrowser_OnTokenAcquired;
                oauthBrowser.Show();

                while (!this.windowClosed && string.IsNullOrEmpty(this.authorizationToken))
                {
                    await Task.Delay(500);
                }

                if (!string.IsNullOrEmpty(this.authorizationToken))
                {
                    await ChannelSession.Services.InitializeTipeeeStream(authorizationToken);
                }
            });

            if (ChannelSession.Services.TipeeeStream == null)
            {
                await MessageBoxHelper.ShowMessageDialog("Unable to authenticate with Tipeee Stream. Please ensure you approved access for the application in a timely manner.");
            }
            else
            {
                this.NewLoginGrid.Visibility = Visibility.Collapsed;
                this.ExistingAccountGrid.Visibility = Visibility.Visible;

                this.SetCompletedIcon(visible: true);
            }
        }

        private async void LogOutButton_Click(object sender, RoutedEventArgs e)
        {
            await this.groupBoxControl.window.RunAsyncOperation(async () =>
            {
                await ChannelSession.Services.DisconnectTipeeeStream();
            });
            ChannelSession.Settings.TipeeeStreamOAuthToken = null;

            this.ExistingAccountGrid.Visibility = Visibility.Collapsed;
            this.NewLoginGrid.Visibility = Visibility.Visible;

            this.SetCompletedIcon(visible: false);
        }

        private void OauthBrowser_OnTokenAcquired(object sender, string e) { this.authorizationToken = e; }

        private void OauthBrowser_Closed(object sender, System.EventArgs e) { this.windowClosed = true; }
    }
}
