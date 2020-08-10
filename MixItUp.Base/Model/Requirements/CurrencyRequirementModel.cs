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
        public int Amount { get; set; }

        public CurrencyRequirementModel() { }

        public CurrencyRequirementModel(CurrencyModel currency, int amount)
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
                this.Amount = requirement.RequiredAmount;
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

        public override async Task<bool> Validate(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            CurrencyModel currency = this.Currency;
            if (currency == null)
            {
                return true;
            }
            return await this.ValidateAmount(user, platform, arguments, specialIdentifiers, currency, this.Amount);
        }

        public override Task Perform(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            CurrencyModel currency = this.Currency;
            if (currency != null && !user.Data.IsCurrencyRankExempt)
            {
                currency.SubtractAmount(user.Data, this.Amount);
            }
            return Task.FromResult(0);
        }

        public override Task Refund(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            CurrencyModel currency = this.Currency;
            if (currency != null && !user.Data.IsCurrencyRankExempt)
            {
                currency.AddAmount(user.Data, this.Amount);
            }
            return Task.FromResult(0);
        }

        protected async Task<bool> ValidateAmount(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers, CurrencyModel currency, int amount)
        {
            if (!user.Data.IsCurrencyRankExempt && !currency.HasAmount(user.Data, amount))
            {
                await this.SendChatMessage(string.Format("You do not have the required {0} {1} to do this", amount, currency.Name));
                return false;
            }
            return true;
        }
    }
}
