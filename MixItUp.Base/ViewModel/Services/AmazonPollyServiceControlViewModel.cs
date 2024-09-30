using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System.Linq;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
{
    public class AmazonPollyServiceControlViewModel : ServiceControlViewModelBase
    {
        public string RegionSystemName
        {
            get { return this.regionSystemName; }
            set
            {
                this.regionSystemName = value;
                this.NotifyPropertyChanged();
            }
        }
        private string regionSystemName;

        public string AccessKey
        {
            get { return this.accessKey; }
            set
            {
                this.accessKey = value;
                this.NotifyPropertyChanged();
            }
        }
        private string accessKey;

        public string SecretKey
        {
            get { return this.secretKey; }
            set
            {
                this.secretKey = value;
                this.NotifyPropertyChanged();
            }
        }
        private string secretKey;

        public ICommand LogInCommand { get; set; }
        public ICommand LogOutCommand { get; set; }

        public override string WikiPageName { get { return "amazon-polly"; } }

        public AmazonPollyServiceControlViewModel()
            : base(Resources.AmazonPolly)
        {
            this.LogInCommand = this.CreateCommand(async () =>
            {
                if (!string.IsNullOrEmpty(this.RegionSystemName) && !string.IsNullOrEmpty(this.AccessKey) && !string.IsNullOrEmpty(this.SecretKey))
                {
                    ChannelSession.Settings.AmazonPollyCustomRegionSystemName = this.RegionSystemName;
                    ChannelSession.Settings.AmazonPollyCustomAccessKey = this.AccessKey;
                    ChannelSession.Settings.AmazonPollyCustomSecretKey = this.SecretKey;

                    ITextToSpeechConnectableService service = ServiceManager.GetAll<ITextToSpeechConnectableService>().Where(s => s.ProviderType == TextToSpeechProviderType.AmazonPolly).First();
                    Result result = await service.TestAccess();
                    if (result.Success)
                    {
                        this.IsConnected = true;
                    }
                    else
                    {
                        ChannelSession.Settings.AmazonPollyCustomRegionSystemName = null;
                        ChannelSession.Settings.AmazonPollyCustomAccessKey = null;
                        ChannelSession.Settings.AmazonPollyCustomSecretKey = null;

                        await this.ShowConnectFailureMessage(result);
                    }
                }
            });

            this.LogOutCommand = this.CreateCommand(() =>
            {
                ChannelSession.Settings.AmazonPollyCustomRegionSystemName = null;
                ChannelSession.Settings.AmazonPollyCustomAccessKey = null;
                ChannelSession.Settings.AmazonPollyCustomSecretKey = null;

                this.IsConnected = false;
            });

            this.IsConnected = !string.IsNullOrEmpty(ChannelSession.Settings.AmazonPollyCustomRegionSystemName) &&
                !string.IsNullOrEmpty(ChannelSession.Settings.AmazonPollyCustomAccessKey) &&
                !string.IsNullOrEmpty(ChannelSession.Settings.AmazonPollyCustomSecretKey);
        }
    }
}
