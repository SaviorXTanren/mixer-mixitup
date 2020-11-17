using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        public string Amount { get; set; }

        public CurrencyRequirementModel() { }

        public CurrencyRequirementModel(CurrencyModel currency, string amount)
        {
            this.CurrencyID = currency.ID;
            this.Amount = amount;
        }

        internal CurrencyRequirementModel(MixItUp.Base.ViewModel.Requirement.CurrencyRequirementViewModel requirement)
            : this()
        {
            if (requirement.RequirementType == ViewModel.Requirement.CurrencyRequirementTypeEnum.RequiredAmount)
            {
                this.CurrencyID = requirement.CurrencyID;
                this.Amount = requirement.RequiredAmount.ToString();
            }
            else if (requirement.RequirementType == ViewModel.Requirement.CurrencyRequirementTypeEnum.MinimumOnly)
            {

            }
            else if (requirement.RequirementType == ViewModel.Requirement.CurrencyRequirementTypeEnum.MinimumAndMaximum)
            {

            }
        }

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
            return await this.ValidateAmount(parameters, currency, await this.GetAmount(this.Amount, parameters));
        }

        public override async Task Perform(CommandParametersModel parameters)
        {
            CurrencyModel currency = this.Currency;
            if (currency != null && !parameters.User.Data.IsCurrencyRankExempt)
            {
                currency.SubtractAmount(parameters.User.Data, await this.GetAmount(this.Amount, parameters));
            }
        }

        public override async Task Refund(CommandParametersModel parameters)
        {
            CurrencyModel currency = this.Currency;
            if (currency != null && !parameters.User.Data.IsCurrencyRankExempt)
            {
                currency.AddAmount(parameters.User.Data, await this.GetAmount(this.Amount, parameters));
            }
        }

        protected async Task<bool> ValidateAmount(CommandParametersModel parameters, CurrencyModel currency, int amount)
        {
            if (!parameters.User.Data.IsCurrencyRankExempt && !currency.HasAmount(parameters.User.Data, amount))
            {
                await this.SendChatMessage(string.Format("You do not have the required {0} {1} to do this", amount, currency.Name));
                return false;
            }
            return true;
        }
    }
}
