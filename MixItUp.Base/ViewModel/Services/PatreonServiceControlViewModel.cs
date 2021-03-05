using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
{
    public class PatreonServiceControlViewModel : ServiceControlViewModelBase
    {
        public ObservableCollection<PatreonTier> Tiers { get; set; } = new ObservableCollection<PatreonTier>().EnableSync();

        public PatreonTier SelectedTier
        {
            get { return this.selectedTier; }
            set
            {
                this.selectedTier = value;
                this.NotifyPropertyChanged();

                if (this.SelectedTier != null)
                {
                    ChannelSession.Settings.PatreonTierMixerSubscriberEquivalent = this.SelectedTier.ID;
                }
                else
                {
                    ChannelSession.Settings.PatreonTierMixerSubscriberEquivalent = null;
                }
            }
        }
        private PatreonTier selectedTier;

        public ICommand LogInCommand { get; set; }
        public ICommand LogOutCommand { get; set; }

        public PatreonServiceControlViewModel()
            : base("Patreon")
        {
            this.LogInCommand = this.CreateCommand(async (parameter) =>
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

            this.LogOutCommand = this.CreateCommand(async (parameter) =>
            {
                await ServiceManager.Get<PatreonService>().Disconnect();

                ChannelSession.Settings.PatreonOAuthToken = null;
                ChannelSession.Settings.PatreonTierMixerSubscriberEquivalent = null;

                this.IsConnected = false;
            });

            this.IsConnected = ServiceManager.Get<PatreonService>().IsConnected;
        }

        protected override Task OnLoadedInternal()
        {
            if (this.IsConnected)
            {
                this.RefreshTiers();
                this.SelectedTier = this.Tiers.FirstOrDefault(t => t.ID.Equals(ChannelSession.Settings.PatreonTierMixerSubscriberEquivalent));
            }
            return Task.FromResult(0);
        }

        public void RefreshTiers()
        {
            this.Tiers.Clear();
            if (ServiceManager.Get<PatreonService>().Campaign != null && ServiceManager.Get<PatreonService>().Campaign.ActiveTiers != null)
            {
                foreach (PatreonTier tier in ServiceManager.Get<PatreonService>().Campaign.ActiveTiers)
                {
                    this.Tiers.Add(tier);
                }
            }
        }
    }
}
