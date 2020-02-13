using MixItUp.Base.Services.External;
using MixItUp.Base.ViewModel.Controls.Services;
using MixItUp.WPF.Windows.OAuth;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for TiltifyServiceControl.xaml
    /// </summary>
    public partial class TiltifyServiceControl : ServiceControlBase
    {
        private TiltifyServiceControlViewModel viewModel;

        private string authorizationToken = null;
        private bool windowClosed = false;

        public TiltifyServiceControl()
        {
            this.DataContext = this.ViewModel = this.viewModel = new TiltifyServiceControlViewModel();

            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            await this.viewModel.OnLoaded();
        }

        private async void LogInButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            OAuthWebBrowserWindow oauthBrowser = new OAuthWebBrowserWindow(string.Format(TiltifyService.AuthorizationURL, TiltifyService.ClientID, TiltifyService.ListeningURL), TiltifyService.ListeningURL);
            oauthBrowser.Closed += OauthBrowser_Closed;
            oauthBrowser.OnTokenAcquired += OauthBrowser_OnTokenAcquired;
            oauthBrowser.Show();

            while (!this.windowClosed && string.IsNullOrEmpty(this.authorizationToken))
            {
                await Task.Delay(500);
            }

            if (!string.IsNullOrEmpty(this.authorizationToken))
            {
                await this.viewModel.LogIn(this.authorizationToken);
            }
            this.authorizationToken = null;
        }

        private void OauthBrowser_OnTokenAcquired(object sender, string e) { this.authorizationToken = e; }

        private void OauthBrowser_Closed(object sender, System.EventArgs e) { this.windowClosed = true; }
    }
}
