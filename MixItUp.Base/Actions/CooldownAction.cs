using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public class CooldownAction : ActionBase
    {
        public int CooldownAmount { get; set; }

        public CooldownAction() : base("Cooldown") { }

        public override Task Perform()
        {
            return Task.FromResult(0);
        }
    }
}
