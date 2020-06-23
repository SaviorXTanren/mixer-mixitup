using MixItUp.Base.Services.External;
using System;
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

        private void OAuthWebBrowserWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Browser.Navigating += Browser_Navigating;
            this.Browser.Navigate(this.oauthURL);

            this.redirectPageText = OAuthExternalServiceBase.LoginRedirectPageHTML;
            this.redirectPageText = this.redirectPageText.Replace(LogInTextBoxPositionHTML, "left: 50%; top: 60%;");
        }

        private void Browser_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.Uri != null && e.Uri.AbsoluteUri.StartsWith(this.listenerURL))
            {
                this.Browser.NavigateToString(this.redirectPageText);

                string codeParameter = OAuthExternalServiceBase.DEFAULT_AUTHORIZATION_CODE_URL_PARAMETER + "=";
                if (e.Uri.AbsoluteUri.Contains(codeParameter))
                {
                    int startIndex = e.Uri.AbsoluteUri.IndexOf(codeParameter);

                    string token = e.Uri.AbsoluteUri.Substring(startIndex + codeParameter.Length);

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
