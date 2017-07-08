using Mixer.Base.ViewModel;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public class CooldownAction : ActionBase
    {
        public int CooldownAmount { get; set; }

        public CooldownAction() : base("Cooldown") { }

        public override Task Perform(UserViewModel user)
        {
            return Task.FromResult(0);
        }
    }
}
