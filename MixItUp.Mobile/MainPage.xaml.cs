using Mixer.Base;
using MixItUp.Base;
using System;
using Xamarin.Forms;

namespace MixItUp.Mobile
{
    public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();
		}

        private void StreamerButton_Clicked(object sender, EventArgs e)
        {

        }

        private async void ModeratorButton_Clicked(object sender, EventArgs e)
        {
            this.LoginWebView.IsVisible = true;
            this.LoginButtonsStack.IsVisible = false;

            this.LoginWebView.Source = await MixerConnection.GetAuthorizationCodeURLForOAuthBrowser(ChannelSession.ClientID, ChannelSession.ModeratorScopes, MixerConnection.OAUTH_LOCALHOST_URL);
            //await ChannelSession.ConnectUser(ChannelSession.ModeratorScopes, "SaviorXTanren");
        }

        private void RemoteButton_Clicked(object sender, EventArgs e)
        {

        }
    }
}
