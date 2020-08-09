using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Requirements
{
    [DataContract]
    public class RequirementsSetModel
    {
        [DataMember]
        public List<RequirementModelBase> Requirements { get; set; } = new List<RequirementModelBase>();

        public RequirementsSetModel() { }

        internal RequirementsSetModel(MixItUp.Base.ViewModel.Requirement.RequirementViewModel requirements)
            : this()
        {
            if (requirements.Role != null)
            {
                this.Requirements.Add(new RoleRequirementModel(requirements.Role));
            }
            else
            {
                this.Requirements.Add(new RoleRequirementModel());
            }

            if (requirements.Cooldown != null)
            {
                this.Requirements.Add(new CooldownRequirementModel(requirements.Cooldown));
            }
            else
            {
                this.Requirements.Add(new CooldownRequirementModel());
            }

            if (requirements.Currency != null && requirements.Currency.RequirementType != ViewModel.Requirement.CurrencyRequirementTypeEnum.NoCurrencyCost)
            {
                this.Requirements.Add(new CurrencyRequirementModel(requirements.Currency));
            }

            if (requirements.Rank != null)
            {
                this.Requirements.Add(new RankRequirementModel(requirements.Rank));
            }

            if (requirements.Inventory != null)
            {
                this.Requirements.Add(new InventoryRequirementModel(requirements.Inventory));
            }

            if (requirements.Threshold != null)
            {
                this.Requirements.Add(new ThresholdRequirementModel(requirements.Threshold));
            }
            else
            {
                this.Requirements.Add(new ThresholdRequirementModel());
            }

            if (requirements.Settings != null)
            {
                this.Requirements.Add(new SettingsRequirementModel(requirements.Settings));
                RoleRequirementModel role = this.Role;
                if (role != null)
                {
                    role.PatreonBenefitID = requirements.Settings.PatreonBenefitIDRequirement;
                }
            }
            else
            {
                this.Requirements.Add(new SettingsRequirementModel());
            }
        }

        public RoleRequirementModel Role { get { return (RoleRequirementModel)this.Requirements.FirstOrDefault(r => r is RoleRequirementModel); } }

        public CooldownRequirementModel Cooldown { get { return (CooldownRequirementModel)this.Requirements.FirstOrDefault(r => r is CooldownRequirementModel); } }

        public ThresholdRequirementModel Threshold { get { return (ThresholdRequirementModel)this.Requirements.FirstOrDefault(r => r is ThresholdRequirementModel); } }

        public SettingsRequirementModel Settings { get { return (SettingsRequirementModel)this.Requirements.FirstOrDefault(r => r is SettingsRequirementModel); } }

        public async Task<bool> Validate(UserViewModel user)
        {
            foreach (RequirementModelBase requirement in this.Requirements)
            {
                if (!await requirement.Validate(user))
                {
                    return false;
                }
            }
            return true;
        }

        public async Task Perform(UserViewModel user)
        {
            IEnumerable<UserViewModel> users = this.GetRequirementUsers(user);
            foreach (RequirementModelBase requirement in this.Requirements)
            {
                foreach (UserViewModel u in users)
                {
                    await requirement.Perform(u);
                }
            }
        }

        public async Task Refund(UserViewModel user)
        {
            IEnumerable<UserViewModel> users = this.GetRequirementUsers(user);
            foreach (RequirementModelBase requirement in this.Requirements)
            {
                foreach (UserViewModel u in users)
                {
                    await requirement.Refund(u);
                }
            }
        }

        public void Reset()
        {
            foreach (RequirementModelBase requirement in this.Requirements)
            {
                requirement.Reset();
            }
        }

        public IEnumerable<UserViewModel> GetPerformingUsers(UserViewModel user)
        {
            ThresholdRequirementModel threshold = this.Threshold;
            if (threshold != null && threshold.IsEnabled && threshold.RunForEachUser)
            {
                return this.GetRequirementUsers(user);
            }
            else
            {
                return new List<UserViewModel>() { user };
            }
        }

        public IEnumerable<UserViewModel> GetRequirementUsers(UserViewModel user)
        {
            List<UserViewModel> users = new List<UserViewModel>();
            ThresholdRequirementModel threshold = this.Threshold;
            if (threshold != null && threshold.IsEnabled)
            {
                foreach (UserViewModel u in threshold.GetApplicableUsers())
                {
                    users.Add(u);
                }
            }
            else
            {
                users.Add(user);
            }
            return users;
        }
    }
}
