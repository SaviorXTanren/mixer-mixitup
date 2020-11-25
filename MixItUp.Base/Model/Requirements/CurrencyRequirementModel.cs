using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Currency;
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

        public CurrencyRequirementModel(CurrencyModel currency, int amount)
        {
            this.CurrencyID = currency.ID;
            this.Amount = amount;
        }

        internal CurrencyRequirementModel(MixItUp.Base.ViewModel.Requirement.CurrencyRequirementViewModel requirement)
        {
            if (requirement.RequirementType != ViewModel.Requirement.CurrencyRequirementTypeEnum.NoCurrencyCost)
            {
                this.CurrencyID = requirement.CurrencyID;
                this.Amount = requirement.RequiredAmount;
            }
        }

        protected CurrencyRequirementModel() { }

        [JsonIgnore]
        public CurrencyModel Currency
        {
            get
            {
                if (this.CurrencyID != Guid.Empty && ChannelSession.Settings.Currency.ContainsKey(this.CurrencyID))
                {
                    return ChannelSession.Settings.Currency[this.CurrencyID];
                }
                return null;
            }
        }

        public override async Task<bool> Validate(CommandParametersModel parameters)
        {
            CurrencyModel currency = this.Currency;
            if (currency == null)
            {
                return true;
            }
            return await this.ValidateAmount(parameters, currency, this.Amount);
        }

        public override Task Perform(CommandParametersModel parameters)
        {
            CurrencyModel currency = this.Currency;
            if (currency != null && !parameters.User.Data.IsCurrencyRankExempt)
            {
                currency.SubtractAmount(parameters.User.Data, this.Amount);
            }
            return Task.FromResult(0);
        }

        public override Task Refund(CommandParametersModel parameters)
        {
            CurrencyModel currency = this.Currency;
            if (currency != null && !parameters.User.Data.IsCurrencyRankExempt)
            {
                currency.AddAmount(parameters.User.Data, this.Amount);
            }
            return Task.FromResult(0);
        }

        protected async Task<bool> ValidateAmount(CommandParametersModel parameters, CurrencyModel currency, int amount)
        {
            if (!parameters.User.Data.IsCurrencyRankExempt && !currency.HasAmount(parameters.User.Data, amount))
            {
                await this.SendChatMessage(string.Format(MixItUp.Base.Resources.CurrencyRequirementDoNotHaveAmount, amount, currency.Name));
                return false;
            }
            return true;
        }
    }
}
