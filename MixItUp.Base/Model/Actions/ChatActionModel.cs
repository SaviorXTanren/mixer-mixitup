using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    [DataContract]
    public class ChatActionModel : ActionModelBase
    {
        [DataMember]
        public StreamingPlatformTypeEnum StreamingPlatform { get; set; } = StreamingPlatformTypeEnum.Default;

        [DataMember]
        public string ChatText { get; set; }

        [DataMember]
        public bool SendAsStreamer { get; set; }

        [DataMember]
        public bool IsWhisper { get; set; }
        [DataMember]
        public string WhisperUserName { get; set; }

        public ChatActionModel(StreamingPlatformTypeEnum streamingPlatform, string chatText, bool sendAsStreamer = false, bool isWhisper = false, string whisperUserName = null)
            : base(ActionTypeEnum.Chat)
        {
            this.StreamingPlatform = streamingPlatform;
            this.ChatText = chatText;
            this.SendAsStreamer = sendAsStreamer;
            this.IsWhisper = isWhisper;
            this.WhisperUserName = whisperUserName;
        }

        private ChatActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            StreamingPlatformTypeEnum platform = this.StreamingPlatform;
            if (this.StreamingPlatform == StreamingPlatformTypeEnum.Default)
            {
                platform = parameters.Platform;
            }

            string message = await ReplaceStringWithSpecialModifiers(this.ChatText, parameters);

            if (this.IsWhisper)
            {
                string whisperUserName = parameters.User.Username;
                if (!string.IsNullOrEmpty(this.WhisperUserName))
                {
                    whisperUserName = await ReplaceStringWithSpecialModifiers(this.WhisperUserName, parameters);
                }
                await ServiceManager.Get<ChatService>().Whisper(whisperUserName, platform, message, this.SendAsStreamer);
            }
            else
            {
                string replyMessageID = ChannelSession.Settings.TwitchReplyToCommandChatMessages ? parameters.TriggeringChatMessageID : null;
                await ServiceManager.Get<ChatService>().SendMessage(message, platform, this.SendAsStreamer, replyMessageID: replyMessageID);
            }
        }
    }
}
