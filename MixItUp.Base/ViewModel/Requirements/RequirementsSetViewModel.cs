using MixItUp.Base.Model.Requirements;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Requirements
{
    public class RequirementsSetViewModel
    {
        public RoleRequirementViewModel Role { get; set; } = new RoleRequirementViewModel();

        public CooldownRequirementViewModel Cooldown { get; set; } = new CooldownRequirementViewModel();

        public CurrencyListRequirementViewModel Currency { get; set; } = new CurrencyListRequirementViewModel();

        public RankListRequirementViewModel Rank { get; set; } = new RankListRequirementViewModel();

        public InventoryListRequirementViewModel Inventory { get; set; } = new InventoryListRequirementViewModel();

        public ThresholdRequirementViewModel Threshold { get; set; } = new ThresholdRequirementViewModel();

        public IEnumerable<RequirementViewModelBase> Requirements
        {
            get
            {
                List<RequirementViewModelBase> requirements = new List<RequirementViewModelBase>();
                requirements.Add(this.Role);
                requirements.Add(this.Cooldown);
                requirements.AddRange(this.Currency.Items);
                requirements.AddRange(this.Rank.Items);
                requirements.AddRange(this.Inventory.Items);
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
                    this.Currency.Add((CurrencyRequirementModel)requirement);
                }
                else if (requirement is RankRequirementModel)
                {
                    this.Rank.Add((RankRequirementModel)requirement);
                }
                else if (requirement is InventoryRequirementModel)
                {
                    this.Inventory.Add((InventoryRequirementModel)requirement);
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
