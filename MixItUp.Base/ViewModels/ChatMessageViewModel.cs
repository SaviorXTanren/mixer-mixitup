using Mixer.Base.Model.Chat;
using System;

namespace MixItUp.Base.ViewModels
{
    public class ChatMessageViewModel
    {
        public ChatUserViewModel User { get; private set; }

        public string Message { get; private set; }

        public DateTimeOffset Timestamp { get; private set; }

        private ChatMessageEventModel chatMessageEvent;

        public ChatMessageViewModel(ChatMessageEventModel chatMessageEvent)
        {
            this.chatMessageEvent = chatMessageEvent;
            this.User = new ChatUserViewModel(chatMessageEvent);
            this.Timestamp = DateTimeOffset.Now;
            this.Message = string.Empty;
            foreach (ChatMessageDataModel message in this.chatMessageEvent.message.message)
            {
                this.Message += message.text;
            }
        }
        public override string ToString() { return string.Format("{0}: {1}", this.User, this.Message); }
    }
}
