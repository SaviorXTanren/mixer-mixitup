using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
{
    public class OBSStudioServiceControlViewModel : StreamingServiceControlViewModelBase
    {
        public const string DefaultOBSStudio28Connection = "ws://127.0.0.1:4455";

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

        public override string WikiPageName { get { return "obs-studio"; } }

        public OBSStudioServiceControlViewModel()
            : base(Resources.OBSStudio)
        {
            this.IPAddress = OBSStudioServiceControlViewModel.DefaultOBSStudio28Connection;

            this.ConnectCommand = this.CreateCommand(async () =>
            {
                ChannelSession.Settings.OBSStudioServerIP = this.IPAddress;
                ChannelSession.Settings.OBSStudioServerPassword = this.Password();

                Result result = await ServiceManager.Get<IOBSStudioService>().Connect();
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

                await ServiceManager.Get<IOBSStudioService>().Disconnect();
                this.IsConnected = false;
            });

            this.TestConnectionCommand = this.CreateCommand(async () =>
            {
                if (await ServiceManager.Get<IOBSStudioService>().TestConnection())
                {
                    await DialogHelper.ShowMessage(Resources.OBSStudioSuccess);
                }
                else
                {
                    await DialogHelper.ShowMessage(Resources.OBSStudioFailed);
                }
            });

            this.IsConnected = ServiceManager.Get<IOBSStudioService>().IsConnected;
        }
    }
}
