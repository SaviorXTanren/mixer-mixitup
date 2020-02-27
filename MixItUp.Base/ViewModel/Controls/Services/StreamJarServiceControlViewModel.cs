using MixItUp.Base.Util;
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
                Result result = await ChannelSession.Services.StreamJar.Connect();
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

                ChannelSession.Settings.StreamJarOAuthToken = null;

                this.IsConnected = false;
            });

            this.IsConnected = ChannelSession.Services.StreamJar.IsConnected;
        }
    }
}
