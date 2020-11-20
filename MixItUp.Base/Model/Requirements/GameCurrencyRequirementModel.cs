using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Currency;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Requirements
{
    public enum GameCurrencyRequirementTypeEnum
    {
        NoCost,
        RequiredAmount,
        MinimumOnly,
        MinimumAndMaximum
    }

    [DataContract]
    public class GameCurrencyRequirementModel : CurrencyRequirementModel
    {
        [DataMember]
        public GameCurrencyRequirementTypeEnum GameCurrencyRequirementType { get; set; } = GameCurrencyRequirementTypeEnum.NoCost;

        [DataMember]
        public int MaxAmount { get; set; }

        public GameCurrencyRequirementModel(CurrencyModel currency, GameCurrencyRequirementTypeEnum gameCurrencyRequirementType, int minAmount, int maxAmount)
            : base(currency, minAmount)
        {
            this.GameCurrencyRequirementType = gameCurrencyRequirementType;
            this.MaxAmount = maxAmount;
        }

        private GameCurrencyRequirementModel() { }

        public override async Task<bool> Validate(CommandParametersModel parameters)
        {
            if (this.GameCurrencyRequirementType == GameCurrencyRequirementTypeEnum.NoCost)
            {
                return true;
            }

            CurrencyModel currency = this.Currency;
            if (currency == null)
            {
                return true;
            }

            if (this.GameCurrencyRequirementType == GameCurrencyRequirementTypeEnum.RequiredAmount)
            {
                return await base.Validate(parameters);
            }
            else
            {
                int amount = this.GetGameAmount(parameters);
                if (this.GameCurrencyRequirementType == GameCurrencyRequirementTypeEnum.MinimumOnly)
                {
                    if (amount < this.Amount)
                    {
                        await this.SendChatMessage(string.Format(MixItUp.Base.Resources.GameCurrencyRequirementAmountGreaterThan, this.Amount, currency.Name));
                        return false;
                    }
                    return await this.ValidateAmount(parameters, currency, amount);
                }
                else if (this.GameCurrencyRequirementType == GameCurrencyRequirementTypeEnum.MinimumAndMaximum)
                {
                    if (amount < this.Amount || amount > this.MaxAmount)
                    {
                        await this.SendChatMessage(string.Format(MixItUp.Base.Resources.GameCurrencyRequirementAmountBetween, this.Amount, this.MaxAmount, currency.Name));
                        return false;
                    }
                    return await this.ValidateAmount(parameters, currency, amount);
                }
            }
            return false;
        }

        public override async Task Perform(CommandParametersModel parameters)
        {
            if (this.GameCurrencyRequirementType == GameCurrencyRequirementTypeEnum.NoCost)
            {
                await base.Perform(parameters);
            }
            else
            {
                CurrencyModel currency = this.Currency;
                if (currency != null && !parameters.User.Data.IsCurrencyRankExempt)
                {
                    if (this.GameCurrencyRequirementType == GameCurrencyRequirementTypeEnum.RequiredAmount)
                    {
                        await base.Perform(parameters);
                    }
                    else if (this.GameCurrencyRequirementType == GameCurrencyRequirementTypeEnum.MinimumOnly)
                    {
                        currency.SubtractAmount(parameters.User.Data, this.GetGameAmount(parameters));
                    }
                    else if (this.GameCurrencyRequirementType == GameCurrencyRequirementTypeEnum.MinimumAndMaximum)
                    {
                        currency.SubtractAmount(parameters.User.Data, this.GetGameAmount(parameters));
                    }
                }
            }
        }

        public override async Task Refund(CommandParametersModel parameters)
        {
            if (this.GameCurrencyRequirementType == GameCurrencyRequirementTypeEnum.NoCost)
            {
                await base.Perform(parameters);
            }
            else
            {
                CurrencyModel currency = this.Currency;
                if (currency != null && !parameters.User.Data.IsCurrencyRankExempt)
                {
                    if (this.GameCurrencyRequirementType == GameCurrencyRequirementTypeEnum.RequiredAmount)
                    {
                        await base.Refund(parameters);
                    }
                    else if (this.GameCurrencyRequirementType == GameCurrencyRequirementTypeEnum.MinimumOnly)
                    {
                        currency.AddAmount(parameters.User.Data, this.GetGameAmount(parameters));
                    }
                    else if (this.GameCurrencyRequirementType == GameCurrencyRequirementTypeEnum.MinimumAndMaximum)
                    {
                        currency.AddAmount(parameters.User.Data, this.GetGameAmount(parameters));
                    }
                }
            }
        }

        public int GetGameAmount(CommandParametersModel parameters)
        {
            if (this.GameCurrencyRequirementType == GameCurrencyRequirementTypeEnum.NoCost)
            {
                return 0;
            }
            else if (this.GameCurrencyRequirementType == GameCurrencyRequirementTypeEnum.RequiredAmount)
            {
                return this.Amount;
            }
            else
            {
                int amount = 0;
                if (parameters.Arguments.Count() > 0)
                {
                    if (!int.TryParse(parameters.Arguments.ElementAt(0), out amount))
                    {
                        if (parameters.Arguments.Count() > 1)
                        {
                            int.TryParse(parameters.Arguments.ElementAt(1), out amount);
                        }
                    }
                }
                return amount;
            }
        }
    }
}
