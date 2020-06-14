using MixItUp.Base.Model.Requirements;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Requirements
{
    public class RequirementsSetViewModel
    {
        public RoleRequirementViewModel Role { get; set; } = new RoleRequirementViewModel();

        public CooldownRequirementViewModel Cooldown { get; set; } = new CooldownRequirementViewModel();

        public List<CurrencyRequirementViewModel> Currency { get; set; } = new List<CurrencyRequirementViewModel>();

        public List<RankRequirementViewModel> Rank { get; set; } = new List<RankRequirementViewModel>();

        public List<InventoryRequirementViewModel> Inventory { get; set; } = new List<InventoryRequirementViewModel>();

        public ThresholdRequirementViewModel Threshold { get; set; } = new ThresholdRequirementViewModel();

        public IEnumerable<RequirementViewModelBase> Requirements
        {
            get
            {
                List<RequirementViewModelBase> requirements = new List<RequirementViewModelBase>();
                requirements.Add(this.Role);
                requirements.Add(this.Cooldown);
                requirements.AddRange(this.Currency);
                requirements.AddRange(this.Rank);
                requirements.AddRange(this.Inventory);
                requirements.Add(this.Threshold);
                return requirements;
            }
        }

        public RequirementsSetViewModel() { }

        public RequirementsSetViewModel(RequirementsSetModel requirements)
        {
            foreach (RequirementModelBase requirement in requirements.Requirements)
            {
                if (requirement is RoleRequirementModel)
                {
                    this.Role = new RoleRequirementViewModel((RoleRequirementModel)requirement);
                }
                else if (requirement is CooldownRequirementModel)
                {
                    this.Cooldown = new CooldownRequirementViewModel((CooldownRequirementModel)requirement);
                }
                else if (requirement is CurrencyRequirementModel)
                {
                    this.Currency.Add(new CurrencyRequirementViewModel((CurrencyRequirementModel)requirement));
                }
                else if (requirement is RankRequirementModel)
                {
                    this.Rank.Add(new RankRequirementViewModel((RankRequirementModel)requirement));
                }
                else if (requirement is InventoryRequirementModel)
                {
                    this.Inventory.Add(new InventoryRequirementViewModel((InventoryRequirementModel)requirement));
                }
                else if (requirement is ThresholdRequirementModel)
                {
                    this.Threshold = new ThresholdRequirementViewModel((ThresholdRequirementModel)requirement);
                }
            }
        }

        public async Task<bool> Validate()
        {
            foreach (RequirementViewModelBase requirement in this.Requirements)
            {
                if (!await requirement.Validate())
                {
                    return false;
                }
            }
            return true;
        }

        public RequirementsSetModel GetRequirements()
        {
            RequirementsSetModel requirements = new RequirementsSetModel();
            foreach (RequirementViewModelBase requirement in this.Requirements)
            {
                requirements.Requirements.Add(requirement.GetRequirement());
            }
            return requirements;
        }
    }
}
