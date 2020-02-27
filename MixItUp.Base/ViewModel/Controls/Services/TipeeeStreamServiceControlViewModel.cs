using MixItUp.Base.Util;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.Services
{
    public class TipeeeStreamServiceControlViewModel : ServiceControlViewModelBase
    {
        public ICommand LogInCommand { get; set; }
        public ICommand LogOutCommand { get; set; }

        public TipeeeStreamServiceControlViewModel()
            : base("TipeeeStream")
        {
            this.LogInCommand = this.CreateCommand(async (parameter) =>
            {
                Result result = await ChannelSession.Services.TipeeeStream.Connect();
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
                await ChannelSession.Services.TipeeeStream.Disconnect();

                ChannelSession.Settings.TipeeeStreamOAuthToken = null;

                this.IsConnected = false;
            });

            this.IsConnected = ChannelSession.Services.TipeeeStream.IsConnected;
        }
    }
}
