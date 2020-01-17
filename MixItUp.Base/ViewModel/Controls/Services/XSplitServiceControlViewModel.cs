using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.Services
{
    public class XSplitServiceControlViewModel : ServiceControlViewModelBase
    {
        public ICommand ConnectCommand { get; set; }
        public ICommand DisconnectCommand { get; set; }
        public ICommand TestConnectionCommand { get; set; }

        public XSplitServiceControlViewModel()
            : base("XSplit")
        {
            this.ConnectCommand = this.CreateCommand(async (parameter) =>
            {
                ExternalServiceResult result = await ChannelSession.Services.XSplit.Connect();
                if (result.Success)
                {
                    this.IsConnected = true;
                }
                else
                {
                    await this.ShowConnectFailureMessage(result);
                }
            });

            this.DisconnectCommand = this.CreateCommand(async (parameter) =>
            {
                await ChannelSession.Services.XSplit.Disconnect();
                this.IsConnected = false;
            });

            this.TestConnectionCommand = this.CreateCommand(async (parameter) =>
            {
                if (await ChannelSession.Services.XSplit.TestConnection())
                {
                    await DialogHelper.ShowMessage("XSplit connection test successful!");
                }
                else
                {
                    await DialogHelper.ShowMessage("XSplit connection test failed, please ensure you have the Mix It Up XSplit extension added and open in XSplit.");
                }
            });

            this.IsConnected = ChannelSession.Services.XSplit.IsConnected;
        }
    }
}