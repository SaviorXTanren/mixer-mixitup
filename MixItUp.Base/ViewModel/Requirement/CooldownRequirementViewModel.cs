using Mixer.Base.Util;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using StreamingClient.Base.Util;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Requirement
{
    public enum CooldownTypeEnum
    {
        Individual,
        [Name("Per Person")]
        PerPerson,
        Group,
    }

    [DataContract]
    public class CooldownRequirementViewModel
    {
        private static LockedDictionary<string, DateTimeOffset> groupCooldowns = new LockedDictionary<string, DateTimeOffset>();

        [JsonProperty]
        public CooldownTypeEnum Type { get; set; }

        [JsonProperty]
        public int Amount { get; set; }

        [JsonProperty]
        public string GroupName { get; set; }

        [JsonIgnore]
        private DateTimeOffset globalCooldown = DateTimeOffset.MinValue;
        [JsonIgnore]
        private LockedDictionary<uint, DateTimeOffset> individualCooldowns = new LockedDictionary<uint, DateTimeOffset>();

        public CooldownRequirementViewModel()
        {
            this.Type = CooldownTypeEnum.Individual;
        }

        public CooldownRequirementViewModel(CooldownTypeEnum type, int amount)
        {
            this.Type = type;
            this.Amount = amount;
        }

        public CooldownRequirementViewModel(CooldownTypeEnum type, string groupName, int amount)
        {
            this.Type = type;
            this.GroupName = groupName;

            if (!string.IsNullOrEmpty(this.GroupName) && ChannelSession.Settings != null)
            {
                ChannelSession.Settings.CooldownGroups[this.GroupName] = amount;
            }
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

        public bool DoesMeetRequirement(UserViewModel user)
        {
            if (this.Type == CooldownTypeEnum.Individual && this.globalCooldown.AddSeconds(this.CooldownAmount) > DateTimeOffset.Now)
            {
                return false;
            }
            else if (this.Type == CooldownTypeEnum.PerPerson && this.individualCooldowns.ContainsKey(user.ID) && this.individualCooldowns[user.ID].AddSeconds(this.CooldownAmount) > DateTimeOffset.Now)
            {
                return false;
            }
            else if (this.IsGroup)
            {
                if (CooldownRequirementViewModel.groupCooldowns.ContainsKey(this.GroupName) &&
                    CooldownRequirementViewModel.groupCooldowns[this.GroupName].AddSeconds(this.CooldownAmount) > DateTimeOffset.Now)
                {
                    return false;
                }
            }
            return true;
        }

        public async Task SendNotMetWhisper(UserViewModel user)
        {
            TimeSpan timeLeft = new TimeSpan();
            if (this.Type == CooldownTypeEnum.Individual)
            {
                timeLeft = this.globalCooldown.AddSeconds(this.CooldownAmount) - DateTimeOffset.Now;
            }
            else if (this.Type == CooldownTypeEnum.PerPerson)
            {
                timeLeft = this.individualCooldowns[user.ID].AddSeconds(this.CooldownAmount) - DateTimeOffset.Now;
            }
            else if (this.IsGroup && CooldownRequirementViewModel.groupCooldowns.ContainsKey(this.GroupName))
            {
                timeLeft = CooldownRequirementViewModel.groupCooldowns[this.GroupName].AddSeconds(this.CooldownAmount) - DateTimeOffset.Now;
            }
            await ChannelSession.Services.Chat.Whisper(user.UserName, string.Format("This command is currently on cooldown, please wait another {0} second(s).", Math.Max((int)timeLeft.TotalSeconds, 1)));
        }

        public void UpdateCooldown(UserViewModel user)
        {
            if (this.Type == CooldownTypeEnum.Individual)
            {
                this.globalCooldown = DateTimeOffset.Now;
            }
            else if (this.Type == CooldownTypeEnum.PerPerson)
            {
                this.individualCooldowns[user.ID] = DateTimeOffset.Now;
            }
            else if (this.IsGroup)
            {
                CooldownRequirementViewModel.groupCooldowns[this.GroupName] = DateTimeOffset.Now;
            }
        }

        public void ResetCooldown(UserViewModel user)
        {
            if (this.Type == CooldownTypeEnum.Individual)
            {
                this.globalCooldown = DateTimeOffset.Now.Subtract(TimeSpan.FromSeconds(this.CooldownAmount));
            }
            else if (this.Type == CooldownTypeEnum.PerPerson)
            {
                this.individualCooldowns[user.ID] = DateTimeOffset.Now.Subtract(TimeSpan.FromSeconds(this.CooldownAmount));
            }
            else if (this.IsGroup)
            {
                CooldownRequirementViewModel.groupCooldowns[this.GroupName] = DateTimeOffset.Now.Subtract(TimeSpan.FromSeconds(this.CooldownAmount));
            }
        }
    }
}
