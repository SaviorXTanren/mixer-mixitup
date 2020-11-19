using MixItUp.Base.Model.Requirements;

namespace MixItUp.Base.ViewModel.Requirements
{
    public class SettingsRequirementViewModel : RequirementViewModelBase
    {
        public SettingsRequirementViewModel() { }

        public SettingsRequirementViewModel(SettingsRequirementModel requirement)
        {

        }

        public override RequirementModelBase GetRequirement()
        {
            return new SettingsRequirementModel();
        }
    }
}
