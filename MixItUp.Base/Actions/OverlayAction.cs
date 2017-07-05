using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public class OverlayAction : ActionBase
    {
        public OverlayAction() : base("Overlay") { }

        public override Task Perform()
        {
            return Task.FromResult(0);
        }
    }
}
