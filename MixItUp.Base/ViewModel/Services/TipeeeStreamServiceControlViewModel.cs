using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
{
    public class TipeeeStreamServiceControlViewModel : ServiceControlViewModelBase
    {
        public ICommand LogInCommand { get; set; }
        public ICommand LogOutCommand { get; set; }

        public override string WikiPageName { get { return "tipeeestream"; } }

        public TipeeeStreamServiceControlViewModel()
            : base(Resources.TipeeeStream)
        {
            this.LogInCommand = this.CreateCommand(async () =>
            {
                Result result = await ServiceManager.Get<TipeeeStreamService>().Connect();
                if (result.Success)
                {
                    this.IsConnected = true;
                }
                else
                {
                    await this.ShowConnectFailureMessage(result);
                }
            });

            this.LogOutCommand = this.CreateCommand(async () =>
            {
                await ServiceManager.Get<TipeeeStreamService>().Disconnect();

                ChannelSession.Settings.TipeeeStreamOAuthToken = null;

                this.IsConnected = false;
            });

            this.IsConnected = ServiceManager.Get<TipeeeStreamService>().IsConnected;
        }
    }
}
