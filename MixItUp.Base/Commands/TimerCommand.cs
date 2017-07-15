using MixItUp.Base.Actions;
using System;
using System.Collections.Generic;

namespace MixItUp.Base.Commands
{
    public class TimerCommand : CommandBase
    {
        public int Interval { get; set; }
        public int MinimumMessages { get; set; }

        public TimerCommand() { }

        public TimerCommand(string name, string command, IEnumerable<ActionBase> actions, int interval, int minimumMessages)
            : base(name, CommandTypeEnum.Timer, command, actions)
        {
            this.Interval = interval;
            this.MinimumMessages = minimumMessages;
        }
    }
}
