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

        public XSplitServiceControlViewModel()
            : base("XSplit")
        {
            this.ConnectCommand = this.CreateCommand(async (parameter) =>
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

            this.DisconnectCommand = this.CreateCommand(async (parameter) =>
            {
                await ServiceManager.Get<XSplitService>().Disconnect();
                ChannelSession.Settings.EnableXSplitConnection = false;
                this.IsConnected = false;
            });

            this.TestConnectionCommand = this.CreateCommand(async (parameter) =>
            {
                if (await ServiceManager.Get<XSplitService>().TestConnection())
                {
                    await DialogHelper.ShowMessage("XSplit connection test successful!");
                }
                else
                {
                    await DialogHelper.ShowMessage("XSplit connection test failed, please ensure you have the Mix It Up XSplit extension added and open in XSplit.");
                }
            });

            this.IsConnected = ServiceManager.Get<XSplitService>().IsConnected;
        }
    }
}