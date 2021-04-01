using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
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
            : base(Resources.Overlay)
        {
            this.ConnectCommand = this.CreateCommand(async (parameter) =>
            {
                Result result = await ChannelSession.Services.Overlay.Connect();
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
                    await DialogHelper.ShowMessage(Resources.OverlayConnectionSuccess + Environment.NewLine + Environment.NewLine + string.Format(Resources.OverlaysConnected, total));
                }
                else
                {
                    string message = Resources.OverlayConnectionFailed1;
                    message += Environment.NewLine + Environment.NewLine;
                    message += Resources.OverlayConnectionFailed2;
                    await DialogHelper.ShowMessage(message);
                }
            });

            this.IsConnected = ChannelSession.Services.Overlay.IsConnected;
        }
    }
}
