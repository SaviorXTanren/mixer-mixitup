using MixItUp.Base.Model;
using MixItUp.Base.Model.Actions;
using MixItUp.Base.Model.Requirements;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Actions
{
    public class GameQueueActionEditorControlViewModel : ActionEditorControlViewModelBase
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.GameQueue; } }

        public IEnumerable<GameQueueActionType> ActionTypes { get { return EnumHelper.GetEnumList<GameQueueActionType>(); } }

        public GameQueueActionType SelectedActionType
        {
            get { return this.selectedActionType; }
            set
            {
                this.selectedActionType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("ShowTargetUsername");
                this.NotifyPropertyChanged("ShowUserRoles");
            }
        }
        private GameQueueActionType selectedActionType;

        public string TargetUsername
        {
            get { return this.targetUsername; }
            set
            {
                this.targetUsername = value;
                this.NotifyPropertyChanged();
            }
        }
        private string targetUsername;

        public bool ShowTargetUsername
        {
            get
            {
                return this.SelectedActionType == GameQueueActionType.JoinFrontOfQueue || this.SelectedActionType == GameQueueActionType.JoinQueue ||
                    this.SelectedActionType == GameQueueActionType.LeaveQueue || this.SelectedActionType == GameQueueActionType.QueuePosition;
            }
        }

        public IEnumerable<UserRoleEnum> UserRoles { get { return MixItUp.Base.Model.User.UserRoles.All; } }

        public UserRoleEnum SelectedUserRole
        {
            get { return this.selectedUserRole; }
            set
            {
                this.selectedUserRole = value;
                this.NotifyPropertyChanged();
            }
        }
        private UserRoleEnum selectedUserRole;

        public bool ShowUserRoles { get { return this.SelectedActionType == GameQueueActionType.SelectFirstType; } }

        public GameQueueActionEditorControlViewModel(GameQueueActionModel action)
            : base(action)
        {
            this.SelectedActionType = action.ActionType;
            if (this.ShowTargetUsername)
            {
                this.TargetUsername = action.TargetUsername;
            }
            else if (this.ShowUserRoles)
            {
                this.SelectedUserRole = (action.RoleRequirement != null) ? action.RoleRequirement.UserRole : UserRoleEnum.User;
            }
        }

        public GameQueueActionEditorControlViewModel() : base() { }

        protected override Task<ActionModelBase> GetActionInternal()
        {
            if (this.ShowTargetUsername)
            {
                return Task.FromResult<ActionModelBase>(new GameQueueActionModel(this.SelectedActionType, targetUsername: this.TargetUsername));
            }
            else if (this.ShowUserRoles)
            {
                return Task.FromResult<ActionModelBase>(new GameQueueActionModel(this.SelectedActionType, roleRequirement: new RoleRequirementModel(StreamingPlatformTypeEnum.All, this.SelectedUserRole)));
            }
            else
            {
                return Task.FromResult<ActionModelBase>(new GameQueueActionModel(this.SelectedActionType));
            }
        }
    }
}
