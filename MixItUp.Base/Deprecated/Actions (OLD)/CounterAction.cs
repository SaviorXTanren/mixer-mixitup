using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [Obsolete]
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

        protected override Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            return Task.FromResult(0);
        }
    }
}
