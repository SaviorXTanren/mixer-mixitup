using MixItUp.Base.Util;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
{
    public class RainmakerServiceControlViewModel : ServiceControlViewModelBase
    {
        public ICommand LogInCommand { get; set; }
        public ICommand LogOutCommand { get; set; }

        public RainmakerServiceControlViewModel()
            : base(Resources.Rainmaker)
        {
            this.LogInCommand = this.CreateCommand(async () =>
            {
                Result result = await ChannelSession.Services.Rainmaker.Connect();
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
                await ChannelSession.Services.Rainmaker.Disconnect();

                ChannelSession.Settings.RainMakerOAuthToken = null;

                this.IsConnected = false;
            });

            this.IsConnected = ChannelSession.Services.Rainmaker.IsConnected;
        }
    }
}
