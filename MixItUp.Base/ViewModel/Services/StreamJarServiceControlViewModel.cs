using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
{
    public class StreamJarServiceControlViewModel : ServiceControlViewModelBase
    {
        public ICommand LogInCommand { get; set; }
        public ICommand LogOutCommand { get; set; }

        public StreamJarServiceControlViewModel()
            : base(Resources.StreamJar)
        {
            this.LogInCommand = this.CreateCommand(async () =>
            {
                Result result = await ServiceManager.Get<StreamJarService>().Connect();
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
                await ServiceManager.Get<StreamJarService>().Disconnect();

                ChannelSession.Settings.StreamJarOAuthToken = null;

                this.IsConnected = false;
            });

            this.IsConnected = ServiceManager.Get<StreamJarService>().IsConnected;
        }
    }
}
