using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.Requirements;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Requirements
{
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

        public CurrencyRequirementViewModel() { }

        public CurrencyRequirementViewModel(CurrencyRequirementModel requirement)
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
