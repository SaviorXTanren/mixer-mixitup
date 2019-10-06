using MixItUp.Base;
using MixItUp.Base.Services;
using MixItUp.Desktop.Services;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.OAuth;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for TiltifyServiceControl.xaml
    /// </summary>
    public partial class TiltifyServiceControl : ServicesControlBase
    {
        private ObservableCollection<TiltifyCampaign> campaigns = new ObservableCollection<TiltifyCampaign>();

        private string authorizationToken = null;
        private bool windowClosed = false;

        public TiltifyServiceControl()
        {
            InitializeComponent();

            this.CampaignComboBox.ItemsSource = this.campaigns;
        }

        protected override async Task OnLoaded()
        {
            this.SetHeaderText("Tiltify");

            if (ChannelSession.Settings.TiltifyOAuthToken != null)
            {
                this.ExistingAccountGrid.Visibility = Visibility.Visible;

                if (ChannelSession.Settings.TiltifyCampaign > 0)
                {
                    await this.RefreshCampaigns();
                }

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
                    await ChannelSession.Services.InitializeTiltify(authorizationToken);
                }
            });

            if (ChannelSession.Services.Tiltify == null)
            {
                await MessageBoxHelper.ShowMessageDialog("Unable to authenticate with Tiltify. Please ensure you approved access for the application in a timely manner.");
            }
            else
            {
                await this.RefreshCampaigns();

                this.NewLoginGrid.Visibility = Visibility.Collapsed;
                this.ExistingAccountGrid.Visibility = Visibility.Visible;

                this.SetCompletedIcon(visible: true);
            }
        }

        private async void LogOutButton_Click(object sender, RoutedEventArgs e)
        {
            await this.groupBoxControl.window.RunAsyncOperation(async () =>
            {
                await ChannelSession.Services.DisconnectTiltify();
            });
            ChannelSession.Settings.TiltifyOAuthToken = null;

            this.ExistingAccountGrid.Visibility = Visibility.Collapsed;
            this.NewLoginGrid.Visibility = Visibility.Visible;

            this.SetCompletedIcon(visible: false);
        }

        private void OauthBrowser_OnTokenAcquired(object sender, string e) { this.authorizationToken = e; }

        private void OauthBrowser_Closed(object sender, System.EventArgs e) { this.windowClosed = true; }

        private async Task RefreshCampaigns()
        {
            this.campaigns.Clear();

            TiltifyUser user = await ChannelSession.Services.Tiltify.GetUser();

            Dictionary<int, TiltifyCampaign> campaignDictionary = new Dictionary<int, TiltifyCampaign>();

            foreach (TiltifyCampaign campaign in await ChannelSession.Services.Tiltify.GetUserCampaigns(user))
            {
                campaignDictionary[campaign.ID] = campaign;
            }

            foreach (TiltifyTeam team in await ChannelSession.Services.Tiltify.GetUserTeams(user))
            {
                foreach (TiltifyCampaign campaign in await ChannelSession.Services.Tiltify.GetTeamCampaigns(team))
                {
                    campaignDictionary[campaign.ID] = campaign;
                }
            }

            foreach (TiltifyCampaign campaign in campaignDictionary.Values)
            {
                if (campaign.Ends > DateTimeOffset.Now)
                {
                    this.campaigns.Add(campaign);
                }
            }
        }

        private void CampaignComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.CampaignComboBox.SelectedIndex >= 0)
            {
                TiltifyCampaign campaign = (TiltifyCampaign)this.CampaignComboBox.SelectedItem;
                ChannelSession.Settings.TiltifyCampaign = campaign.ID;
            }
        }
    }
}
