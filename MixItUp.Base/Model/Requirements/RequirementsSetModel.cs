using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
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

        public RequirementsSetModel(IEnumerable<RequirementModelBase> requirements) { this.Requirements.AddRange(requirements); }

        public RequirementsSetModel() { }

        public RoleRequirementModel Role { get { return (RoleRequirementModel)this.Requirements.FirstOrDefault(r => r is RoleRequirementModel); } }

        public CooldownRequirementModel Cooldown { get { return (CooldownRequirementModel)this.Requirements.FirstOrDefault(r => r is CooldownRequirementModel); } }

        public IEnumerable<CurrencyRequirementModel> Currency { get { return this.Requirements.Where(r => r is CurrencyRequirementModel).Select(r => (CurrencyRequirementModel)r); } }

        public IEnumerable<RankRequirementModel> Rank { get { return this.Requirements.Where(r => r is RankRequirementModel).Select(r => (RankRequirementModel)r); } }

        public IEnumerable<InventoryRequirementModel> Inventory { get { return this.Requirements.Where(r => r is InventoryRequirementModel).Select(r => (InventoryRequirementModel)r); } }

        public ThresholdRequirementModel Threshold { get { return (ThresholdRequirementModel)this.Requirements.FirstOrDefault(r => r is ThresholdRequirementModel); } }

        public ArgumentsRequirementModel Arguments { get { return (ArgumentsRequirementModel)this.Requirements.FirstOrDefault(r => r is ArgumentsRequirementModel); } }

        public SettingsRequirementModel Settings { get { return (SettingsRequirementModel)this.Requirements.FirstOrDefault(r => r is SettingsRequirementModel); } }

        public void SetIndividualErrorCooldown(DateTimeOffset datetime)
        {
            foreach (RequirementModelBase requirement in this.Requirements)
            {
                requirement.SetIndividualErrorCooldown(datetime);
            }
        }

        public async Task<Result> Validate(CommandParametersModel parameters)
        {
            foreach (RequirementModelBase requirement in this.Requirements)
            {
                Result result = await requirement.Validate(parameters);
                if (!result.Success)
                {
                    await requirement.SendErrorChatMessage(parameters.User, result);
                    return result;
                }
            }
            return new Result();
        }

        public async Task Perform(CommandParametersModel parameters, HashSet<Type> requirementsToSkip = null)
        {
            IEnumerable<CommandParametersModel> users = this.GetRequirementUsers(parameters);
            foreach (RequirementModelBase requirement in this.Requirements)
            {
                if (requirementsToSkip == null || !requirementsToSkip.Contains(requirement.GetType()))
                {
                    foreach (CommandParametersModel u in users)
                    {
                        await requirement.Perform(u);
                    }
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
            this.Requirements.Add(new ArgumentsRequirementModel());
            this.Requirements.Add(new SettingsRequirementModel());
        }
    }
}
