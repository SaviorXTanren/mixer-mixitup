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
        Individual,
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
        public int Amount { get; set; }

        [DataMember]
        public string GroupName { get; set; }

        [JsonIgnore]
        private DateTimeOffset globalCooldown = DateTimeOffset.MinValue;

        [JsonIgnore]
        private LockedDictionary<Guid, DateTimeOffset> individualCooldowns = new LockedDictionary<Guid, DateTimeOffset>();

        public CooldownRequirementModel() { }

        public CooldownRequirementModel(CooldownTypeEnum type, int amount, string groupName = null)
        {
            this.Type = type;
            this.Amount = amount;
            this.GroupName = groupName;
        }

        [JsonIgnore]
        public bool IsGroup { get { return this.Type == CooldownTypeEnum.Group && !string.IsNullOrEmpty(this.GroupName); } }

        [JsonIgnore]
        public int CooldownAmount
        {
            get
            {
                if (this.IsGroup)
                {
                    if (ChannelSession.Settings.CooldownGroups.ContainsKey(this.GroupName))
                    {
                        return ChannelSession.Settings.CooldownGroups[this.GroupName];
                    }
                    return 0;
                }
                else
                {
                    return this.Amount;
                }
            }
        }

        public override async Task<bool> Validate(UserViewModel user)
        {
            TimeSpan timeLeft = new TimeSpan(0, 0, 1);
            if (this.Type == CooldownTypeEnum.Individual)
            {
                timeLeft = this.globalCooldown.AddSeconds(this.Amount) - DateTimeOffset.Now;
            }
            else if (this.Type == CooldownTypeEnum.Group)
            {
                timeLeft = this.individualCooldowns[user.ID].AddSeconds(this.Amount) - DateTimeOffset.Now;
            }
            else if (this.Type == CooldownTypeEnum.PerPerson)
            {
                if (CooldownRequirementModel.groupCooldowns.ContainsKey(this.GroupName))
                {
                    timeLeft = CooldownRequirementModel.groupCooldowns[this.GroupName].AddSeconds(this.Amount) - DateTimeOffset.Now;
                }
            }

            if (timeLeft.TotalSeconds >= 0)
            {
                await this.SendChatWhisper(user, string.Format("This command is currently on cooldown, please wait another {0} second(s).", (int)Math.Ceiling(timeLeft.TotalSeconds)));
                return false;
            }
            return true;
        }

        public override Task Perform(UserViewModel user)
        {
            if (this.Type == CooldownTypeEnum.Individual)
            {
                this.globalCooldown = DateTimeOffset.Now;
            }
            else if (this.Type == CooldownTypeEnum.Group)
            {
                this.individualCooldowns[user.ID] = DateTimeOffset.Now;
            }
            else if (this.Type == CooldownTypeEnum.PerPerson)
            {
                CooldownRequirementModel.groupCooldowns[this.GroupName] = DateTimeOffset.Now;
            }
            return Task.FromResult(0);
        }

        public override Task Refund(UserViewModel user)
        {
            if (this.Type == CooldownTypeEnum.Individual)
            {
                this.globalCooldown = DateTimeOffset.MinValue;
            }
            else if (this.Type == CooldownTypeEnum.Group)
            {
                this.individualCooldowns[user.ID] = DateTimeOffset.MinValue;
            }
            else if (this.Type == CooldownTypeEnum.PerPerson)
            {
                CooldownRequirementModel.groupCooldowns[this.GroupName] = DateTimeOffset.MinValue;
            }
            return Task.FromResult(0);
        }

        public override void Reset()
        {
            this.globalCooldown = DateTimeOffset.MinValue;
            this.individualCooldowns.Clear();
            CooldownRequirementModel.groupCooldowns[this.GroupName] = DateTimeOffset.MinValue;
        }
    }
}
