using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System.Linq;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
{
    public class MicrosoftAzureSpeechServiceControlViewModel : ServiceControlViewModelBase
    {
        public string RegionName
        {
            get { return this.regionName; }
            set
            {
                this.regionName = value;
                this.NotifyPropertyChanged();
            }
        }
        private string regionName;

        public string SubscriptionKey
        {
            get { return this.subscriptionKey; }
            set
            {
                this.subscriptionKey = value;
                this.NotifyPropertyChanged();
            }
        }
        private string subscriptionKey;

        public ICommand LogInCommand { get; set; }
        public ICommand LogOutCommand { get; set; }

        public override string WikiPageName { get { return "microsoft-azure-speech"; } }

        public MicrosoftAzureSpeechServiceControlViewModel()
            : base(Resources.MicrosoftAzureSpeech)
        {
            this.LogInCommand = this.CreateCommand(async () =>
            {
                if (!string.IsNullOrEmpty(this.RegionName) && !string.IsNullOrEmpty(this.SubscriptionKey))
                {
                    ChannelSession.Settings.MicrosoftAzureSpeechCustomRegionName = this.RegionName;
                    ChannelSession.Settings.MicrosoftAzureSpeechCustomSubscriptionKey = this.SubscriptionKey;

                    ITextToSpeechConnectableService service = ServiceManager.GetAll<ITextToSpeechConnectableService>().Where(s => s.ProviderType == TextToSpeechProviderType.MicrosoftAzureSpeech).First();
                    Result result = await service.TestAccess();
                    if (result.Success)
                    {
                        this.IsConnected = true;
                    }
                    else
                    {
                        ChannelSession.Settings.MicrosoftAzureSpeechCustomRegionName = null;
                        ChannelSession.Settings.MicrosoftAzureSpeechCustomSubscriptionKey = null;

                        await this.ShowConnectFailureMessage(result);
                    }
                }
            });

            this.LogOutCommand = this.CreateCommand(() =>
            {
                ChannelSession.Settings.MicrosoftAzureSpeechCustomRegionName = null;
                ChannelSession.Settings.MicrosoftAzureSpeechCustomSubscriptionKey = null;

                this.IsConnected = false;
            });

            this.IsConnected = !string.IsNullOrEmpty(ChannelSession.Settings.MicrosoftAzureSpeechCustomRegionName) &&
                !string.IsNullOrEmpty(ChannelSession.Settings.MicrosoftAzureSpeechCustomSubscriptionKey);
        }
    }
}
