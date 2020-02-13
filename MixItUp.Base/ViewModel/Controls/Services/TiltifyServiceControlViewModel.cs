using MixItUp.Base.Services.External;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.Services
{
    public class TiltifyServiceControlViewModel : ServiceControlViewModelBase
    {
        public ObservableCollection<TiltifyCampaign> Campaigns { get; set; } = new ObservableCollection<TiltifyCampaign>();

        public TiltifyCampaign SelectedCampaign
        {
            get { return this.selectedCampaign; }
            set
            {
                this.selectedCampaign = value;
                this.NotifyPropertyChanged();

                if (this.SelectedCampaign != null)
                {
                    ChannelSession.Settings.TiltifyCampaign = this.SelectedCampaign.ID;
                }
                else
                {
                    ChannelSession.Settings.TiltifyCampaign = 0;
                }
            }
        }
        private TiltifyCampaign selectedCampaign;

        public ICommand LogOutCommand { get; set; }

        public TiltifyServiceControlViewModel()
            : base("Tiltify")
        {
            this.LogOutCommand = this.CreateCommand(async (parameter) =>
            {
                await ChannelSession.Services.Tiltify.Disconnect();
                this.IsConnected = false;
            });

            this.IsConnected = ChannelSession.Services.Tiltify.IsConnected;
        }

        protected override async Task OnLoadedInternal()
        {
            if (this.IsConnected)
            {
                await this.RefreshCampaigns();
                this.SelectedCampaign = this.Campaigns.FirstOrDefault(c => c.ID == ChannelSession.Settings.TiltifyCampaign);
            }
        }

        public async Task LogIn(string authorizationToken)
        {
            ExternalServiceResult result = await ChannelSession.Services.Tiltify.Connect(authorizationToken);
            if (result.Success)
            {
                this.IsConnected = true;
                await this.RefreshCampaigns();
            }
            else
            {
                await this.ShowConnectFailureMessage(result);
            }
        }

        public async Task RefreshCampaigns()
        {
            this.Campaigns.Clear();

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
                    this.Campaigns.Add(campaign);
                }
            }
        }
    }
}
