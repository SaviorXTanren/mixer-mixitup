using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Requirement
{
    public enum CooldownTypeEnum
    {
        Global,
        Individual
    }

    [DataContract]
    public class CooldownRequirementViewModel
    {
        [JsonProperty]
        public CooldownTypeEnum Type { get; set; }

        [JsonProperty]
        public int Amount { get; set; }

        [JsonIgnore]
        private DateTimeOffset globalCooldown = DateTimeOffset.MinValue;
        [JsonIgnore]
        private LockedDictionary<uint, DateTimeOffset> individualCooldowns = new LockedDictionary<uint, DateTimeOffset>();

        public CooldownRequirementViewModel()
        {
            this.Type = CooldownTypeEnum.Global;
        }

        public CooldownRequirementViewModel(CooldownTypeEnum type, int amount)
        {
            this.Type = type;
            this.Amount = amount;
        }

        public bool DoesMeetCooldownRequirement(UserViewModel user)
        {
            if (this.Type == CooldownTypeEnum.Global && this.globalCooldown.AddSeconds(this.Amount) > DateTimeOffset.Now)
            {
                return false;
            }
            else if (this.Type == CooldownTypeEnum.Individual && this.individualCooldowns.ContainsKey(user.ID) && this.individualCooldowns[user.ID].AddSeconds(this.Amount) > DateTimeOffset.Now)
            {
                return false;
            }
            return true;
        }

        public async Task SendCooldownNotMetWhisper(UserViewModel user)
        {
            TimeSpan timeLeft = new TimeSpan();
            if (this.Type == CooldownTypeEnum.Global)
            {
                timeLeft = this.globalCooldown.AddSeconds(this.Amount) - DateTimeOffset.Now;
            }
            else if (this.Type == CooldownTypeEnum.Individual)
            {
                timeLeft = this.individualCooldowns[user.ID].AddSeconds(this.Amount) - DateTimeOffset.Now;
            }
            await ChannelSession.Chat.Whisper(user.UserName, string.Format("This command is currently on cooldown, please wait another {0} second(s).", Math.Max((int)timeLeft.TotalSeconds, 1)));
        }

        public void UpdateCooldown(UserViewModel user)
        {
            if (this.Type == CooldownTypeEnum.Global)
            {
                this.globalCooldown = DateTimeOffset.Now;
            }
            else if (this.Type == CooldownTypeEnum.Individual)
            {
                this.individualCooldowns[user.ID] = DateTimeOffset.Now;
            }
        }
    }
}
