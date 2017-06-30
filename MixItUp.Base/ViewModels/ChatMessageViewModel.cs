using Mixer.Base.Model.Chat;
using System;

namespace MixItUp.Base.ViewModels
{
    public class ChatMessageViewModel : IEquatable<ChatMessageViewModel>
    {
        public Guid ID { get; private set; }

        public ChatUserViewModel User { get; private set; }

        public string Message { get; private set; }

        public DateTimeOffset Timestamp { get; private set; }

        public bool IsDeleted { get; set; }

        private ChatMessageEventModel chatMessageEvent;

        public ChatMessageViewModel(ChatMessageEventModel chatMessageEvent)
        {
            this.chatMessageEvent = chatMessageEvent;
            this.ID = this.chatMessageEvent.id;
            this.User = new ChatUserViewModel(this.chatMessageEvent);
            this.Timestamp = DateTimeOffset.Now;
            this.Message = string.Empty;
            foreach (ChatMessageDataModel message in this.chatMessageEvent.message.message)
            {
                this.Message += message.text;
            }
        }

        public ChatMessageViewModel(string alertText)
        {
            this.ID = Guid.Empty;
            this.User = null;
            this.Timestamp = DateTimeOffset.Now;
            this.Message = alertText;
        }

        public override bool Equals(object obj)
        {
            if (obj is ChatMessageViewModel)
            {
                return this.Equals((ChatMessageViewModel)obj);
            }
            return false;
        }

        public bool Equals(ChatMessageViewModel other) { return this.ID.Equals(other.ID); }

        public override int GetHashCode() { return this.ID.GetHashCode(); }

        public override string ToString() { return string.Format("{0}: {1}", this.User, this.Message); }
    }
}
