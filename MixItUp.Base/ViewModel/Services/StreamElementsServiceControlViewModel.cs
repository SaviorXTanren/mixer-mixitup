using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
{
    public class StreamElementsServiceControlViewModel : ServiceControlViewModelBase
    {
        public ICommand LogInCommand { get; set; }
        public ICommand LogOutCommand { get; set; }

        public override string WikiPageName { get { return "streamelements"; } }

        public StreamElementsServiceControlViewModel()
            : base(Resources.StreamElements)
        {
            this.LogInCommand = this.CreateCommand(async () =>
            {
                Result result = await ServiceManager.Get<StreamElementsService>().Connect();
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
                await ServiceManager.Get<StreamElementsService>().Disconnect();

                ChannelSession.Settings.StreamElementsOAuthToken = null;

                this.IsConnected = false;
            });

            this.IsConnected = ServiceManager.Get<StreamElementsService>().IsConnected;
        }
    }
}
