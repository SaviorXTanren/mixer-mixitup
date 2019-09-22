using Mixer.Base.Model.MixPlay;
using MixItUp.Base.MixerAPI;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Interactive;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
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

        [Name("Update Control")]
        UpdateControl,

        [Name("Set Custom Metadata")]
        SetCustomMetadata,

        [Name("Move All Users to Scene")]
        MoveAllUsersToScene,
        [Name("Move All Users to Group")]
        MoveAllUsersToGroup,

        [Name("Enable/Disable Control")]
        EnableDisableControl
    }

    public enum InteractiveActionUpdateControlTypeEnum
    {
        Text,
        [Name("Text Color")]
        TextColor,
        [Name("Text Size")]
        TextSize,
        Tooltip,
        [Name("Spark Cost")]
        SparkCost,
        [Name("Accent Color")]
        AccentColor,
        [Name("Focus Color")]
        FocusColor,
        [Name("Border Color")]
        BorderColor,
        [Name("Background Color")]
        BackgroundColor,
        [Name("Background Image")]
        BackgroundImage,
        [Name("Progress")]
        Progress,
    }

    [DataContract]
    public class InteractiveAction : ActionBase
    {
        public static InteractiveAction CreateMoveUserToGroupAction(string groupName, MixerRoleEnum requiredRole, string username = null)
        {
            return new InteractiveAction(InteractiveActionTypeEnum.MoveUserToGroup)
            {
                GroupName = groupName,
                RoleRequirement = requiredRole,
                OptionalUserName = username,
            };
        }

        public static InteractiveAction CreateMoveUserToSceneAction(string sceneID, MixerRoleEnum requiredRole, string username = null)
        {
            return new InteractiveAction(InteractiveActionTypeEnum.MoveUserToScene)
            {
                GroupName = sceneID,
                SceneID = sceneID,
                RoleRequirement = requiredRole,
                OptionalUserName = username,
            };
        }

        public static InteractiveAction CreateMoveGroupToSceneAction(string groupName, string sceneID)
        {
            return new InteractiveAction(InteractiveActionTypeEnum.MoveGroupToScene)
            {
                GroupName = groupName,
                SceneID = sceneID,
            };
        }

        public static InteractiveAction CreateMoveAllUsersToSceneAction(string sceneID, MixerRoleEnum requiredRole)
        {
            return new InteractiveAction(InteractiveActionTypeEnum.MoveAllUsersToScene)
            {
                GroupName = sceneID,
                SceneID = sceneID,
                RoleRequirement = requiredRole,
            };
        }

        public static InteractiveAction CreateMoveAllUsersToGroupAction(string groupName, MixerRoleEnum requiredRole)
        {
            return new InteractiveAction(InteractiveActionTypeEnum.MoveAllUsersToGroup)
            {
                GroupName = groupName,
                RoleRequirement = requiredRole,
            };
        }

        public static InteractiveAction CreateCooldownAction(InteractiveActionTypeEnum type, string cooldownID, int cooldownAmount)
        {
            return new InteractiveAction(type)
            {
                CooldownID = cooldownID,
                CooldownAmount = cooldownAmount,
            };
        }

        public static InteractiveAction CreateConnectAction(MixPlayGameModel game)
        {
            return new InteractiveAction(InteractiveActionTypeEnum.Connect)
            {
                InteractiveGameID = game.id
            };
        }

        public static InteractiveAction CreateUpdateControlAction(InteractiveActionUpdateControlTypeEnum updateControlType, string controlID, string updateValue)
        {
            return new InteractiveAction(InteractiveActionTypeEnum.UpdateControl)
            {
                UpdateControlType = updateControlType,
                ControlID = controlID,
                UpdateValue = updateValue
            };
        }

        public static InteractiveAction CreateSetCustomMetadataAction(string controlID, Dictionary<string, string> customMetadata)
        {
            return new InteractiveAction(InteractiveActionTypeEnum.SetCustomMetadata)
            {
                ControlID = controlID,
                CustomMetadata = customMetadata
            };
        }

        public static InteractiveAction CreateEnableDisableControlAction(string controlID, bool enableDisable)
        {
            return new InteractiveAction(InteractiveActionTypeEnum.EnableDisableControl)
            {
                ControlID = controlID,
                EnableDisableControl = enableDisable
            };
        }

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

        [DataMember]
        public string OptionalUserName { get; set; }

        [DataMember]
        public string ControlID { get; set; }

        [DataMember]
        public InteractiveActionUpdateControlTypeEnum UpdateControlType { get; set; }
        [DataMember]
        public string UpdateValue { get; set; }

        [DataMember]
        public Dictionary<string, string> CustomMetadata { get; set; }

        [DataMember]
        public bool EnableDisableControl { get; set; }

        public InteractiveAction()
            : base(ActionTypeEnum.Interactive)
        {
            this.RoleRequirement = MixerRoleEnum.User;
        }

        public InteractiveAction(InteractiveActionTypeEnum interactiveType)
            : this()
        {
            this.InteractiveType = interactiveType;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            if (ChannelSession.Interactive != null)
            {
                if (this.InteractiveType == InteractiveActionTypeEnum.Connect)
                {
                    if (ChannelSession.Interactive.IsConnected())
                    {
                        await ChannelSession.Interactive.Disconnect();
                        GlobalEvents.InteractiveDisconnected();
                    }

                    IEnumerable<MixPlayGameModel> games = await ChannelSession.Interactive.GetAllConnectableGames();
                    MixPlayGameModel game = games.FirstOrDefault(g => g.id.Equals(this.InteractiveGameID));
                    if (game != null)
                    {
                        if (await ChannelSession.Interactive.Connect(game) == MixPlayConnectionResult.Success)
                        {
                            GlobalEvents.InteractiveConnected(game);
                        }
                    }
                }
                else if (this.InteractiveType == InteractiveActionTypeEnum.Disconnect)
                {
                    if (ChannelSession.Interactive.IsConnected())
                    {
                        await ChannelSession.Interactive.Disconnect();
                        GlobalEvents.InteractiveDisconnected();
                    }
                }
                else if (ChannelSession.Interactive.IsConnected())
                {
                    if (!user.HasPermissionsTo(this.RoleRequirement))
                    {
                        if (ChannelSession.Services.Chat != null)
                        {
                            await ChannelSession.Services.Chat.Whisper(user.UserName, "You do not permission to perform this action.");
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
                        if (!string.IsNullOrEmpty(this.OptionalUserName))
                        {
                            string optionalUserName = await this.ReplaceStringWithSpecialModifiers(this.OptionalUserName, user, arguments);
                            UserViewModel optionalUser = ChannelSession.Services.User.GetUserByUsername(optionalUserName);
                            if (optionalUser != null)
                            {
                                await ChannelSession.Interactive.AddUserToGroup(optionalUser, this.GroupName);
                            }
                        }
                        else
                        {
                            await ChannelSession.Interactive.AddUserToGroup(user, this.GroupName);
                        }
                    }
                    if (this.InteractiveType == InteractiveActionTypeEnum.MoveAllUsersToGroup || this.InteractiveType == InteractiveActionTypeEnum.MoveAllUsersToScene)
                    {
                        foreach (UserViewModel chatUser in ChannelSession.Services.User.GetAllUsers())
                        {
                            await ChannelSession.Interactive.AddUserToGroup(chatUser, this.GroupName);
                        }

                        IEnumerable<MixPlayParticipantModel> participants = ChannelSession.Interactive.Participants.Values;
                        foreach (MixPlayParticipantModel participant in participants)
                        {
                            participant.groupID = this.GroupName;
                        }
                        await ChannelSession.Interactive.UpdateParticipants(participants);
                    }
                    else if (this.InteractiveType == InteractiveActionTypeEnum.CooldownButton || this.InteractiveType == InteractiveActionTypeEnum.CooldownGroup ||
                        this.InteractiveType == InteractiveActionTypeEnum.CooldownScene)
                    {
                        MixPlayConnectedSceneModel scene = null;
                        List<MixPlayConnectedButtonControlModel> buttons = new List<MixPlayConnectedButtonControlModel>();
                        if (this.InteractiveType == InteractiveActionTypeEnum.CooldownButton)
                        {
                            if (ChannelSession.Interactive.ControlCommands.ContainsKey(this.CooldownID) && ChannelSession.Interactive.ControlCommands[this.CooldownID] is InteractiveConnectedButtonCommand)
                            {
                                InteractiveConnectedButtonCommand command = (InteractiveConnectedButtonCommand)ChannelSession.Interactive.ControlCommands[this.CooldownID];
                                scene = command.Scene;
                                buttons.Add(command.Button);
                            }
                        }

                        if (this.InteractiveType == InteractiveActionTypeEnum.CooldownGroup)
                        {
                            var allButtons = ChannelSession.Interactive.ControlCommands.Values.Where(c => c is InteractiveConnectedButtonCommand).Select(c => (InteractiveConnectedButtonCommand)c);
                            allButtons = allButtons.Where(c => this.CooldownID.Equals(c.ButtonCommand.CooldownGroupName));
                            if (allButtons.Count() > 0)
                            {
                                scene = allButtons.FirstOrDefault().Scene;
                                buttons.AddRange(allButtons.Select(c => c.Button));
                            }
                        }

                        if (this.InteractiveType == InteractiveActionTypeEnum.CooldownScene)
                        {
                            var allButtons = ChannelSession.Interactive.ControlCommands.Values.Where(c => c is InteractiveConnectedButtonCommand).Select(c => (InteractiveConnectedButtonCommand)c);
                            allButtons = allButtons.Where(c => this.CooldownID.Equals(c.ButtonCommand.SceneID));
                            if (allButtons.Count() > 0)
                            {
                                scene = allButtons.FirstOrDefault().Scene;
                                buttons.AddRange(allButtons.Select(c => c.Button));
                            }
                        }

                        if (buttons.Count > 0)
                        {
                            long timestamp = DateTimeOffset.Now.AddSeconds(this.CooldownAmount).ToUnixTimeMilliseconds();
                            foreach (MixPlayConnectedButtonControlModel button in buttons)
                            {
                                button.SetCooldownTimestamp(timestamp);
                            }
                            await ChannelSession.Interactive.UpdateControls(scene, buttons);
                        }
                    }
                    else if (this.InteractiveType == InteractiveActionTypeEnum.UpdateControl || this.InteractiveType == InteractiveActionTypeEnum.SetCustomMetadata ||
                        this.InteractiveType == InteractiveActionTypeEnum.EnableDisableControl)
                    {
                        MixPlayConnectedSceneModel scene = null;
                        MixPlayControlModel control = null;

                        string processedControlId = await this.ReplaceStringWithSpecialModifiers(this.ControlID, user, arguments);
                        foreach (MixPlayConnectedSceneModel s in ChannelSession.Interactive.Scenes)
                        {
                            foreach (MixPlayControlModel c in s.allControls)
                            {
                                if (c.controlID.Equals(processedControlId))
                                {
                                    scene = s;
                                    control = c;
                                    break;
                                }
                            }

                            if (control != null)
                            {
                                break;
                            }
                        }

                        if (scene != null && control != null)
                        {
                            if (this.InteractiveType == InteractiveActionTypeEnum.UpdateControl)
                            {
                                string replacementValue = await this.ReplaceStringWithSpecialModifiers(this.UpdateValue, user, arguments);
                                int.TryParse(replacementValue, out int replacementNumberValue);
                                float.TryParse(replacementValue, out float replacementFloatValue);

                                if (control is MixPlayButtonControlModel)
                                {
                                    MixPlayButtonControlModel button = (MixPlayButtonControlModel)control;
                                    switch (this.UpdateControlType)
                                    {
                                        case InteractiveActionUpdateControlTypeEnum.Text: button.text = replacementValue; break;
                                        case InteractiveActionUpdateControlTypeEnum.TextSize: button.textSize = replacementValue; break;
                                        case InteractiveActionUpdateControlTypeEnum.TextColor: button.textColor = replacementValue; break;
                                        case InteractiveActionUpdateControlTypeEnum.Tooltip: button.tooltip = replacementValue; break;
                                        case InteractiveActionUpdateControlTypeEnum.SparkCost: button.cost = replacementNumberValue; break;
                                        case InteractiveActionUpdateControlTypeEnum.AccentColor: button.accentColor = replacementValue; break;
                                        case InteractiveActionUpdateControlTypeEnum.FocusColor: button.focusColor = replacementValue; break;
                                        case InteractiveActionUpdateControlTypeEnum.BorderColor: button.borderColor = replacementValue; break;
                                        case InteractiveActionUpdateControlTypeEnum.BackgroundColor: button.backgroundColor = replacementValue; break;
                                        case InteractiveActionUpdateControlTypeEnum.BackgroundImage: button.backgroundImage = replacementValue; break;
                                        case InteractiveActionUpdateControlTypeEnum.Progress: ((MixPlayConnectedButtonControlModel)button).SetProgress(replacementFloatValue); break;
                                    }
                                }
                                else if (control is MixPlayLabelControlModel)
                                {
                                    MixPlayLabelControlModel label = (MixPlayLabelControlModel)control;
                                    switch (this.UpdateControlType)
                                    {
                                        case InteractiveActionUpdateControlTypeEnum.Text: label.text = replacementValue; break;
                                        case InteractiveActionUpdateControlTypeEnum.TextSize: label.textSize = replacementValue; break;
                                        case InteractiveActionUpdateControlTypeEnum.TextColor: label.textColor = replacementValue; break;
                                    }
                                }
                                else if (control is MixPlayTextBoxControlModel)
                                {
                                    MixPlayTextBoxControlModel textbox = (MixPlayTextBoxControlModel)control;
                                    switch (this.UpdateControlType)
                                    {
                                        case InteractiveActionUpdateControlTypeEnum.Text: textbox.submitText = replacementValue; break;
                                        case InteractiveActionUpdateControlTypeEnum.Tooltip: textbox.placeholder = replacementValue; break;
                                        case InteractiveActionUpdateControlTypeEnum.SparkCost: textbox.cost = replacementNumberValue; break;
                                    }
                                }
                            }
                            else if (this.InteractiveType == InteractiveActionTypeEnum.SetCustomMetadata)
                            {
                                control.meta["userID"] = user.ID;
                                foreach (var kvp in this.CustomMetadata)
                                {
                                    string value = await this.ReplaceStringWithSpecialModifiers(kvp.Value, user, arguments);
                                    if (bool.TryParse(value, out bool boolValue))
                                    {
                                        control.meta[kvp.Key] = boolValue;
                                    }
                                    else if (int.TryParse(value, out int intValue))
                                    {
                                        control.meta[kvp.Key] = intValue;
                                    }
                                    else if (double.TryParse(value, out double doubleValue))
                                    {
                                        control.meta[kvp.Key] = doubleValue;
                                    }
                                    else
                                    {
                                        control.meta[kvp.Key] = value;
                                    }
                                }
                            }
                            else if (this.InteractiveType == InteractiveActionTypeEnum.EnableDisableControl)
                            {
                                control.disabled = !this.EnableDisableControl;
                            }

                            await ChannelSession.Interactive.UpdateControls(scene, new List<MixPlayControlModel>() { control });
                        }
                    }
                }
            }
        }
    }
}
