using Mixer.Base.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public class WaitAction : ActionBase
    {
        public int WaitAmount { get; set; }

        public WaitAction(int waitAmount)
            : base(ActionTypeEnum.Cooldown)
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

        public override SerializableAction Serialize()
        {
            return new SerializableAction()
            {
                Type = this.Type,
                Values = new List<string>() { this.WaitAmount.ToString() }
            };
        }
    }
}
