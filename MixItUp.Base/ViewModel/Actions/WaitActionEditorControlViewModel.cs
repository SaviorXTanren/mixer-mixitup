using MixItUp.Base.Model.Actions;
using MixItUp.Base.Util;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Actions
{
    public class WaitActionEditorControlViewModel : ActionEditorControlViewModelBase
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.Wait; } }

        public string Amount
        {
            get { return this.amount; }
            set
            {
                this.amount = value;
                this.NotifyPropertyChanged();
            }
        }
        private string amount;

        public WaitActionEditorControlViewModel(WaitActionModel action)
            : base(action)
        {
            this.Amount = action.Amount;
        }

        public WaitActionEditorControlViewModel() : base() { }

        public override Task<Result> Validate()
        {
            if (string.IsNullOrEmpty(this.Amount))
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.WaitActionMissingAmount));
            }
            return Task.FromResult(new Result());
        }

        protected override Task<ActionModelBase> GetActionInternal() { return Task.FromResult<ActionModelBase>(new WaitActionModel(this.Amount)); }
    }
}
