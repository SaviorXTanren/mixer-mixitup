using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
{
    public class XSplitServiceControlViewModel : StreamingServiceControlViewModelBase
    {
        public ICommand ConnectCommand { get; set; }
        public ICommand DisconnectCommand { get; set; }
        public ICommand TestConnectionCommand { get; set; }

        public override string WikiPageName { get { return "xsplit"; } }

        public XSplitServiceControlViewModel()
            : base(Resources.XSplit)
        {
            this.ConnectCommand = this.CreateCommand(async () =>
            {
                ChannelSession.Settings.EnableXSplitConnection = false;

                Result result = await ServiceManager.Get<XSplitService>().Connect();
                if (result.Success)
                {
                    this.IsConnected = true;
                    ChannelSession.Settings.EnableXSplitConnection = true;
                    this.ChangeDefaultStreamingSoftware();
                }
                else
                {
                    await this.ShowConnectFailureMessage(result);
                }
            });

            this.DisconnectCommand = this.CreateCommand(async () =>
            {
                await ServiceManager.Get<XSplitService>().Disconnect();
                ChannelSession.Settings.EnableXSplitConnection = false;
                this.IsConnected = false;
            });

            this.TestConnectionCommand = this.CreateCommand(async () =>
            {
                if (await ServiceManager.Get<XSplitService>().TestConnection())
                {
                    await DialogHelper.ShowMessage(Resources.XSplitConnectionSuccess);
                }
                else
                {
                    await DialogHelper.ShowMessage(Resources.XSplitConnectionFailed);
                }
            });

            this.IsConnected = ServiceManager.Get<XSplitService>().IsConnected;
        }
    }
}