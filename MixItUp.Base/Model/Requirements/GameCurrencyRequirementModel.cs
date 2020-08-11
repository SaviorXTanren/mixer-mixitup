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
        public int MaxAmount { get; set; }

        public GameCurrencyRequirementModel() { }

        public GameCurrencyRequirementModel(GameCurrencyRequirementTypeEnum gameCurrencyRequirementType, CurrencyModel currency, int amount, int maxAmount = 0)
            : base(currency, amount)
        {
            this.GameCurrencyRequirementType = gameCurrencyRequirementType;
            this.MaxAmount = maxAmount;
        }

        public override async Task<bool> Validate(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
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
                return await base.Validate(user, platform, arguments, specialIdentifiers);
            }
            else
            {
                int amount = this.GetAmountFromArguments(arguments);
                if (this.GameCurrencyRequirementType == GameCurrencyRequirementTypeEnum.MinimumOnly)
                {
                    if (amount < this.Amount)
                    {
                        await this.SendChatMessage(string.Format("You must specify an amount greater than or equal to {0} {1}", this.Amount, currency.Name));
                        return false;
                    }
                    return await this.ValidateAmount(user, platform, arguments, specialIdentifiers, currency, this.Amount);
                }
                else if (this.GameCurrencyRequirementType == GameCurrencyRequirementTypeEnum.MinimumAndMaximum)
                {
                    if (amount < this.Amount || amount > this.MaxAmount)
                    {
                        await this.SendChatMessage(string.Format("You must specify an amount between {0} - {1} {2}", this.Amount, this.MaxAmount, currency.Name));
                        return false;
                    }
                    return await this.ValidateAmount(user, platform, arguments, specialIdentifiers, currency, this.Amount);
                }
            }
            return false;
        }

        public override async Task Perform(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            if (this.GameCurrencyRequirementType == GameCurrencyRequirementTypeEnum.NoCost)
            {
                await base.Perform(user, platform, arguments, specialIdentifiers);
            }
            else
            {
                CurrencyModel currency = this.Currency;
                if (currency != null && !user.Data.IsCurrencyRankExempt)
                {
                    if (this.GameCurrencyRequirementType == GameCurrencyRequirementTypeEnum.RequiredAmount)
                    {
                        await base.Perform(user, platform, arguments, specialIdentifiers);
                    }
                    else if (this.GameCurrencyRequirementType == GameCurrencyRequirementTypeEnum.MinimumOnly)
                    {
                        currency.SubtractAmount(user.Data, this.GetAmountFromArguments(arguments));
                    }
                    else if (this.GameCurrencyRequirementType == GameCurrencyRequirementTypeEnum.MinimumAndMaximum)
                    {
                        currency.SubtractAmount(user.Data, this.GetAmountFromArguments(arguments));
                    }
                }
            }
        }

        public override async Task Refund(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            if (this.GameCurrencyRequirementType == GameCurrencyRequirementTypeEnum.NoCost)
            {
                await base.Perform(user, platform, arguments, specialIdentifiers);
            }
            else
            {
                CurrencyModel currency = this.Currency;
                if (currency != null && !user.Data.IsCurrencyRankExempt)
                {
                    if (this.GameCurrencyRequirementType == GameCurrencyRequirementTypeEnum.RequiredAmount)
                    {
                        await base.Refund(user, platform, arguments, specialIdentifiers);
                    }
                    else if (this.GameCurrencyRequirementType == GameCurrencyRequirementTypeEnum.MinimumOnly)
                    {
                        currency.AddAmount(user.Data, this.GetAmountFromArguments(arguments));
                    }
                    else if (this.GameCurrencyRequirementType == GameCurrencyRequirementTypeEnum.MinimumAndMaximum)
                    {
                        currency.AddAmount(user.Data, this.GetAmountFromArguments(arguments));
                    }
                }
            }
        }

        public int GetAmountFromArguments(IEnumerable<string> arguments)
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
                if (arguments.Count() > 0)
                {
                    if (!int.TryParse(arguments.ElementAt(0), out amount))
                    {
                        if (arguments.Count() > 1)
                        {
                            int.TryParse(arguments.ElementAt(1), out amount);
                        }
                    }
                }
                return amount;
            }
        }
    }
}
