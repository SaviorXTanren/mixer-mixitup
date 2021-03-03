using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Requirement
{
    [Obsolete]
    public enum CooldownTypeEnum
    {
        Individual,
        PerPerson,
        Group,
    }

    [Obsolete]
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
        private LockedDictionary<Guid, DateTimeOffset> individualCooldowns = new LockedDictionary<Guid, DateTimeOffset>();

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
                    return 0;
                }
                else
                {
                    return this.Amount;
                }
            }
        }
    }
}
