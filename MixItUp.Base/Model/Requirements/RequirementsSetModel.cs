using MixItUp.Base.Model.Commands;
using System;
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

        public RequirementsSetModel(IEnumerable<RequirementModelBase> requirements) { this.Requirements.AddRange(requirements); }

        internal RequirementsSetModel(MixItUp.Base.ViewModel.Requirement.RequirementViewModel requirements)
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

            if (requirements.Currency != null)
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

        public RoleRequirementModel Role { get { return this.GetRequirementOrDefault<RoleRequirementModel>(); } }

        public CooldownRequirementModel Cooldown { get { return this.GetRequirementOrDefault<CooldownRequirementModel>(); } }

        public IEnumerable<CurrencyRequirementModel> Currency { get { return this.Requirements.Where(r => r is CurrencyRequirementModel).Select(r => (CurrencyRequirementModel)r); } }

        public IEnumerable<RankRequirementModel> Rank { get { return this.Requirements.Where(r => r is RankRequirementModel).Select(r => (RankRequirementModel)r); } }

        public IEnumerable<InventoryRequirementModel> Inventory { get { return this.Requirements.Where(r => r is InventoryRequirementModel).Select(r => (InventoryRequirementModel)r); } }

        public ThresholdRequirementModel Threshold { get { return this.GetRequirementOrDefault<ThresholdRequirementModel>(); } }

        public SettingsRequirementModel Settings { get { return this.GetRequirementOrDefault<SettingsRequirementModel>(); } }

        public async Task<bool> Validate(CommandParametersModel parameters)
        {
            foreach (RequirementModelBase requirement in this.Requirements)
            {
                if (!await requirement.Validate(parameters))
                {
                    return false;
                }
            }
            return true;
        }

        public async Task Perform(CommandParametersModel parameters)
        {
            IEnumerable<CommandParametersModel> users = this.GetRequirementUsers(parameters);
            foreach (RequirementModelBase requirement in this.Requirements)
            {
                foreach (CommandParametersModel u in users)
                {
                    await requirement.Perform(u);
                }
            }
        }

        public async Task Refund(CommandParametersModel parameters)
        {
            IEnumerable<CommandParametersModel> users = this.GetRequirementUsers(parameters);
            foreach (RequirementModelBase requirement in this.Requirements)
            {
                foreach (CommandParametersModel u in users)
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

        public IEnumerable<CommandParametersModel> GetPerformingUsers(CommandParametersModel parameters)
        {
            ThresholdRequirementModel threshold = this.Threshold;
            if (threshold != null && threshold.IsEnabled && threshold.RunForEachUser)
            {
                return this.GetRequirementUsers(parameters);
            }
            else
            {
                return new List<CommandParametersModel>() { parameters };
            }
        }

        public IEnumerable<CommandParametersModel> GetRequirementUsers(CommandParametersModel parameters)
        {
            List<CommandParametersModel> users = new List<CommandParametersModel>();
            ThresholdRequirementModel threshold = this.Threshold;
            if (threshold != null && threshold.IsEnabled)
            {
                foreach (CommandParametersModel u in threshold.GetApplicableUsers())
                {
                    users.Add(u);
                }
            }
            else
            {
                users.Add(parameters);
            }
            return users;
        }

        public void AddBasicRequirements()
        {
            this.Requirements.Add(new RoleRequirementModel());
            this.Requirements.Add(new CooldownRequirementModel());
            this.Requirements.Add(new ThresholdRequirementModel());
            this.Requirements.Add(new SettingsRequirementModel());
        }

        private T GetRequirementOrDefault<T>() where T : RequirementModelBase
        {
             T requirement = (T)this.Requirements.FirstOrDefault(r => r is T);
            if (requirement == null)
            {
                requirement = (T)Activator.CreateInstance(typeof(T));
            }
            return requirement;
        }
    }
}
