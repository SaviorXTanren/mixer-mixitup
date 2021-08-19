using MixItUp.Base.Util;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
{
    public class VoicemodServiceControlViewModel : ServiceControlViewModelBase
    {
        public ICommand ConnectCommand { get; set; }
        public ICommand DisconnectCommand { get; set; }

        public VoicemodServiceControlViewModel()
            : base(Resources.Voicemod)
        {
            this.ConnectCommand = this.CreateCommand(async () =>
            {
                Result result = await ChannelSession.Services.Voicemod.Connect();
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
                await ChannelSession.Services.Voicemod.Disconnect();
                ChannelSession.Settings.EnableVoicemodStudio = false;
                this.IsConnected = false;
            });

            this.IsConnected = ChannelSession.Services.Voicemod.IsConnected;
        }
    }
}