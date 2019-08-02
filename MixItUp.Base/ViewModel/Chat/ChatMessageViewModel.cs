using Mixer.Base.Model.Chat;
using MixItUp.Base.Model;
using MixItUp.Base.Model.Skill;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Chat
{
    public class ChatMessageViewModel : IEquatable<ChatMessageViewModel>
    {
        private const string TaggingRegexFormat = "(^|\\s+)@{0}(\\s+|$)";

        public string ID { get; private set; }

        public PlatformTypeEnum Platform { get; private set; }

        public UserViewModel User { get; private set; }

        public string Message { get; private set; }

        public string TargetUsername { get; private set; }

        public DateTimeOffset Timestamp { get; private set; } = DateTimeOffset.Now;

        public bool ContainsLink { get; private set; }

        public bool IsInUsersChannel { get; private set; }

        public bool IsAlert { get; private set; }

        public Dictionary<string, string> Images { get; set; } = new Dictionary<string, string>();

        public ChatSkillModel ChatSkill { get; private set; }

        public SkillInstanceModel Skill { get; private set; }

        public bool IsDeleted { get; set; }

        public string DeletedBy { get; set; }

        public string ModerationReason { get; set; }

        public ChatMessageEventModel ChatMessageEvent { get; private set; }

        public List<ChatMessageDataModel> MessageComponents = new List<ChatMessageDataModel>();

        public ChatMessageViewModel(ChatMessageEventModel chatMessageEvent, UserViewModel user = null)
            : this(chatMessageEvent.message, user)
        {
            this.ChatMessageEvent = chatMessageEvent;
            this.ID = this.ChatMessageEvent.id.ToString();
            this.Platform = PlatformTypeEnum.Mixer;

            this.User = (user != null) ? user : new UserViewModel(this.ChatMessageEvent);
            this.IsInUsersChannel = ChannelSession.Channel.id.Equals(this.ChatMessageEvent.channel);
            this.TargetUsername = this.ChatMessageEvent.target;

            if (this.ChatMessageEvent.message.ContainsSkill)
            {
                this.ChatSkill = this.ChatMessageEvent.message.Skill;
            }
        }

        public ChatMessageViewModel(ChatMessageContentsModel chatMessageContents, UserViewModel user = null)
        {
            this.User = user;

            this.Message = string.Empty;
            this.SetMessageContents(chatMessageContents);
        }

        public ChatMessageViewModel(string alertText, UserViewModel user = null, string foregroundBrush = null)
        {
            this.User = user;
            this.IsInUsersChannel = true;
            this.IsAlert = true;
            this.Message = "---  " + alertText + "  ---";
            this.MessageComponents.Add(new ChatMessageDataModel() { type = "text", text = this.Message });

            string color = ColorSchemes.GetColorCode(foregroundBrush);
            this.AlertMessageBrush = (!string.IsNullOrEmpty(color)) ? color : "#000000";
        }

        public ChatMessageViewModel(SkillInstanceModel skill, UserViewModel user)
        {
            this.User = user;
            this.IsAlert = true;
            this.IsInUsersChannel = true;
            this.Message = "---  \"" + skill.Skill.name + "\" Skill Used  ---";
            this.Skill = skill;
        }

        public string AlertMessageBrush { get; private set; }

        public bool IsWhisper { get { return !string.IsNullOrEmpty(this.TargetUsername); } }

        public bool IsUserTagged { get { return Regex.IsMatch(this.Message, string.Format(TaggingRegexFormat, ChannelSession.User.username)); } }

        public bool ContainsImage { get { return this.Images.Count > 0; } }

        public bool IsChatSkill { get { return this.ChatSkill != null; } }

        public bool IsSkill { get { return this.Skill != null; } }

        public async Task<string> ShouldBeModerated()
        {
            if (this.IsWhisper)
            {
                return string.Empty;
            }

            if ((this.IsSkill || this.IsChatSkill) && string.IsNullOrEmpty(this.Message))
            {
                return string.Empty;
            }

            return await ModerationHelper.ShouldBeModerated(this.User, this.Message, this.ContainsLink);
        }

        public bool IsStreamerOrBot()
        {
            return this.User.ID.Equals(ChannelSession.User.id) || (ChannelSession.BotUser != null && this.User.ID.Equals(ChannelSession.BotUser.id));
        }

        public void AddToMessage(string text)
        {
            this.Message += text;
        }

        public bool ContainsOnlyEmotes()
        {
            if (this.MessageComponents.Count > 0)
            {
                return this.MessageComponents.All(m => m.type.Equals("emoticon") || (m.type.Equals("text") && string.IsNullOrWhiteSpace(m.text)));
            }
            return true;
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
            if (this.IsAlert)
            {
                return this.Message;
            }
            else if (this.IsWhisper)
            {
                return string.Format("{0} -> {1}: {2}", this.User, this.TargetUsername, this.Message);
            }
            else
            {
                return string.Format("{0}: {1}", this.User, this.Message);
            }
        }

        private void SetMessageContents(ChatMessageContentsModel chatMessageContents)
        {
            foreach (ChatMessageDataModel message in chatMessageContents.message)
            {
                this.MessageComponents.Add(message);
                switch (message.type)
                {
                    case "emoticon":
                        // Special code here to process emoticons
                        ChannelSession.EnsureEmoticonForMessage(message);
                        this.Message += message.text;
                        break;
                    case "link":
                        this.ContainsLink = true;
                        this.Message += message.text;
                        break;
                    case "image":
                        this.Images[message.text] = message.url;
                        break;
                    case "text":
                    case "tag":
                    default:
                        this.Message += message.text;
                        break;
                }
            }
            this.Message = this.Message.Trim().Replace(Environment.NewLine, string.Empty).Replace("\n", string.Empty);
        }
    }
}
