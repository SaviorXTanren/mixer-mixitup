using System.Threading;

namespace MixItUp.Base.Model.Commands
{
    public class CustomCommandModel : CommandModelBase
    {
        private static SemaphoreSlim commandLockSemaphore = new SemaphoreSlim(1);

        public CustomCommandModel(string name) : base(name, CommandTypeEnum.Custom) { }

        internal CustomCommandModel(MixItUp.Base.Commands.CustomCommand command)
            : base(command)
        {
            this.Name = command.Name;
            this.Type = CommandTypeEnum.Custom;
        }

        protected override SemaphoreSlim CommandLockSemaphore { get { return CustomCommandModel.commandLockSemaphore; } }
    }
}
