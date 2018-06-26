using MixItUp.Base.ViewModel.User;
using System;
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

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            string amountText = await this.ReplaceStringWithSpecialModifiers(this.Amount, user, arguments);
            if (double.TryParse(amountText, out double amount) && amount > 0.0)
            {
                await Task.Delay((int)(1000 * amount));
            }
        }
    }
}
