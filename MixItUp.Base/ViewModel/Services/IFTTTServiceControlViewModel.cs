using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using MixItUp.Base.Model.Web;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
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

        public override string WikiPageName { get { return "ifttt"; } }

        public IFTTTServiceControlViewModel()
            : base(Resources.IFTTT)
        {
            this.LogInCommand = this.CreateCommand(async () =>
            {
                if (string.IsNullOrEmpty(this.IFTTTWebHookKey))
                {
                    await DialogHelper.ShowMessage(Resources.IFTTTInvalidWebHookKey);
                }
                else
                {
                    Result result = await ServiceManager.Get<IFTTTService>().Connect(new OAuthTokenModel() { accessToken = this.IFTTTWebHookKey });
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
                await ServiceManager.Get<IFTTTService>().Disconnect();

                ChannelSession.Settings.IFTTTOAuthToken = null;

                this.IsConnected = false;
            });

            this.IsConnected = ServiceManager.Get<IFTTTService>().IsConnected;
        }
    }
}
