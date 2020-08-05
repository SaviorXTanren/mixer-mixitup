using System.Runtime.Serialization;
using System.Threading;

namespace MixItUp.Base.Model.Commands
{
    [DataContract]
    public class ActionGroupCommandModel : CommandModelBase
    {
        private static SemaphoreSlim commandLockSemaphore = new SemaphoreSlim(1);

        public ActionGroupCommandModel(string name) : base(name, CommandTypeEnum.ActionGroup) { }

        protected override SemaphoreSlim CommandLockSemaphore { get { return ActionGroupCommandModel.commandLockSemaphore; } }
    }
}
