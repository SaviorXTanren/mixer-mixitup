using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
{
    public class TwitterServiceControlViewModel : ServiceControlViewModelBase
    {
        public bool AuthorizationInProgress
        {
            get { return authorizationInProgress; }
            set
            {
                this.authorizationInProgress = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool authorizationInProgress;

        public string AuthorizationPin
        {
            get { return this.authorizationPin; }
            set
            {
                this.authorizationPin = value;
                this.NotifyPropertyChanged();
            }
        }
        private string authorizationPin;

        public ICommand LogInCommand { get; set; }
        public ICommand LogOutCommand { get; set; }
        public ICommand AuthorizePinCommand { get; set; }

        public override string WikiPageName { get { return "twitter"; } }

        public TwitterServiceControlViewModel()
            : base(Resources.Twitter)
        {
            this.LogInCommand = this.CreateCommand(() =>
            {
                this.AuthorizationInProgress = true;
                Task.Run(async () =>
                {
                    Result result = await ServiceManager.Get<ITwitterService>().Connect();
                    await DispatcherHelper.Dispatcher.InvokeAsync(async () =>
                    {
                        if (result.Success)
                        {
                            this.IsConnected = true;
                        }
                        else
                        {
                            await this.ShowConnectFailureMessage(result);
                        }

                        this.AuthorizationInProgress = false;
                        this.AuthorizationPin = string.Empty;
                    });
                });
            });

            this.LogOutCommand = this.CreateCommand(async () =>
            {
                await ServiceManager.Get<ITwitterService>().Disconnect();

                ChannelSession.Settings.TwitterOAuthToken = null;

                this.IsConnected = false;
            });

            this.AuthorizePinCommand = this.CreateCommand(() =>
            {
                if (!string.IsNullOrEmpty(this.AuthorizationPin))
                {
                    ServiceManager.Get<ITwitterService>().SetAuthPin(this.AuthorizationPin);
                }
            });

            this.IsConnected = ServiceManager.Get<ITwitterService>().IsConnected;
        }
    }
}
