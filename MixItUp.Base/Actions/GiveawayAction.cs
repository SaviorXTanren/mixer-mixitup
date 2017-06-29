using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public class GiveawayAction : ActionBase
    {
        public GiveawayAction()
            : base("Giveaway")
        {
        }

        public override Task Perform()
        {
            return Task.FromResult(0);
        }
    }
}
