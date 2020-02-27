using MixItUp.Base.Util;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.Services
{
    public class StreamElementsServiceControlViewModel : ServiceControlViewModelBase
    {
        public ICommand LogInCommand { get; set; }
        public ICommand LogOutCommand { get; set; }

        public StreamElementsServiceControlViewModel()
            : base("StreamElements")
        {
            this.LogInCommand = this.CreateCommand(async (parameter) =>
            {
                Result result = await ChannelSession.Services.StreamElements.Connect();
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
                await ChannelSession.Services.StreamElements.Disconnect();

                ChannelSession.Settings.StreamElementsOAuthToken = null;

                this.IsConnected = false;
            });

            this.IsConnected = ChannelSession.Services.StreamElements.IsConnected;
        }
    }
}
