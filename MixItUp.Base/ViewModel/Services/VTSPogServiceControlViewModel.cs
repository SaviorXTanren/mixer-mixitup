using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
{
    public class VTSPogServiceControlViewModel : ServiceControlViewModelBase
    {
        public ICommand ConnectCommand { get; set; }
        public ICommand DisconnectCommand { get; set; }

        public override string WikiPageName { get { return "vts-pog"; } }

        public VTSPogServiceControlViewModel()
            : base(Resources.VTSPog)
        {
            this.ConnectCommand = this.CreateCommand(async () =>
            {
                Result result = await ServiceManager.Get<VTSPogService>().Connect();
                if (result.Success)
                {
                    this.IsConnected = true;
                    ChannelSession.Settings.VTSPogEnabled = true;
                }
                else
                {
                    await this.ShowConnectFailureMessage(result);
                }
            });

            this.DisconnectCommand = this.CreateCommand(async () =>
            {
                await ServiceManager.Get<VTSPogService>().Disconnect();

                ChannelSession.Settings.VTSPogEnabled = false;

                this.IsConnected = false;
            });

            this.IsConnected = ServiceManager.Get<VTSPogService>().IsConnected;
        }
    }
}
