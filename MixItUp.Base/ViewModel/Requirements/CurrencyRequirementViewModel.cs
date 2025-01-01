using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.Requirements;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Requirements
{
    public class CurrencyListRequirementViewModel : ListRequirementViewModelBase
    {
        public ObservableCollection<CurrencyRequirementViewModel> Items { get; set; } = new ObservableCollection<CurrencyRequirementViewModel>();

        public bool RestrictToRequiredAmountOnly { get; set; }

        public ICommand AddItemCommand { get; private set; }

        public CurrencyListRequirementViewModel()
        {
            this.AddItemCommand = this.CreateCommand(() =>
            {
                this.Items.Add(new CurrencyRequirementViewModel(this));
            });
        }

        public void Add(CurrencyRequirementModel requirement)
        {
            this.Items.Add(new CurrencyRequirementViewModel(this, requirement));
        }

        public void Delete(CurrencyRequirementViewModel requirement)
        {
            this.Items.Remove(requirement);
        }
    }

    public class CurrencyRequirementViewModel : RequirementViewModelBase
    {
        public IEnumerable<CurrencyModel> Currencies { get { return ChannelSession.Settings.Currency.Values.ToList(); } }

        public CurrencyModel SelectedCurrency
        {
            get { return this.selectedCurrency; }
            set
            {
                this.selectedCurrency = value;
                this.NotifyPropertyChanged();
            }
        }
        private CurrencyModel selectedCurrency;

        public IEnumerable<CurrencyRequirementTypeEnum> RequirementTypes
        {
            get
            {
                if (this.viewModel.RestrictToRequiredAmountOnly)
                {
                    return new List<CurrencyRequirementTypeEnum>() { CurrencyRequirementTypeEnum.RequiredAmount };
                }
                return EnumHelper.GetEnumList<CurrencyRequirementTypeEnum>();
            }
        }

        public CurrencyRequirementTypeEnum SelectedRequirementType
        {
            get { return selectedRequirementType; }
            set
            {
                this.selectedRequirementType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("CostEnabled");
                this.NotifyPropertyChanged("ShowOnlyMin");
                this.NotifyPropertyChanged("ShowMinAndMax");
            }
        }
        private CurrencyRequirementTypeEnum selectedRequirementType = CurrencyRequirementTypeEnum.RequiredAmount;

        public bool ShowOnlyMin { get { return !this.ShowMinAndMax; } }

        public bool ShowMinAndMax { get { return this.SelectedRequirementType == CurrencyRequirementTypeEnum.MinimumAndMaximum; } }

        public int MinAmount
        {
            get { return this.minAmount; }
            set
            {
                this.minAmount = value;
                this.NotifyPropertyChanged();
            }
        }
        private int minAmount = 0;

        public int MaxAmount
        {
            get { return this.maxAmount; }
            set
            {
                this.maxAmount = value;
                this.NotifyPropertyChanged();
            }
        }
        private int maxAmount = 0;

        public ICommand DeleteCommand { get; private set; }

        private CurrencyListRequirementViewModel viewModel;

        public CurrencyRequirementViewModel(CurrencyListRequirementViewModel viewModel)
        {
            this.viewModel = viewModel;
            this.SelectedRequirementType = CurrencyRequirementTypeEnum.RequiredAmount;

            this.DeleteCommand = this.CreateCommand(() =>
            {
                this.viewModel.Delete(this);
            });
        }

        public CurrencyRequirementViewModel(CurrencyListRequirementViewModel viewModel, CurrencyRequirementModel requirement)
            : this(viewModel)
        {
            this.SelectedCurrency = requirement.Currency;
            this.SelectedRequirementType = requirement.RequirementType;
            this.MinAmount = requirement.MinAmount;
            this.MaxAmount = requirement.MaxAmount;
        }

        public override Task<Result> Validate()
        {
            if (this.SelectedCurrency == null)
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.ValidCurrencyMustBeSelected));
            }

            if (this.MinAmount < 0)
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.ValidCurrencyAmountMustBeSpecified));
            }

            if (this.SelectedRequirementType == CurrencyRequirementTypeEnum.MinimumAndMaximum)
            {
                if (this.MaxAmount < 0)
                {
                    return Task.FromResult(new Result(MixItUp.Base.Resources.ValidCurrencyAmountMustBeSpecified));
                }

                if (this.MaxAmount < this.MinAmount)
                {
                    return Task.FromResult(new Result(MixItUp.Base.Resources.ValidCurrencyAmountMustBeSpecified));
                }
            }

            return Task.FromResult(new Result());
        }

        public override RequirementModelBase GetRequirement()
        {
            if (this.SelectedCurrency != null)
            {
                return new CurrencyRequirementModel(this.SelectedCurrency, this.SelectedRequirementType, this.MinAmount, this.MaxAmount);
            }
            return null;
        }
    }
}
