using MixItUp.Base.Actions;
using MixItUp.Base.ViewModel.Import;
using System.Threading;

namespace MixItUp.Base.Commands
{
    public class TimerCommand : CommandBase
    {
        private static SemaphoreSlim timerCommandPerformSemaphore = new SemaphoreSlim(1);

        public TimerCommand() { }

        public TimerCommand(string name)
            : base(name, CommandTypeEnum.Timer, name)
        { }

        public TimerCommand(ScorpBotTimer timer)
            : this(timer.Name)
        {
            this.Actions.Add(new ChatAction(timer.Text));
            this.IsEnabled = timer.Enabled;
        }

        protected override SemaphoreSlim AsyncSemaphore { get { return TimerCommand.timerCommandPerformSemaphore; } }
    }
}
