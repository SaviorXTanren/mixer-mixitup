using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.Services
{
    public class OverlayServiceControlViewModel : ServiceControlViewModelBase
    {
        public string StreamingSoftwareSourceName
        {
            get { return ChannelSession.Settings.OverlaySourceName; }
            set
            {
                ChannelSession.Settings.OverlaySourceName = value;
                this.NotifyPropertyChanged();
            }
        }

        public ICommand ConnectCommand { get; set; }
        public ICommand DisconnectCommand { get; set; }
        public ICommand TestConnectionCommand { get; set; }

        public OverlayServiceControlViewModel()
            : base("Overlay")
        {
            this.ConnectCommand = this.CreateCommand(async (parameter) =>
            {
                ExternalServiceResult result = await ChannelSession.Services.Overlay.Connect();
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
                await ChannelSession.Services.Overlay.Disconnect();
                this.IsConnected = false;
            });

            this.TestConnectionCommand = this.CreateCommand(async (parameter) =>
            {
                int total = await ChannelSession.Services.Overlay.TestConnections();
                if (total > 0)
                {
                    await DialogHelper.ShowMessage("Overlay connection test successful!" + Environment.NewLine + Environment.NewLine + total + " overlays connected in total");
                }
                else
                {
                    string message = "Overlay connection test failed, please ensure you have the Mix It Up Overlay page visible and running in your streaming software.";
                    message += Environment.NewLine + Environment.NewLine;
                    message += "If you launched your streaming software before Mix It Up, try refreshing the webpage source in your streaming software.";
                    await DialogHelper.ShowMessage(message);
                }
            });

            this.IsConnected = ChannelSession.Services.Overlay.IsConnected;
        }
    }
}
