using MixItUp.Base.Actions;
using System.Collections.Generic;

namespace MixItUp.Base.Commands
{
    public class InteractiveCommand : CommandBase
    {
        public InteractiveCommand() { }

        public InteractiveCommand(string name, string command, IEnumerable<ActionBase> actions)
            : base(name, CommandTypeEnum.Interactive, command, actions)
        {
        }
    }
}
