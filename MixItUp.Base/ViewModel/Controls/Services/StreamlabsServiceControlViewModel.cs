using MixItUp.Base.Util;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.Services
{
    public class StreamlabsServiceControlViewModel : ServiceControlViewModelBase
    {
        public ICommand LogInCommand { get; set; }
        public ICommand LogOutCommand { get; set; }

        public StreamlabsServiceControlViewModel()
            : base("Streamlabs")
        {
            this.LogInCommand = this.CreateCommand(async (parameter) =>
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

            this.LogOutCommand = this.CreateCommand(async (parameter) =>
            {
                await ChannelSession.Services.Streamlabs.Disconnect();

                ChannelSession.Settings.StreamlabsOAuthToken = null;

                this.IsConnected = false;
            });

            this.IsConnected = ChannelSession.Services.Streamlabs.IsConnected;
        }
    }
}
