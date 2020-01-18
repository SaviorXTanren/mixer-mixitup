using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.Services
{
    public class OBSStudioServiceControlViewModel : ServiceControlViewModelBase
    {
        public string IPAddress
        {
            get { return this.ipAddress; }
            set
            {
                this.ipAddress = value;
                this.NotifyPropertyChanged();
            }
        }
        private string ipAddress;

        public ICommand ConnectCommand { get; set; }
        public ICommand DisconnectCommand { get; set; }
        public ICommand TestConnectionCommand { get; set; }

        public Func<string> Password { get; set; }

        public OBSStudioServiceControlViewModel()
            : base("OBS Studio")
        {
            this.IPAddress = ChannelSession.DefaultOBSStudioConnection;

            this.ConnectCommand = this.CreateCommand(async (parameter) =>
            {
                ChannelSession.Settings.OBSStudioServerIP = this.IPAddress;
                ChannelSession.Settings.OBSStudioServerPassword = this.Password();

                ExternalServiceResult result = await ChannelSession.Services.OBSStudio.Connect();
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
                await ChannelSession.Services.OBSStudio.Disconnect();
                this.IsConnected = false;
            });

            this.TestConnectionCommand = this.CreateCommand(async (parameter) =>
            {
                if (await ChannelSession.Services.OBSStudio.TestConnection())
                {
                    await DialogHelper.ShowMessage("OBS Studio connection test successful!");
                }
                else
                {
                    await DialogHelper.ShowMessage("OBS Studio connection test failed, Please make sure OBS Studio is running, the obs-websocket plugin is installed, and the connection and password match your settings in OBS Studio.");
                }
            });

            this.IsConnected = ChannelSession.Services.OBSStudio.IsConnected;
        }
    }
}
