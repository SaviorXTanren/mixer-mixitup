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

        internal ChatActionModel(MixItUp.Base.Actions.ChatAction action)
            : base(ActionTypeEnum.Chat)
        {
            this.ChatText = action.ChatText;
            this.SendAsStreamer = action.SendAsStreamer;
            this.IsWhisper = action.IsWhisper;
            this.WhisperUserName = action.WhisperUserName;
        }

        private ChatActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            string message = await this.ReplaceStringWithSpecialModifiers(this.ChatText, parameters);
            if (this.IsWhisper)
            {
                string whisperUserName = parameters.User.Username;
                if (!string.IsNullOrEmpty(this.WhisperUserName))
                {
                    whisperUserName = await this.ReplaceStringWithSpecialModifiers(this.WhisperUserName, parameters);
                }
                await ChannelSession.Services.Chat.Whisper(StreamingPlatformTypeEnum.All, whisperUserName, message, this.SendAsStreamer);
            }
            else
            {
                await ChannelSession.Services.Chat.SendMessage(message, this.SendAsStreamer);
            }
        }
    }
}
