using System.Runtime.Serialization;
using System.Threading;

namespace MixItUp.Base.Model.Commands
{
    [DataContract]
    public class TimerCommandModel : CommandModelBase
    {
        private static SemaphoreSlim commandLockSemaphore = new SemaphoreSlim(1);

        public TimerCommandModel(string name) : base(name, CommandTypeEnum.Timer) { }

        protected override SemaphoreSlim CommandLockSemaphore { get { return TimerCommandModel.commandLockSemaphore; } }
    }
}
