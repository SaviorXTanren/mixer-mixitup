using Mixer.Base.Model.Chat;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace MixItUp.Base.ViewModel.Chat
{
    public class ChatMessageViewModel : IEquatable<ChatMessageViewModel>
    {
        public static readonly Regex EmoteRegex = new Regex(":\\w+");

        public Guid ID { get; private set; }

        public ChatUserViewModel User { get; private set; }

        public string Message { get; private set; }

        public string TargetUsername { get; private set; }

        public DateTimeOffset Timestamp { get; private set; }

        public bool IsDeleted { get; set; }

        public ChatMessageEventModel ChatMessageEvent { get; private set; }

        public ChatMessageViewModel(ChatMessageEventModel chatMessageEvent)
        {
            this.ChatMessageEvent = chatMessageEvent;
            this.ID = this.ChatMessageEvent.id;

            if (ChannelSession.ChatUsers.ContainsKey(this.ChatMessageEvent.user_id))
            {
                this.User = ChannelSession.ChatUsers[this.ChatMessageEvent.user_id];
            }
            else
            {
                this.User = new ChatUserViewModel(this.ChatMessageEvent);
            }
            
            this.TargetUsername = this.ChatMessageEvent.target;
            this.Timestamp = DateTimeOffset.Now;
            this.Message = string.Empty;
            foreach (ChatMessageDataModel message in this.ChatMessageEvent.message.message)
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

        public bool IsWhisper { get { return !string.IsNullOrEmpty(this.TargetUsername); } }

        public bool ShouldBeModerated()
        {
            string lower = this.Message.ToLower();
            foreach (string word in ChannelSession.Settings.BannedWords)
            {
                if (lower.Contains(word))
                {
                    return true;
                }
            }

            if (ChannelSession.Settings.CapsBlockCount > 0 && this.Message.Count(c => char.IsUpper(c)) >= ChannelSession.Settings.CapsBlockCount)
            {
                return true;
            }

            if (ChannelSession.Settings.SymbolEmoteBlockCount > 0)
            {
                if (lower.Count(c => char.IsSymbol(c) || char.IsPunctuation(c)) >= ChannelSession.Settings.SymbolEmoteBlockCount)
                {
                    return true;
                }

                MatchCollection matches = EmoteRegex.Matches(lower);
                if (matches.Count >= ChannelSession.Settings.SymbolEmoteBlockCount)
                {
                    return true;
                }
            }

            if (ChannelSession.Settings.BlockLinks && lower.Contains(".com"))
            {
                return true;
            }

            return false;
        }

        public bool Equals(ChatMessageViewModel other) { return this.ID.Equals(other.ID); }

        public override int GetHashCode() { return this.ID.GetHashCode(); }

        public override string ToString() { return string.Format("{0}: {1}", this.User, this.Message); }
    }
}
