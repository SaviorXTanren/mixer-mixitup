using MixItUp.Base.Util;
using StreamingClient.Base.Model.OAuth;
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
                    Result result = await ChannelSession.Services.PixelChat.Connect(new OAuthTokenModel() { accessToken = this.APIKey });
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
                await ChannelSession.Services.PixelChat.Disconnect();

                ChannelSession.Settings.PixelChatOAuthToken = null;

                this.IsConnected = false;
            });

            this.IsConnected = ChannelSession.Services.PixelChat.IsConnected;
        }
    }
}
