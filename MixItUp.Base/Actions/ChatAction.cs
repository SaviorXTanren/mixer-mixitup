using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [DataContract]
    public class ChatAction : ActionBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return ChatAction.asyncSemaphore; } }

        [DataMember]
        public string ChatText { get; set; }

        [DataMember]
        public bool IsWhisper { get; set; }
        [DataMember]
        public string WhisperUserName { get; set; }

        [DataMember]
        public bool SendAsStreamer { get; set; }

        public ChatAction() : base(ActionTypeEnum.Chat) { }

        public ChatAction(string chatText, bool sendAsStreamer = false, bool isWhisper = false, string whisperUserName = null)
            : this()
        {
            this.ChatText = chatText;
            this.SendAsStreamer = sendAsStreamer;
            this.IsWhisper = isWhisper;
            this.WhisperUserName = whisperUserName;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            if (ChannelSession.Chat != null)
            {
                string message = await this.ReplaceStringWithSpecialModifiers(this.ChatText, user, arguments);
                if (this.IsWhisper)
                {
                    string whisperUserName = user.UserName;
                    if (!string.IsNullOrEmpty(this.WhisperUserName))
                    {
                        whisperUserName = await this.ReplaceStringWithSpecialModifiers(this.WhisperUserName, user, arguments);
                    }
                    await ChannelSession.Chat.Whisper(whisperUserName, message, this.SendAsStreamer);
                }
                else
                {
                    await ChannelSession.Chat.SendMessage(message, this.SendAsStreamer);
                }
            }
        }
    }
}
