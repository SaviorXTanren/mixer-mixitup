using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
{
    public class PatreonServiceControlViewModel : ServiceControlViewModelBase
    {
        public ObservableCollection<PatreonTier> Tiers { get; set; } = new ObservableCollection<PatreonTier>();

        public PatreonTier SelectedTier
        {
            get { return this.selectedTier; }
            set
            {
                this.selectedTier = value;
                this.NotifyPropertyChanged();

                if (this.SelectedTier != null)
                {
                    ChannelSession.Settings.PatreonTierSubscriberEquivalent = this.SelectedTier.ID;
                }
                else
                {
                    ChannelSession.Settings.PatreonTierSubscriberEquivalent = null;
                }
            }
        }
        private PatreonTier selectedTier;

        public ICommand LogInCommand { get; set; }
        public ICommand LogOutCommand { get; set; }

        public override string WikiPageName { get { return "patreon"; } }

        public PatreonServiceControlViewModel()
            : base(Resources.Patreon)
        {
            this.LogInCommand = this.CreateCommand(async () =>
            {
                Result result = await ServiceManager.Get<PatreonService>().Connect();
                if (result.Success)
                {
                    this.IsConnected = true;
                    this.RefreshTiers();
                }
                else
                {
                    await this.ShowConnectFailureMessage(result);
                }
            });

            this.LogOutCommand = this.CreateCommand(async () =>
            {
                await ServiceManager.Get<PatreonService>().Disconnect();

                ChannelSession.Settings.PatreonOAuthToken = null;
                ChannelSession.Settings.PatreonTierSubscriberEquivalent = null;

                this.IsConnected = false;
            });

            this.IsConnected = ServiceManager.Get<PatreonService>().IsConnected;
        }

        protected override Task OnOpenInternal()
        {
            if (this.IsConnected)
            {
                this.RefreshTiers();
                this.SelectedTier = this.Tiers.FirstOrDefault(t => t.ID.Equals(ChannelSession.Settings.PatreonTierSubscriberEquivalent));
            }
            return Task.CompletedTask;
        }

        public void RefreshTiers()
        {
            List<PatreonTier> tiers = new List<PatreonTier>();
            if (ServiceManager.Get<PatreonService>().Campaign != null && ServiceManager.Get<PatreonService>().Campaign.ActiveTiers != null)
            {
                tiers.AddRange(ServiceManager.Get<PatreonService>().Campaign.ActiveTiers);
            }
            this.Tiers.ClearAndAddRange(tiers);
        }
    }
}
