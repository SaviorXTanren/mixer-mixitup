using MixItUp.Base.ViewModel.Settings.Generic;
using MixItUp.Base.ViewModels;

namespace MixItUp.Base.ViewModel.Settings
{
    public class AlertsSettingsControlViewModel : UIViewModelBase
    {
        public GenericToggleSettingsOptionControlViewModel OnlyShowAlertsInDashboard { get; set; }

        public GenericColorComboBoxSettingsOptionControlViewModel UserJoinLeave { get; set; }
        public GenericColorComboBoxSettingsOptionControlViewModel Follow { get; set; }
        public GenericColorComboBoxSettingsOptionControlViewModel Host { get; set; }
        public GenericColorComboBoxSettingsOptionControlViewModel Raid { get; set; }
        public GenericColorComboBoxSettingsOptionControlViewModel Sub { get; set; }
        public GenericColorComboBoxSettingsOptionControlViewModel GiftedSub { get; set; }
        public GenericColorComboBoxSettingsOptionControlViewModel MassGiftedSub { get; set; }
        public GenericColorComboBoxSettingsOptionControlViewModel BitsCheered { get; set; }
        public GenericColorComboBoxSettingsOptionControlViewModel HypeTrain { get; set; }
        public GenericColorComboBoxSettingsOptionControlViewModel ChannelPoints { get; set; }
        public GenericColorComboBoxSettingsOptionControlViewModel Donation { get; set; }
        public GenericColorComboBoxSettingsOptionControlViewModel Streamloots { get; set; }
        public GenericColorComboBoxSettingsOptionControlViewModel Moderation { get; set; }

        public AlertsSettingsControlViewModel()
        {
            this.OnlyShowAlertsInDashboard = new GenericToggleSettingsOptionControlViewModel(MixItUp.Base.Resources.OnlyShowAlertsInDashboard, ChannelSession.Settings.OnlyShowAlertsInDashboard, (value) => { ChannelSession.Settings.OnlyShowAlertsInDashboard = value; });

            this.UserJoinLeave = new GenericToggleColorComboBoxSettingsControlViewModel(MixItUp.Base.Resources.ShowUserJoinLeave, ChannelSession.Settings.AlertUserJoinLeaveColor, (value) => { ChannelSession.Settings.AlertUserJoinLeaveColor = value; });
            this.Follow = new GenericToggleColorComboBoxSettingsControlViewModel(MixItUp.Base.Resources.ShowFollows, ChannelSession.Settings.AlertFollowColor, (value) => { ChannelSession.Settings.AlertFollowColor = value; });
            this.Host = new GenericToggleColorComboBoxSettingsControlViewModel(MixItUp.Base.Resources.ShowHosts, ChannelSession.Settings.AlertHostColor, (value) => { ChannelSession.Settings.AlertHostColor = value; });
            this.Raid = new GenericToggleColorComboBoxSettingsControlViewModel(MixItUp.Base.Resources.ShowRaids, ChannelSession.Settings.AlertRaidColor, (value) => { ChannelSession.Settings.AlertRaidColor = value; });
            this.Sub = new GenericToggleColorComboBoxSettingsControlViewModel(MixItUp.Base.Resources.ShowSubsResubs, ChannelSession.Settings.AlertSubColor, (value) => { ChannelSession.Settings.AlertSubColor = value; });
            this.GiftedSub = new GenericToggleColorComboBoxSettingsControlViewModel(MixItUp.Base.Resources.ShowGiftedSubs, ChannelSession.Settings.AlertGiftedSubColor, (value) => { ChannelSession.Settings.AlertGiftedSubColor = value; });
            this.MassGiftedSub = new GenericToggleColorComboBoxSettingsControlViewModel(MixItUp.Base.Resources.ShowMassGiftedSubs, ChannelSession.Settings.AlertMassGiftedSubColor, (value) => { ChannelSession.Settings.AlertMassGiftedSubColor = value; });
            this.BitsCheered = new GenericToggleColorComboBoxSettingsControlViewModel(MixItUp.Base.Resources.ShowBitsCheered, ChannelSession.Settings.AlertBitsCheeredColor, (value) => { ChannelSession.Settings.AlertBitsCheeredColor = value; });
            this.HypeTrain = new GenericToggleColorComboBoxSettingsControlViewModel(MixItUp.Base.Resources.ShowHypeTrain, ChannelSession.Settings.AlertHypeTrainColor, (value) => { ChannelSession.Settings.AlertHypeTrainColor = value; });
            this.ChannelPoints = new GenericToggleColorComboBoxSettingsControlViewModel(MixItUp.Base.Resources.ShowChannelPoints, ChannelSession.Settings.AlertChannelPointsColor, (value) => { ChannelSession.Settings.AlertChannelPointsColor = value; });
            this.Donation = new GenericToggleColorComboBoxSettingsControlViewModel(MixItUp.Base.Resources.ShowDonations, ChannelSession.Settings.AlertDonationColor, (value) => { ChannelSession.Settings.AlertDonationColor = value; });
            this.Streamloots = new GenericToggleColorComboBoxSettingsControlViewModel(MixItUp.Base.Resources.ShowStreamloots, ChannelSession.Settings.AlertStreamlootsColor, (value) => { ChannelSession.Settings.AlertStreamlootsColor = value; });
            this.Moderation = new GenericToggleColorComboBoxSettingsControlViewModel(MixItUp.Base.Resources.ShowModeration, ChannelSession.Settings.AlertModerationColor, (value) => { ChannelSession.Settings.AlertModerationColor = value; });
        }
    }
}
