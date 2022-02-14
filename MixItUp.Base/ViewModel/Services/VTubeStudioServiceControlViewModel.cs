using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
{
    public class VTubeStudioServiceControlViewModel : ServiceControlViewModelBase
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

        public override string WikiPageName { get { return "vtube-studio"; } }

        public VTubeStudioServiceControlViewModel()
            : base(Resources.VTubeStudio)
        {
            this.PortNumber = ChannelSession.Settings.VTubeStudioPortNumber;

            this.ConnectCommand = this.CreateCommand(async () =>
            {
                ChannelSession.Settings.VTubeStudioPortNumber = this.PortNumber;

                Result result = await ServiceManager.Get<VTubeStudioService>().Connect();
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
                await ServiceManager.Get<VTubeStudioService>().Disconnect();

                ChannelSession.Settings.VTubeStudioOAuthToken = null;

                this.IsConnected = false;
            });

            this.IsConnected = ServiceManager.Get<VTubeStudioService>().IsConnected;
        }
    }
}
