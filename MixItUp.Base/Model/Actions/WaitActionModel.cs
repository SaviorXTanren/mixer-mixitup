using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    [DataContract]
    public class WaitActionModel : ActionModelBase
    {
        // Allow multiple wait actions to be executed at the same time
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(int.MaxValue);

        protected override SemaphoreSlim AsyncSemaphore { get { return WaitActionModel.asyncSemaphore; } }

        [DataMember]
        public string Amount { get; set; }

        public WaitActionModel(string amount)
            : base(ActionTypeEnum.Wait)
        {
            this.Amount = amount;
        }

        internal WaitActionModel(MixItUp.Base.Actions.WaitAction action)
            : base(ActionTypeEnum.Wait)
        {
            this.Amount = action.Amount;
        }

        protected override async Task PerformInternal(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            string amountText = await this.ReplaceStringWithSpecialModifiers(this.Amount, user, platform, arguments, specialIdentifiers);
            if (double.TryParse(amountText, out double amount) && amount > 0.0)
            {
                await Task.Delay((int)(1000 * amount));
            }
        }
    }
}
