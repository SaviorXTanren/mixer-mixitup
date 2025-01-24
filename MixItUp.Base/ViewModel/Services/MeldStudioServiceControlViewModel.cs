using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System.Threading;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
{
    public class MeldStudioServiceControlViewModel : ServiceControlViewModelBase
    {
        private const string DefaultWebSocketAddress = "ws://127.0.0.1:13376";

        public string WebSocketAddress
        {
            get { return this.webSocketAddress; }
            set
            {
                this.webSocketAddress = value;
                this.NotifyPropertyChanged();
            }
        }
        private string webSocketAddress;

        public ICommand ConnectCommand { get; set; }
        public ICommand DisconnectCommand { get; set; }

        public override string WikiPageName { get { return "meld-studio"; } }

        public MeldStudioServiceControlViewModel()
            : base(Resources.MeldStudio)
        {
            this.WebSocketAddress = DefaultWebSocketAddress;

            this.ConnectCommand = this.CreateCommand(async () =>
            {
                ChannelSession.Settings.MeldStudioWebSocketAddress = this.WebSocketAddress;

                Result result = await ServiceManager.Get<MeldStudioService>().ManualConnect(CancellationToken.None);
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
                await ServiceManager.Get<MeldStudioService>().Disable();

                ChannelSession.Settings.MeldStudioWebSocketAddress = null;

                this.IsConnected = false;
            });

            this.IsConnected = ServiceManager.Get<MeldStudioService>().IsConnected;
        }
    }
}
