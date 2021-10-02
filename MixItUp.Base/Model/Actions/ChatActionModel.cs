using MixItUp.Base.Model.Commands;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    [DataContract]
    public class ChatActionModel : ActionModelBase
    {
        [DataMember]
        public string ChatText { get; set; }

        [DataMember]
        public bool SendAsStreamer { get; set; }

        [DataMember]
        public bool IsWhisper { get; set; }
        [DataMember]
        public string WhisperUserName { get; set; }

        public ChatActionModel(string chatText, bool sendAsStreamer = false, bool isWhisper = false, string whisperUserName = null)
            : base(ActionTypeEnum.Chat)
        {
            this.ChatText = chatText;
            this.SendAsStreamer = sendAsStreamer;
            this.IsWhisper = isWhisper;
            this.WhisperUserName = whisperUserName;
        }

#pragma warning disable CS0612 // Type or member is obsolete
        internal ChatActionModel(MixItUp.Base.Actions.ChatAction action)
            : base(ActionTypeEnum.Chat)
        {
            this.ChatText = action.ChatText;
            this.SendAsStreamer = action.SendAsStreamer;
            this.IsWhisper = action.IsWhisper;
            this.WhisperUserName = action.WhisperUserName;
        }
#pragma warning restore CS0612 // Type or member is obsolete

        private ChatActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            string message = await ReplaceStringWithSpecialModifiers(this.ChatText, parameters);
            if (this.IsWhisper)
            {
                string whisperUserName = parameters.User.Username;
                if (!string.IsNullOrEmpty(this.WhisperUserName))
                {
                    whisperUserName = await ReplaceStringWithSpecialModifiers(this.WhisperUserName, parameters);
                }
                await ChannelSession.Services.Chat.Whisper(parameters.Platform, whisperUserName, message, this.SendAsStreamer);
            }
            else
            {
                string replyMessageID = ChannelSession.Settings.TwitchReplyToCommandChatMessages ? parameters.TriggeringChatMessageID : null;
                await ChannelSession.Services.Chat.SendMessage(message, this.SendAsStreamer, platform: StreamingPlatformTypeEnum.All, replyMessageID: replyMessageID);
            }
        }
    }
}
