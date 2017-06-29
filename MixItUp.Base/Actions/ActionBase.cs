using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public abstract class ActionBase
    {
        public string ActionType { get; private set; }

        public ActionBase(string actionType)
        {
            this.ActionType = actionType;
        }

        public abstract Task Perform();

        protected async Task Wait500()
        {
            await Task.Delay(500);
        }
    }
}
