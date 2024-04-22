using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
{
    public class MtionStudioServiceControlViewModel : ServiceControlViewModelBase
    {
        public ICommand ConnectCommand { get; set; }
        public ICommand DisconnectCommand { get; set; }

        public override string WikiPageName { get { return "mtion-studio"; } }

        public MtionStudioServiceControlViewModel()
            : base(Resources.MtionStudio)
        {
            this.ConnectCommand = this.CreateCommand(async () =>
            {
                Result result = await ServiceManager.Get<MtionStudioService>().Connect();
                if (result.Success)
                {
                    this.IsConnected = true;
                    ChannelSession.Settings.MtionStudioEnabled = true;
                }
                else
                {
                    await this.ShowConnectFailureMessage(result);
                }
            });

            this.DisconnectCommand = this.CreateCommand(async () =>
            {
                await ServiceManager.Get<MtionStudioService>().Disconnect();

                ChannelSession.Settings.MtionStudioEnabled = false;

                this.IsConnected = false;
            });

            this.IsConnected = ServiceManager.Get<MtionStudioService>().IsConnected;
        }
    }
}
