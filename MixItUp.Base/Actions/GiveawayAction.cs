using Mixer.Base.ViewModel;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public class GiveawayAction : ActionBase
    {
        public GiveawayAction() : base("Giveaway") { }

        public override Task Perform(UserViewModel user)
        {
            return Task.FromResult(0);
        }
    }
}
