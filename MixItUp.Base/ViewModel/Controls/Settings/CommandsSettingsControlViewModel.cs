using MixItUp.Base.ViewModel.Controls.Settings.Generic;
using MixItUp.Base.ViewModels;

namespace MixItUp.Base.ViewModel.Controls.Settings
{
    public class CommandsSettingsControlViewModel : UIViewModelBase
    {
        public GenericToggleSettingsOptionControlViewModel AllowCommandWhispering { get; set; }
        public GenericToggleSettingsOptionControlViewModel IgnoreBotAccount { get; set; }
        public GenericToggleSettingsOptionControlViewModel DeleteChatCommandsWhenRun { get; set; }
        public GenericToggleSettingsOptionControlViewModel UnlockAllCommandTypes { get; set; }

        public CommandsSettingsControlViewModel()
        {
            this.AllowCommandWhispering = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.AllowCommandsToBeWhispered,
                ChannelSession.Settings.AllowCommandWhispering, (value) => { ChannelSession.Settings.AllowCommandWhispering = value; });

            this.IgnoreBotAccount = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.IgnoreYourBotAccountForCommands,
                ChannelSession.Settings.IgnoreBotAccountCommands, (value) => { ChannelSession.Settings.IgnoreBotAccountCommands = value; });

            this.DeleteChatCommandsWhenRun = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.DeleteChatCommandsWhenRun,
                ChannelSession.Settings.DeleteChatCommandsWhenRun, (value) => { ChannelSession.Settings.DeleteChatCommandsWhenRun = value; });

            this.UnlockAllCommandTypes = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.UnlockAllCommandTypes,
                ChannelSession.Settings.UnlockAllCommands, (value) => { ChannelSession.Settings.UnlockAllCommands = value; }, MixItUp.Base.Resources.UnlockAllCommandTypesTooltip);
        }
    }
}
