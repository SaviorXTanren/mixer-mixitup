using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Requirements
{
    [DataContract]
    public class RoleRequirementModel : RequirementModelBase
    {
        private static DateTimeOffset requirementErrorCooldown = DateTimeOffset.MinValue;

        [DataMember]
        public UserRoleEnum Role { get; set; }
        [DataMember]
        public HashSet<UserRoleEnum> RoleList { get; set; } = new HashSet<UserRoleEnum>();

        [DataMember]
        public int SubscriberTier { get; set; } = 1;

        [DataMember]
        public string PatreonBenefitID { get; set; }

        public RoleRequirementModel() { }

#pragma warning disable CS0612 // Type or member is obsolete
        internal RoleRequirementModel(MixItUp.Base.ViewModel.Requirement.RoleRequirementViewModel requirement)
            : this()
        {
            this.Role = requirement.MixerRole;
            this.SubscriberTier = requirement.SubscriberTier;
        }
#pragma warning restore CS0612 // Type or member is obsolete

        public RoleRequirementModel(UserRoleEnum role, int subscriberTier = 1, string patreonBenefitID = null)
        {
            this.Role = role;
            this.SubscriberTier = subscriberTier;
            this.PatreonBenefitID = patreonBenefitID;
        }

        public RoleRequirementModel(IEnumerable<UserRoleEnum> roleList, int subscriberTier = 1, string patreonBenefitID = null)
        {
            this.RoleList = new HashSet<UserRoleEnum>(roleList);
            this.SubscriberTier = subscriberTier;
            this.PatreonBenefitID = patreonBenefitID;
        }

        public string DisplayRole
        {
            get
            {
                if (this.RoleList.Count > 0)
                {
                    return MixItUp.Base.Resources.Multiple;
                }
                else
                {
                    return EnumLocalizationHelper.GetLocalizedName(this.Role);
                }
            }
        }

        protected override DateTimeOffset RequirementErrorCooldown { get { return RoleRequirementModel.requirementErrorCooldown; } set { RoleRequirementModel.requirementErrorCooldown = value; } }

        public override Task<Result> Validate(CommandParametersModel parameters)
        {
            if (this.RoleList.Count > 0)
            {
                foreach (UserRoleEnum role in parameters.User.UserRoles)
                {
                    if (this.RoleList.Contains(role))
                    {
                        if (role != UserRoleEnum.Subscriber || parameters.User.SubscribeTier >= this.SubscriberTier)
                        {
                            return Task.FromResult(new Result());
                        }
                    }
                }
                return Task.FromResult(this.CreateErrorMessage(parameters));
            }
            else
            {
                if (!parameters.User.HasPermissionsTo(this.Role))
                {
                    if (!string.IsNullOrEmpty(this.PatreonBenefitID) && ServiceManager.Get<PatreonService>().IsConnected)
                    {
                        PatreonBenefit benefit = ServiceManager.Get<PatreonService>().Campaign.GetBenefit(this.PatreonBenefitID);
                        if (benefit != null)
                        {
                            PatreonTier tier = parameters.User.PatreonTier;
                            if (tier != null && tier.BenefitIDs.Contains(benefit.ID))
                            {
                                return Task.FromResult(new Result());
                            }
                        }
                    }

                    return Task.FromResult(this.CreateErrorMessage(parameters));
                }

                if (this.Role == UserRoleEnum.Subscriber && !parameters.User.ExceedsPermissions(this.Role))
                {
                    if (parameters.User.SubscribeTier < this.SubscriberTier)
                    {
                        return Task.FromResult(this.CreateErrorMessage(parameters));
                    }
                }
            }
            return Task.FromResult(new Result());
        }

        private Result CreateErrorMessage(CommandParametersModel parameters)
        {
            List<string> roleNames = new List<string>();
            if (this.RoleList.Count > 0)
            {
                foreach (UserRoleEnum role in this.RoleList)
                {
                    roleNames.Add(this.GetRoleName(role));
                }
            }
            else
            {
                roleNames.Add(this.GetRoleName(this.Role));
            }
            return new Result(string.Format(MixItUp.Base.Resources.RoleErrorInsufficientRole, string.Join(" / ", roleNames)));
        }

        private string GetRoleName(UserRoleEnum role)
        {
            string roleName = EnumLocalizationHelper.GetLocalizedName(role);
            if (role == UserRoleEnum.Subscriber)
            {
                string tierText = string.Empty;
                switch (this.SubscriberTier)
                {
                    case 1: tierText = MixItUp.Base.Resources.Tier1; break;
                    case 2: tierText = MixItUp.Base.Resources.Tier2; break;
                    case 3: tierText = MixItUp.Base.Resources.Tier3; break;
                }
                roleName = tierText + " " + roleName;
            }
            else if (role == UserRoleEnum.VIPExclusive)
            {
                roleName = EnumLocalizationHelper.GetLocalizedName(UserRoleEnum.VIP);
            }
            return roleName;
        }
    }
}
