using MixItUp.Base.Model.Actions;
using MixItUp.Base.Util;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Actions
{
    public class RandomActionEditorControlViewModel : GroupActionEditorControlViewModel
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

                if (!this.NoDuplicates)
                {
                    this.PersistNoDuplicates = false;
                }

                this.NotifyPropertyChanged(nameof(this.IsPersistNoDuplicatesEnabled));
            }
        }
        private bool noDuplicates;

        public bool IsPersistNoDuplicatesEnabled { get { return this.NoDuplicates; } }

        public bool PersistNoDuplicates
        {
            get { return this.persistNoDuplicates; }
            set
            {
                this.persistNoDuplicates = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool persistNoDuplicates;

        public RandomActionEditorControlViewModel(RandomActionModel action)
            : base(action)
        {
            this.Amount = action.Amount;
            this.NoDuplicates = action.NoDuplicates;
            this.PersistNoDuplicates = action.PersistNoDuplicates;
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
            return new RandomActionModel(this.Amount, this.NoDuplicates, this.PersistNoDuplicates, await this.ActionEditorList.GetActions());
        }
    }
}
