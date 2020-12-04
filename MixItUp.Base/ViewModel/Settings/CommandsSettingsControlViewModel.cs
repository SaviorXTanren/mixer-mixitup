using MixItUp.Base.Model.Actions;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Settings.Generic;
using MixItUp.Base.ViewModels;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MixItUp.Base.ViewModel.Settings
{
    public class CommandsSettingsControlViewModel : UIViewModelBase
    {
        public GenericToggleSettingsOptionControlViewModel AllowCommandWhispering { get; set; }
        public GenericToggleSettingsOptionControlViewModel IgnoreBotAccount { get; set; }
        public GenericToggleSettingsOptionControlViewModel DeleteChatCommandsWhenRun { get; set; }
        public GenericToggleSettingsOptionControlViewModel UnlockAllCommandTypes { get; set; }

        public GenericToggleNumberSettingsOptionControlViewModel TwitchMassGiftedSubsFilterAmount { get; set; }

        public ObservableCollection<GenericToggleSettingsOptionControlViewModel> HideActionsList { get; set; } = new ObservableCollection<GenericToggleSettingsOptionControlViewModel>();

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

            this.TwitchMassGiftedSubsFilterAmount = new GenericToggleNumberSettingsOptionControlViewModel(MixItUp.Base.Resources.TwitchMassGiftedSubsFilterAmount, ChannelSession.Settings.TwitchMassGiftedSubsFilterAmount,
                (value) => { ChannelSession.Settings.TwitchMassGiftedSubsFilterAmount = value; }, MixItUp.Base.Resources.TwitchMassGiftedSubsFilterAmountTooltip);

            List<ActionTypeEnum> actions = new List<ActionTypeEnum>(EnumHelper.GetEnumList<ActionTypeEnum>());
            actions.Remove(ActionTypeEnum.Custom);
            foreach (ActionTypeEnum action in actions.OrderBy(at => EnumLocalizationHelper.GetLocalizedName(at)))
            {
                string name = EnumHelper.GetEnumName(action);
                name = MixItUp.Base.Resources.ResourceManager.GetString(name) ?? name;
                this.HideActionsList.Add(new GenericToggleSettingsOptionControlViewModel(name, ChannelSession.Settings.ActionsToHide.Contains(action),
                    (value) =>
                    {
                        if (value)
                        {
                            ChannelSession.Settings.ActionsToHide.Add(action);
                        }
                        else
                        {
                            ChannelSession.Settings.ActionsToHide.Remove(action);
                        }
                    }));
            }
        }
    }
}
