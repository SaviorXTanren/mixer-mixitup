using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
{
    public class DeveloperAPIServiceControlViewModel : ServiceControlViewModelBase
    {
        public ICommand ConnectCommand { get; set; }
        public ICommand DisconnectCommand { get; set; }

        public override string WikiPageName { get { return "developer-api"; } }

        public bool EnableDeveloperAPIAdvancedMode
        {
            get { return ChannelSession.Settings.EnableDeveloperAPIAdvancedMode; }
            set
            {
                ChannelSession.Settings.EnableDeveloperAPIAdvancedMode = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("EnableDeveloperAPIAdvancedMode");
            }
        }

        public DeveloperAPIServiceControlViewModel()
            : base(Resources.DeveloperAPI)
        {
            this.ConnectCommand = this.CreateCommand(async () =>
            {
                ChannelSession.Settings.EnableDeveloperAPI = false;
                Result result = await ServiceManager.Get<IDeveloperAPIService>().Connect();
                if (result.Success)
                {
                    ChannelSession.Settings.EnableDeveloperAPI = true;
                    this.IsConnected = true;
                }
                else
                {
                    await this.ShowConnectFailureMessage(result);
                }
            });

            this.DisconnectCommand = this.CreateCommand(async () =>
            {
                await ServiceManager.Get<IDeveloperAPIService>().Disconnect();
                ChannelSession.Settings.EnableDeveloperAPI = false;
                this.IsConnected = false;
            });

            this.IsConnected = ServiceManager.Get<IDeveloperAPIService>().IsConnected;
        }
    }
}
