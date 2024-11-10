using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using MixItUp.Base.Model.Web;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
{
    public class PixelChatServiceControlViewModel : ServiceControlViewModelBase
    {
        public string APIKey
        {
            get { return this.apiKey; }
            set
            {
                this.apiKey = value;
                this.NotifyPropertyChanged();
            }
        }
        private string apiKey;

        public ICommand LogInCommand { get; set; }
        public ICommand LogOutCommand { get; set; }

        public override string WikiPageName { get { return "pixel-chat"; } }

        public PixelChatServiceControlViewModel()
            : base(Resources.PixelChat)
        {
            this.LogInCommand = this.CreateCommand(async () =>
            {
                if (string.IsNullOrEmpty(this.APIKey))
                {
                    await DialogHelper.ShowMessage(Resources.PixelChatInvalidAPIKey);
                }
                else
                {
                    Result result = await ServiceManager.Get<PixelChatService>().Connect(new OAuthTokenModel() { accessToken = this.APIKey });
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

            this.LogOutCommand = this.CreateCommand(async () =>
            {
                await ServiceManager.Get<PixelChatService>().Disconnect();

                ChannelSession.Settings.PixelChatOAuthToken = null;

                this.IsConnected = false;
            });

            this.IsConnected = ServiceManager.Get<PixelChatService>().IsConnected;
        }
    }
}
