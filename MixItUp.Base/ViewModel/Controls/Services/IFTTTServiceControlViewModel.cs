using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using StreamingClient.Base.Model.OAuth;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.Services
{
    public class IFTTTServiceControlViewModel : ServiceControlViewModelBase
    {
        public string IFTTTWebHookKey
        {
            get { return this.iftttWebHookKey; }
            set
            {
                this.iftttWebHookKey = value;
                this.NotifyPropertyChanged();
            }
        }
        private string iftttWebHookKey;

        public ICommand LogInCommand { get; set; }
        public ICommand LogOutCommand { get; set; }

        public IFTTTServiceControlViewModel()
            : base("IFTTT")
        {
            this.LogInCommand = this.CreateCommand(async (parameter) =>
            {
                if (string.IsNullOrEmpty(this.IFTTTWebHookKey))
                {
                    await DialogHelper.ShowMessage("Please enter a valid IFTTT Web Hook key.");
                }
                else
                {
                    ExternalServiceResult result = await ChannelSession.Services.IFTTT.Connect(new OAuthTokenModel() { accessToken = this.IFTTTWebHookKey });
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

            this.LogOutCommand = this.CreateCommand(async (parameter) =>
            {
                await ChannelSession.Services.IFTTT.Disconnect();

                ChannelSession.Settings.IFTTTOAuthToken = null;

                this.IsConnected = false;
            });

            this.IsConnected = ChannelSession.Services.IFTTT.IsConnected;
        }
    }
}
