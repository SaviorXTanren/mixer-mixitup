using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Currency;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Requirements
{
    public enum CurrencyRequirementTypeEnum
    {
        NoCost,
        RequiredAmount,
        MinimumOnly,
        MinimumAndMaximum
    }

    [DataContract]
    public class CurrencyRequirementModel : RequirementModelBase
    {
        [DataMember]
        public Guid CurrencyID { get; set; }

        [DataMember]
        public CurrencyRequirementTypeEnum RequirementType { get; set; } = CurrencyRequirementTypeEnum.NoCost;

        [DataMember]
        public int MinAmount { get; set; }

        [DataMember]
        public int MaxAmount { get; set; }

        public CurrencyRequirementModel(CurrencyModel currency, int amount) : this(currency, CurrencyRequirementTypeEnum.RequiredAmount, amount, 0) { }

        public CurrencyRequirementModel(CurrencyModel currency, CurrencyRequirementTypeEnum requirementType, int minAmount, int maxAmount)
        {
            this.CurrencyID = currency.ID;
            this.RequirementType = CurrencyRequirementTypeEnum.RequiredAmount;
            this.MinAmount = minAmount;
            this.MaxAmount = maxAmount;
        }

        internal CurrencyRequirementModel(MixItUp.Base.ViewModel.Requirement.CurrencyRequirementViewModel requirement)
        {
            this.CurrencyID = requirement.CurrencyID;
            this.MinAmount = requirement.RequiredAmount;
            this.MaxAmount = requirement.MaximumAmount;
            switch (requirement.RequirementType)
            {
                case ViewModel.Requirement.CurrencyRequirementTypeEnum.NoCurrencyCost: this.RequirementType = CurrencyRequirementTypeEnum.NoCost; break;
                case ViewModel.Requirement.CurrencyRequirementTypeEnum.RequiredAmount: this.RequirementType = CurrencyRequirementTypeEnum.RequiredAmount; break;
                case ViewModel.Requirement.CurrencyRequirementTypeEnum.MinimumOnly: this.RequirementType = CurrencyRequirementTypeEnum.MinimumOnly; break;
                case ViewModel.Requirement.CurrencyRequirementTypeEnum.MinimumAndMaximum: this.RequirementType = CurrencyRequirementTypeEnum.MinimumAndMaximum; break;
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
            if (this.RequirementType == CurrencyRequirementTypeEnum.NoCost)
            {
                return true;
            }

            CurrencyModel currency = this.Currency;
            if (currency == null)
            {
                return true;
            }

            if (this.RequirementType == CurrencyRequirementTypeEnum.RequiredAmount)
            {
                return await this.ValidateAmount(parameters, currency, this.MinAmount);
            }
            else
            {
                int amount = this.GetGameAmount(parameters);
                if (this.RequirementType == CurrencyRequirementTypeEnum.MinimumOnly)
                {
                    if (amount < this.MinAmount)
                    {
                        await this.SendChatMessage(string.Format(MixItUp.Base.Resources.GameCurrencyRequirementAmountGreaterThan, this.MinAmount, currency.Name));
                        return false;
                    }
                    return await this.ValidateAmount(parameters, currency, amount);
                }
                else if (this.RequirementType == CurrencyRequirementTypeEnum.MinimumAndMaximum)
                {
                    if (amount < this.MinAmount || amount > this.MaxAmount)
                    {
                        await this.SendChatMessage(string.Format(MixItUp.Base.Resources.GameCurrencyRequirementAmountBetween, this.MinAmount, this.MaxAmount, currency.Name));
                        return false;
                    }
                    return await this.ValidateAmount(parameters, currency, amount);
                }
            }
            return false;
        }

        public override Task Perform(CommandParametersModel parameters)
        {
            if (this.RequirementType != CurrencyRequirementTypeEnum.NoCost)
            {
                CurrencyModel currency = this.Currency;
                if (currency != null && !parameters.User.Data.IsCurrencyRankExempt)
                {
                    if (this.RequirementType == CurrencyRequirementTypeEnum.RequiredAmount)
                    {
                        currency.SubtractAmount(parameters.User.Data, this.MinAmount);
                    }
                    else if (this.RequirementType == CurrencyRequirementTypeEnum.MinimumOnly)
                    {
                        currency.SubtractAmount(parameters.User.Data, this.GetGameAmount(parameters));
                    }
                    else if (this.RequirementType == CurrencyRequirementTypeEnum.MinimumAndMaximum)
                    {
                        currency.SubtractAmount(parameters.User.Data, this.GetGameAmount(parameters));
                    }
                }
            }
            return Task.FromResult(0);
        }

        public override Task Refund(CommandParametersModel parameters)
        {
            if (this.RequirementType != CurrencyRequirementTypeEnum.NoCost)
            {
                CurrencyModel currency = this.Currency;
                if (currency != null && !parameters.User.Data.IsCurrencyRankExempt)
                {
                    if (this.RequirementType == CurrencyRequirementTypeEnum.RequiredAmount)
                    {
                        currency.AddAmount(parameters.User.Data, this.MinAmount);
                    }
                    else if (this.RequirementType == CurrencyRequirementTypeEnum.MinimumOnly)
                    {
                        currency.AddAmount(parameters.User.Data, this.GetGameAmount(parameters));
                    }
                    else if (this.RequirementType == CurrencyRequirementTypeEnum.MinimumAndMaximum)
                    {
                        currency.AddAmount(parameters.User.Data, this.GetGameAmount(parameters));
                    }
                }
            }
            return Task.FromResult(0);
        }

        public int GetGameAmount(CommandParametersModel parameters)
        {
            if (this.RequirementType == CurrencyRequirementTypeEnum.NoCost)
            {
                return 0;
            }
            else if (this.RequirementType == CurrencyRequirementTypeEnum.RequiredAmount)
            {
                return this.MinAmount;
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
