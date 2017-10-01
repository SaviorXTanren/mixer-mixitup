using MixItUp.Base.ViewModel;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [DataContract]
    public class WaitAction : ActionBase
    {
        [DataMember]
        public double WaitAmount { get; set; }

        public WaitAction(double waitAmount)
            : base(ActionTypeEnum.Wait)
        {
            this.WaitAmount = waitAmount;
        }

        public override async Task Perform(UserViewModel user, IEnumerable<string> arguments)
        {
            await Task.Delay((int)(1000 * this.WaitAmount));
        }
    }
}
