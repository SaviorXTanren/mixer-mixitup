namespace MixItUp.Base.Commands
{
    public class TimerCommand : CommandBase
    {
        private const string TimerCommandTextFormat = "{0} Seconds - {1} Messages";

        public int Interval { get; set; }
        public int MinimumMessages { get; set; }

        public TimerCommand() { }

        public TimerCommand(string name, int interval, int minimumMessages)
            : base(name, CommandTypeEnum.Timer, string.Format(TimerCommandTextFormat, interval, minimumMessages))
        {
            this.Interval = interval;
            this.MinimumMessages = minimumMessages;
        }
    }
}
