using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [DataContract]
    public class ActionGroupAction : ActionBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return ActionGroupAction.asyncSemaphore; } }

        [DataMember]
        public Guid ActionGroupID { get; set; }

        [DataMember]
        [Obsolete]
        public string ActionGroupName { get; set; }

        public ActionGroupAction() : base(ActionTypeEnum.ActionGroup) { }

        public ActionGroupAction(ActionGroupCommand command)
            : this()
        {
            this.ActionGroupID = command.ID;
        }

        public ActionGroupCommand GetCommand()
        {
            if (this.ActionGroupID == Guid.Empty)
            {
#pragma warning disable CS0612 // Type or member is obsolete
                if (!string.IsNullOrEmpty(this.ActionGroupName))
                {
                    return ChannelSession.Settings.ActionGroupCommands.FirstOrDefault(c => c.Name.Equals(this.ActionGroupName));
                }
#pragma warning restore CS0612 // Type or member is obsolete
            }
            return ChannelSession.Settings.ActionGroupCommands.FirstOrDefault(c => c.ID.Equals(this.ActionGroupID));
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            ActionGroupCommand command = this.GetCommand();
            if (command != null)
            {
                this.ActionGroupID = command.ID;
                await command.Perform(user, arguments, this.GetExtraSpecialIdentifiers());
            }
        }
    }
}
