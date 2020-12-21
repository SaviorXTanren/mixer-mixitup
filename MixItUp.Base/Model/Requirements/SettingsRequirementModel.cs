using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Requirements
{
    public class SettingsRequirementModel : RequirementModelBase
    {
        [DataMember]
        public bool DeleteChatMessageWhenRun { get; set; }
        [DataMember]
        public bool DontDeleteChatMessageWhenRun { get; set; }

        [DataMember]
        public bool ShowOnChatContextMenu { get; set; }

        [DataMember]
        public bool RunOneRandomly { get; set; }

        public SettingsRequirementModel() { }

        internal SettingsRequirementModel(MixItUp.Base.ViewModel.Requirement.SettingsRequirementViewModel requirement)
            : this()
        {
            this.DeleteChatMessageWhenRun = requirement.DeleteChatCommandWhenRun;
            this.DontDeleteChatMessageWhenRun = requirement.DontDeleteChatCommandWhenRun;
            this.ShowOnChatContextMenu = requirement.ShowOnChatMenu;
        }
    }
}
