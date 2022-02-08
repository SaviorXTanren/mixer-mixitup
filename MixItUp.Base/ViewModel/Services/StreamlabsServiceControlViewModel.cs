using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
{
    public class StreamlabsServiceControlViewModel : ServiceControlViewModelBase
    {
        public ICommand LogInCommand { get; set; }
        public ICommand LogOutCommand { get; set; }

        public override string WikiPageName { get { return "streamlabs"; } }

        public StreamlabsServiceControlViewModel()
            : base(Resources.Streamlabs)
        {
            this.LogInCommand = this.CreateCommand(async () =>
            {
                Result result = await ServiceManager.Get<StreamlabsService>().Connect();
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
                await ServiceManager.Get<StreamlabsService>().Disconnect();

                ChannelSession.Settings.StreamlabsOAuthToken = null;

                this.IsConnected = false;
            });

            this.IsConnected = ServiceManager.Get<StreamlabsService>().IsConnected;
        }
    }
}
