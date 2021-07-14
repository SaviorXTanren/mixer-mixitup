using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
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
                Result result = await ServiceManager.Get<VTubeStudioService>().Connect();
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
                await ServiceManager.Get<VTubeStudioService>().Disconnect();

                ChannelSession.Settings.StreamlabsOAuthToken = null;

                this.IsConnected = false;
            });

            this.IsConnected = ServiceManager.Get<VTubeStudioService>().IsConnected;
        }
    }
}
