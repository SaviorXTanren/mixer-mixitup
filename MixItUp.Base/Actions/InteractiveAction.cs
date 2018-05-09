using Mixer.Base.Model.Interactive;
using Mixer.Base.Util;
using MixItUp.Base.MixerAPI;
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
        [Name("Cooldown Button")]
        CooldownButton,
        [Name("Cooldown Group")]
        CooldownGroup,
        [Name("Cooldown Scene")]
        CooldownScene,
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
        public string CooldownID { get; set; }
        [DataMember]
        public int CooldownAmount { get; set; }

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

        public InteractiveAction(InteractiveActionTypeEnum interactiveType, string cooldownID, int cooldownAmount)
            : this()
        {
            this.InteractiveType = interactiveType;
            this.CooldownID = cooldownID;
            this.CooldownAmount = cooldownAmount;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            if (ChannelSession.Interactive != null && ChannelSession.Interactive.IsConnected())
            {
                if (!user.MixerRoles.Any(r => r >= this.RoleRequirement))
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

                if (this.InteractiveType == InteractiveActionTypeEnum.CooldownButton || this.InteractiveType == InteractiveActionTypeEnum.CooldownGroup ||
                    this.InteractiveType == InteractiveActionTypeEnum.CooldownScene)
                {
                    InteractiveConnectedSceneModel scene = null;
                    List<InteractiveConnectedButtonControlModel> buttons = new List<InteractiveConnectedButtonControlModel>();
                    if (this.InteractiveType == InteractiveActionTypeEnum.CooldownButton)
                    {
                        if (ChannelSession.Interactive.Controls.ContainsKey(this.CooldownID) && ChannelSession.Interactive.Controls[this.CooldownID].Button != null)
                        {
                            InteractiveConnectedControlCommand command = ChannelSession.Interactive.Controls[this.CooldownID];
                            scene = command.Scene;
                            buttons.Add(command.Button);
                        }
                    }

                    if (this.InteractiveType == InteractiveActionTypeEnum.CooldownGroup)
                    {
                        IEnumerable<InteractiveConnectedControlCommand> commands = ChannelSession.Interactive.Controls.Values.Where(c => c.Button != null &&
                            this.CooldownID.Equals(c.Command.CooldownGroup));
                        if (commands.Count() > 0)
                        {
                            scene = commands.FirstOrDefault().Scene;
                            buttons.AddRange(commands.Select(c => c.Button));
                        }
                    }

                    if (this.InteractiveType == InteractiveActionTypeEnum.CooldownScene)
                    {
                        IEnumerable<InteractiveConnectedControlCommand> commands = ChannelSession.Interactive.Controls.Values.Where(c => c.Button != null &&
                            this.CooldownID.Equals(c.Command.SceneID));
                        if (commands.Count() > 0)
                        {
                            scene = commands.FirstOrDefault().Scene;
                            buttons.AddRange(commands.Select(c => c.Button));
                        }
                    }

                    if (buttons.Count > 0)
                    {
                        long timestamp = DateTimeHelper.DateTimeOffsetToUnixTimestamp(DateTimeOffset.Now.AddSeconds(this.CooldownAmount));
                        foreach (InteractiveConnectedButtonControlModel button in buttons)
                        {
                            button.cooldown = timestamp;
                        }
                        await ChannelSession.Interactive.UpdateControls(scene, buttons);
                    }
                }
            }
        }
    }
}
