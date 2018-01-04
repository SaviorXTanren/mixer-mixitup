using System.Runtime.Serialization;
using System.Threading;

namespace MixItUp.Base.Commands
{
    [DataContract]
    public class ActionGroupCommand : CommandBase
    {
        private static SemaphoreSlim actionGroupCommandPerformSemaphore = new SemaphoreSlim(1);

        public ActionGroupCommand() { }

        public ActionGroupCommand(string name) : base(name, CommandTypeEnum.ActionGroup, name) { }

        protected override SemaphoreSlim AsyncSemaphore { get { return ActionGroupCommand.actionGroupCommandPerformSemaphore; } }
    }
}
