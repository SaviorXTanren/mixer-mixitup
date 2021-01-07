using MixItUp.Base.Model.Requirements;

namespace MixItUp.Base.ViewModel.Requirements
{
    public class SettingsRequirementViewModel : RequirementViewModelBase
    {
        public bool ShowOnChatContextMenu
        {
            get { return this.showOnChatContextMenu; }
            set
            {
                this.showOnChatContextMenu = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool showOnChatContextMenu;

        public bool DeleteChatMessageWhenRun
        {
            get { return this.deleteChatMessageWhenRun; }
            set
            {
                this.deleteChatMessageWhenRun = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool deleteChatMessageWhenRun;

        public bool ShowDeleteChatMessageWhenRun { get { return !ChannelSession.Settings.DeleteChatCommandsWhenRun; } }

        public bool ShowDontDeleteChatMessageWhenRun { get { return ChannelSession.Settings.DeleteChatCommandsWhenRun; } }

        public SettingsRequirementViewModel() { }

        public SettingsRequirementViewModel(SettingsRequirementModel requirement)
        {
            this.ShowOnChatContextMenu = requirement.ShowOnChatContextMenu;
            if (ChannelSession.Settings.DeleteChatCommandsWhenRun)
            {
                this.DeleteChatMessageWhenRun = requirement.DontDeleteChatMessageWhenRun;
            }
            else
            {
                this.DeleteChatMessageWhenRun = requirement.DeleteChatMessageWhenRun;
            }
        }

        public override RequirementModelBase GetRequirement()
        {
            return new SettingsRequirementModel()
            {
                ShowOnChatContextMenu = this.ShowOnChatContextMenu,
                DeleteChatMessageWhenRun = (!ChannelSession.Settings.DeleteChatCommandsWhenRun) ? this.DeleteChatMessageWhenRun : false,
                DontDeleteChatMessageWhenRun = ChannelSession.Settings.DeleteChatCommandsWhenRun ? this.DeleteChatMessageWhenRun : false
            };
        }
    }
}
