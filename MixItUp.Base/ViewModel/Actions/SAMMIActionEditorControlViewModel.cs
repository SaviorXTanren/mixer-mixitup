using MixItUp.Base.Model.Actions;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Actions
{
    public class SAMMIActionEditorControlViewModel : ActionEditorControlViewModelBase
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.SAMMI; } }

        public IEnumerable<SAMMIActionTypeEnum> ActionTypes { get { return EnumHelper.GetEnumList<SAMMIActionTypeEnum>(); } }

        public SAMMIActionTypeEnum SelectedActionType
        {
            get { return this.selectedActionType; }
            set
            {
                this.selectedActionType = value;
                this.NotifyPropertyChanged();
            }
        }
        private SAMMIActionTypeEnum selectedActionType = SAMMIActionTypeEnum.TriggerButton;

        public string ButtonID
        {
            get { return this.buttonID; }
            set
            {
                this.buttonID = value;
                this.NotifyPropertyChanged();
            }
        }
        private string buttonID;

        public SAMMIActionEditorControlViewModel(SAMMIActionModel action)
            : base(action)
        {
            this.SelectedActionType = action.ActionType;
            this.ButtonID = action.ButtonID;
        }

        public SAMMIActionEditorControlViewModel() : base() { }

        public override Task<Result> Validate()
        {
            if (string.IsNullOrWhiteSpace(this.ButtonID))
            {
                return Task.FromResult<Result>(new Result(Resources.SAMMIActionMissingButtonID));
            }
            return Task.FromResult<Result>(new Result());
        }

        protected override Task<ActionModelBase> GetActionInternal()
        {
            if (!string.IsNullOrWhiteSpace(this.ButtonID))
            {
                return Task.FromResult<ActionModelBase>(new SAMMIActionModel(this.SelectedActionType, this.ButtonID));
            }
            return Task.FromResult<ActionModelBase>(null);
        }
    }
}
