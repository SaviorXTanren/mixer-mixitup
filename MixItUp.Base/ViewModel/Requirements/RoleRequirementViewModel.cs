using Google.Apis.YouTube.v3.Data;
using MixItUp.Base.Model;
using MixItUp.Base.Model.Requirements;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Services.Trovo.New;
using MixItUp.Base.Services.YouTube;
using MixItUp.Base.Services.YouTube.New;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Requirements
{
    public class UserRoleViewModel : UIViewModelBase, IComparable<UserRoleViewModel>
    {
        public UserRoleEnum Role
        {
            get { return this.role; }
            set
            {
                this.role = value;
                this.NotifyPropertyChanged();
            }
        }
        private UserRoleEnum role;

        public ICommand DeleteAdvancedRoleCommand { get; private set; }

        private RoleRequirementViewModel viewModel;

        public UserRoleViewModel(RoleRequirementViewModel viewModel, UserRoleEnum role)
        {
            this.viewModel = viewModel;
            this.Role = role;

            this.DeleteAdvancedRoleCommand = this.CreateCommand(() =>
            {
                this.viewModel.SelectedAdvancedRoles.Remove(this);
            });
        }

        public string Name { get { return EnumLocalizationHelper.GetLocalizedName(this.Role); } }

        public int CompareTo(UserRoleViewModel other) { return this.Role.CompareTo(other.Role); }
    }

    public class RoleRequirementViewModel : RequirementViewModelBase
    {
        private static PatreonBenefit NonePatreonBenefit = new PatreonBenefit() { ID = string.Empty, Title = "None" };

        public IEnumerable<StreamingPlatformTypeEnum> Platforms { get { return StreamingPlatforms.SelectablePlatforms; } }

        public StreamingPlatformTypeEnum SelectedPlatform
        {
            get { return this.selectedPlatform; }
            set
            {
                this.selectedPlatform = value;
                this.NotifyPropertyChanged();
            }
        }
        private StreamingPlatformTypeEnum selectedPlatform = StreamingPlatformTypeEnum.All;

        public bool IsAdvancedRolesSelected
        {
            get { return this.isAdvancedRolesSelected; }
            set
            {
                this.isAdvancedRolesSelected = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("ShowSimpleRoles");
                this.NotifyPropertyChanged("IsSubscriberRole");
            }
        }
        private bool isAdvancedRolesSelected = false;

        public bool ShowSimpleRoles { get { return !this.IsAdvancedRolesSelected; } }

        public IEnumerable<UserRoleEnum> Roles { get { return UserRoles.Generic; } }

        public UserRoleEnum SelectedRole
        {
            get { return this.selectedRole; }
            set
            {
                this.selectedRole = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("IsSubscriberRole");
            }
        }
        private UserRoleEnum selectedRole = UserRoleEnum.User;

        public IEnumerable<UserRoleEnum> AdvancedRoles { get { return UserRoles.All; } }

        public UserRoleEnum SelectedAdvancedRole
        {
            get { return this.selectedAdvancedRole; }
            set
            {
                this.selectedAdvancedRole = value;
                this.NotifyPropertyChanged();
            }
        }
        private UserRoleEnum selectedAdvancedRole = UserRoleEnum.User;

        public ObservableCollection<UserRoleViewModel> SelectedAdvancedRoles { get; set; } = new ObservableCollection<UserRoleViewModel>();

        public bool IsSubscriberRole
        {
            get
            {
                if (this.IsAdvancedRolesSelected)
                {
                    return this.SelectedAdvancedRoles.Any(r => r.Role == UserRoleEnum.Subscriber);
                }
                else
                {
                    return this.SelectedRole == UserRoleEnum.Subscriber;
                }
            }
        }

        public IEnumerable<int> SubscriberTiers { get { return new List<int>() { 1, 2, 3 }; } }
        public int SubscriberTier
        {
            get { return this.subscriberTier; }
            set
            {
                this.subscriberTier = value;
                this.NotifyPropertyChanged();
            }
        }
        private int subscriberTier = 1;

        public bool IsYouTubeConnected { get { return ServiceManager.Get<YouTubeSession>().IsConnected; } }

        public IEnumerable<MembershipsLevel> YouTubeMembershipLevels { get; private set; }

        public MembershipsLevel YouTubeMembershipLevel
        {
            get { return this.youtubeMembershipLevel; }
            set
            {
                this.youtubeMembershipLevel = value;
                this.NotifyPropertyChanged();
            }
        }
        private MembershipsLevel youtubeMembershipLevel;

        public bool IsTrovoConnected { get { return ServiceManager.Get<TrovoSession>().IsConnected; } }

        public string TrovoCustomRole
        {
            get { return this.trovoCustomRole; }
            set
            {
                this.trovoCustomRole = value;
                this.NotifyPropertyChanged();
            }
        }
        private string trovoCustomRole;

        public bool IsPatreonConnected { get { return ServiceManager.Get<PatreonService>().IsConnected; } }

        public IEnumerable<PatreonBenefit> PatreonBenefits
        {
            get
            {
                List<PatreonBenefit> benefits = new List<PatreonBenefit>();
                benefits.Add(RoleRequirementViewModel.NonePatreonBenefit);
                if (this.IsPatreonConnected)
                {
                    benefits.AddRange(ServiceManager.Get<PatreonService>().Campaign.Benefits.Values.OrderBy(b => b.Title));
                }
                return benefits;
            }
        }

        public PatreonBenefit SelectedPatreonBenefit
        {
            get { return this.selectedPatreonBenefit; }
            set
            {
                this.selectedPatreonBenefit = value;
                this.NotifyPropertyChanged();
            }
        }
        private PatreonBenefit selectedPatreonBenefit = RoleRequirementViewModel.NonePatreonBenefit;

        public ICommand AddAdvancedRoleCommand { get; set; }

        public RoleRequirementViewModel()
        {
            this.AddAdvancedRoleCommand = this.CreateCommand(() =>
            {
                if (!this.SelectedAdvancedRoles.Any(r => r.Role == this.SelectedAdvancedRole))
                {
                    this.SelectedAdvancedRoles.Add(new UserRoleViewModel(this, this.SelectedAdvancedRole));
                }
                this.NotifyPropertyChanged("IsSubscriberRole");
            });
        }

        public RoleRequirementViewModel(RoleRequirementModel requirement)
            : this()
        {
            this.SelectedPlatform = requirement.StreamingPlatform;
            if (requirement.UserRoleList.Count > 0)
            {
                this.IsAdvancedRolesSelected = true;
                foreach (UserRoleEnum role in requirement.UserRoleList)
                {
                    this.SelectedAdvancedRoles.Add(new UserRoleViewModel(this, role));
                }
            }
            else
            {
                this.SelectedRole = requirement.UserRole;
            }

            this.SubscriberTier = requirement.SubscriberTier;

            if (this.IsYouTubeConnected)
            {
                this.YouTubeMembershipLevels = ServiceManager.Get<YouTubeSession>().MembershipLevels;
                this.YouTubeMembershipLevel = this.YouTubeMembershipLevels.FirstOrDefault(m => string.Equals(m.Id, requirement.YouTubeMembershipLevelID));
            }

            this.TrovoCustomRole = requirement.TrovoCustomRole;

            if (this.IsPatreonConnected && !string.IsNullOrEmpty(requirement.PatreonBenefitID))
            {
                this.SelectedPatreonBenefit = this.PatreonBenefits.FirstOrDefault(b => b.ID.Equals(requirement.PatreonBenefitID));
                if (this.SelectedPatreonBenefit == null)
                {
                    this.SelectedPatreonBenefit = RoleRequirementViewModel.NonePatreonBenefit;
                }
            }
        }

        public override async Task<Result> Validate()
        {
            if (this.IsAdvancedRolesSelected)
            {
                if (this.SelectedAdvancedRoles.Count == 0)
                {
                    return new Result(MixItUp.Base.Resources.RoleRequirementAtLeastOneRoleMustBeSelected);
                }
            }
            return await base.Validate();
        }

        public override RequirementModelBase GetRequirement()
        {
            if (this.IsAdvancedRolesSelected)
            {
                return new RoleRequirementModel(this.SelectedPlatform, this.SelectedAdvancedRoles.Select(r => r.Role), this.SubscriberTier, this.YouTubeMembershipLevel?.Id, this.TrovoCustomRole, this.selectedPatreonBenefit?.ID);
            }
            else
            {
                return new RoleRequirementModel(this.SelectedPlatform, this.SelectedRole, this.SubscriberTier, this.YouTubeMembershipLevel?.Id, this.TrovoCustomRole, this.selectedPatreonBenefit?.ID);
            }
        }
    }
}
