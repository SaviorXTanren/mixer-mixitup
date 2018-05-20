using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.User;
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
        public string ActionGroupName { get; set; }

        public ActionGroupAction() : base(ActionTypeEnum.ActionGroup) { }

        public ActionGroupAction(ActionGroupCommand command)
            : this()
        {
            this.ActionGroupName = command.Name;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            ActionGroupCommand command = ChannelSession.Settings.ActionGroupCommands.FirstOrDefault(c => c.Name.Equals(this.ActionGroupName));
            if (command != null)
            {
                command.AddSpecialIdentifiers(this.GetAdditiveSpecialIdentifiers());
                await command.Perform(user, arguments);
            }
        }
    }
}
