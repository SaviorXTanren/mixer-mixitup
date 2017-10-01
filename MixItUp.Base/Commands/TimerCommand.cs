namespace MixItUp.Base.Commands
{
    public class TimerCommand : CommandBase
    {
        public TimerCommand() { }

        public TimerCommand(string name)
            : base(name, CommandTypeEnum.Timer, name)
        { }
    }
}
