using MixItUp.Base.Model.User;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Requirement
{
    [Obsolete]
    [DataContract]
    public class RequirementViewModel
    {
        [JsonProperty]
        public RoleRequirementViewModel Role { get; set; }

        [JsonProperty]
        public CooldownRequirementViewModel Cooldown { get; set; }

        [JsonProperty]
        public CurrencyRequirementViewModel Currency { get; set; }

        [JsonProperty]
        public CurrencyRequirementViewModel Rank { get; set; }

        [JsonProperty]
        public InventoryRequirementViewModel Inventory { get; set; }

        [JsonProperty]
        public ThresholdRequirementViewModel Threshold { get; set; }

        [JsonProperty]
        public SettingsRequirementViewModel Settings { get; set; }

        [JsonProperty]
        [Obsolete]
        public UserRoleEnum UserRole { get; set; }

        public RequirementViewModel()
        {
            this.Role = new RoleRequirementViewModel();
            this.Cooldown = new CooldownRequirementViewModel();
            this.Threshold = new ThresholdRequirementViewModel();
            this.Settings = new SettingsRequirementViewModel();
        }

        public RequirementViewModel(UserRoleEnum userRole, int cooldown)
            : this()
        {
            this.Role.MixerRole = userRole;
            this.Cooldown.Amount = cooldown;
        }

        public async Task<bool> DoesMeetUserRoleRequirement(UserV2ViewModel user)
        {
            if (this.Role != null)
            {
                bool doesMeetRoleRequirements = this.Role.DoesMeetRequirement(user);
                if (!doesMeetRoleRequirements)
                {
                    // Force a refresh to get updated roles, just in case they recently changed
                    await user.RefreshDetails();
                    doesMeetRoleRequirements = this.Role.DoesMeetRequirement(user);
                }
                return doesMeetRoleRequirements;
            }
            return true;
        }

        public bool DoesMeetCooldownRequirement(UserV2ViewModel user)
        {
            if (this.Cooldown != null)
            {

            }
            return true;
        }

        public bool DoesMeetCurrencyRequirement(UserV2ViewModel user)
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

        public bool DoesMeetRankRequirement(UserV2ViewModel user)
        {
            if (this.Rank != null)
            {
                return this.Rank.DoesMeetRankRequirement(user.Data);
            }
            return true;
        }

        public bool DoesMeetInventoryRequirement(UserV2ViewModel user)
        {
            if (this.Inventory != null)
            {
                return this.Inventory.DoesMeetRequirement(user.Data);
            }
            return true;
        }

        public bool DoesMeetSettingsRequirement(UserV2ViewModel user)
        {
            if (this.Settings != null)
            {
                return this.Settings.DoesMeetRequirement(user);
            }
            return true;
        }

        public bool TrySubtractCurrencyAmount(UserV2ViewModel user, bool requireAmount = false)
        {
            if (this.Currency != null)
            {
                return this.Currency.TrySubtractAmount(user.Data, requireAmount);
            }
            return true;
        }

        public bool TrySubtractCurrencyAmount(UserV2ViewModel user, int amount, bool requireAmount = false)
        {
            if (this.Currency != null)
            {
                return this.Currency.TrySubtractAmount(user.Data, amount, requireAmount);
            }
            return true;
        }

        public bool TrySubtractInventoryAmount(UserV2ViewModel user, bool requireAmount = false)
        {
            if (this.Inventory != null)
            {
                return this.Inventory.TrySubtractAmount(user.Data, requireAmount);
            }
            return true;
        }

        public bool TrySubtractInventoryAmount(UserV2ViewModel user, int amount, bool requireAmount = false)
        {
            if (this.Inventory != null)
            {
                return this.Inventory.TrySubtractAmount(user.Data, amount, requireAmount);
            }
            return true;
        }
    }
}
