using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public class CooldownAction : ActionBase
    {
        public CooldownAction()
            : base("Cooldown")
        {
        }

        public override Task Perform()
        {
            return Task.FromResult(0);
        }
    }
}
