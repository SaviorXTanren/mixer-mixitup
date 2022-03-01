using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
{
    public class OvrStreamServiceControlViewModel : ServiceControlViewModelBase
    {
        public const string DefaultOvrStreamConnection = "ws://127.0.0.1:8023";

        public string OvrStreamAddress
        {
            get { return this.ovrStreamAddress; }
            set
            {
                this.ovrStreamAddress = value;
                this.NotifyPropertyChanged();
            }
        }
        private string ovrStreamAddress;

        public ICommand ConnectCommand { get; set; }
        public ICommand DisconnectCommand { get; set; }

        public override string WikiPageName { get { return "ovrstream"; } }

        public OvrStreamServiceControlViewModel()
            : base(Resources.OvrStream)
        {
            this.OvrStreamAddress = OvrStreamServiceControlViewModel.DefaultOvrStreamConnection;

            this.ConnectCommand = this.CreateCommand(async () =>
            {
                ChannelSession.Settings.OvrStreamServerIP = this.OvrStreamAddress;

                Result result = await ServiceManager.Get<PolyPopService>().Connect();
                if (result.Success)
                {
                    this.IsConnected = true;
                }
                else
                {
                    await this.ShowConnectFailureMessage(result);
                }
            });

            this.DisconnectCommand = this.CreateCommand(async () =>
            {
                await ServiceManager.Get<PolyPopService>().Disconnect();
                this.IsConnected = false;
            });

            this.IsConnected = ServiceManager.Get<PolyPopService>().IsConnected;
        }
    }
}