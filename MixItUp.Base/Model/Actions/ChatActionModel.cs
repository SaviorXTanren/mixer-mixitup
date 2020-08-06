using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    [DataContract]
    public class ChatActionModel : ActionModelBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

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

        protected override SemaphoreSlim AsyncSemaphore { get { return ChatActionModel.asyncSemaphore; } }

        protected override async Task PerformInternal(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            string message = await this.ReplaceStringWithSpecialModifiers(this.ChatText, user, platform, arguments, specialIdentifiers);
            if (this.IsWhisper)
            {
                string whisperUserName = user.Username;
                if (!string.IsNullOrEmpty(this.WhisperUserName))
                {
                    whisperUserName = await this.ReplaceStringWithSpecialModifiers(this.WhisperUserName, user, platform, arguments, specialIdentifiers);
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
}
