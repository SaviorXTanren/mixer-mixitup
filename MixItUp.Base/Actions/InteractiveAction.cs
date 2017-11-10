using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [DataContract]
    public class InteractiveAction : ActionBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSempahore { get { return InteractiveAction.asyncSemaphore; } }

        [DataMember]
        public string GroupName { get; set; }

        [DataMember]
        public bool AddUserToGroup { get; set; }

        [DataMember]
        public string MoveToScene { get; set; }

        public InteractiveAction() : base(ActionTypeEnum.Interactive) { }

        public InteractiveAction(string groupName)
            : this()
        {
            this.GroupName = groupName;
            this.AddUserToGroup = true;
        }

        public InteractiveAction(string groupName, string moveToScene)
            : this()
        {
            this.GroupName = groupName;
            this.MoveToScene = moveToScene;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            if (ChannelSession.Interactive != null && ChannelSession.Interactive.Client.Authenticated)
            {
                if (this.AddUserToGroup)
                {

                }
                else if ()
                {

                }
            }
        }
    }
}
