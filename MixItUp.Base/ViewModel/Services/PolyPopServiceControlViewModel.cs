using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
{
    public class PolyPopServiceControlViewModel : ServiceControlViewModelBase
    {
        public const int DefaultPolyPopPortNumber = 38031;

        public int PolyPopPortNumber
        {
            get { return this.polyPopPortNumber; }
            set
            {
                this.polyPopPortNumber = value;
                this.NotifyPropertyChanged();
            }
        }
        private int polyPopPortNumber;

        public ICommand ConnectCommand { get; set; }
        public ICommand DisconnectCommand { get; set; }

        public override string WikiPageName { get { return "polypop"; } }

        public PolyPopServiceControlViewModel()
            : base(Resources.PolyPop)
        {
            this.PolyPopPortNumber = PolyPopServiceControlViewModel.DefaultPolyPopPortNumber;
            if (ChannelSession.Settings.PolyPopPortNumber > 0)
            {
                this.PolyPopPortNumber = ChannelSession.Settings.PolyPopPortNumber;
            }

            this.ConnectCommand = this.CreateCommand(async () =>
            {
                ChannelSession.Settings.PolyPopPortNumber = this.PolyPopPortNumber;

                Result result = await ServiceManager.Get<PolyPopService>().Connect();
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
                await ServiceManager.Get<PolyPopService>().Disconnect();
                ChannelSession.Settings.PolyPopPortNumber = 0;
                this.IsConnected = false;
            });

            this.IsConnected = ServiceManager.Get<PolyPopService>().IsConnected;
        }
    }
}
