using Mixer.Base.Util;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public enum CommandActionTypeEnum
    {
        [Name("Run Command")]
        RunCommand,
        [Name("Enable/Disable Command")]
        EnableDisableCommand,
    }

    [DataContract]
    public class CommandAction : ActionBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return CommandAction.asyncSemaphore; } }

        [DataMember]
        public CommandActionTypeEnum CommandActionType { get; set; }

        public CommandAction() : base(ActionTypeEnum.Command) { }

        public CommandAction(CommandActionTypeEnum commandActionType)
            : this()
        {
            this.CommandActionType = commandActionType;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            
        }
    }
}
