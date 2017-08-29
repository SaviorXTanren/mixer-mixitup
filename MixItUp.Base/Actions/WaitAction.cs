using Mixer.Base.ViewModel;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [DataContract]
    public class WaitAction : ActionBase
    {
        [DataMember]
        public int WaitAmount { get; set; }

        public WaitAction(int waitAmount)
            : base(ActionTypeEnum.Wait)
        {
            this.WaitAmount = waitAmount;
        }

        public override async Task Perform(UserViewModel user, IEnumerable<string> arguments)
        {
            for (int i = 0; i < this.WaitAmount; i++)
            {
                await this.Wait500();
                await this.Wait500();
            }
        }
    }
}
