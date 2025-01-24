using MixItUp.Base.Model.Actions;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Actions
{
    public class TrovoActionEditorControlViewModel : ActionEditorControlViewModelBase
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.Trovo; } }

        public IEnumerable<TrovoActionType> ActionTypes { get { return EnumHelper.GetEnumList<TrovoActionType>(); } }

        public TrovoActionType SelectedActionType
        {
            get { return this.selectedActionType; }
            set
            {
                this.selectedActionType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("ShowUsernameGrid");
                this.NotifyPropertyChanged("ShowTextGrid");
                this.NotifyPropertyChanged("ShowAmountGrid");
                this.NotifyPropertyChanged("ShowRoleGrid");
            }
        }
        private TrovoActionType selectedActionType;

        public bool ShowUsernameGrid
        {
            get
            {
                return this.SelectedActionType == TrovoActionType.Host || this.SelectedActionType == TrovoActionType.AddUserRole || this.SelectedActionType == TrovoActionType.RemoveUserRole;
            }
        }

        public string Username
        {
            get { return this.username; }
            set
            {
                this.username = value;
                this.NotifyPropertyChanged();
            }
        }
        private string username;

        public bool ShowTextGrid { get { return this.SelectedActionType == TrovoActionType.SetTitle || this.SelectedActionType == TrovoActionType.SetGame; } }

        public string Text
        {
            get { return this.text; }
            set
            {
                this.text = value;
                this.NotifyPropertyChanged();
            }
        }
        private string text;

        public bool ShowAmountGrid
        {
            get
            {
                return this.SelectedActionType == TrovoActionType.EnableSlowMode;
            }
        }

        public int Amount
        {
            get { return this.amount; }
            set
            {
                this.amount = value;
                this.NotifyPropertyChanged();
            }
        }
        private int amount;

        public bool ShowRoleGrid
        {
            get
            {
                return this.SelectedActionType == TrovoActionType.AddUserRole || this.SelectedActionType == TrovoActionType.RemoveUserRole;
            }
        }

        public string RoleName
        {
            get { return this.roleName; }
            set
            {
                this.roleName = value;
                this.NotifyPropertyChanged();
            }
        }
        private string roleName;

        public TrovoActionEditorControlViewModel(TrovoActionModel action)
            : base(action)
        {
            this.SelectedActionType = action.ActionType;

            if (this.ShowUsernameGrid)
            {
                this.Username = action.Username;
            }

            if (this.ShowTextGrid)
            {
                this.Text = action.Text;
            }

            if (this.ShowAmountGrid)
            {
                this.Amount = action.Amount;
            }

            if (this.ShowRoleGrid)
            {
                this.RoleName = action.RoleName;
            }
        }

        public TrovoActionEditorControlViewModel() : base() { }

        public override async Task<Result> Validate()
        {
            if (this.selectedActionType == TrovoActionType.Host)
            {
                if (string.IsNullOrEmpty(this.Username))
                {
                    return new Result(MixItUp.Base.Resources.TrovoActionUsernameMissing);
                }
            }

            if (this.ShowTextGrid)
            {
                if (string.IsNullOrEmpty(this.Text))
                {
                    return new Result(MixItUp.Base.Resources.TrovoActionNameMissing);
                }
            }

            if (this.ShowAmountGrid)
            {
                if (this.Amount <= 0)
                {
                    return new Result(MixItUp.Base.Resources.TrovoActionAmountMissing);
                }
            }

            if (this.ShowRoleGrid)
            {
                if (string.IsNullOrEmpty(this.RoleName))
                {
                    return new Result(MixItUp.Base.Resources.TrovoActionRoleNameMissing);
                }
            }

            return await base.Validate();
        }

        protected override Task<ActionModelBase> GetActionInternal()
        {
            if (this.SelectedActionType == TrovoActionType.Host)
            {
                return Task.FromResult<ActionModelBase>(TrovoActionModel.CreateHostAction(this.Username));
            }
            else if (this.SelectedActionType == TrovoActionType.EnableSlowMode)
            {
                return Task.FromResult<ActionModelBase>(TrovoActionModel.CreateEnableSlowModeAction(this.Amount));
            }
            else if (this.SelectedActionType == TrovoActionType.DisableSlowMode)
            {
                return Task.FromResult<ActionModelBase>(TrovoActionModel.CreateBasicAction(TrovoActionType.DisableSlowMode));
            }
            else if (this.SelectedActionType == TrovoActionType.EnableFollowerMode)
            {
                return Task.FromResult<ActionModelBase>(TrovoActionModel.CreateBasicAction(TrovoActionType.EnableFollowerMode));
            }
            else if (this.SelectedActionType == TrovoActionType.DisableFollowerMode)
            {
                return Task.FromResult<ActionModelBase>(TrovoActionModel.CreateBasicAction(TrovoActionType.DisableFollowerMode));
            }
            else if (this.SelectedActionType == TrovoActionType.AddUserRole)
            {
                return Task.FromResult<ActionModelBase>(TrovoActionModel.CreateUserRoleAction(TrovoActionType.AddUserRole, this.Username, this.RoleName));
            }
            else if (this.SelectedActionType == TrovoActionType.RemoveUserRole)
            {
                return Task.FromResult<ActionModelBase>(TrovoActionModel.CreateUserRoleAction(TrovoActionType.RemoveUserRole, this.Username, this.RoleName));
            }
            else if (this.SelectedActionType == TrovoActionType.FastClip90Seconds)
            {
                return Task.FromResult<ActionModelBase>(TrovoActionModel.CreateBasicAction(TrovoActionType.FastClip90Seconds));
            }
            else if (this.SelectedActionType == TrovoActionType.SetTitle)
            {
                return Task.FromResult<ActionModelBase>(TrovoActionModel.CreateTextAction(TrovoActionType.SetTitle, this.Text));
            }
            else if (this.SelectedActionType == TrovoActionType.SetGame)
            {
                return Task.FromResult<ActionModelBase>(TrovoActionModel.CreateTextAction(TrovoActionType.SetGame, this.Text));
            }
            else if (this.SelectedActionType == TrovoActionType.EnableSubscriberMode)
            {
                return Task.FromResult<ActionModelBase>(TrovoActionModel.CreateBasicAction(TrovoActionType.EnableSubscriberMode));
            }
            else if (this.SelectedActionType == TrovoActionType.DisableSubscriberMode)
            {
                return Task.FromResult<ActionModelBase>(TrovoActionModel.CreateBasicAction(TrovoActionType.DisableSubscriberMode));
            }
            return Task.FromResult<ActionModelBase>(null);
        }
    }
}