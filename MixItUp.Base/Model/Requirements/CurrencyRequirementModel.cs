using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Requirements
{
    public enum CurrencyRequirementTypeEnum
    {
        RequiredAmount,
        MinimumOnly,
        MinimumAndMaximum
    }

    [DataContract]
    public class CurrencyRequirementModel : RequirementModelBase
    {
        private static DateTimeOffset requirementErrorCooldown = DateTimeOffset.MinValue;

        [DataMember]
        public Guid CurrencyID { get; set; }

        [DataMember]
        public CurrencyRequirementTypeEnum RequirementType { get; set; }

        [DataMember]
        public int MinAmount { get; set; }

        [DataMember]
        public int MaxAmount { get; set; }

        [JsonIgnore]
        public int ArgumentIndex { get; set; } = 0;

        [JsonIgnore]
        private int temporaryAmount { get; set; } = -1;

        public CurrencyRequirementModel(CurrencyModel currency, int amount) : this(currency, CurrencyRequirementTypeEnum.RequiredAmount, amount, 0) { }

        public CurrencyRequirementModel(CurrencyModel currency, CurrencyRequirementTypeEnum requirementType, int minAmount, int maxAmount)
        {
            this.CurrencyID = currency.ID;
            this.RequirementType = requirementType;
            this.MinAmount = minAmount;
            this.MaxAmount = maxAmount;
        }

        [Obsolete]
        public CurrencyRequirementModel() { }

        protected override DateTimeOffset RequirementErrorCooldown { get { return CurrencyRequirementModel.requirementErrorCooldown; } set { CurrencyRequirementModel.requirementErrorCooldown = value; } }

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

        public override Task<Result> Validate(CommandParametersModel parameters)
        {
            CurrencyModel currency = this.Currency;
            if (currency == null)
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.CurrencyDoesNotExist));
            }

            if (this.HasTemporaryAmount())
            {
                return Task.FromResult(this.ValidateAmount(parameters.User, this.temporaryAmount));
            }
            else if (this.RequirementType == CurrencyRequirementTypeEnum.RequiredAmount)
            {
                return Task.FromResult(this.ValidateAmount(parameters.User, this.MinAmount));
            }
            else
            {
                int amount = this.GetAmount(parameters);
                if (this.RequirementType == CurrencyRequirementTypeEnum.MinimumOnly)
                {
                    if (amount < this.MinAmount)
                    {
                        return Task.FromResult(new Result(string.Format(MixItUp.Base.Resources.GameCurrencyRequirementAmountGreaterThan, this.MinAmount, currency.Name)));
                    }
                    return Task.FromResult(this.ValidateAmount(parameters.User, amount));
                }
                else if (this.RequirementType == CurrencyRequirementTypeEnum.MinimumAndMaximum)
                {
                    if (amount < this.MinAmount || amount > this.MaxAmount)
                    {
                        return Task.FromResult(new Result(string.Format(MixItUp.Base.Resources.GameCurrencyRequirementAmountBetween, this.MinAmount, this.MaxAmount, currency.Name)));
                    }
                    return Task.FromResult(this.ValidateAmount(parameters.User, amount));
                }
            }
            return Task.FromResult(new Result());
        }

        public override async Task Perform(CommandParametersModel parameters)
        {
            await base.Perform(parameters);
            if (this.HasTemporaryAmount())
            {
                this.AddSubtractAmount(parameters.User, -this.temporaryAmount);
            }
            else if (this.RequirementType == CurrencyRequirementTypeEnum.RequiredAmount)
            {
                this.AddSubtractAmount(parameters.User, -this.MinAmount);
            }
            else if (this.RequirementType == CurrencyRequirementTypeEnum.MinimumOnly || this.RequirementType == CurrencyRequirementTypeEnum.MinimumAndMaximum)
            {
                this.AddSubtractAmount(parameters.User, -this.GetAmount(parameters));
            }
        }

        public override Task Refund(CommandParametersModel parameters)
        {
            if (this.HasTemporaryAmount())
            {
                this.AddSubtractAmount(parameters.User, this.temporaryAmount);
            }
            else if (this.RequirementType == CurrencyRequirementTypeEnum.RequiredAmount)
            {
                this.AddSubtractAmount(parameters.User, this.MinAmount);
            }
            else if (this.RequirementType == CurrencyRequirementTypeEnum.MinimumOnly || this.RequirementType == CurrencyRequirementTypeEnum.MinimumAndMaximum)
            {
                this.AddSubtractAmount(parameters.User, this.GetAmount(parameters));
            }
            return Task.CompletedTask;
        }

        public int GetAmount(CommandParametersModel parameters)
        {
            if (this.HasTemporaryAmount())
            {
                return this.temporaryAmount;
            }
            else if (this.RequirementType == CurrencyRequirementTypeEnum.RequiredAmount)
            {
                return this.MinAmount;
            }
            else
            {
                int amount = this.MinAmount;
                if (parameters.Arguments.Count() > 0)
                {
                    if (this.ArgumentIndex > 0 && parameters.Arguments.Count() > this.ArgumentIndex)
                    {
                        int.TryParse(parameters.Arguments.ElementAt(this.ArgumentIndex), out amount);
                    }
                    else
                    {
                        if (!int.TryParse(parameters.Arguments.ElementAt(0), out amount))
                        {
                            if (parameters.Arguments.Count() > 1)
                            {
                                int.TryParse(parameters.Arguments.ElementAt(1), out amount);
                            }
                        }
                    }
                }
                return amount;
            }
        }

        public Result ValidateAmount(UserV2ViewModel user, int amount)
        {
            if (!user.IsSpecialtyExcluded && !this.Currency.HasAmount(user, amount))
            {
                int currentAmount = this.Currency.GetAmount(user);
                return new Result(string.Format(MixItUp.Base.Resources.CurrencyRequirementDoNotHaveAmount, amount, this.Currency.Name) + " " + string.Format(MixItUp.Base.Resources.RequirementCurrentAmount, currentAmount));
            }
            return new Result();
        }

        public void AddSubtractAmount(UserV2ViewModel user, int amount)
        {
            CurrencyModel currency = this.Currency;
            if (currency != null && !user.IsSpecialtyExcluded)
            {
                if (amount > 0)
                {
                    currency.AddAmount(user, amount);
                }
                else if (amount < 0)
                {
                    currency.SubtractAmount(user, -amount);
                }
            }
        }

        public void SetTemporaryAmount(int amount) { this.temporaryAmount = amount; }

        public void ResetTemporaryAmount() { this.SetTemporaryAmount(-1); }

        public bool HasTemporaryAmount() { return this.temporaryAmount >= 0; }
    }
}
