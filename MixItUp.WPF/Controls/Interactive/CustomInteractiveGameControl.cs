using Mixer.Base.Model.Interactive;
using MixItUp.Base;
using MixItUp.Base.MixerAPI;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Interactive
{
    public class CustomInteractiveGameControl : UserControl
    {
        public InteractiveGameModel Game { get; private set; }
        public InteractiveGameVersionModel Version { get; private set; }

        public CustomInteractiveGameControl(InteractiveGameModel game, InteractiveGameVersionModel version)
        {
            this.Game = game;
            this.Version = version;

            this.Loaded += CustomInteractiveGameControl_Loaded;
        }

        public async Task GameConnected()
        {
            ChannelSession.Interactive.OnInteractiveControlUsed += Interactive_OnInteractiveControlUsed;
            await this.GameConnectedInternal();
        }

        public async Task GameDisconnected()
        {
            ChannelSession.Interactive.OnInteractiveControlUsed -= Interactive_OnInteractiveControlUsed;
            await this.GameDisconnectedInternal();
        }

        protected virtual Task OnLoaded() { return Task.FromResult(0); }

        protected virtual Task GameConnectedInternal() { return Task.FromResult(0); }

        protected virtual Task GameDisconnectedInternal() { return Task.FromResult(0); }

        protected virtual Task OnInteractiveControlUsed(UserViewModel user, InteractiveGiveInputModel input, InteractiveConnectedControlCommand command) { return Task.FromResult(0); }

        protected JObject GetCustomSettings()
        {
            if (ChannelSession.Settings.CustomInteractiveSettings.ContainsKey(this.Game.id))
            {
                return ChannelSession.Settings.CustomInteractiveSettings[this.Game.id];
            }
            return new JObject();
        }

        protected void SaveCustomSettings(JObject settings)
        {
            ChannelSession.Settings.CustomInteractiveSettings[this.Game.id] = settings;
        }

        private async void CustomInteractiveGameControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.OnLoaded();
        }

        private async void Interactive_OnInteractiveControlUsed(object sender, InteractiveInputEvent e)
        {
            await this.OnInteractiveControlUsed(e.User, e.Input, e.Command);
        }
    }
}
