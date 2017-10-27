using Mixer.Base.Model.Chat;
using MixItUp.Base.ViewModel.User;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace MixItUp.Base.ViewModel.Chat
{
    public class EmoticonImage
    {
        public string Name { get; set; }
        public string FilePath { get; set; }
        public CoordinatesModel Coordinates { get; set; }
    }

    public class ChatMessageViewModel : IEquatable<ChatMessageViewModel>
    {
        private const string DefaultEmoticonsLinkFormat = "https://mixer.com/_latest/assets/emoticons/{0}.png";

        private static readonly Regex EmoteRegex = new Regex(":\\w+");
        private static readonly string BannedWordRegexFormat = "(^|\\s){0}(\\s|$)";

        public Guid ID { get; private set; }

        public UserViewModel User { get; private set; }

        public string Message { get; private set; }

        public string TargetUsername { get; private set; }

        public DateTimeOffset Timestamp { get; private set; }

        public bool ContainsLink { get; private set; }

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
                this.User = new UserViewModel(this.ChatMessageEvent);
            }
            
            this.TargetUsername = this.ChatMessageEvent.target;
            this.Timestamp = DateTimeOffset.Now;
            this.Message = string.Empty;
            foreach (ChatMessageDataModel message in this.ChatMessageEvent.message.message)
            {
                switch (message.type)
                {
                    case "emoticon":
                        // Special code here to process emoticons
                        //string imageLink = null;
                        //if (message.source.Equals("external") && Uri.IsWellFormedUriString(message.pack, UriKind.Absolute))
                        //{
                        //    imageLink = message.pack;
                        //}
                        //else if (message.source.Equals("builtin"))
                        //{
                        //    imageLink = string.Format(ChatMessageViewModel.DefaultEmoticonsLinkFormat, message.pack);
                        //}

                        //if (!string.IsNullOrEmpty(imageLink))
                        //{
                        //    string imageFilePath = Path.GetTempFileName();
                        //    using (WebClient client = new WebClient())
                        //    {
                        //        client.DownloadFile(new Uri(imageLink), imageFilePath);
                        //    }

                        //    EmoticonImage emoticonImage = new EmoticonImage() { Name = message.text, FilePath = imageFilePath, Coordinates = message.coords };
                        //}

                        this.Message += message.text;

                        break;
                    case "link":
                        this.ContainsLink = true;
                        this.Message += message.text;
                        break;
                    case "text":
                    case "tag":
                    default:
                        this.Message += message.text;
                        break;
                }
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

        public bool ShouldBeModerated(out string reason)
        {
            string lower = this.Message.ToLower();
            foreach (string word in ChannelSession.Settings.BannedWords)
            {
                if (Regex.IsMatch(lower, string.Format(BannedWordRegexFormat, word)))
                {
                    reason = "Banned Word";
                    return true;
                }
            }

            if (ChannelSession.Settings.CapsBlockCount > 0 && this.Message.Count(c => char.IsUpper(c)) >= ChannelSession.Settings.CapsBlockCount)
            {
                reason = "Too Many Caps";
                return true;
            }

            if (ChannelSession.Settings.SymbolEmoteBlockCount > 0)
            {
                MatchCollection matches = EmoteRegex.Matches(lower);
                if (matches.Count >= ChannelSession.Settings.SymbolEmoteBlockCount || lower.Count(c => char.IsSymbol(c) || char.IsPunctuation(c)) >= ChannelSession.Settings.SymbolEmoteBlockCount)
                {
                    reason = "Too Many Symbols/Emotes";
                    return true;
                }
            }

            if (ChannelSession.Settings.BlockLinks && this.ContainsLink)
            {
                reason = "No Links";
                return true;
            }

            reason = "";
            return false;
        }

        public void AddToMessage(string text)
        {
            this.Message += text;
        }

        public bool Equals(ChatMessageViewModel other) { return this.ID.Equals(other.ID); }

        public override int GetHashCode() { return this.ID.GetHashCode(); }

        public override string ToString() { return string.Format("{0}: {1}", this.User, this.Message); }
    }
}
