using Mixer.Base.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public class GiveawayAction : ActionBase
    {
        public GiveawayAction(string giveawayItem) : base(ActionTypeEnum.Giveaway) { }

        public override Task Perform(UserViewModel user, IEnumerable<string> arguments)
        {
            return Task.FromResult(0);
        }

        public override SerializableAction Serialize()
        {
            return new SerializableAction()
            {
                Type = this.Type
            };
        }
    }
}
