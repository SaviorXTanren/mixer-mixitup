using MixItUp.Base.ViewModel.Settings.Generic;
using MixItUp.Base.ViewModels;

namespace MixItUp.Base.ViewModel.Settings
{
    public class AlertsSettingsControlViewModel : UIViewModelBase
    {
        public GenericToggleSettingsOptionControlViewModel OnlyShowAlertsInDashboard { get; set; }

        public GenericColorComboBoxSettingsOptionControlViewModel UserJoinLeave { get; set; }
        public GenericColorComboBoxSettingsOptionControlViewModel UserFirstMessage { get; set; }
        public GenericColorComboBoxSettingsOptionControlViewModel Follow { get; set; }
        public GenericColorComboBoxSettingsOptionControlViewModel Host { get; set; }
        public GenericColorComboBoxSettingsOptionControlViewModel Raid { get; set; }
        public GenericColorComboBoxSettingsOptionControlViewModel Sub { get; set; }
        public GenericColorComboBoxSettingsOptionControlViewModel GiftedSub { get; set; }
        public GenericColorComboBoxSettingsOptionControlViewModel MassGiftedSub { get; set; }
        public GenericColorComboBoxSettingsOptionControlViewModel TwitchBitsCheered { get; set; }
        public GenericColorComboBoxSettingsOptionControlViewModel TwitchChannelPoints { get; set; }
        public GenericColorComboBoxSettingsOptionControlViewModel TwitchHypeTrain { get; set; }
        public GenericColorComboBoxSettingsOptionControlViewModel TwitchAds { get; set; }
        public GenericColorComboBoxSettingsOptionControlViewModel YouTubeSuperChat { get; set; }
        public GenericColorComboBoxSettingsOptionControlViewModel TrovoSpellCast { get; set; }
        public GenericColorComboBoxSettingsOptionControlViewModel Donation { get; set; }
        public GenericColorComboBoxSettingsOptionControlViewModel Streamloots { get; set; }
        public GenericColorComboBoxSettingsOptionControlViewModel Moderation { get; set; }

        public AlertsSettingsControlViewModel()
        {
            this.OnlyShowAlertsInDashboard = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.OnlyShowAlertsInDashboard, ChannelSession.Settings.OnlyShowAlertsInDashboard, (value) => { ChannelSession.Settings.OnlyShowAlertsInDashboard = value; });

            this.UserJoinLeave = new GenericToggleColorComboBoxSettingsControlViewModel(MixItUp.Base.Resources.ShowUserJoinLeave, ChannelSession.Settings.AlertUserJoinLeaveColor, (value) => { ChannelSession.Settings.AlertUserJoinLeaveColor = value; });
            this.UserFirstMessage = new GenericToggleColorComboBoxSettingsControlViewModel(MixItUp.Base.Resources.ShowUserFirstMessage, ChannelSession.Settings.AlertUserFirstMessageColor, (value) => { ChannelSession.Settings.AlertUserFirstMessageColor = value; });
            this.Follow = new GenericToggleColorComboBoxSettingsControlViewModel(MixItUp.Base.Resources.ShowFollows, ChannelSession.Settings.AlertFollowColor, (value) => { ChannelSession.Settings.AlertFollowColor = value; });
            this.Host = new GenericToggleColorComboBoxSettingsControlViewModel(MixItUp.Base.Resources.ShowHosts, ChannelSession.Settings.AlertHostColor, (value) => { ChannelSession.Settings.AlertHostColor = value; });
            this.Raid = new GenericToggleColorComboBoxSettingsControlViewModel(MixItUp.Base.Resources.ShowRaids, ChannelSession.Settings.AlertRaidColor, (value) => { ChannelSession.Settings.AlertRaidColor = value; });
            this.Sub = new GenericToggleColorComboBoxSettingsControlViewModel(MixItUp.Base.Resources.ShowSubsResubs, ChannelSession.Settings.AlertSubColor, (value) => { ChannelSession.Settings.AlertSubColor = value; });
            this.GiftedSub = new GenericToggleColorComboBoxSettingsControlViewModel(MixItUp.Base.Resources.ShowGiftedSubs, ChannelSession.Settings.AlertGiftedSubColor, (value) => { ChannelSession.Settings.AlertGiftedSubColor = value; });
            this.MassGiftedSub = new GenericToggleColorComboBoxSettingsControlViewModel(MixItUp.Base.Resources.ShowMassGiftedSubs, ChannelSession.Settings.AlertMassGiftedSubColor, (value) => { ChannelSession.Settings.AlertMassGiftedSubColor = value; });
            this.TwitchBitsCheered = new GenericToggleColorComboBoxSettingsControlViewModel(MixItUp.Base.Resources.ShowTwitchBitsCheered, ChannelSession.Settings.AlertTwitchBitsCheeredColor, (value) => { ChannelSession.Settings.AlertTwitchBitsCheeredColor = value; });
            this.TwitchChannelPoints = new GenericToggleColorComboBoxSettingsControlViewModel(MixItUp.Base.Resources.ShowTwitchChannelPoints, ChannelSession.Settings.AlertTwitchChannelPointsColor, (value) => { ChannelSession.Settings.AlertTwitchChannelPointsColor = value; });
            this.TwitchHypeTrain = new GenericToggleColorComboBoxSettingsControlViewModel(MixItUp.Base.Resources.ShowTwitchHypeTrain, ChannelSession.Settings.AlertTwitchHypeTrainColor, (value) => { ChannelSession.Settings.AlertTwitchHypeTrainColor = value; });
            this.TwitchAds = new GenericToggleColorComboBoxSettingsControlViewModel(MixItUp.Base.Resources.ShowTwitchAds, ChannelSession.Settings.AlertTwitchAdsColor, (value) => { ChannelSession.Settings.AlertTwitchAdsColor = value; });
            this.YouTubeSuperChat = new GenericToggleColorComboBoxSettingsControlViewModel(MixItUp.Base.Resources.ShowYouTubeSuperChat, ChannelSession.Settings.AlertYouTubeSuperChatColor, (value) => { ChannelSession.Settings.AlertYouTubeSuperChatColor = value; });
            this.TrovoSpellCast = new GenericToggleColorComboBoxSettingsControlViewModel(MixItUp.Base.Resources.ShowTrovoSpellCast, ChannelSession.Settings.AlertTrovoSpellCastColor, (value) => { ChannelSession.Settings.AlertTrovoSpellCastColor = value; });
            this.Donation = new GenericToggleColorComboBoxSettingsControlViewModel(MixItUp.Base.Resources.ShowDonations, ChannelSession.Settings.AlertDonationColor, (value) => { ChannelSession.Settings.AlertDonationColor = value; });
            this.Streamloots = new GenericToggleColorComboBoxSettingsControlViewModel(MixItUp.Base.Resources.ShowStreamloots, ChannelSession.Settings.AlertStreamlootsColor, (value) => { ChannelSession.Settings.AlertStreamlootsColor = value; });
            this.Moderation = new GenericToggleColorComboBoxSettingsControlViewModel(MixItUp.Base.Resources.ShowModeration, ChannelSession.Settings.AlertModerationColor, (value) => { ChannelSession.Settings.AlertModerationColor = value; });
        }
    }
}
