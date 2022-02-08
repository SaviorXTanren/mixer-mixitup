using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
{
    public class StreamlabsOBSServiceControlViewModel : StreamingServiceControlViewModelBase
    {
        public ICommand ConnectCommand { get; set; }
        public ICommand DisconnectCommand { get; set; }
        public ICommand TestConnectionCommand { get; set; }

        public override string WikiPageName { get { return "streamlabs-desktop"; } }

        public StreamlabsOBSServiceControlViewModel()
            : base(Resources.StreamlabsOBS)
        {
            this.ConnectCommand = this.CreateCommand(async () =>
            {
                ChannelSession.Settings.EnableStreamlabsOBSConnection = false;
                Result result = await ServiceManager.Get<StreamlabsOBSService>().Connect();
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
                await ServiceManager.Get<StreamlabsOBSService>().Disconnect();
                ChannelSession.Settings.EnableStreamlabsOBSConnection = false;
                this.IsConnected = false;
            });

            this.TestConnectionCommand = this.CreateCommand(async () =>
            {
                if (await ServiceManager.Get<StreamlabsOBSService>().TestConnection())
                {
                    await DialogHelper.ShowMessage(Resources.StreamlabsOBSConnectionSuccess);
                }
                else
                {
                    await DialogHelper.ShowMessage(Resources.StreamlabsOBSConnectionFailed);
                }
            });

            this.IsConnected = ServiceManager.Get<StreamlabsOBSService>().IsConnected;
        }
    }
}
