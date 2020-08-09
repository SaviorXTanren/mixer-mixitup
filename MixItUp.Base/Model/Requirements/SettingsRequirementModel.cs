using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Requirements
{
    public class SettingsRequirementModel : RequirementModelBase
    {
        [DataMember]
        public bool ShowOnChatContextMenu { get; set; }

        public SettingsRequirementModel() { }

        internal SettingsRequirementModel(MixItUp.Base.ViewModel.Requirement.SettingsRequirementViewModel requirement)
            : this()
        {
            this.ShowOnChatContextMenu = requirement.ShowOnChatMenu;
        }
    }
}
