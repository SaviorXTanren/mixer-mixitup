using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public class ExternalProgramAction : ActionBase
    {
        public ExternalProgramAction()
            : base("External Program")
        {
        }

        public override Task Perform()
        {
            return Task.FromResult(0);
        }
    }
}
