using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
{
    public class SAMMIServiceControlViewModel : ServiceControlViewModelBase
    {
        public ICommand LogInCommand { get; set; }
        public ICommand LogOutCommand { get; set; }

        public override string WikiPageName { get { return "sammi"; } }

        public SAMMIServiceControlViewModel()
            : base(Resources.SAMMI)
        {
            this.LogInCommand = this.CreateCommand(async () =>
            {
                Result result = await ServiceManager.Get<SAMMIService>().Connect();
                if (result.Success)
                {
                    this.IsConnected = true;
                    ChannelSession.Settings.EnableSAMMI = true;
                }
                else
                {
                    await this.ShowConnectFailureMessage(result);
                }
            });

            this.LogOutCommand = this.CreateCommand(async () =>
            {
                await ServiceManager.Get<SAMMIService>().Disconnect();

                ChannelSession.Settings.EnableSAMMI = false;

                this.IsConnected = false;
            });

            this.IsConnected = ServiceManager.Get<SAMMIService>().IsConnected;
        }
    }
}
