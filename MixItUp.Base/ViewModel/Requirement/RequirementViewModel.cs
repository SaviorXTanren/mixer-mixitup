using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Requirement
{
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
        public MixerRoleEnum UserRole { get; set; }


        public RequirementViewModel()
        {
            this.Role = new RoleRequirementViewModel();
            this.Cooldown = new CooldownRequirementViewModel();
            this.Threshold = new ThresholdRequirementViewModel();
            this.Settings = new SettingsRequirementViewModel();
        }

        public RequirementViewModel(MixerRoleEnum userRole, int cooldown)
            : this()
        {
            this.Role.MixerRole = userRole;
            this.Cooldown.Amount = cooldown;
        }

        public async Task<bool> DoesMeetUserRoleRequirement(UserViewModel user)
        {
            if (this.Role != null)
            {
                bool doesMeetRoleRequirements = this.Role.DoesMeetRequirement(user);
                if (!doesMeetRoleRequirements)
                {
                    // Force a refresh to get updated roles, just in case they recently changed
                    await user.RefreshChatDetails();
                    await user.RefreshDetails();
                    doesMeetRoleRequirements = this.Role.DoesMeetRequirement(user);
                }
                return doesMeetRoleRequirements;
            }
            return true;
        }

        public bool DoesMeetCooldownRequirement(UserViewModel user)
        {
            if (this.Cooldown != null)
            {
                return this.Cooldown.DoesMeetRequirement(user);
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

        public bool DoesMeetInventoryRequirement(UserViewModel user)
        {
            if (this.Inventory != null)
            {
                return this.Inventory.DoesMeetRequirement(user.Data);
            }
            return true;
        }

        public bool DoesMeetSettingsRequirement(UserViewModel user)
        {
            if (this.Settings != null)
            {
                return this.Settings.DoesMeetRequirement(user);
            }
            return true;
        }

        public async Task<IEnumerable<UserViewModel>> GetTriggeringUsers(string commandName, UserViewModel user)
        {
            if (this.Threshold != null)
            {
                return await this.Threshold.GetTriggeringUsers(commandName, user);
            }
            return new UserViewModel[] { user };
        }

        public void UpdateCooldown(UserViewModel user)
        {
            if (this.Cooldown != null)
            {
                this.Cooldown.UpdateCooldown(user);
            }
        }

        public void ResetCooldown(UserViewModel user)
        {
            if (this.Cooldown != null)
            {
                this.Cooldown.ResetCooldown(user);
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

        public bool TrySubtractInventoryAmount(UserViewModel user)
        {
            if (this.Inventory != null)
            {
                return this.Inventory.TrySubtractAmount(user.Data);
            }
            return true;
        }
    }
}
