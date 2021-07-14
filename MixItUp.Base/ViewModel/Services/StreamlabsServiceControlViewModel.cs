using MixItUp.Base.Util;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
{
    public class StreamlabsServiceControlViewModel : ServiceControlViewModelBase
    {
        public ICommand LogInCommand { get; set; }
        public ICommand LogOutCommand { get; set; }

        public StreamlabsServiceControlViewModel()
            : base(Resources.Streamlabs)
        {
            this.LogInCommand = this.CreateCommand(async () =>
            {
                Result result = await ChannelSession.Services.Streamlabs.Connect();
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
                await ChannelSession.Services.Streamlabs.Disconnect();

                ChannelSession.Settings.VTubeStudioOAuthToken = null;

                this.IsConnected = false;
            });

            this.IsConnected = ChannelSession.Services.Streamlabs.IsConnected;
        }
    }
}
