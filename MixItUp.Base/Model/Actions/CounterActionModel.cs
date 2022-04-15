using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    public enum CounterActionTypeEnum
    {
        Update,
        Reset,
        Set
    }

    [DataContract]
    public class CounterActionModel : ActionModelBase
    {
        [DataMember]
        public string CounterName { get; set; }

        [DataMember]
        public CounterActionTypeEnum ActionType { get; set; }

        [DataMember]
        public string Amount { get; set; }

        public CounterActionModel(string counterName, CounterActionTypeEnum actionType, string amount)
            : base(ActionTypeEnum.Counter)
        {
            this.CounterName = counterName;
            this.ActionType = actionType;
            this.Amount = amount;
        }

        [Obsolete]
        public CounterActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            string counterName = this.CounterName.ToLower();
            if (ChannelSession.Settings.Counters.ContainsKey(counterName))
            {
                if (this.ActionType == CounterActionTypeEnum.Reset)
                {
                    await ChannelSession.Settings.Counters[counterName].ResetAmount();
                }
                else
                {
                    string amountText = await ReplaceStringWithSpecialModifiers(this.Amount, parameters);
                    amountText = MathHelper.ProcessMathEquation(amountText).ToString();
                    if (double.TryParse(amountText, out double amount))
                    {
                        if (this.ActionType == CounterActionTypeEnum.Update)
                        {
                            await ChannelSession.Settings.Counters[counterName].UpdateAmount(amount);
                        }
                        else if (this.ActionType == CounterActionTypeEnum.Set)
                        {
                            await ChannelSession.Settings.Counters[counterName].SetAmount(amount);
                        }
                    }
                }
            }
        }
    }
}
