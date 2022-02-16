using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
{
    public class RainmakerServiceControlViewModel : ServiceControlViewModelBase
    {
        public ICommand LogInCommand { get; set; }
        public ICommand LogOutCommand { get; set; }

        public override string WikiPageName { get { return "rainmaker"; } }

        public RainmakerServiceControlViewModel()
            : base(Resources.Rainmaker)
        {
            this.LogInCommand = this.CreateCommand(async () =>
            {
                Result result = await ServiceManager.Get<RainmakerService>().Connect();
                if (result.Success)
                {
                    this.IsConnected = true;
                }
                else
                {
                    await this.ShowConnectFailureMessage(result);
                }
            });

            this.LogOutCommand = this.CreateCommand(async () =>
            {
                await ServiceManager.Get<RainmakerService>().Disconnect();

                ChannelSession.Settings.RainMakerOAuthToken = null;

                this.IsConnected = false;
            });

            this.IsConnected = ServiceManager.Get<RainmakerService>().IsConnected;
        }
    }
}
