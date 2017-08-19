using Mixer.Base.Util;
using MixItUp.Base.Actions;
using System.Collections.Generic;

namespace MixItUp.Base.Commands
{
    public enum InteractiveCommandEventType
    {
        [Name("Mouse Down")]
        MouseDown,
        [Name("Mouse Up")]
        MouseUp,
        [Name("Key Up")]
        KeyUp,
        [Name("Key Down")]
        KeyDown,
        [Name("Move")]
        Move,
    }

    public class InteractiveCommand : CommandBase
    {
        public InteractiveCommandEventType EventType { get; set; }

        public InteractiveCommand() { }

        public InteractiveCommand(string name, string command, InteractiveCommandEventType eventType, IEnumerable<ActionBase> actions)
            : base(name, CommandTypeEnum.Interactive, command, actions)
        {
            this.EventType = eventType;
        }

        public string EventTypeTransactionString { get { return this.EventType.ToString().ToLower(); } }
    }
}
