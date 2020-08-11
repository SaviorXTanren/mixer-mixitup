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
                int amount = await this.GetGameAmount(user, platform, arguments, specialIdentifiers);
                int minAmount = await this.GetAmount(this.Amount, user, platform, arguments, specialIdentifiers);
                int maxAmount = await this.GetAmount(this.MaxAmount, user, platform, arguments, specialIdentifiers);
                if (this.GameCurrencyRequirementType == GameCurrencyRequirementTypeEnum.MinimumOnly)
                {
                    if (amount < minAmount)
                    {
                        await this.SendChatMessage(string.Format("You must specify an amount greater than or equal to {0} {1}", this.Amount, currency.Name));
                        return false;
                    }
                    return await this.ValidateAmount(user, platform, arguments, specialIdentifiers, currency, amount);
                }
                else if (this.GameCurrencyRequirementType == GameCurrencyRequirementTypeEnum.MinimumAndMaximum)
                {
                    if (amount < minAmount || amount > maxAmount)
                    {
                        await this.SendChatMessage(string.Format("You must specify an amount between {0} - {1} {2}", this.Amount, this.MaxAmount, currency.Name));
                        return false;
                    }
                    return await this.ValidateAmount(user, platform, arguments, specialIdentifiers, currency, amount);
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
                        currency.SubtractAmount(user.Data, await this.GetGameAmount(user, platform, arguments, specialIdentifiers));
                    }
                    else if (this.GameCurrencyRequirementType == GameCurrencyRequirementTypeEnum.MinimumAndMaximum)
                    {
                        currency.SubtractAmount(user.Data, await this.GetGameAmount(user, platform, arguments, specialIdentifiers));
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
                        currency.AddAmount(user.Data, await this.GetGameAmount(user, platform, arguments, specialIdentifiers));
                    }
                    else if (this.GameCurrencyRequirementType == GameCurrencyRequirementTypeEnum.MinimumAndMaximum)
                    {
                        currency.AddAmount(user.Data, await this.GetGameAmount(user, platform, arguments, specialIdentifiers));
                    }
                }
            }
        }

        public async Task<int> GetGameAmount(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            if (this.GameCurrencyRequirementType == GameCurrencyRequirementTypeEnum.NoCost)
            {
                return 0;
            }
            else if (this.GameCurrencyRequirementType == GameCurrencyRequirementTypeEnum.RequiredAmount)
            {
                return await this.GetAmount(this.Amount, user, platform, arguments, specialIdentifiers);
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
