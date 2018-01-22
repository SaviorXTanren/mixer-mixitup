using Mixer.Base.Model.Interactive;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [DataContract]
    public class InteractiveAction : ActionBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return InteractiveAction.asyncSemaphore; } }

        [DataMember]
        public string GroupName { get; set; }

        [DataMember]
        public bool AddUserToGroup { get; set; }
        [DataMember]
        public UserRole RoleRequirement { get; set; }

        [DataMember]
        public string MoveGroupToScene { get; set; }

        public InteractiveAction()
            : base(ActionTypeEnum.Interactive)
        {
            this.RoleRequirement = UserRole.User;
        }

        public InteractiveAction(string groupName, UserRole roleRequirement)
            : this()
        {
            this.GroupName = groupName;
            this.AddUserToGroup = true;
            this.RoleRequirement = roleRequirement;
        }

        public InteractiveAction(string groupName, string moveToScene, UserRole roleRequirement)
            : this()
        {
            this.GroupName = groupName;
            this.MoveGroupToScene = moveToScene;
            this.RoleRequirement = roleRequirement;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            if (ChannelSession.Interactive != null && ChannelSession.Interactive.Client.Authenticated)
            {
                if (!user.Roles.Any(r => r >= this.RoleRequirement))
                {
                    if (ChannelSession.Chat != null)
                    {
                        await ChannelSession.Chat.Whisper(user.UserName, "You do not permission to perform this action.");
                    }
                    return;
                } 

                if (this.AddUserToGroup)
                {
                    InteractiveParticipantModel participant = ChannelSession.Interactive.InteractiveUsers.Values.FirstOrDefault(p => p.userID.Equals(user.ID));
                    if (participant != null)
                    {
                        participant.groupID = this.GroupName;
                        await ChannelSession.Interactive.UpdateParticipants(new List<InteractiveParticipantModel>() { participant });
                    }
                }
                else if (!string.IsNullOrEmpty(MoveGroupToScene))
                {
                    await ChannelSession.Interactive.UpdateGroups(new List<InteractiveGroupModel>() { new InteractiveGroupModel() { groupID = this.GroupName, sceneID = this.MoveGroupToScene } });
                }
            }
        }
    }
}
