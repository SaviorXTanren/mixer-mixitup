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

        public override string WikiPageName { get { return "treatstream"; } }

        public TreatStreamServiceControlViewModel()
            : base(Resources.TreatStream)
        {
            this.LogInCommand = this.CreateCommand(async () =>
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

            this.LogOutCommand = this.CreateCommand(async () =>
            {
                await ServiceManager.Get<TreatStreamService>().Disconnect();

                ChannelSession.Settings.TreatStreamOAuthToken = null;

                this.IsConnected = false;
            });

            this.IsConnected = ServiceManager.Get<TreatStreamService>().IsConnected;
        }
    }
}
