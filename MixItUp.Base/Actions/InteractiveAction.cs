using Mixer.Base.Model.Interactive;
using Mixer.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public enum InteractiveActionTypeEnum
    {
        [Name("Move User to Scene")]
        MoveUserToScene,
        [Name("Move User to Group")]
        MoveUserToGroup,
        [Name("Move Group to Scene")]
        MoveGroupToScene,
    }

    [DataContract]
    public class InteractiveAction : ActionBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return InteractiveAction.asyncSemaphore; } }

        [DataMember]
        public InteractiveActionTypeEnum InteractiveType { get; set; }

        [DataMember]
        public string GroupName { get; set; }

        [DataMember]
        public UserRole RoleRequirement { get; set; }

        [DataMember]
        public string SceneID { get; set; }

        [DataMember]
        [Obsolete]
        public bool AddUserToGroup { get; set; }

        [DataMember]
        [Obsolete]
        public string MoveGroupToScene { get; set; }

        [JsonIgnore]
        private InteractiveGroupModel group;

        public InteractiveAction()
            : base(ActionTypeEnum.Interactive)
        {
            this.RoleRequirement = UserRole.User;
        }

        public InteractiveAction(InteractiveActionTypeEnum interactiveType, string groupName = null, string sceneID = null, UserRole roleRequirement = UserRole.User)
            : this()
        {
            this.InteractiveType = interactiveType;
            this.GroupName = groupName;
            this.SceneID = sceneID;
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

                if (this.group == null)
                {
                    InteractiveGroupCollectionModel groups = await ChannelSession.Interactive.GetGroups();
                    if (groups != null && groups.groups != null)
                    {
                        this.group = groups.groups.FirstOrDefault(g => g.groupID.Equals(this.GroupName));
                        if (this.group == null)
                        {
                            this.group = new InteractiveGroupModel() { groupID = this.GroupName, sceneID = this.SceneID };
                            await ChannelSession.Interactive.CreateGroups(new List<InteractiveGroupModel>() { this.group });
                        }
                    }
                }

                if (this.InteractiveType == InteractiveActionTypeEnum.MoveGroupToScene || this.InteractiveType == InteractiveActionTypeEnum.MoveUserToScene)
                {
                    this.group.sceneID = this.SceneID;
                    await ChannelSession.Interactive.UpdateGroups(new List<InteractiveGroupModel>() { this.group });
                }

                if (this.InteractiveType == InteractiveActionTypeEnum.MoveUserToGroup || this.InteractiveType == InteractiveActionTypeEnum.MoveUserToScene)
                {
                    InteractiveParticipantModel participant = ChannelSession.Interactive.InteractiveUsers.Values.FirstOrDefault(p => p.userID.Equals(user.ID));
                    if (participant != null)
                    {
                        participant.groupID = this.GroupName;
                        await ChannelSession.Interactive.UpdateParticipants(new List<InteractiveParticipantModel>() { participant });
                    }
                }
            }
        }
    }
}
