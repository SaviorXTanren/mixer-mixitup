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
        public bool SendAsStreamer { get; set; }

        public ChatAction() : base(ActionTypeEnum.Chat) { }

        public ChatAction(string chatText, bool isWhisper, bool sendAsStreamer)
            : this()
        {
            this.ChatText = chatText;
            this.IsWhisper = isWhisper;
            this.SendAsStreamer = sendAsStreamer;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            if (ChannelSession.Chat != null)
            {
                string message = await this.ReplaceStringWithSpecialModifiers(this.ChatText, user, arguments);
                if (this.IsWhisper)
                {
                    await ChannelSession.Chat.Whisper(user.UserName, message, this.SendAsStreamer);
                }
                else
                {
                    await ChannelSession.Chat.SendMessage(message, this.SendAsStreamer);
                }
            }
        }
    }
}
