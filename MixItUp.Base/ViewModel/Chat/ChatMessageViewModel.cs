using MixItUp.Base.Model;
using MixItUp.Base.ViewModel.User;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MixItUp.Base.ViewModel.Chat
{
    public class ChatMessageViewModel : ViewModelBase, IEquatable<ChatMessageViewModel>
    {
        private const string TaggingRegexFormat = "(^|\\s+)@{0}(\\s+|$)";

        public string ID { get; private set; }

        public StreamingPlatformTypeEnum Platform { get; private set; }

        public UserViewModel User { get; private set; }

        public List<object> MessageParts { get; protected set; } = new List<object>();

        public string PlainTextMessage { get; protected set; } = string.Empty;

        public string TargetUsername { get; protected set; }

        public bool IsInUsersChannel { get; protected set; } = true;

        public DateTimeOffset Timestamp { get; protected set; } = DateTimeOffset.Now;

        public bool IsDeleted { get; private set; }

        public string DeletedBy { get; private set; }

        public string ModerationReason { get; private set; }

        public ChatMessageViewModel(string id, StreamingPlatformTypeEnum platform, UserViewModel user)
        {
            this.ID = id;
            this.Platform = platform;
            this.User = user;
        }

        public bool IsWhisper { get { return !string.IsNullOrEmpty(this.TargetUsername); } }

        public bool IsUserTagged { get { return Regex.IsMatch(this.PlainTextMessage, string.Format(TaggingRegexFormat, ChannelSession.MixerStreamerUser.username)); } }

        public bool IsStreamerOrBot { get { return this.User != null && this.User.ID.Equals(ChannelSession.MixerStreamerUser.id) || (ChannelSession.MixerBotUser != null && this.User.ID.Equals(ChannelSession.MixerBotUser.id)); } }

        public void Delete(UserViewModel user = null, string reason = null)
        {
            this.IsDeleted = true;
            if (user != null)
            {
                this.DeletedBy = user.UserName;
            }
            this.ModerationReason = reason;

            this.NotifyPropertyChanged("IsDeleted");
            this.NotifyPropertyChanged("DeletedBy");
            this.NotifyPropertyChanged("ModerationReason");
        }

        protected internal virtual void AddStringMessagePart(string str)
        {
            this.MessageParts.Add(str);
            this.PlainTextMessage += str + " ";
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

        public override string ToString()
        {
            if (this.IsWhisper)
            {
                return string.Format("{0} -> {1}: {2}", this.User, this.TargetUsername, this.PlainTextMessage);
            }
            else
            {
                return string.Format("{0}: {1}", this.User, this.PlainTextMessage);
            }
        }
    }
}
