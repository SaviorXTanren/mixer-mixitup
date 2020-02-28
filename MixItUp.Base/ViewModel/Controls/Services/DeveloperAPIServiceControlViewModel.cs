using MixItUp.Base.Util;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.Services
{
    public class DeveloperAPIServiceControlViewModel : ServiceControlViewModelBase
    {
        public ICommand ConnectCommand { get; set; }
        public ICommand DisconnectCommand { get; set; }

        public DeveloperAPIServiceControlViewModel()
            : base("Developer API")
        {
            this.ConnectCommand = this.CreateCommand(async (parameter) =>
            {
                ChannelSession.Settings.EnableDeveloperAPI = false;
                Result result = await ChannelSession.Services.DeveloperAPI.Connect();
                if (result.Success)
                {
                    ChannelSession.Settings.EnableDeveloperAPI = true;
                    this.IsConnected = true;
                }
                else
                {
                    await this.ShowConnectFailureMessage(result);
                }
            });

            this.DisconnectCommand = this.CreateCommand(async (parameter) =>
            {
                await ChannelSession.Services.DeveloperAPI.Disconnect();
                ChannelSession.Settings.EnableDeveloperAPI = false;
                this.IsConnected = false;
            });

            this.IsConnected = ChannelSession.Services.DeveloperAPI.IsConnected;
        }
    }
}
