using Mixer.Base.Model.Interactive;
using Mixer.Base.Util;
using MixItUp.Base.MixerAPI;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Interactive;
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

        [Name("Connect")]
        Connect,
        [Name("Disconnect")]
        Disconnect,
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
        public MixerRoleEnum RoleRequirement { get; set; }

        [DataMember]
        public string SceneID { get; set; }

        [DataMember]
        public string CooldownID { get; set; }
        [DataMember]
        public int CooldownAmount { get; set; }

        [DataMember]
        public uint InteractiveGameID { get; set; }

        public InteractiveAction()
            : base(ActionTypeEnum.Interactive)
        {
            this.RoleRequirement = MixerRoleEnum.User;
        }

        public InteractiveAction(InteractiveActionTypeEnum interactiveType, string groupName = null, string sceneID = null, MixerRoleEnum roleRequirement = MixerRoleEnum.User)
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


        public InteractiveAction(InteractiveActionTypeEnum interactiveType, uint interactiveGameID)
            : this()
        {
            this.InteractiveType = interactiveType;
            this.InteractiveGameID = interactiveGameID;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            if (ChannelSession.Interactive != null)
            {
                if (this.InteractiveType == InteractiveActionTypeEnum.Connect)
                {
                    IEnumerable<InteractiveGameListingModel> games = await ChannelSession.Connection.GetOwnedInteractiveGames(ChannelSession.Channel);
                    InteractiveGameListingModel game = games.FirstOrDefault(g => g.id.Equals(this.InteractiveGameID));
                    if (game != null)
                    {
                        if (await ChannelSession.Interactive.Connect(game))
                        {
                            GlobalEvents.InteractiveConnected(game);
                        }
                    }
                }
                else if (this.InteractiveType == InteractiveActionTypeEnum.Disconnect)
                {
                    await ChannelSession.Interactive.Disconnect();
                    GlobalEvents.InteractiveDisconnected();
                }
                else if (ChannelSession.Interactive.IsConnected())
                {
                    if (!user.MixerRoles.Any(r => r >= this.RoleRequirement))
                    {
                        if (ChannelSession.Chat != null)
                        {
                            await ChannelSession.Chat.Whisper(user.UserName, "You do not permission to perform this action.");
                        }
                        return;
                    }

                    await ChannelSession.Interactive.AddGroup(this.GroupName, (!string.IsNullOrEmpty(this.SceneID)) ? this.SceneID : InteractiveUserGroupViewModel.DefaultName);

                    if (this.InteractiveType == InteractiveActionTypeEnum.MoveGroupToScene)
                    {
                        await ChannelSession.Interactive.UpdateGroup(this.GroupName, this.SceneID);
                    }
                    else if (this.InteractiveType == InteractiveActionTypeEnum.MoveUserToGroup || this.InteractiveType == InteractiveActionTypeEnum.MoveUserToScene)
                    {
                        await ChannelSession.Interactive.AddUserToGroup(user, this.GroupName);
                    }
                    else if (this.InteractiveType == InteractiveActionTypeEnum.CooldownButton || this.InteractiveType == InteractiveActionTypeEnum.CooldownGroup ||
                        this.InteractiveType == InteractiveActionTypeEnum.CooldownScene)
                    {
                        InteractiveConnectedSceneModel scene = null;
                        List<InteractiveConnectedButtonControlModel> buttons = new List<InteractiveConnectedButtonControlModel>();
                        if (this.InteractiveType == InteractiveActionTypeEnum.CooldownButton)
                        {
                            if (ChannelSession.Interactive.Controls.ContainsKey(this.CooldownID) && ChannelSession.Interactive.Controls[this.CooldownID] is InteractiveConnectedButtonCommand)
                            {
                                InteractiveConnectedButtonCommand command = (InteractiveConnectedButtonCommand)ChannelSession.Interactive.Controls[this.CooldownID];
                                scene = command.Scene;
                                buttons.Add(command.Button);
                            }
                        }

                        if (this.InteractiveType == InteractiveActionTypeEnum.CooldownGroup)
                        {
                            var allButtons = ChannelSession.Interactive.Controls.Values.Where(c => c is InteractiveConnectedButtonCommand).Select(c => (InteractiveConnectedButtonCommand)c);
                            allButtons = allButtons.Where(c => this.CooldownID.Equals(c.ButtonCommand.CooldownGroupName));
                            if (allButtons.Count() > 0)
                            {
                                scene = allButtons.FirstOrDefault().Scene;
                                buttons.AddRange(allButtons.Select(c => c.Button));
                            }
                        }

                        if (this.InteractiveType == InteractiveActionTypeEnum.CooldownScene)
                        {
                            var allButtons = ChannelSession.Interactive.Controls.Values.Where(c => c is InteractiveConnectedButtonCommand).Select(c => (InteractiveConnectedButtonCommand)c);
                            allButtons = allButtons.Where(c => this.CooldownID.Equals(c.ButtonCommand.SceneID));
                            if (allButtons.Count() > 0)
                            {
                                scene = allButtons.FirstOrDefault().Scene;
                                buttons.AddRange(allButtons.Select(c => c.Button));
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
}
