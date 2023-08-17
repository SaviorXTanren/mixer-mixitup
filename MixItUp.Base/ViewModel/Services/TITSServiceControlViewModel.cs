using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
{
    public class TITSServiceControlViewModel : ServiceControlViewModelBase
    {
        public int PortNumber
        {
            get { return this.portNumber; }
            set
            {
                this.portNumber = value;
                this.NotifyPropertyChanged();
            }
        }
        private int portNumber;

        public ICommand ConnectCommand { get; set; }
        public ICommand DisconnectCommand { get; set; }

        public override string WikiPageName { get { return "tits"; } }

        public TITSServiceControlViewModel()
            : base(Resources.TwitchIntegratedThrowingSystem)
        {
            this.PortNumber = ChannelSession.Settings.TITSPortNumber;

            this.ConnectCommand = this.CreateCommand(async () =>
            {
                ChannelSession.Settings.TITSPortNumber = this.PortNumber;

                Result result = await ServiceManager.Get<TITSService>().Connect();
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
                await ServiceManager.Get<TITSService>().Disconnect();

                ChannelSession.Settings.TITSOAuthToken = null;

                this.IsConnected = false;
            });

            this.IsConnected = ServiceManager.Get<TITSService>().IsConnected;
        }
    }
}
