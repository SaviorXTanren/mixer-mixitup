using MixItUp.Base.Model.User;
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
    [Obsolete]
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

        public void ResetCooldown(UserV2ViewModel user) {  }

        protected override async Task<bool> PerformPreChecks(UserV2ViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            return await this.permissionsCheckSemaphore.WaitAndRelease(() =>
            {
                IEnumerable<UserV2ViewModel> triggeringUsers = new List<UserV2ViewModel>();
                if (triggeringUsers == null)
                {
                    // The action did not trigger due to threshold requirements not being met
                    return Task.FromResult(false);
                }

                foreach (UserV2ViewModel triggeringUser in triggeringUsers)
                {
                    // Do our best to subtract the required currency
                    this.Requirements.TrySubtractCurrencyAmount(triggeringUser);

                    this.Requirements.TrySubtractInventoryAmount(triggeringUser);
                }

                return Task.FromResult(true);
            });
        }
    }
}
