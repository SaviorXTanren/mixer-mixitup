using MixItUp.Base.ViewModel;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [DataContract]
    public class CounterAction : ActionBase
    {
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

        public override Task Perform(UserViewModel user, IEnumerable<string> arguments)
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
