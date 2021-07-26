using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public enum CommandActionTypeEnum
    {
        RunCommand,
        DisableCommand,
        EnableCommand,
        DisableCommandGroup,
        EnableCommandGroup,
    }

    [Obsolete]
    [DataContract]
    public class CommandAction : ActionBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return CommandAction.asyncSemaphore; } }

        [DataMember]
        public CommandActionTypeEnum CommandActionType { get; set; }

        [DataMember]
        public Guid CommandID { get; set; }

        [DataMember]
        public string PreMadeType { get; set; }

        [DataMember]
        public string CommandArguments { get; set; }

        [DataMember]
        public string GroupName { get; set; }

        public CommandAction() : base(ActionTypeEnum.Command) { }

        public CommandAction(CommandActionTypeEnum commandActionType, CommandBase command, string commandArguments)
            : this()
        {

        }

        public CommandAction(CommandActionTypeEnum commandActionType, string groupName)
            : this()
        {
            this.CommandActionType = commandActionType;
            this.GroupName = groupName;
        }

        protected override Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            return Task.CompletedTask;
        }
    }

    #region Obsolete Action Group Action

    [Obsolete]
    [DataContract]
    public class ActionGroupAction : ActionBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return ActionGroupAction.asyncSemaphore; } }

        [DataMember]
        public Guid ActionGroupID { get; set; }

#pragma warning disable CS0612 // Type or member is obsolete
        public ActionGroupAction() : base(ActionTypeEnum.ActionGroup) { }
#pragma warning restore CS0612 // Type or member is obsolete

        protected override Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            return Task.CompletedTask;
        }
    }

    #endregion Obsolete Action Group Action
}
