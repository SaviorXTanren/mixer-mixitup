using MixItUp.Base.Actions;
using System;
using System.Runtime.Serialization;
using System.Threading;

namespace MixItUp.Base.Commands
{
    [Obsolete]
    [DataContract]
    public class TimerCommand : CommandBase
    {
        private static SemaphoreSlim timerCommandPerformSemaphore = new SemaphoreSlim(1);

        public TimerCommand() { }

        public TimerCommand(string name)
            : base(name, CommandTypeEnum.Timer, name)
        { }

        protected override SemaphoreSlim AsyncSemaphore { get { return TimerCommand.timerCommandPerformSemaphore; } }
    }
}
