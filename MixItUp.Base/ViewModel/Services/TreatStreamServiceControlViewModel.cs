using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
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
                Result result = await ServiceManager.Get<TreatStreamService>().Connect();
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
                await ServiceManager.Get<TreatStreamService>().Disconnect();

                ChannelSession.Settings.TreatStreamOAuthToken = null;

                this.IsConnected = false;
            });

            this.IsConnected = ServiceManager.Get<TreatStreamService>().IsConnected;
        }
    }
}
