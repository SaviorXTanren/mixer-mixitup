using MixItUp.Base.Model.Actions;
using MixItUp.Base.Util;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Actions
{
    public class RandomActionEditorControlViewModel : SubActionContainerControlViewModel
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.Random; } }

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

        public bool NoDuplicates
        {
            get { return this.noDuplicates; }
            set
            {
                this.noDuplicates = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool noDuplicates;

        public RandomActionEditorControlViewModel(RandomActionModel action)
            : base(action, action.Actions)
        {
            this.Amount = action.Amount;
            this.NoDuplicates = action.NoDuplicates;
        }

        public RandomActionEditorControlViewModel() : base() { }

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
            return new RandomActionModel(this.Amount, this.NoDuplicates, await this.ActionEditorList.GetActions());
        }
    }
}
