using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [DataContract]
    public class CounterAction : ActionBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSempahore { get { return CounterAction.asyncSemaphore; } }

        [DataMember]
        public string CounterName { get; set; }

        [DataMember]
        public int CounterAmount { get; set; }

        public CounterAction() { }

        public CounterAction(string counterName, int counterAmount)
            : base(ActionTypeEnum.Counter)
        {
            this.CounterName = counterName;
            this.CounterAmount = counterAmount;
        }

        protected override Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            if (!ChannelSession.Counters.ContainsKey(this.CounterName))
            {
                ChannelSession.Counters[this.CounterName] = 0;
            }
            ChannelSession.Counters[this.CounterName] += this.CounterAmount;

            return Task.FromResult(0);
        }
    }
}
