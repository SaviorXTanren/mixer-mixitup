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
    public class WaitAction : ActionBase
    {
        // Allow multiple wait actions to be executed at the same time
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(int.MaxValue);

        protected override SemaphoreSlim AsyncSemaphore { get { return WaitAction.asyncSemaphore; } }

        [DataMember]
        [Obsolete]
        public double WaitAmount { get; set; }

        [DataMember]
        public string Amount { get; set; }

        public WaitAction() : base(ActionTypeEnum.Wait) { }

        public WaitAction(string amount)
            : this()
        {
            this.Amount = amount;
        }

        protected override Task PerformInternal(UserV2ViewModel user, IEnumerable<string> arguments)
        {
            return Task.CompletedTask;
        }
    }
}
