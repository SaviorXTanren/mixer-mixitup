using System.Runtime.Serialization;
using System.Threading;

namespace MixItUp.Base.Model.Commands
{
    [DataContract]
    public class TimerCommandModel : CommandModelBase
    {
        private static SemaphoreSlim commandLockSemaphore = new SemaphoreSlim(1);

        public TimerCommandModel(string name) : base(name, CommandTypeEnum.Timer) { }

#pragma warning disable CS0612 // Type or member is obsolete
        internal TimerCommandModel(MixItUp.Base.Commands.TimerCommand command)
            : base(command)
        {
            this.Name = command.Name;
            this.Type = CommandTypeEnum.Timer;
        }
#pragma warning restore CS0612 // Type or member is obsolete

        protected TimerCommandModel() : base() { }

        protected override SemaphoreSlim CommandLockSemaphore { get { return TimerCommandModel.commandLockSemaphore; } }
    }
}
