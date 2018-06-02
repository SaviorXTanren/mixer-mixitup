using Mixer.Base.Web;
using MixItUp.Base;
using MixItUp.Base.Services;
using System;
using System.IO;
using System.Windows;
using System.Windows.Navigation;

namespace MixItUp.WPF.Windows.OAuth
{
    /// <summary>
    /// Interaction logic for OAuthWebBrowserWindow.xaml
    /// </summary>
    public partial class OAuthWebBrowserWindow : Window
    {
        private const string LogInTextBoxPositionHTML = "left: 50%; top: 50%;";

        public event EventHandler<string> OnTokenAcquired = delegate { };

        private string oauthURL;
        private string listenerURL;

        private string redirectPageText;

        public OAuthWebBrowserWindow(string oauthURL, string listenerURL)
        {
            this.oauthURL = oauthURL;
            this.listenerURL = listenerURL;

            InitializeComponent();

            this.Loaded += OAuthWebBrowserWindow_Loaded;
        }

        private async void OAuthWebBrowserWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Browser.Navigating += Browser_Navigating;
            this.Browser.Navigate(this.oauthURL);

            this.redirectPageText = await ChannelSession.Services.FileService.ReadFile(Path.Combine(ChannelSession.Services.FileService.GetApplicationDirectory(), OAuthServiceBase.LoginRedirectPageFileName));
            this.redirectPageText = this.redirectPageText.Replace(LogInTextBoxPositionHTML, "left: 50%; top: 60%;");
        }

        private void Browser_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.Uri != null && e.Uri.AbsoluteUri.StartsWith(this.listenerURL))
            {
                this.Browser.NavigateToString(this.redirectPageText);

                if (e.Uri.AbsoluteUri.Contains(OAuthHttpListenerServer.URL_CODE_IDENTIFIER))
                {
                    int startIndex = e.Uri.AbsoluteUri.IndexOf(OAuthHttpListenerServer.URL_CODE_IDENTIFIER);

                    string token = e.Uri.AbsoluteUri.Substring(startIndex + OAuthHttpListenerServer.URL_CODE_IDENTIFIER.Length);

                    int endIndex = token.IndexOf("&");
                    if (endIndex > 0)
                    {
                        token = token.Substring(0, endIndex);
                    }

                    this.OnTokenAcquired(this, token);
                }
            }
        }
    }
}
