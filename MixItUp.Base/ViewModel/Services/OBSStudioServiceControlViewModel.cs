using MixItUp.Base.Util;
using System;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
{
    public class OBSStudioServiceControlViewModel : StreamingServiceControlViewModelBase
    {
        public const string DefaultOBSStudioConnection = "ws://127.0.0.1:4444";

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
            : base(Resources.OBSStudio)
        {
            this.IPAddress = OBSStudioServiceControlViewModel.DefaultOBSStudioConnection;

            this.ConnectCommand = this.CreateCommand(async () =>
            {
                ChannelSession.Settings.OBSStudioServerIP = this.IPAddress;
                ChannelSession.Settings.OBSStudioServerPassword = this.Password();

                Result result = await ChannelSession.Services.OBSStudio.Connect();
                if (result.Success)
                {
                    this.IsConnected = true;
                    this.ChangeDefaultStreamingSoftware();
                }
                else
                {
                    await this.ShowConnectFailureMessage(result);
                }
            });

            this.DisconnectCommand = this.CreateCommand(async () =>
            {
                ChannelSession.Settings.OBSStudioServerIP = null;
                ChannelSession.Settings.OBSStudioServerPassword = null;

                await ChannelSession.Services.OBSStudio.Disconnect();
                this.IsConnected = false;
            });

            this.TestConnectionCommand = this.CreateCommand(async () =>
            {
                if (await ChannelSession.Services.OBSStudio.TestConnection())
                {
                    await DialogHelper.ShowMessage(Resources.OBSStudioSuccess);
                }
                else
                {
                    await DialogHelper.ShowMessage(Resources.OBSStudioFailed);
                }
            });

            this.IsConnected = ChannelSession.Services.OBSStudio.IsConnected;
        }
    }
}
