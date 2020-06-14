using MixItUp.Base.Model.Currency;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Requirements
{
    [DataContract]
    public class CurrencyRequirementModel : RequirementModelBase
    {
        [DataMember]
        public Guid CurrencyID { get; set; }

        [DataMember]
        public int Amount { get; set; }

        public CurrencyRequirementModel() { }

        public CurrencyRequirementModel(CurrencyModel currency, int amount)
        {
            this.CurrencyID = currency.ID;
            this.Amount = amount;
        }

        [JsonIgnore]
        public CurrencyModel Currency
        {
            get
            {
                if (ChannelSession.Settings.Currency.ContainsKey(this.CurrencyID))
                {
                    return ChannelSession.Settings.Currency[this.CurrencyID];
                }
                return null;
            }
        }

        public override async Task<bool> Validate(UserViewModel user)
        {
            CurrencyModel currency = this.Currency;
            if (currency == null)
            {
                return false;
            }

            if (!user.Data.IsCurrencyRankExempt && !currency.HasAmount(user.Data, this.Amount))
            {
                await this.SendChatWhisper(user, string.Format("You do not have the required {0} {1} to do this", this.Amount, currency.Name));
                return false;
            }

            return true;
        }

        public override Task Perform(UserViewModel user)
        {
            CurrencyModel currency = this.Currency;
            if (currency != null && !user.Data.IsCurrencyRankExempt)
            {
                currency.SubtractAmount(user.Data, this.Amount);
            }
            return Task.FromResult(0);
        }

        public override Task Refund(UserViewModel user)
        {
            CurrencyModel currency = this.Currency;
            if (currency != null && !user.Data.IsCurrencyRankExempt)
            {
                currency.AddAmount(user.Data, this.Amount);
            }
            return Task.FromResult(0);
        }
    }
}
