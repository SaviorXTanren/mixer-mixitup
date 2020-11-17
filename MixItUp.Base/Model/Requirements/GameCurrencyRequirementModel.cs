using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
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
        public string MaxAmount { get; set; }

        public GameCurrencyRequirementModel() { }

        public GameCurrencyRequirementModel(GameCurrencyRequirementTypeEnum gameCurrencyRequirementType, CurrencyModel currency, string amount, string maxAmount = null)
            : base(currency, amount)
        {
            this.GameCurrencyRequirementType = gameCurrencyRequirementType;
            this.MaxAmount = maxAmount;
        }

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
                int amount = await this.GetGameAmount(parameters);
                int minAmount = await this.GetAmount(this.Amount, parameters);
                int maxAmount = await this.GetAmount(this.MaxAmount, parameters);
                if (this.GameCurrencyRequirementType == GameCurrencyRequirementTypeEnum.MinimumOnly)
                {
                    if (amount < minAmount)
                    {
                        await this.SendChatMessage(string.Format("You must specify an amount greater than or equal to {0} {1}", this.Amount, currency.Name));
                        return false;
                    }
                    return await this.ValidateAmount(parameters, currency, amount);
                }
                else if (this.GameCurrencyRequirementType == GameCurrencyRequirementTypeEnum.MinimumAndMaximum)
                {
                    if (amount < minAmount || amount > maxAmount)
                    {
                        await this.SendChatMessage(string.Format("You must specify an amount between {0} - {1} {2}", this.Amount, this.MaxAmount, currency.Name));
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
                        currency.SubtractAmount(parameters.User.Data, await this.GetGameAmount(parameters));
                    }
                    else if (this.GameCurrencyRequirementType == GameCurrencyRequirementTypeEnum.MinimumAndMaximum)
                    {
                        currency.SubtractAmount(parameters.User.Data, await this.GetGameAmount(parameters));
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
                        currency.AddAmount(parameters.User.Data, await this.GetGameAmount(parameters));
                    }
                    else if (this.GameCurrencyRequirementType == GameCurrencyRequirementTypeEnum.MinimumAndMaximum)
                    {
                        currency.AddAmount(parameters.User.Data, await this.GetGameAmount(parameters));
                    }
                }
            }
        }

        public async Task<int> GetGameAmount(CommandParametersModel parameters)
        {
            if (this.GameCurrencyRequirementType == GameCurrencyRequirementTypeEnum.NoCost)
            {
                return 0;
            }
            else if (this.GameCurrencyRequirementType == GameCurrencyRequirementTypeEnum.RequiredAmount)
            {
                return await this.GetAmount(this.Amount, parameters);
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
