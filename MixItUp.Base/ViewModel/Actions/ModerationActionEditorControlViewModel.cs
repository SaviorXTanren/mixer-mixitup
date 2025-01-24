using MixItUp.Base.Model.Actions;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Actions
{
    public class ModerationActionEditorControlViewModel : ActionEditorControlViewModelBase
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.Moderation; } }

        public IEnumerable<ModerationActionTypeEnum> ActionTypes { get { return EnumHelper.GetEnumList<ModerationActionTypeEnum>(); } }

        public ModerationActionTypeEnum SelectedActionType
        {
            get { return this.selectedActionType; }
            set
            {
                this.selectedActionType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("ShowTargetUserGrid");
                this.NotifyPropertyChanged("ShowTimeoutGrid");
                this.NotifyPropertyChanged("ShowModerationReasonGrid");
            }
        }
        private ModerationActionTypeEnum selectedActionType;

        public bool ShowTargetUserGrid
        {
            get
            {
                return this.SelectedActionType == ModerationActionTypeEnum.TimeoutUser || this.SelectedActionType == ModerationActionTypeEnum.PurgeUser ||
                    this.SelectedActionType == ModerationActionTypeEnum.BanUser || this.SelectedActionType == ModerationActionTypeEnum.UnbanUser ||
                    this.SelectedActionType == ModerationActionTypeEnum.ModUser || this.SelectedActionType == ModerationActionTypeEnum.UnmodUser ||
                    this.SelectedActionType == ModerationActionTypeEnum.AddModerationStrike || this.SelectedActionType == ModerationActionTypeEnum.RemoveModerationStrike;
            }
        }

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

        public bool ShowTimeoutGrid { get { return this.SelectedActionType == ModerationActionTypeEnum.TimeoutUser; } }

        public string TimeoutAmount
        {
            get { return this.timeoutAmount; }
            set
            {
                this.timeoutAmount = value;
                this.NotifyPropertyChanged();
            }
        }
        private string timeoutAmount;

        public bool ShowModerationReasonGrid
        {
            get
            {
                return this.SelectedActionType == ModerationActionTypeEnum.TimeoutUser || this.SelectedActionType == ModerationActionTypeEnum.BanUser || 
                    this.selectedActionType == ModerationActionTypeEnum.AddModerationStrike;
            }
        }

        public string ModerationReason
        {
            get { return this.moderationReason; }
            set
            {
                this.moderationReason = value;
                this.NotifyPropertyChanged();
            }
        }
        private string moderationReason;

        public ModerationActionEditorControlViewModel(ModerationActionModel action)
            : base(action)
        {
            this.SelectedActionType = action.ActionType;
            this.TargetUsername = action.TargetUsername;
            this.TimeoutAmount = action.TimeoutAmount;
            this.ModerationReason = action.ModerationReason;
        }

        public ModerationActionEditorControlViewModel() : base() { }

        public override Task<Result> Validate()
        {
            if (this.ShowTimeoutGrid)
            {
                if (string.IsNullOrEmpty(this.TimeoutAmount))
                {
                    return Task.FromResult(new Result(MixItUp.Base.Resources.ModerationActionMissingTimeoutAmount));
                }
            }
            return Task.FromResult(new Result());
        }

        protected override Task<ActionModelBase> GetActionInternal()
        {
            return Task.FromResult<ActionModelBase>(new ModerationActionModel(this.SelectedActionType, this.TargetUsername, this.TimeoutAmount, this.ModerationReason));
        }
    }
}
