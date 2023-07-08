using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
{
    public class StreamlabsDesktopServiceControlViewModel : StreamingServiceControlViewModelBase
    {
        public ICommand ConnectCommand { get; set; }
        public ICommand DisconnectCommand { get; set; }
        public ICommand TestConnectionCommand { get; set; }

        public override string WikiPageName { get { return "streamlabs-desktop"; } }

        public StreamlabsDesktopServiceControlViewModel()
            : base(Resources.StreamlabsDesktop)
        {
            this.ConnectCommand = this.CreateCommand(async () =>
            {
                ChannelSession.Settings.EnableStreamlabsOBSConnection = false;
                Result result = await ServiceManager.Get<StreamlabsDesktopService>().Connect();
                if (result.Success)
                {
                    this.IsConnected = true;
                    ChannelSession.Settings.EnableStreamlabsOBSConnection = true;
                    this.ChangeDefaultStreamingSoftware();
                }
                else
                {
                    await this.ShowConnectFailureMessage(result);
                }
            });

            this.DisconnectCommand = this.CreateCommand(async () =>
            {
                await ServiceManager.Get<StreamlabsDesktopService>().Disconnect();
                ChannelSession.Settings.EnableStreamlabsOBSConnection = false;
                this.IsConnected = false;
            });

            this.TestConnectionCommand = this.CreateCommand(async () =>
            {
                if (await ServiceManager.Get<StreamlabsDesktopService>().TestConnection())
                {
                    await DialogHelper.ShowMessage(Resources.StreamlabsDesktopConnectionSuccess);
                }
                else
                {
                    await DialogHelper.ShowMessage(Resources.StreamlabsDesktopConnectionFailed);
                }
            });

            this.IsConnected = ServiceManager.Get<StreamlabsDesktopService>().IsConnected;
        }
    }
}
