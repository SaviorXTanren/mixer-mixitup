using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
{
    public class PulsoidServiceControlViewModel : ServiceControlViewModelBase
    {
        public ICommand LogInCommand { get; set; }
        public ICommand LogOutCommand { get; set; }

        public override string WikiPageName { get { return "pulsoid"; } }

        public PulsoidServiceControlViewModel()
            : base(Resources.Pulsoid)
        {
            this.LogInCommand = this.CreateCommand(async () =>
            {
                Result result = await ServiceManager.Get<PulsoidService>().Connect();
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
                await ServiceManager.Get<PulsoidService>().Disconnect();

                ChannelSession.Settings.PulsoidOAuthToken = null;

                this.IsConnected = false;
            });

            this.IsConnected = ServiceManager.Get<PulsoidService>().IsConnected;
        }
    }
}
