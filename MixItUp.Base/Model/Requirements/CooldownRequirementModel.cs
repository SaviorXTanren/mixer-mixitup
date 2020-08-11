using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
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
        private static Dictionary<string, DateTimeOffset> groupCooldowns = new Dictionary<string, DateTimeOffset>();

        [DataMember]
        public CooldownTypeEnum Type { get; set; }

        [DataMember]
        public string IndividualAmount { get; set; }

        [DataMember]
        public string GroupName { get; set; }

        [JsonIgnore]
        private DateTimeOffset globalCooldown = DateTimeOffset.MinValue;

        [JsonIgnore]
        private LockedDictionary<Guid, DateTimeOffset> individualCooldowns = new LockedDictionary<Guid, DateTimeOffset>();

        public CooldownRequirementModel() { }

        internal CooldownRequirementModel(MixItUp.Base.ViewModel.Requirement.CooldownRequirementViewModel requirement)
            : this()
        {
            this.Type = (CooldownTypeEnum)(int)requirement.Type;
            this.IndividualAmount = requirement.Amount.ToString();
            this.GroupName = requirement.GroupName;
        }

        public CooldownRequirementModel(CooldownTypeEnum type, string amount, string groupName = null)
        {
            this.Type = type;
            this.IndividualAmount = amount;
            this.GroupName = groupName;
        }

        [JsonIgnore]
        public bool IsGroup { get { return this.Type == CooldownTypeEnum.Group && !string.IsNullOrEmpty(this.GroupName); } }

        [JsonIgnore]
        public string Amount
        {
            get
            {
                string amount = null;
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

        public override async Task<bool> Validate(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            int amount = await this.GetAmount(this.Amount, user, platform, arguments, specialIdentifiers);
            TimeSpan timeLeft = new TimeSpan(0, 0, -1);
            if (this.Type == CooldownTypeEnum.Standard)
            {
                timeLeft = this.globalCooldown.AddSeconds(amount) - DateTimeOffset.Now;
            }
            else if (this.Type == CooldownTypeEnum.Group)
            {
                if (CooldownRequirementModel.groupCooldowns.ContainsKey(this.GroupName))
                {
                    timeLeft = CooldownRequirementModel.groupCooldowns[this.GroupName].AddSeconds(amount) - DateTimeOffset.Now;
                }
            }
            else if (this.Type == CooldownTypeEnum.PerPerson)
            {
                if (this.individualCooldowns.ContainsKey(user.ID))
                {
                    timeLeft = this.individualCooldowns[user.ID].AddSeconds(amount) - DateTimeOffset.Now;
                }
            }

            int totalSeconds = (int)Math.Ceiling(timeLeft.TotalSeconds);
            if (totalSeconds > 0)
            {
                await this.SendChatMessage(string.Format("This command is currently on cooldown, please wait another {0} second(s).", totalSeconds));
                return false;
            }
            return true;
        }

        public override Task Perform(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            if (this.Type == CooldownTypeEnum.Standard)
            {
                this.globalCooldown = DateTimeOffset.Now;
            }
            else if (this.Type == CooldownTypeEnum.Group)
            {
                CooldownRequirementModel.groupCooldowns[this.GroupName] = DateTimeOffset.Now;
            }
            else if (this.Type == CooldownTypeEnum.PerPerson)
            {
                this.individualCooldowns[user.ID] = DateTimeOffset.Now;
            }
            return Task.FromResult(0);
        }

        public override Task Refund(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            if (this.Type == CooldownTypeEnum.Standard)
            {
                this.globalCooldown = DateTimeOffset.MinValue;
            }
            else if (this.Type == CooldownTypeEnum.Group)
            {
                CooldownRequirementModel.groupCooldowns[this.GroupName] = DateTimeOffset.MinValue;
            }
            else if (this.Type == CooldownTypeEnum.PerPerson)
            {
                this.individualCooldowns[user.ID] = DateTimeOffset.MinValue;
            }
            return Task.FromResult(0);
        }

        public override void Reset()
        {
            this.globalCooldown = DateTimeOffset.MinValue;
            CooldownRequirementModel.groupCooldowns[this.GroupName] = DateTimeOffset.MinValue;
            this.individualCooldowns.Clear();
        }
    }
}
