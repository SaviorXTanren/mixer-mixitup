using MixItUp.Base.Util;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
{
    public class VTubeStudioServiceControlViewModel : ServiceControlViewModelBase
    {
        public ICommand ConnectCommand { get; set; }
        public ICommand DisconnectCommand { get; set; }

        public VTubeStudioServiceControlViewModel()
            : base(Resources.VTubeStudio)
        {
            this.ConnectCommand = this.CreateCommand(async () =>
            {
                Result result = await ChannelSession.Services.VTubeStudio.Connect();
                if (result.Success)
                {
                    this.IsConnected = true;
                }
                else
                {
                    await this.ShowConnectFailureMessage(result);
                }
            });

            this.DisconnectCommand = this.CreateCommand(async () =>
            {
                await ChannelSession.Services.VTubeStudio.Disconnect();

                ChannelSession.Settings.VTubeStudioOAuthToken = null;

                this.IsConnected = false;
            });

            this.IsConnected = ChannelSession.Services.VTubeStudio.IsConnected;
        }
    }
}
