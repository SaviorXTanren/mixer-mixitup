using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.Requirements;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Requirements
{
    public class CurrencyListRequirementViewModel : ListRequirementViewModelBase
    {
        public ObservableCollection<CurrencyRequirementViewModel> Items { get; set; } = new ObservableCollection<CurrencyRequirementViewModel>();

        public ICommand AddItemCommand { get; private set; }

        public CurrencyListRequirementViewModel()
        {
            this.AddItemCommand = this.CreateCommand((parameter) =>
            {
                this.Items.Add(new CurrencyRequirementViewModel(this));
                return Task.FromResult(0);
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
        public IEnumerable<CurrencyModel> Currencies { get { return ChannelSession.Settings.Currency.Values; } }

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

        public int Amount
        {
            get { return this.amount; }
            set
            {
                if (this.amount >= 0)
                {
                    this.amount = value;
                }
                this.NotifyPropertyChanged();
            }
        }
        private int amount = 0;

        public ICommand DeleteCommand { get; private set; }

        private CurrencyListRequirementViewModel viewModel;

        public CurrencyRequirementViewModel(CurrencyListRequirementViewModel viewModel)
        {
            this.viewModel = viewModel;

            this.DeleteCommand = this.CreateCommand((parameter) =>
            {
                this.viewModel.Delete(this);
                return Task.FromResult(0);
            });
        }

        public CurrencyRequirementViewModel(CurrencyListRequirementViewModel viewModel, CurrencyRequirementModel requirement)
            : this(viewModel)
        {
            this.SelectedCurrency = requirement.Currency;
            this.Amount = requirement.Amount;
        }

        public override async Task<bool> Validate()
        {
            if (this.SelectedCurrency == null)
            {
                await DialogHelper.ShowMessage(MixItUp.Base.Resources.ValidCurrencyMustBeSelected);
                return false;
            }

            if (this.Amount <= 0)
            {
                await DialogHelper.ShowMessage(MixItUp.Base.Resources.ValidCurrencyAmountMustBeSpecified);
                return false;
            }

            return true;
        }

        public override RequirementModelBase GetRequirement()
        {
            return new CurrencyRequirementModel(this.SelectedCurrency, this.Amount);
        }
    }
}
