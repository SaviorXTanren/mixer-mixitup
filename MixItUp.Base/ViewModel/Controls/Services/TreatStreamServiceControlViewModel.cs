using MixItUp.Base.Util;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.Services
{
    public class TreatStreamServiceControlViewModel : ServiceControlViewModelBase
    {
        public ICommand LogInCommand { get; set; }
        public ICommand LogOutCommand { get; set; }

        public TreatStreamServiceControlViewModel()
            : base("TreatStream")
        {
            this.LogInCommand = this.CreateCommand(async (parameter) =>
            {
                Result result = await ChannelSession.Services.TreatStream.Connect();
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
                await ChannelSession.Services.TreatStream.Disconnect();

                ChannelSession.Settings.TreatStreamOAuthToken = null;

                this.IsConnected = false;
            });

            this.IsConnected = ChannelSession.Services.TreatStream.IsConnected;
        }
    }
}
