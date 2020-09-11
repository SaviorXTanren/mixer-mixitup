using MixItUp.Base.Model.Actions;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Controls.Actions
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
            }
        }
        private ModerationActionTypeEnum selectedActionType;

        public bool ShowTargetUserGrid { get { return this.SelectedActionType != ModerationActionTypeEnum.ClearChat; } }

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

        public bool ShowModerationReasonGrid { get { return this.selectedActionType == ModerationActionTypeEnum.AddModerationStrike; } }

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
        {
            this.SelectedActionType = action.ActionType;
            this.TargetUsername = action.TargetUsername;
            this.TimeoutAmount = action.TimeoutAmount;
            this.ModerationReason = action.ModerationReason;
        }

        public ModerationActionEditorControlViewModel() { }

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

        public override Task<ActionModelBase> GetAction()
        {
            return Task.FromResult<ActionModelBase>(new ModerationActionModel(this.SelectedActionType, this.TargetUsername, this.TimeoutAmount, this.ModerationReason));
        }
    }
}
