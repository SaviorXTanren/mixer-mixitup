using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.Services
{
    public class StreamlabsOBSServiceControlViewModel : ServiceControlViewModelBase
    {
        public ICommand ConnectCommand { get; set; }
        public ICommand DisconnectCommand { get; set; }
        public ICommand TestConnectionCommand { get; set; }

        public StreamlabsOBSServiceControlViewModel()
            : base("Streamlabs OBS")
        {
            this.ConnectCommand = this.CreateCommand(async (parameter) =>
            {
                ExternalServiceResult result = await ChannelSession.Services.StreamlabsOBS.Connect();
                if (result.Success)
                {
                    this.IsConnected = true;
                }
                else
                {
                    await this.ShowConnectFailureMessage(result);
                }
            });

            this.DisconnectCommand = this.CreateCommand(async (parameter) =>
            {
                await ChannelSession.Services.StreamlabsOBS.Disconnect();
                this.IsConnected = false;
            });

            this.TestConnectionCommand = this.CreateCommand(async (parameter) =>
            {
                if (await ChannelSession.Services.StreamlabsOBS.TestConnection())
                {
                    await DialogHelper.ShowMessage("Streamlabs OBS connection test successful!");
                }
                else
                {
                    await DialogHelper.ShowMessage("Streamlabs OBS connection test failed, please ensure that Streamlabs OBS is running.");
                }
            });

            this.IsConnected = ChannelSession.Services.StreamlabsOBS.IsConnected;
        }
    }
}
