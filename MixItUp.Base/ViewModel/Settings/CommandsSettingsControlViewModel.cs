using MixItUp.Base.Model.Actions;
using MixItUp.Base.Model.Requirements;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Settings.Generic;
using MixItUp.Base.ViewModels;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Linq;

namespace MixItUp.Base.ViewModel.Settings
{
    public class CommandsSettingsControlViewModel : UIViewModelBase
    {
        public GenericToggleSettingsOptionControlViewModel AllowCommandWhispering { get; set; }
        public GenericToggleSettingsOptionControlViewModel IgnoreBotAccount { get; set; }
        public GenericToggleSettingsOptionControlViewModel DeleteChatCommandsWhenRun { get; set; }
        public GenericToggleNumberSettingsOptionControlViewModel MassGiftedSubsFilterAmount { get; set; }
        public GenericToggleSettingsOptionControlViewModel UserEntranceCommandsOnlyWhenLive { get; set; }
        public GenericComboBoxSettingsOptionControlViewModel<CommandServiceLockTypeEnum> CommandLockSystem { get; set; }

        public GenericComboBoxSettingsOptionControlViewModel<RequirementErrorCooldownTypeEnum> RequirementErrorsCooldownType { get; set; }
        public GenericNumberSettingsOptionControlViewModel RequirementErrorsCooldownAmount { get; set; }
        public GenericToggleSettingsOptionControlViewModel IncludeUsernameWithRequirementErrors { get; set; }
        public GenericTextSettingsOptionControlViewModel DelimitedArgumentSeparator { get; set; }

        public GenericToggleSettingsOptionControlViewModel TwitchReplyToCommandChatMessages { get; set; }

        public ThreadSafeObservableCollection<GenericToggleSettingsOptionControlViewModel> HideActionsList { get; set; } = new ThreadSafeObservableCollection<GenericToggleSettingsOptionControlViewModel>();

        public CommandsSettingsControlViewModel()
        {
            this.AllowCommandWhispering = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.AllowCommandsToBeWhispered,
                ChannelSession.Settings.AllowCommandWhispering, (value) => { ChannelSession.Settings.AllowCommandWhispering = value; });
            this.IgnoreBotAccount = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.IgnoreYourBotAccountForCommands,
                ChannelSession.Settings.IgnoreBotAccountCommands, (value) => { ChannelSession.Settings.IgnoreBotAccountCommands = value; });
            this.DeleteChatCommandsWhenRun = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.DeleteChatCommandsWhenRun,
                ChannelSession.Settings.DeleteChatCommandsWhenRun, (value) => { ChannelSession.Settings.DeleteChatCommandsWhenRun = value; });
            this.MassGiftedSubsFilterAmount = new GenericToggleNumberSettingsOptionControlViewModel(MixItUp.Base.Resources.MassGiftedSubsFilterAmount, ChannelSession.Settings.MassGiftedSubsFilterAmount,
                (value) => { ChannelSession.Settings.MassGiftedSubsFilterAmount = value; }, MixItUp.Base.Resources.MassGiftedSubsFilterAmountTooltip);
            this.UserEntranceCommandsOnlyWhenLive = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.IgnoreYourBotAccountForCommands,
                ChannelSession.Settings.UserEntranceCommandsOnlyWhenLive, (value) => { ChannelSession.Settings.UserEntranceCommandsOnlyWhenLive = value; });
            this.CommandLockSystem = new GenericComboBoxSettingsOptionControlViewModel<CommandServiceLockTypeEnum>(MixItUp.Base.Resources.CommandLockSystem, EnumHelper.GetEnumList<CommandServiceLockTypeEnum>(),
                ChannelSession.Settings.CommandServiceLockType, (value) => { ChannelSession.Settings.CommandServiceLockType = value; }, MixItUp.Base.Resources.CommandLockSystemTooltip);

            this.RequirementErrorsCooldownType = new GenericComboBoxSettingsOptionControlViewModel<RequirementErrorCooldownTypeEnum>(MixItUp.Base.Resources.RequirementErrorsCooldownType, EnumHelper.GetEnumList<RequirementErrorCooldownTypeEnum>(),
                ChannelSession.Settings.RequirementErrorsCooldownType, (value) => { ChannelSession.Settings.RequirementErrorsCooldownType = value; }, MixItUp.Base.Resources.RequirementErrorsCooldownTypeTooltip);
            this.RequirementErrorsCooldownAmount = new GenericNumberSettingsOptionControlViewModel(MixItUp.Base.Resources.RequirementErrorsCooldownAmount,
                ChannelSession.Settings.RequirementErrorsCooldownAmount, (value) => { ChannelSession.Settings.RequirementErrorsCooldownAmount = value; });
            this.IncludeUsernameWithRequirementErrors = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.IncludeUsernameWithRequirementErrors,
                ChannelSession.Settings.IncludeUsernameWithRequirementErrors, (value) => { ChannelSession.Settings.IncludeUsernameWithRequirementErrors = value; });
            this.DelimitedArgumentSeparator = new GenericTextSettingsOptionControlViewModel(MixItUp.Base.Resources.DelimitedArgumentsSeparator,
                ChannelSession.Settings.DelimitedArgumentsSeparator, (value) => { ChannelSession.Settings.DelimitedArgumentsSeparator = value; });

            this.TwitchReplyToCommandChatMessages = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.TwitchReplyToCommandChatMessages, ChannelSession.Settings.TwitchReplyToCommandChatMessages,
                (value) => { ChannelSession.Settings.TwitchReplyToCommandChatMessages = value; });

            List<ActionTypeEnum> actions = new List<ActionTypeEnum>(EnumHelper.GetEnumList<ActionTypeEnum>());
            actions.Remove(ActionTypeEnum.Custom);
            foreach (ActionTypeEnum action in actions.OrderBy(at => EnumLocalizationHelper.GetLocalizedName(at)))
            {
                string name = EnumHelper.GetEnumName(action);
                name = MixItUp.Base.Resources.ResourceManager.GetSafeString(name);
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
