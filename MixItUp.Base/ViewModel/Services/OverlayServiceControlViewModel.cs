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

        public OverlayServiceControlViewModel()
            : base("Overlay")
        {
            this.ConnectCommand = this.CreateCommand(async (parameter) =>
            {
                Result result = await ServiceManager.Get<OverlayService>().Connect();
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
                await ServiceManager.Get<OverlayService>().Disconnect();
                this.IsConnected = false;
            });

            this.TestConnectionCommand = this.CreateCommand(async (parameter) =>
            {
                int total = await ServiceManager.Get<OverlayService>().TestConnections();
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

            this.IsConnected = ServiceManager.Get<OverlayService>().IsConnected;
        }
    }
}
