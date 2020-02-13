using MixItUp.Base.Services.External;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.Services
{
    public class StreamJarServiceControlViewModel : ServiceControlViewModelBase
    {
        public ICommand LogInCommand { get; set; }
        public ICommand LogOutCommand { get; set; }

        public StreamJarServiceControlViewModel()
            : base("StreamJar")
        {
            this.LogInCommand = this.CreateCommand(async (parameter) =>
            {
                ExternalServiceResult result = await ChannelSession.Services.StreamJar.Connect();
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
                await ChannelSession.Services.StreamJar.Disconnect();
                this.IsConnected = false;
            });

            this.IsConnected = ChannelSession.Services.StreamJar.IsConnected;
        }
    }
}
