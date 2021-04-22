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
        public ThreadSafeObservableCollection<PatreonTier> Tiers { get; set; } = new ThreadSafeObservableCollection<PatreonTier>();

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

        public PatreonServiceControlViewModel()
            : base(Resources.Patreon)
        {
            this.LogInCommand = this.CreateCommand(async () =>
            {
                Result result = await ChannelSession.Services.Patreon.Connect();
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
                await ChannelSession.Services.Patreon.Disconnect();

                ChannelSession.Settings.PatreonOAuthToken = null;
                ChannelSession.Settings.PatreonTierSubscriberEquivalent = null;

                this.IsConnected = false;
            });

            this.IsConnected = ChannelSession.Services.Patreon.IsConnected;
        }

        protected override Task OnLoadedInternal()
        {
            if (this.IsConnected)
            {
                this.RefreshTiers();
                this.SelectedTier = this.Tiers.FirstOrDefault(t => t.ID.Equals(ChannelSession.Settings.PatreonTierSubscriberEquivalent));
            }
            return Task.FromResult(0);
        }

        public void RefreshTiers()
        {
            List<PatreonTier> tiers = new List<PatreonTier>();
            if (ChannelSession.Services.Patreon.Campaign != null && ChannelSession.Services.Patreon.Campaign.ActiveTiers != null)
            {
                tiers.AddRange(ChannelSession.Services.Patreon.Campaign.ActiveTiers);
            }
            this.Tiers.ClearAndAddRange(tiers);
        }
    }
}
