using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using MixItUp.Base.Model.Web;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
{
    public class LumiaStreamServiceControlViewModel : ServiceControlViewModelBase
    {
        public string APIToken
        {
            get { return this.apiToken; }
            set
            {
                this.apiToken = value;
                this.NotifyPropertyChanged();
            }
        }
        private string apiToken;

        public ICommand ConnectCommand { get; set; }
        public ICommand DisconnectCommand { get; set; }

        public override string WikiPageName { get { return "lumia-stream"; } }

        public LumiaStreamServiceControlViewModel()
            : base(Resources.LumiaStream)
        {
            this.APIToken = ChannelSession.Settings.LumiaStreamOAuthToken?.accessToken;

            this.ConnectCommand = this.CreateCommand(async () =>
            {
                if (!string.IsNullOrEmpty(this.APIToken))
                {
                    ChannelSession.Settings.LumiaStreamOAuthToken = new OAuthTokenModel() { accessToken = this.APIToken };

                    Result result = await ServiceManager.Get<LumiaStreamService>().Connect();
                    if (result.Success)
                    {
                        this.IsConnected = true;
                    }
                    else
                    {
                        await this.ShowConnectFailureMessage(result);
                    }
                }
            });

            this.DisconnectCommand = this.CreateCommand(async () =>
            {
                await ServiceManager.Get<LumiaStreamService>().Disconnect();

                ChannelSession.Settings.LumiaStreamOAuthToken = null;

                this.IsConnected = false;
            });

            this.IsConnected = ServiceManager.Get<LumiaStreamService>().IsConnected;
        }
    }
}
