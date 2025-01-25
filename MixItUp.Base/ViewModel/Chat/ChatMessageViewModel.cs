using MixItUp.Base.Model;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.User;
using MixItUp.Base.ViewModels;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Chat
{
    public class ChatMessageViewModel : ViewModelBase, IEquatable<ChatMessageViewModel>
    {
        private const string TaggingRegexFormat = "(^|\\s+)@{0}(\\s+|$)";

        public string ID { get; private set; }

        public StreamingPlatformTypeEnum Platform { get; private set; }

        public List<object> MessageParts { get; protected set; } = new List<object>();

        public string PlainTextMessage { get; protected set; } = string.Empty;

        public string TargetUsername { get; protected set; }

        public bool IsInUsersChannel { get; protected set; } = true;

        public bool ContainsLink { get; protected set; } = false;

        public DateTimeOffset Timestamp { get; protected set; } = DateTimeOffset.Now;

        public bool IsDeleted
        {
            get { return this.isDeleted; }
            set
            {
                this.isDeleted = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.DeletedInformation));
            }
        }
        private bool isDeleted;

        public string DeletedBy
        {
            get { return this.deletedBy; }
            set
            {
                this.deletedBy = value;
                this.NotifyPropertyChanged();
            }
        }
        private string deletedBy;

        public string ModerationReason
        {
            get { return this.moderationReason; }
            set
            {
                this.moderationReason = value;
                this.NotifyPropertyChanged();
            }
        }
        private string moderationReason;

        public string DeletedInformation
        {
            get
            {
                if (!string.IsNullOrEmpty(this.DeletedBy))
                {
                    if (!string.IsNullOrEmpty(this.ModerationReason))
                    {
                        return $" ({this.ModerationReason} {MixItUp.Base.Resources.By}: {this.DeletedBy})";
                    }
                    else
                    {
                        return $" ({MixItUp.Base.Resources.DeletedBy}: {this.DeletedBy})";
                    }
                }
                else if (!string.IsNullOrEmpty(this.ModerationReason))
                {
                    return $" ({MixItUp.Base.Resources.AutoModerated}: {this.ModerationReason})";
                }
                else
                {
                    return $" ({MixItUp.Base.Resources.ManualDeletion})";
                }
            }
        }

        public UserV2ViewModel User { get; set; }

        public DateTimeOffset ProcessingStartTime { get; set; }

        public ChatMessageViewModel(string id, StreamingPlatformTypeEnum platform, UserV2ViewModel user)
        {
            this.ID = id;
            this.Platform = platform;
            this.User = user;
        }

        public string PlatformImageURL { get { return StreamingPlatforms.GetPlatformImage(this.Platform); } }

        public bool ShowPlatformImage { get { return StreamingPlatforms.GetConnectedPlatformSessions().Count() > 1; } }

        public double ProcessingTime { get { return (DateTimeOffset.Now - this.ProcessingStartTime).TotalMilliseconds; } }

        public bool IsWhisper { get { return !string.IsNullOrEmpty(this.TargetUsername); } }

        public bool IsStreamerTagged { get { return Regex.IsMatch(this.PlainTextMessage.ToLower(), string.Format(TaggingRegexFormat, ChannelSession.User.Username ?? string.Empty)); } }

        public virtual bool IsStreamerOrBot
        {
            get
            {
                if (this.User != null && this.Platform != StreamingPlatformTypeEnum.None)
                {
                    if (StreamingPlatforms.GetPlatformSession(this.Platform).IsConnected && string.Equals(this.User?.PlatformID, StreamingPlatforms.GetPlatformSession(this.Platform)?.StreamerID))
                    {
                        return true;
                    }
                    else if (StreamingPlatforms.GetPlatformSession(this.Platform).IsBotConnected && string.Equals(this.User?.PlatformID, StreamingPlatforms.GetPlatformSession(this.Platform)?.BotID))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool ShowPronouns { get { return ChannelSession.Settings.ShowAlejoPronouns; } }

        public string PronounsDisplay
        {
            get
            {
                if (this.User != null)
                {
                    string pronoun = this.User.AlejoPronoun;
                    if (!string.IsNullOrEmpty(pronoun))
                    {
                        return $"({this.User.AlejoPronoun})";
                    }
                }
                return string.Empty;
            }
        }

        public bool ShowTimestamp { get { return ChannelSession.Settings.ShowChatMessageTimestamps; } }

        public string TimestampDisplay { get { return string.Format("({0})", this.Timestamp.ToString("t")); } }

        public int FontSize { get { return ChannelSession.Settings.ChatFontSize; } }

        public string PrimaryTaggedUsername
        {
            get
            {
                if (this.PlainTextMessage.StartsWith("@"))
                {
                    int endIndex = this.PlainTextMessage.IndexOf(' ');
                    if (endIndex > 0)
                    {
                        return this.PlainTextMessage.Substring(1, endIndex - 1);
                    }
                    return this.PlainTextMessage.Substring(1);
                }
                return null;
            }
        }

        public string TextOnlyMessageContents { get { return string.Join(" ", this.MessageParts.Where(p => p is string)); } }

        public IEnumerable<ChatEmoteViewModelBase> EmotesOnlyContents { get { return this.MessageParts.Where(p => p is ChatEmoteViewModelBase).Select(p => p as ChatEmoteViewModelBase); } }

        public virtual IEnumerable<string> ToArguments() { return CommandParametersModel.GenerateArguments(this.PlainTextMessage); }

        public async Task<bool> CheckForModeration()
        {
            if (this.User != null && !this.IsWhisper)
            {
                if (!ServiceManager.Get<ModerationService>().DoesUserMeetChatInteractiveParticipationRequirement(this.User, this))
                {
                    Logger.Log(LogLevel.Debug, string.Format("Deleting Message As User does not meet requirement - {0} - {1}", ChannelSession.Settings.ModerationChatInteractiveParticipation, this.PlainTextMessage));
                    await this.Delete(reason: MixItUp.Base.Resources.ModerationChatParticipation);
                    await ServiceManager.Get<ModerationService>().SendChatInteractiveParticipationWhisper(this.User, isChat: true);
                    return true;
                }

                string moderationReason = await ServiceManager.Get<ModerationService>().ShouldTextBeModerated(this.User, this.PlainTextMessage, this.ContainsLink);
                if (!string.IsNullOrEmpty(moderationReason))
                {
                    Logger.Log(LogLevel.Debug, string.Format("Moderation Being Performed - {0}", this.ToString()));
                    await this.Delete(reason: moderationReason);
                    return true;
                }
            }
            return false;
        }

        public async Task Delete(UserV2ViewModel moderator = null, string reason = null, bool triggerEventCommand = true)
        {
            try
            {
                if (!this.IsDeleted)
                {
                    if (moderator != null && !string.IsNullOrEmpty(moderator.Username))
                    {
                        this.DeletedBy = moderator.Username;
                    }
                    this.ModerationReason = (!string.IsNullOrEmpty(reason)) ? reason : MixItUp.Base.Resources.ManualDeletion;
                    this.IsDeleted = true;

                    ServiceManager.Get<ChatService>().HandleDeletion(this);

                    if (this.User != null && !string.IsNullOrEmpty(this.PlainTextMessage))
                    {
                        CommandParametersModel parameters = new CommandParametersModel(moderator ?? this.User);
                        parameters.Arguments.Add(this.User.Username);
                        parameters.TargetUser = this.User;
                        parameters.SpecialIdentifiers["message"] = this.PlainTextMessage;
                        parameters.SpecialIdentifiers["reason"] = this.ModerationReason;
                        await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.ChatMessageDeleted, parameters);
                    }

                    await ServiceManager.Get<ChatService>().WriteToChatEventLog(this, $"{MixItUp.Base.Resources.ChatMessageDeleted} - {this.ModerationReason} - {this.DeletedBy}");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        protected internal virtual void AddStringMessagePart(string str)
        {
            this.MessageParts.Add(str);
            if (string.IsNullOrEmpty(this.PlainTextMessage))
            {
                this.PlainTextMessage = str;
            }
            else
            {
                this.PlainTextMessage += " " + str;
            }
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
            if (this.User == null)
            {
                return this.PlainTextMessage;
            }
            else if (this.IsWhisper)
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
