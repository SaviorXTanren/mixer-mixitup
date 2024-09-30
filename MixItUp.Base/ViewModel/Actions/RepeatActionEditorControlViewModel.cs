using MixItUp.Base.Model.Actions;
using MixItUp.Base.Util;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Actions
{
    public class RepeatActionEditorControlViewModel : GroupActionEditorControlViewModel
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.Repeat; } }

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

        public RepeatActionEditorControlViewModel(RepeatActionModel action)
            : base(action)
        {
            this.Amount = action.Amount;
        }

        public RepeatActionEditorControlViewModel() : base() { }

        public override async Task<Result> Validate()
        {
            Result result = await base.Validate();
            if (!result.Success)
            {
                return result;
            }

            if (string.IsNullOrEmpty(this.Amount))
            {
                return new Result(Resources.ValidAmountGreaterThan0MustBeSpecified);
            }

            return new Result();
        }

        protected override async Task<ActionModelBase> GetActionInternal()
        {
            return new RepeatActionModel(this.Amount, await this.ActionEditorList.GetActions());
        }
    }
}
