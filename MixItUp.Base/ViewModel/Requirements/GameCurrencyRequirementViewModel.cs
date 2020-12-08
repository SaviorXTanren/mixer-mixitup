using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.Requirements;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Requirements
{
    public class GameCurrencyRequirementViewModel : RequirementViewModelBase
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

        public IEnumerable<GameCurrencyRequirementTypeEnum> GameCurrencyRequirementTypes { get { return EnumHelper.GetEnumList<GameCurrencyRequirementTypeEnum>(); } }

        public GameCurrencyRequirementTypeEnum SelectedGameCurrencyRequirementType
        {
            get { return selectedGameCurrencyRequirementType; }
            set
            {
                this.selectedGameCurrencyRequirementType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("CostEnabled");
                this.NotifyPropertyChanged("ShowOnlyMin");
                this.NotifyPropertyChanged("ShowMinAndMax");
            }
        }
        private GameCurrencyRequirementTypeEnum selectedGameCurrencyRequirementType;

        public bool CostEnabled { get { return this.SelectedGameCurrencyRequirementType != GameCurrencyRequirementTypeEnum.NoCost; } }

        public bool ShowOnlyMin { get { return !this.ShowMinAndMax; } }

        public bool ShowMinAndMax { get { return this.SelectedGameCurrencyRequirementType == GameCurrencyRequirementTypeEnum.MinimumAndMaximum; } }

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

        public GameCurrencyRequirementViewModel(CurrencyModel currency)
        {
            this.SelectedCurrency = currency;
            this.SelectedGameCurrencyRequirementType = GameCurrencyRequirementTypeEnum.RequiredAmount;
            this.MinAmount = 10;
            this.MaxAmount = 100;
        }

        public GameCurrencyRequirementViewModel(GameCurrencyRequirementModel requirement)
        {
            this.SelectedCurrency = requirement.Currency;
            this.SelectedGameCurrencyRequirementType = requirement.GameCurrencyRequirementType;
            this.MinAmount = requirement.Amount;
            this.MaxAmount = requirement.MaxAmount;
        }

        public override Task<Result> Validate()
        {
            if (this.SelectedCurrency == null)
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.ValidCurrencyMustBeSelected));
            }

            if (this.SelectedGameCurrencyRequirementType != GameCurrencyRequirementTypeEnum.NoCost)
            {
                if (this.MinAmount < 0)
                {
                    return Task.FromResult(new Result(MixItUp.Base.Resources.ValidCurrencyAmountMustBeSpecified));
                }

                if (this.SelectedGameCurrencyRequirementType == GameCurrencyRequirementTypeEnum.MinimumAndMaximum)
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
            }

            return Task.FromResult(new Result());
        }

        public override RequirementModelBase GetRequirement()
        {
            return new GameCurrencyRequirementModel(this.SelectedCurrency, this.SelectedGameCurrencyRequirementType, this.MinAmount, this.MaxAmount);
        }
    }
}
