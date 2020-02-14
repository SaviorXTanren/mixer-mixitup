using MixItUp.Base.Model.Settings;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [DataContract]
    public class CounterAction : ActionBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return CounterAction.asyncSemaphore; } }

        [DataMember]
        public string CounterName { get; set; }

        [DataMember]
        public string Amount { get; set; }

        [DataMember]
        public bool UpdateAmount { get; set; }

        [DataMember]
        public bool ResetAmount { get; set; }

        [DataMember]
        public bool SetAmount { get; set; }

        [DataMember]
        [Obsolete]
        public bool SaveToFile { get; set; }
        [DataMember]
        [Obsolete]
        public bool ResetOnLoad { get; set; }

        public CounterAction() : base(ActionTypeEnum.Counter) { }

        public CounterAction(string counterName)
            : this()
        {
            this.CounterName = counterName;
            this.ResetAmount = true;
        }

        public CounterAction(string counterName, string amount, bool set)
            : this()
        {
            this.CounterName = counterName;
            this.UpdateAmount = !set;
            this.SetAmount = set;
            this.Amount = amount;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            if (ChannelSession.Settings.Counters.ContainsKey(this.CounterName))
            {
                if (this.UpdateAmount || this.SetAmount)
                {
                    string amountText = await this.ReplaceStringWithSpecialModifiers(this.Amount, user, arguments);
                    if (double.TryParse(amountText, out double amount))
                    {
                        if (this.UpdateAmount)
                        {
                            await ChannelSession.Settings.Counters[this.CounterName].UpdateAmount(amount);
                        }
                        else if (this.SetAmount)
                        {
                            await ChannelSession.Settings.Counters[this.CounterName].SetAmount(amount);
                        }
                    }
                }
                else if (this.ResetAmount)
                {
                    await ChannelSession.Settings.Counters[this.CounterName].ResetAmount();
                }
            }
        }
    }
}
