using MixItUp.Base.Actions;
using MixItUp.Base.Model.Import.ScorpBot;
using MixItUp.Base.Model.Import.Streamlabs;
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

        public TimerCommand(ScorpBotTimerModel timer)
            : this(timer.Name)
        {
            this.Actions.Add(new ChatAction(timer.Text));
            this.IsEnabled = timer.Enabled;
        }

        public TimerCommand(StreamlabsChatBotTimerModel timer)
            : this(timer.Name)
        {
            this.Actions.AddRange(timer.Actions);
            this.IsEnabled = timer.Enabled;
        }

        protected override SemaphoreSlim AsyncSemaphore { get { return TimerCommand.timerCommandPerformSemaphore; } }
    }
}
