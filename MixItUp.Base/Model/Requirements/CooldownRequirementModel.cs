using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Requirements
{
    public enum CooldownTypeEnum
    {
        Standard,
        PerPerson,
        Group,
    }

    [DataContract]
    public class CooldownRequirementModel : RequirementModelBase
    {
        private const int InitialCooldownAmount = 5;

        private static DateTimeOffset requirementErrorCooldown = DateTimeOffset.MinValue;

        private static Dictionary<string, DateTimeOffset> groupCooldowns = new Dictionary<string, DateTimeOffset>();

        [DataMember]
        public CooldownTypeEnum Type { get; set; }

        [DataMember]
        public int IndividualAmount { get; set; }

        [DataMember]
        public string GroupName { get; set; }

        [JsonIgnore]
        private DateTimeOffset globalCooldown = DateTimeOffset.MinValue;

        [JsonIgnore]
        private LockedDictionary<Guid, DateTimeOffset> individualCooldowns = new LockedDictionary<Guid, DateTimeOffset>();

        public static Result GetCooldownAmountMessage(DateTimeOffset cooldownTime)
        {
            if (cooldownTime > DateTimeOffset.Now)
            {
                int totalSeconds = (int)Math.Ceiling((cooldownTime - DateTimeOffset.Now).TotalSeconds);
                if (totalSeconds > 0)
                {
                    TimeSpan timeSpan = TimeSpan.FromSeconds(totalSeconds);

                    if (timeSpan.TotalHours >= 1.0)
                    {
                        return new Result(string.Format(MixItUp.Base.Resources.CooldownRequirementOnCooldownHours, timeSpan.Hours));
                    }
                    else if (timeSpan.TotalMinutes >= 1.0)
                    {
                        return new Result(string.Format(MixItUp.Base.Resources.CooldownRequirementOnCooldownMinutes, timeSpan.Minutes));
                    }
                    else
                    {
                        return new Result(string.Format(MixItUp.Base.Resources.CooldownRequirementOnCooldownSeconds, totalSeconds));
                    }
                }
            }
            return new Result();
        }

        public CooldownRequirementModel(CooldownTypeEnum type, int amount, string groupName = null)
        {
            this.Type = type;
            this.IndividualAmount = amount;
            this.GroupName = groupName;
        }

        public CooldownRequirementModel() { }

        protected override DateTimeOffset RequirementErrorCooldown { get { return CooldownRequirementModel.requirementErrorCooldown; } set { CooldownRequirementModel.requirementErrorCooldown = value; } }

        [JsonIgnore]
        public bool IsGroup { get { return this.Type == CooldownTypeEnum.Group && !string.IsNullOrEmpty(this.GroupName); } }

        [JsonIgnore]
        public int Amount
        {
            get
            {
                int amount = 0;
                if (this.IsGroup)
                {
                    if (ChannelSession.Settings.CooldownGroupAmounts.ContainsKey(this.GroupName))
                    {
                        amount = ChannelSession.Settings.CooldownGroupAmounts[this.GroupName];
                    }
                }
                else
                {
                    amount = this.IndividualAmount;
                }
                return amount;
            }
        }

        public override Task<Result> Validate(CommandParametersModel parameters)
        {
            DateTimeOffset cooldownTime = DateTimeOffset.MinValue;
            if (this.Type == CooldownTypeEnum.Standard)
            {
                cooldownTime = this.globalCooldown;
            }
            else if (this.Type == CooldownTypeEnum.Group)
            {
                if (!string.IsNullOrEmpty(this.GroupName) && CooldownRequirementModel.groupCooldowns.ContainsKey(this.GroupName))
                {
                    cooldownTime = CooldownRequirementModel.groupCooldowns[this.GroupName];
                }
            }
            else if (this.Type == CooldownTypeEnum.PerPerson)
            {
                if (this.individualCooldowns.ContainsKey(parameters.User.ID))
                {
                    cooldownTime = this.individualCooldowns[parameters.User.ID];
                }
            }

            return Task.FromResult(CooldownRequirementModel.GetCooldownAmountMessage(cooldownTime));
        }

        public override async Task Perform(CommandParametersModel parameters)
        {
            await base.Perform(parameters);
            this.Perform(parameters, this.Amount);
        }

        public void Perform(CommandParametersModel parameters, int amount)
        {
            if (amount > 0)
            {
                this.individualErrorCooldown = DateTimeOffset.Now.AddSeconds(InitialCooldownAmount);

                if (this.Type == CooldownTypeEnum.Standard)
                {
                    this.globalCooldown = DateTimeOffset.Now.AddSeconds(amount);
                }
                else if (this.Type == CooldownTypeEnum.Group)
                {
                    if (!string.IsNullOrEmpty(this.GroupName))
                    {
                        CooldownRequirementModel.groupCooldowns[this.GroupName] = DateTimeOffset.Now.AddSeconds(amount);
                    }
                }
                else if (this.Type == CooldownTypeEnum.PerPerson)
                {
                    this.individualCooldowns[parameters.User.ID] = DateTimeOffset.Now.AddSeconds(amount);
                }
            }
        }

        public override Task Refund(CommandParametersModel parameters)
        {
            if (this.Type == CooldownTypeEnum.Standard)
            {
                this.globalCooldown = DateTimeOffset.MinValue;
            }
            else if (this.Type == CooldownTypeEnum.Group)
            {
                if (!string.IsNullOrEmpty(this.GroupName))
                {
                    CooldownRequirementModel.groupCooldowns[this.GroupName] = DateTimeOffset.MinValue;
                }
            }
            else if (this.Type == CooldownTypeEnum.PerPerson)
            {
                this.individualCooldowns[parameters.User.ID] = DateTimeOffset.MinValue;
            }
            return Task.CompletedTask;
        }

        public override void Reset()
        {
            this.globalCooldown = DateTimeOffset.MinValue;
            if (!string.IsNullOrEmpty(this.GroupName) && CooldownRequirementModel.groupCooldowns.ContainsKey(this.GroupName))
            {
                CooldownRequirementModel.groupCooldowns[this.GroupName] = DateTimeOffset.MinValue;
            }
            this.individualCooldowns.Clear();
        }
    }
}
