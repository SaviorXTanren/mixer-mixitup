using Mixer.Base.Model.MixPlay;
using MixItUp.Base;
using MixItUp.Base.Services.Mixer;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Interactive
{
    public class CustomInteractiveGameControl : UserControl
    {
        public MixPlayGameModel Game { get; private set; }
        public MixPlayGameVersionModel Version { get; private set; }

        public CustomInteractiveGameControl(MixPlayGameModel game, MixPlayGameVersionModel version)
        {
            this.Game = game;
            this.Version = version;

            this.Loaded += CustomInteractiveGameControl_Loaded;
        }

        public async Task<bool> GameConnected()
        {
            ChannelSession.Services.MixPlay.OnControlUsed += MixPlay_OnControlUsed;
            return await this.GameConnectedInternal();
        }

        public async Task GameDisconnected()
        {
            ChannelSession.Services.MixPlay.OnControlUsed -= MixPlay_OnControlUsed;
            await this.GameDisconnectedInternal();
        }

        protected virtual Task OnLoaded() { return Task.FromResult(0); }

        protected virtual Task<bool> GameConnectedInternal() { return Task.FromResult(true); }

        protected virtual Task GameDisconnectedInternal() { return Task.FromResult(0); }

        protected virtual Task OnMixPlayControlUsed(UserViewModel user, MixPlayGiveInputModel input, MixPlayControlModel control) { return Task.FromResult(0); }

        protected JObject GetCustomSettings()
        {
            if (ChannelSession.Settings.CustomMixPlaySettings.ContainsKey(this.Game.id))
            {
                return ChannelSession.Settings.CustomMixPlaySettings[this.Game.id];
            }
            return new JObject();
        }

        protected void SaveCustomSettings(JObject settings)
        {
            ChannelSession.Settings.CustomMixPlaySettings[this.Game.id] = settings;
        }

        private async void CustomInteractiveGameControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.OnLoaded();
        }

        private async void MixPlay_OnControlUsed(object sender, MixPlayInputEvent e)
        {
            await this.OnMixPlayControlUsed(e.User, e.Input, e.Control);
        }
    }
}
