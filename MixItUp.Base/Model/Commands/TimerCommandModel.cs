using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Commands
{
    [DataContract]
    public class TimerCommandModel : CommandModelBase
    {
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
    }
}
