using MixItUp.Base.Actions;
using System.Collections.Generic;

namespace MixItUp.Base.Commands
{
    public class TimerCommand : CommandBase
    {
        public static IEnumerable<ActionTypeEnum> AllowedActions
        {
            get
            {
                return new List<ActionTypeEnum>()
                {
                    ActionTypeEnum.Chat, ActionTypeEnum.Currency, ActionTypeEnum.ExternalProgram, ActionTypeEnum.Input, ActionTypeEnum.Overlay, ActionTypeEnum.Sound, ActionTypeEnum.Wait
                };
            }
        }

        public TimerCommand() { }

        public TimerCommand(string name)
            : base(name, CommandTypeEnum.Timer, name)
        { }
    }
}
