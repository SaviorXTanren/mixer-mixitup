using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
{
    public class CrowdControlServiceControlViewModel : ServiceControlViewModelBase
    {
        public ICommand LogInCommand { get; set; }
        public ICommand LogOutCommand { get; set; }

        public override string WikiPageName { get { return "crowd-control"; } }

        public CrowdControlServiceControlViewModel()
            : base(Resources.CrowdControl)
        {
            this.LogInCommand = this.CreateCommand(async () =>
            {
                Result result = await ServiceManager.Get<CrowdControlService>().Connect();
                if (result.Success)
                {
                    this.IsConnected = true;
                    ChannelSession.Settings.EnableCrowdControl = true;
                }
                else
                {
                    await this.ShowConnectFailureMessage(result);
                }
            });

            this.LogOutCommand = this.CreateCommand(async () =>
            {
                await ServiceManager.Get<CrowdControlService>().Disconnect();

                ChannelSession.Settings.EnableCrowdControl = false;

                this.IsConnected = false;
            });

            this.IsConnected = ServiceManager.Get<CrowdControlService>().IsConnected;
        }
    }
}
