using System.Threading;

namespace MixItUp.Base.Model.Commands
{
    public class CustomCommandModel : CommandModelBase
    {
        private static SemaphoreSlim commandLockSemaphore = new SemaphoreSlim(1);

        public CustomCommandModel(string name) : base(name, CommandTypeEnum.Timer) { }

        protected override SemaphoreSlim CommandLockSemaphore { get { return CustomCommandModel.commandLockSemaphore; } }
    }
}
