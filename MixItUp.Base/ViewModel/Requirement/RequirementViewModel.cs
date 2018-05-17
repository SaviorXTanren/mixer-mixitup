using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Requirement
{
    [DataContract]
    public class RequirementViewModel
    {
        [JsonProperty]
        [Obsolete]
        public MixerRoleEnum UserRole { get; set; }

        [JsonProperty]
        public RoleRequirementViewModel Role { get; set; }

        [JsonProperty]
        public CooldownRequirementViewModel Cooldown { get; set; }

        [JsonProperty]
        public CurrencyRequirementViewModel Currency { get; set; }

        [JsonProperty]
        public CurrencyRequirementViewModel Rank { get; set; }

        public RequirementViewModel()
        {
            this.Role = new RoleRequirementViewModel();
            this.Cooldown = new CooldownRequirementViewModel();
        }

        public RequirementViewModel(MixerRoleEnum userRole, int cooldown)
            : this()
        {
            this.Role.MixerRole = userRole;
            this.Cooldown.Amount = cooldown;
        }

        public RequirementViewModel(RoleRequirementViewModel role, CooldownRequirementViewModel cooldown = null, CurrencyRequirementViewModel currency = null, CurrencyRequirementViewModel rank = null)
        {
            this.Role = role;
            this.Cooldown = (cooldown != null) ? cooldown : new CooldownRequirementViewModel();
            this.Currency = currency;
            this.Rank = rank;
        }

        public bool DoesMeetUserRoleRequirement(UserViewModel user)
        {
            if (this.Role != null)
            {
                return this.Role.DoesMeetUserRoleRequirement(user);
            }
            return true;
        }

        public bool DoesMeetCooldownRequirement(UserViewModel user)
        {
            if (this.Cooldown != null)
            {
                return this.Cooldown.DoesMeetCooldownRequirement(user);
            }
            return true;
        }

        public bool DoesMeetCurrencyRequirement(UserViewModel user)
        {
            if (this.Currency != null)
            {
                return this.Currency.DoesMeetCurrencyRequirement(user.Data);
            }
            return true;
        }

        public bool DoesMeetCurrencyRequirement(int amount)
        {
            if (this.Currency != null)
            {
                return this.Currency.DoesMeetCurrencyRequirement(amount);
            }
            return true;
        }

        public bool DoesMeetRankRequirement(UserViewModel user)
        {
            if (this.Rank != null)
            {
                return this.Rank.DoesMeetRankRequirement(user.Data);
            }
            return true;
        }

        public void UpdateCooldown(UserViewModel user)
        {
            if (this.Cooldown != null)
            {
                this.Cooldown.UpdateCooldown(user);
            }
        }

        public bool TrySubtractCurrencyAmount(UserViewModel user)
        {
            if (this.Currency != null)
            {
                return this.Currency.TrySubtractAmount(user.Data);
            }
            return true;
        }

        public bool TrySubtractCurrencyAmount(UserViewModel user, int amount)
        {
            if (this.Currency != null)
            {
                return this.Currency.TrySubtractAmount(user.Data, amount);
            }
            return true;
        }
    }
}
