using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Commands
{
    [DataContract]
    public abstract class PermissionsCommandBase : CommandBase
    {
        [DataMember]
        public RequirementViewModel Requirements { get; set; }

        private SemaphoreSlim permissionsCheckSemaphore = new SemaphoreSlim(1);

        public PermissionsCommandBase()
        {
            this.Requirements = new RequirementViewModel();
        }

        public PermissionsCommandBase(string name, CommandTypeEnum type, string command, RequirementViewModel requirements)
            : this(name, type, new List<string>() { command }, requirements)
        { }

        public PermissionsCommandBase(string name, CommandTypeEnum type, IEnumerable<string> commands, RequirementViewModel requirements)
            : base(name, type, commands)
        {
            this.Requirements = requirements;
        }

        [JsonIgnore]
        public UserRoleEnum UserRoleRequirement
        {
            get
            {
                if (this.Requirements.Role != null)
                {
                    return this.Requirements.Role.MixerRole;
                }
                return UserRoleEnum.User;
            }
        }

        public virtual async Task<bool> CheckCooldownRequirement(UserViewModel user)
        {
            if (!this.Requirements.DoesMeetCooldownRequirement(user))
            {
                await this.Requirements.Cooldown.SendNotMetWhisper(user);
                return false;
            }
            return true;
        }

        public virtual async Task<bool> CheckUserRoleRequirement(UserViewModel user)
        {
            if (!await this.Requirements.DoesMeetUserRoleRequirement(user))
            {
                await this.Requirements.Role.SendNotMetWhisper(user);
                return false;
            }
            return true;
        }

        public virtual async Task<bool> CheckRankRequirement(UserViewModel user)
        {
            if (this.Requirements.Rank != null && this.Requirements.Rank.GetCurrency() != null)
            {
                if (!this.Requirements.Rank.DoesMeetRankRequirement(user.Data))
                {
                    await this.Requirements.Rank.SendRankNotMetWhisper(user);
                    return false;
                }
            }
            return true;
        }

        public virtual async Task<bool> CheckCurrencyRequirement(UserViewModel user)
        {
            if (this.Requirements.Currency != null && this.Requirements.Currency.GetCurrency() != null)
            {
                if (!this.Requirements.Currency.DoesMeetCurrencyRequirement(user.Data))
                {
                    await this.Requirements.Currency.SendCurrencyNotMetWhisper(user);
                    return false;
                }
            }
            return true;
        }

        public virtual async Task<bool> CheckInventoryRequirement(UserViewModel user)
        {
            if (!this.Requirements.DoesMeetInventoryRequirement(user))
            {
                await this.Requirements.Inventory.SendNotMetWhisper(user);
                return false;
            }
            return true;
        }

        public virtual async Task<bool> CheckSettingsRequirement(UserViewModel user)
        {
            if (!this.Requirements.DoesMeetSettingsRequirement(user))
            {
                await this.Requirements.Settings.SendSettingsNotMetWhisper(user);
                return false;
            }
            return true;
        }

        public void ResetCooldown(UserViewModel user) { this.Requirements.ResetCooldown(user); }

        public async Task<bool> CheckAllRequirements(UserViewModel user)
        {
            return await this.CheckCooldownRequirement(user) && await this.CheckUserRoleRequirement(user) && await this.CheckRankRequirement(user)
                && await this.CheckCurrencyRequirement(user) && await this.CheckInventoryRequirement(user) && await this.CheckSettingsRequirement(user);
        }

        protected override async Task<bool> PerformPreChecks(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            return await this.permissionsCheckSemaphore.WaitAndRelease(async () =>
            {
                if (!await this.CheckAllRequirements(user))
                {
                    return false;
                }

                IEnumerable<UserViewModel> triggeringUsers = await this.Requirements.GetTriggeringUsers(this.Name, user);
                if (triggeringUsers == null)
                {
                    // The action did not trigger due to threshold requirements not being met
                    return false;
                }

                foreach (UserViewModel triggeringUser in triggeringUsers)
                {
                    // Do our best to subtract the required currency
                    this.Requirements.TrySubtractCurrencyAmount(triggeringUser);

                    this.Requirements.TrySubtractInventoryAmount(triggeringUser);

                    this.Requirements.UpdateCooldown(triggeringUser);
                }

                return true;
            });
        }
    }
}
