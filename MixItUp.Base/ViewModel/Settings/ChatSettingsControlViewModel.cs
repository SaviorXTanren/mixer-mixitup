using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Settings.Generic;
using MixItUp.Base.ViewModels;
using StreamingClient.Base.Util;
using System.Collections.ObjectModel;
using System.Linq;

namespace MixItUp.Base.ViewModel.Settings
{
    public class ChatSettingsControlViewModel : UIViewModelBase
    {
        public GenericToggleSettingsOptionControlViewModel SaveChatEventLogs { get; set; }
        public GenericSliderSettingsOptionControlViewModel FontSize { get; set; }
        public GenericToggleSettingsOptionControlViewModel AddSeparatorsBetweenMessages { get; set; }
        public GenericToggleSettingsOptionControlViewModel UseAlternatingBackgroundColors { get; set; }
        public GenericToggleSettingsOptionControlViewModel DisableAnimatedEmotes { get; set; }

        public GenericToggleSettingsOptionControlViewModel ShowLatestChatMessagesAtTop { get; set; }
        public GenericToggleSettingsOptionControlViewModel ShowMessageTimestamp { get; set; }
        public GenericToggleSettingsOptionControlViewModel TrackWhisperNumbers { get; set; }

        public GenericToggleSettingsOptionControlViewModel HideViewerChatterCount { get; set; }
        public GenericToggleSettingsOptionControlViewModel HideChatUserList { get; set; }
        public GenericToggleSettingsOptionControlViewModel HideDeletedMessages { get; set; }
        public GenericToggleSettingsOptionControlViewModel HideBotMessages { get; set; }

        public GenericToggleSettingsOptionControlViewModel ShowAlejoPronouns { get; set; }
        public GenericToggleSettingsOptionControlViewModel ShowBetterTTVEmotes { get; set; }
        public GenericToggleSettingsOptionControlViewModel ShowFrankerFaceZEmotes { get; set; }

        public GenericToggleSettingsOptionControlViewModel HideUserAvatar { get; set; }
        public GenericToggleSettingsOptionControlViewModel HideUserRoleBadge { get; set; }
        public GenericToggleSettingsOptionControlViewModel HideUserSubscriberBadge { get; set; }
        public GenericToggleSettingsOptionControlViewModel HideUserSpecialtyBadge { get; set; }
        public GenericToggleSettingsOptionControlViewModel UseCustomUsernameColors { get; set; }
        public ObservableCollection<GenericColorComboBoxSettingsOptionControlViewModel> CustomUsernameColorsList { get; set; } = new ObservableCollection<GenericColorComboBoxSettingsOptionControlViewModel>();

        public ChatSettingsControlViewModel()
        {
            this.SaveChatEventLogs = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.SaveChatEventLogs, ChannelSession.Settings.SaveChatEventLogs,
                (value) => { ChannelSession.Settings.SaveChatEventLogs = value; });
            this.FontSize = new GenericSliderSettingsOptionControlViewModel(MixItUp.Base.Resources.FontSize, ChannelSession.Settings.ChatFontSize, 6, 100,
                (value) =>
                {
                    ChannelSession.Settings.ChatFontSize = value;
                    GlobalEvents.ChatVisualSettingsChanged();
                });
            this.AddSeparatorsBetweenMessages = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.AddSeparatorsBetweenMessages, ChannelSession.Settings.AddSeparatorsBetweenMessages,
                (value) =>
                {
                    ChannelSession.Settings.AddSeparatorsBetweenMessages = value;
                    GlobalEvents.ChatVisualSettingsChanged();
                });
            this.UseAlternatingBackgroundColors = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.UseAlternatingBackgroundColors, ChannelSession.Settings.UseAlternatingBackgroundColors,
                (value) =>
                {
                    ChannelSession.Settings.UseAlternatingBackgroundColors = value;
                    GlobalEvents.ChatVisualSettingsChanged();
                });
            this.DisableAnimatedEmotes = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.DisableAnimatedEmotes, ChannelSession.Settings.DisableAnimatedEmotes,
                (value) =>
                {
                    ChannelSession.Settings.DisableAnimatedEmotes = value;
                    GlobalEvents.ChatVisualSettingsChanged();
                });

            this.ShowLatestChatMessagesAtTop = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.ShowLatestChatMessagesAtTopInsteadOfBottom, ChannelSession.Settings.LatestChatAtTop,
                (value) => { ChannelSession.Settings.LatestChatAtTop = value; });
            this.ShowMessageTimestamp = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.ShowMessageTimestamp, ChannelSession.Settings.ShowChatMessageTimestamps,
                (value) => { ChannelSession.Settings.ShowChatMessageTimestamps = value; });
            this.TrackWhisperNumbers = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.TrackWhispererNumbers, ChannelSession.Settings.TrackWhispererNumber,
                (value) => { ChannelSession.Settings.TrackWhispererNumber = value; });

            this.HideViewerChatterCount = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.HideViewerAndChatterNumbers, ChannelSession.Settings.HideViewerAndChatterNumbers,
                (value) => { ChannelSession.Settings.HideViewerAndChatterNumbers = value; });
            this.HideChatUserList = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.HideChatUserList, ChannelSession.Settings.HideChatUserList,
                (value) => { ChannelSession.Settings.HideChatUserList = value; });
            this.HideDeletedMessages = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.HideDeletedMessages, ChannelSession.Settings.HideDeletedMessages,
                (value) => { ChannelSession.Settings.HideDeletedMessages = value; });
            this.HideBotMessages = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.HideBotMessages, ChannelSession.Settings.HideBotMessages,
                (value) => { ChannelSession.Settings.HideBotMessages = value; });

            this.ShowAlejoPronouns = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.ShowAlejoPronouns, ChannelSession.Settings.ShowAlejoPronouns,
                (value) => { ChannelSession.Settings.ShowAlejoPronouns = value; });
            this.ShowBetterTTVEmotes = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.ShowBetterTTVEmotes, ChannelSession.Settings.ShowBetterTTVEmotes,
                (value) => { ChannelSession.Settings.ShowBetterTTVEmotes = value; });
            this.ShowFrankerFaceZEmotes = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.ShowFrankerFaceZEmotes, ChannelSession.Settings.ShowFrankerFaceZEmotes,
                (value) => { ChannelSession.Settings.ShowFrankerFaceZEmotes = value; });

            this.HideUserAvatar = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.HideUserAvatar, ChannelSession.Settings.HideUserAvatar,
                (value) =>
                {
                    ChannelSession.Settings.HideUserAvatar = value;
                    GlobalEvents.ChatVisualSettingsChanged();
                });
            this.HideUserRoleBadge = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.HideUserRoleBadge, ChannelSession.Settings.HideUserRoleBadge,
                (value) =>
                {
                    ChannelSession.Settings.HideUserRoleBadge = value;
                    GlobalEvents.ChatVisualSettingsChanged();
                });
            this.HideUserSubscriberBadge = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.HideUserSubscriberBadge, ChannelSession.Settings.HideUserSubscriberBadge,
                (value) =>
                {
                    ChannelSession.Settings.HideUserSubscriberBadge = value;
                    GlobalEvents.ChatVisualSettingsChanged();
                });
            this.HideUserSpecialtyBadge = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.HideUserSpecialtyBadge, ChannelSession.Settings.HideUserSpecialtyBadge,
                (value) =>
                {
                    ChannelSession.Settings.HideUserSpecialtyBadge = value;
                    GlobalEvents.ChatVisualSettingsChanged();
                });

            this.UseCustomUsernameColors = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.UseCustomUsernameColors, ChannelSession.Settings.UseCustomUsernameColors,
                (value) =>
                {
                    ChannelSession.Settings.UseCustomUsernameColors = value;
                    this.EnableDisableUsernameColors();
                    GlobalEvents.ChatVisualSettingsChanged();
                });

            foreach (UserRoleEnum role in UserRoles.All.OrderBy(r => r))
            {
                string name = EnumHelper.GetEnumName(role);
                name = MixItUp.Base.Resources.ResourceManager.GetSafeString(name);
                this.CustomUsernameColorsList.Add(new GenericColorComboBoxSettingsOptionControlViewModel(name,
                    ChannelSession.Settings.CustomUsernameRoleColors.ContainsKey(role) ? ChannelSession.Settings.CustomUsernameRoleColors[role] : null,
                    (value) =>
                    {
                        if (!string.IsNullOrEmpty(value) && !value.Equals(GenericColorComboBoxSettingsOptionControlViewModel.NoneOption))
                        {
                            ChannelSession.Settings.CustomUsernameRoleColors[role] = value;
                        }
                        else
                        {
                            ChannelSession.Settings.CustomUsernameRoleColors.Remove(role);
                        }
                        GlobalEvents.ChatVisualSettingsChanged();
                    }));
            }

            foreach (GenericColorComboBoxSettingsOptionControlViewModel colorOption in this.CustomUsernameColorsList)
            {
                colorOption.AddNoneOption();
            }

            this.EnableDisableUsernameColors();
        }

        private void EnableDisableUsernameColors()
        {
            foreach (GenericColorComboBoxSettingsOptionControlViewModel colorOption in this.CustomUsernameColorsList)
            {
                colorOption.Enabled = ChannelSession.Settings.UseCustomUsernameColors;
            }
        }
    }
}
