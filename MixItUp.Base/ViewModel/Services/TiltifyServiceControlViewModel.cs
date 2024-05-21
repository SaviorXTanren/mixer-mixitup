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
                    ChannelSession.Settings.TiltifyCampaignV5IsTeam = this.SelectedCampaign.IsPartOfTeam;
                }
                else
                {
                    ChannelSession.Settings.TiltifyCampaignV5 = null;
                    ChannelSession.Settings.TiltifyCampaignV5IsTeam = false;
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
                ChannelSession.Settings.TiltifyCampaignV5IsTeam = false;

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
            this.Campaigns.Clear();

            TiltifyUser user = await ServiceManager.Get<TiltifyService>().GetUser();
            if (user != null)
            {
                Dictionary<string, TiltifyCampaign> campaignDictionary = new Dictionary<string, TiltifyCampaign>();

                foreach (TiltifyCampaign campaign in await ServiceManager.Get<TiltifyService>().GetUserCampaigns(user))
                {
                    if (!campaign.IsRetired)
                    {
                        campaignDictionary[campaign.id] = campaign;
                    }
                }

                foreach (TiltifyTeam team in await ServiceManager.Get<TiltifyService>().GetUserTeams(user))
                {
                    foreach (TiltifyCampaign campaign in await ServiceManager.Get<TiltifyService>().GetTeamCampaigns(team))
                    {
                        if (!campaign.IsRetired)
                        {
                            campaignDictionary[campaign.id] = campaign;
                        }
                    }
                }

                foreach (var kvp in campaignDictionary)
                {
                    this.Campaigns.Add(kvp.Value);
                }
            }
        }
    }
}
