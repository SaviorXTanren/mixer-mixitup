using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [DataContract]
    public class WaitAction : ActionBase
    {
        // Allow multiple wait actions to be executed at the same time
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(int.MaxValue);

        protected override SemaphoreSlim AsyncSempahore { get { return WaitAction.asyncSemaphore; } }

        [DataMember]
        public double WaitAmount { get; set; }

        public WaitAction() : base(ActionTypeEnum.Wait) { }

        public WaitAction(double waitAmount)
            : this()
        {
            this.WaitAmount = waitAmount;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            await Task.Delay((int)(1000 * this.WaitAmount));
        }
    }
}
