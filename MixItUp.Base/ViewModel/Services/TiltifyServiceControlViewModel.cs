using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
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
                    ChannelSession.Settings.TiltifyCampaignV5 = this.SelectedCampaign.id;
                }
                else
                {
                    ChannelSession.Settings.TiltifyCampaignV5 = null;
                }
            }
        }
        private TiltifyCampaign selectedCampaign;

        public ICommand LogInCommand { get; set; }
        public ICommand LogOutCommand { get; set; }

        public override string WikiPageName { get { return "tiltify"; } }

        public TiltifyServiceControlViewModel()
            : base(Resources.Tiltify)
        {
            this.LogInCommand = this.CreateCommand(async () =>
            {
                Result result = await ServiceManager.Get<TiltifyService>().Connect();
                if (result.Success)
                {
                    this.IsConnected = true;
                    await this.RefreshCampaigns();
                }
                else
                {
                    await this.ShowConnectFailureMessage(result);
                }
            });

            this.LogOutCommand = this.CreateCommand(async () =>
            {
                await ServiceManager.Get<TiltifyService>().Disconnect();

                ChannelSession.Settings.TiltifyOAuthToken = null;
                ChannelSession.Settings.TiltifyCampaignV5 = null;

                this.IsConnected = false;
            });

            this.IsConnected = ServiceManager.Get<TiltifyService>().IsConnected;
        }

        protected override async Task OnOpenInternal()
        {
            if (this.IsConnected)
            {
                await this.RefreshCampaigns();
                this.SelectedCampaign = this.Campaigns.FirstOrDefault(c => string.Equals(c.id, ChannelSession.Settings.TiltifyCampaignV5));
            }
        }

        public async Task RefreshCampaigns()
        {
            TiltifyUser user = await ServiceManager.Get<TiltifyService>().GetUser();

            Dictionary<string, TiltifyCampaign> campaignDictionary = new Dictionary<string, TiltifyCampaign>();

            foreach (TiltifyCampaign campaign in await ServiceManager.Get<TiltifyService>().GetUserCampaigns(user))
            {
                campaignDictionary[campaign.id] = campaign;
            }

            foreach (TiltifyTeam team in await ServiceManager.Get<TiltifyService>().GetUserTeams(user))
            {
                foreach (TiltifyCampaign campaign in await ServiceManager.Get<TiltifyService>().GetTeamCampaigns(team))
                {
                    campaignDictionary[campaign.id] = campaign;
                }
            }

            TiltifyCampaign activeCampaign = await ServiceManager.Get<TiltifyService>().GetCampaign("a37eb5be-b18e-4e76-b436-2e153bc4fbfb");

            TiltifyCampaign completedCampaign = await ServiceManager.Get<TiltifyService>().GetCampaign("ac0a6017-52b4-485e-9aba-e5da8348f01e");

            //this.Campaigns.ClearAndAddRange(campaignDictionary.Values.Where(c => c.Ends > DateTimeOffset.Now));
        }
    }
}
