using MixItUp.Base.Services;
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

        public override string WikiPageName { get { return "overlay"; } }

        public OverlayServiceControlViewModel()
            : base(Resources.Overlay)
        {
            this.ConnectCommand = this.CreateCommand(async () =>
            {
                Result result = await ServiceManager.Get<OverlayV3Service>().Enable();
                if (result.Success)
                {
                    result = await ServiceManager.Get<OverlayV3Service>().Connect();
                    if (result.Success)
                    {
                        this.IsConnected = true;
                    }
                    else
                    {
                        await this.ShowConnectFailureMessage(result);
                    }
                }
                else
                {
                    await this.ShowConnectFailureMessage(result);
                }
            });

            this.DisconnectCommand = this.CreateCommand(async () =>
            {
                await ServiceManager.Get<OverlayV3Service>().Disconnect();
                await ServiceManager.Get<OverlayV3Service>().Disable();
                this.IsConnected = false;
            });

            this.TestConnectionCommand = this.CreateCommand(async () =>
            {
                int total = await ServiceManager.Get<OverlayV3Service>().TestConnections();
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

            this.IsConnected = ServiceManager.Get<OverlayV3Service>().IsConnected;
        }
    }
}
