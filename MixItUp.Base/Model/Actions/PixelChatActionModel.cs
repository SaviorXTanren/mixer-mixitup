using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    public enum PixelChatActionTypeEnum
    {
        ShowHideSceneComponent,
        TriggerGiveaway,
        TriggerCredits,
        TriggerShoutout,
        TriggerCountdown,
        TriggerCountup,
        StartStreamathon,
        AddStreamathonTime,
        AddUserToGiveaway,
    }

    [DataContract]
    public class PixelChatActionModel : ActionModelBase
    {
        public static PixelChatActionModel CreateShowHideSceneComponent(string sceneID, string componentID, bool visible)
        {
            return new PixelChatActionModel(PixelChatActionTypeEnum.ShowHideSceneComponent)
            {
                SceneID = sceneID,
                ComponentID = componentID,
                SceneComponentVisible = visible,
            };
        }

        public static PixelChatActionModel CreateBasicOverlay(PixelChatActionTypeEnum actionType, string overlayID)
        {
            return new PixelChatActionModel(actionType)
            {
                OverlayID = overlayID
            };
        }

        public static PixelChatActionModel CreateOverlayTimeAmount(PixelChatActionTypeEnum actionType, string overlayID, string timeAmount)
        {
            return new PixelChatActionModel(actionType)
            {
                OverlayID = overlayID,
                TimeAmount = timeAmount
            };
        }

        public static PixelChatActionModel CreateOverlayTargetUser(PixelChatActionTypeEnum actionType, string overlayID, string targetUsername)
        {
            return new PixelChatActionModel(actionType)
            {
                OverlayID = overlayID,
                TargetUsername = targetUsername
            };
        }

        [DataMember]
        public PixelChatActionTypeEnum ActionType { get; set; }

        [DataMember]
        public string SceneID { get; set; }
        [DataMember]
        public string ComponentID { get; set; }
        [DataMember]
        public bool SceneComponentVisible { get; set; }

        [DataMember]
        public string OverlayID { get; set; }

        [DataMember]
        public string TimeAmount { get; set; }

        [DataMember]
        public string TargetUsername { get; set; }

        public PixelChatActionModel(PixelChatActionTypeEnum actionType)
            : base(ActionTypeEnum.PixelChat)
        {
            this.ActionType = actionType;
        }

        [Obsolete]
        public PixelChatActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (ServiceManager.Get<PixelChatService>().IsConnected)
            {
                if (this.ActionType == PixelChatActionTypeEnum.ShowHideSceneComponent)
                {
                    Result result = await ServiceManager.Get<PixelChatService>().EditSceneComponent(this.SceneID, this.ComponentID, this.SceneComponentVisible);
                    if (!result.Success)
                    {
                        Logger.Log(LogLevel.Error, result.Message);
                    }
                }
                else
                {
                    PixelChatSendMessageModel sendMessage = null;
                    if (this.ActionType == PixelChatActionTypeEnum.TriggerShoutout || this.ActionType == PixelChatActionTypeEnum.AddUserToGiveaway)
                    {
                        UserV2ViewModel user = parameters.User;
                        if (!string.IsNullOrEmpty(this.TargetUsername))
                        {
                            string targetUsername = await ReplaceStringWithSpecialModifiers(this.TargetUsername, parameters);
                            UserV2ViewModel targetUser = await ServiceManager.Get<UserService>().GetUserByPlatform(parameters.Platform, platformUsername: targetUsername, performPlatformSearch: true);
                            if (targetUser != null)
                            {
                                user = targetUser;
                            }
                        }

                        if (this.ActionType == PixelChatActionTypeEnum.TriggerShoutout)
                        {
                            sendMessage = new PixelChatSendMessageModel(this.ActionType.ToString(), user.Username, user.Platform);
                        }
                        else if (this.ActionType == PixelChatActionTypeEnum.AddUserToGiveaway)
                        {
                            sendMessage = new PixelChatSendMessageModel(this.ActionType.ToString(), user.Username);
                        }
                    }
                    else if (this.ActionType == PixelChatActionTypeEnum.TriggerCountdown || this.ActionType == PixelChatActionTypeEnum.TriggerCountup ||
                        this.ActionType == PixelChatActionTypeEnum.AddStreamathonTime)
                    {
                        int.TryParse(await ReplaceStringWithSpecialModifiers(this.TimeAmount, parameters), out int timeAmount);
                        sendMessage = new PixelChatSendMessageModel(this.ActionType.ToString(), timeAmount);
                    }
                    else
                    {
                        sendMessage = new PixelChatSendMessageModel(this.ActionType.ToString());
                    }

                    if (sendMessage != null)
                    {
                        char[] characters = sendMessage.type.ToCharArray();
                        characters[0] = Char.ToLower(characters[0]);
                        sendMessage.type = new string(characters);

                        Result result = await ServiceManager.Get<PixelChatService>().SendMessageToOverlay(this.OverlayID, sendMessage);
                        if (!result.Success)
                        {
                            Logger.Log(LogLevel.Error, result.Message);
                        }
                    }
                }
            }
        }
    }
}
