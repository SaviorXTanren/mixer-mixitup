using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
{
    public class VoicemodServiceControlViewModel : ServiceControlViewModelBase
    {
        public ICommand ConnectCommand { get; set; }
        public ICommand DisconnectCommand { get; set; }

        public override string WikiPageName { get { return "voicemod"; } }

        public VoicemodServiceControlViewModel()
            : base(Resources.Voicemod)
        {
            this.ConnectCommand = this.CreateCommand(async () =>
            {
                Result result = await ServiceManager.Get<IVoicemodService>().Connect();
                if (result.Success)
                {
                    this.IsConnected = true;
                    ChannelSession.Settings.EnableVoicemodStudio = true;
                }
                else
                {
                    await this.ShowConnectFailureMessage(result);
                }
            });

            this.DisconnectCommand = this.CreateCommand(async () =>
            {
                await ServiceManager.Get<IVoicemodService>().Disconnect();
                ChannelSession.Settings.EnableVoicemodStudio = false;
                this.IsConnected = false;
            });

            this.IsConnected = ServiceManager.Get<IVoicemodService>().IsConnected;
        }
    }
}