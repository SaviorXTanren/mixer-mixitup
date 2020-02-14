using Mixer.Base.Model.MixPlay;
using MixItUp.Base.Model.MixPlay;
using MixItUp.Base.Services.External;
using MixItUp.Base.Services.Mixer;
using MixItUp.Base.Util;
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
        public static InteractiveAction CreateMoveUserToGroupAction(string groupName, UserRoleEnum requiredRole, string username = null)
        {
            return new InteractiveAction(InteractiveActionTypeEnum.MoveUserToGroup)
            {
                GroupName = groupName,
                RoleRequirement = requiredRole,
                OptionalUserName = username,
            };
        }

        public static InteractiveAction CreateMoveUserToSceneAction(string sceneID, UserRoleEnum requiredRole, string username = null)
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

        public static InteractiveAction CreateMoveAllUsersToSceneAction(string sceneID, UserRoleEnum requiredRole)
        {
            return new InteractiveAction(InteractiveActionTypeEnum.MoveAllUsersToScene)
            {
                GroupName = sceneID,
                SceneID = sceneID,
                RoleRequirement = requiredRole,
            };
        }

        public static InteractiveAction CreateMoveAllUsersToGroupAction(string groupName, UserRoleEnum requiredRole)
        {
            return new InteractiveAction(InteractiveActionTypeEnum.MoveAllUsersToGroup)
            {
                GroupName = groupName,
                RoleRequirement = requiredRole,
            };
        }

        public static InteractiveAction CreateCooldownAction(InteractiveActionTypeEnum type, string cooldownID, string cooldownAmount)
        {
            return new InteractiveAction(type)
            {
                CooldownID = cooldownID,
                CooldownAmountString = cooldownAmount,
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
        public UserRoleEnum RoleRequirement { get; set; }

        [DataMember]
        public string SceneID { get; set; }

        [DataMember]
        public string CooldownID { get; set; }
        [DataMember]
        public string CooldownAmountString { get; set; }
        [DataMember]
        [Obsolete]
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
            this.RoleRequirement = UserRoleEnum.User;
        }

        public InteractiveAction(InteractiveActionTypeEnum interactiveType)
            : this()
        {
            this.InteractiveType = interactiveType;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            if (this.InteractiveType == InteractiveActionTypeEnum.Connect)
            {
                if (ChannelSession.Services.MixPlay.IsConnected)
                {
                    await ChannelSession.Services.MixPlay.Disconnect();
                    GlobalEvents.InteractiveDisconnected();
                }

                IEnumerable<MixPlayGameModel> games = await ChannelSession.Services.MixPlay.GetAllGames();
                MixPlayGameModel game = games.FirstOrDefault(g => g.id.Equals(this.InteractiveGameID));
                if (game != null)
                {
                    await ChannelSession.Services.MixPlay.SetGame(game);
                    ExternalServiceResult result = await ChannelSession.Services.MixPlay.Connect();
                    if (result.Success)
                    {
                        GlobalEvents.InteractiveConnected(game);
                    }
                }
            }
            else if (this.InteractiveType == InteractiveActionTypeEnum.Disconnect)
            {
                if (ChannelSession.Services.MixPlay.IsConnected)
                {
                    await ChannelSession.Services.MixPlay.Disconnect();
                    GlobalEvents.InteractiveDisconnected();
                }
            }
            else if (ChannelSession.Services.MixPlay.IsConnected)
            {
                if (user != null && !user.HasPermissionsTo(this.RoleRequirement))
                {
                    if (ChannelSession.Services.Chat != null)
                    {
                        await ChannelSession.Services.Chat.Whisper(user, "You do not permission to perform this action.");
                    }
                    return;
                }

                await ChannelSession.Services.MixPlay.AddGroup(this.GroupName, (!string.IsNullOrEmpty(this.SceneID)) ? this.SceneID : MixPlayUserGroupModel.DefaultName);

                if (this.InteractiveType == InteractiveActionTypeEnum.MoveGroupToScene)
                {
                    await ChannelSession.Services.MixPlay.UpdateGroup(this.GroupName, this.SceneID);
                }
                else if (this.InteractiveType == InteractiveActionTypeEnum.MoveUserToGroup || this.InteractiveType == InteractiveActionTypeEnum.MoveUserToScene)
                {
                    if (!string.IsNullOrEmpty(this.OptionalUserName))
                    {
                        string optionalUserName = await this.ReplaceStringWithSpecialModifiers(this.OptionalUserName, user, arguments);
                        UserViewModel optionalUser = ChannelSession.Services.User.GetUserByUsername(optionalUserName);
                        if (optionalUser != null)
                        {
                            await ChannelSession.Services.MixPlay.AddUserToGroup(optionalUser, this.GroupName);
                        }
                    }
                    else
                    {
                        await ChannelSession.Services.MixPlay.AddUserToGroup(user, this.GroupName);
                    }
                }
                if (this.InteractiveType == InteractiveActionTypeEnum.MoveAllUsersToGroup || this.InteractiveType == InteractiveActionTypeEnum.MoveAllUsersToScene)
                {
                    foreach (UserViewModel chatUser in ChannelSession.Services.User.GetAllUsers())
                    {
                        await ChannelSession.Services.MixPlay.AddUserToGroup(chatUser, this.GroupName);
                    }

                    IEnumerable<MixPlayParticipantModel> participants = ChannelSession.Services.MixPlay.Participants.Values;
                    foreach (MixPlayParticipantModel participant in participants)
                    {
                        participant.groupID = this.GroupName;
                    }
                    await ChannelSession.Services.MixPlay.UpdateParticipants(participants);
                }
                else if (this.InteractiveType == InteractiveActionTypeEnum.CooldownButton || this.InteractiveType == InteractiveActionTypeEnum.CooldownGroup ||
                    this.InteractiveType == InteractiveActionTypeEnum.CooldownScene)
                {
                    string amountString = await this.ReplaceStringWithSpecialModifiers(this.CooldownAmountString, user, arguments);
                    if (int.TryParse(amountString, out int amount) && amount >= 0)
                    {
                        long timestamp = DateTimeOffset.Now.AddSeconds(amount).ToUnixTimeMilliseconds();
                        if (this.InteractiveType == InteractiveActionTypeEnum.CooldownButton)
                        {
                            await ChannelSession.Services.MixPlay.CooldownButton(this.CooldownID, timestamp);
                        }
                        else if (this.InteractiveType == InteractiveActionTypeEnum.CooldownGroup)
                        {
                            await ChannelSession.Services.MixPlay.CooldownGroup(this.CooldownID, timestamp);
                        }
                        else if (this.InteractiveType == InteractiveActionTypeEnum.CooldownScene)
                        {
                            await ChannelSession.Services.MixPlay.CooldownScene(this.CooldownID, timestamp);
                        }
                    }
                }
                else if (this.InteractiveType == InteractiveActionTypeEnum.UpdateControl || this.InteractiveType == InteractiveActionTypeEnum.SetCustomMetadata ||
                    this.InteractiveType == InteractiveActionTypeEnum.EnableDisableControl)
                {
                    MixPlayConnectedSceneModel scene = null;
                    MixPlayControlModel control = null;

                    string processedControlId = await this.ReplaceStringWithSpecialModifiers(this.ControlID, user, arguments);
                    foreach (MixPlayConnectedSceneModel s in ChannelSession.Services.MixPlay.Scenes.Values)
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
                            control.meta["userID"] = user.MixerID;
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

                        await ChannelSession.Services.MixPlay.UpdateControls(scene, new List<MixPlayControlModel>() { control });
                    }
                }
            }
        }
    }
}
