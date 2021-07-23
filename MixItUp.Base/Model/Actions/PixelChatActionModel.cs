using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services.External;
using MixItUp.Base.ViewModel.User;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    public enum PixelChatActionTypeEnum
    {
        TriggerGiveaway,
        TriggerCredits,
        TriggerShoutout,
        TriggerCountdown,
        TriggerCountup,
        StartStreamathon,
        AddStreamathonTime
    }

    [DataContract]
    public class PixelChatActionModel : ActionModelBase
    {
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

        private PixelChatActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (ChannelSession.Services.PixelChat.IsConnected)
            {
                PixelChatSendMessageModel sendMessage;
                if (this.ActionType == PixelChatActionTypeEnum.TriggerShoutout)
                {
                    UserViewModel user = parameters.User;
                    if (!string.IsNullOrEmpty(this.TargetUsername))
                    {
                        string targetUsername = await this.ReplaceStringWithSpecialModifiers(this.TargetUsername, parameters);
                        UserViewModel targetUser = await ChannelSession.Services.User.GetUserFullSearch(parameters.Platform, userID: null, targetUsername);
                        if (targetUser != null)
                        {
                            user = targetUser;
                        }
                    }
                    sendMessage = new PixelChatSendMessageModel(this.ActionType.ToString(), user.Username, StreamingPlatformTypeEnum.Twitch);
                }
                else if (this.ActionType == PixelChatActionTypeEnum.TriggerCountdown || this.ActionType == PixelChatActionTypeEnum.TriggerCountup ||
                    this.ActionType == PixelChatActionTypeEnum.AddStreamathonTime)
                {
                    int.TryParse(await this.ReplaceStringWithSpecialModifiers(this.TimeAmount, parameters), out int timeAmount);
                    sendMessage = new PixelChatSendMessageModel(this.ActionType.ToString(), timeAmount);
                }
                else
                {
                    sendMessage = new PixelChatSendMessageModel(this.ActionType.ToString());
                }

                char[] characters = sendMessage.type.ToCharArray();
                characters[0] = Char.ToLower(characters[0]);
                sendMessage.type = new string(characters);

                await ChannelSession.Services.PixelChat.SendMessageToOverlay(this.OverlayID, sendMessage);
            }
        }
    }
}
