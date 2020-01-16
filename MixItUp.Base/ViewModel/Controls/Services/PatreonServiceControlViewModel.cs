using MixItUp.Base.Services.External;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.Services
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
                ExternalServiceResult result = await ChannelSession.Services.Patreon.Connect();
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
                await ChannelSession.Services.Patreon.Disconnect();
                this.IsConnected = false;
            });

            this.IsConnected = ChannelSession.Services.Patreon.IsConnected;
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
            foreach (PatreonTier tier in ChannelSession.Services.Patreon.Campaign.ActiveTiers)
            {
                this.Tiers.Add(tier);
            }
        }
    }
}
