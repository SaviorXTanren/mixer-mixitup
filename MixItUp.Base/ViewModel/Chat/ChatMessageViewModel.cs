using Mixer.Base.Model.Chat;
using MixItUp.Base.Model.Skill;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Chat
{
    public class ChatMessageViewModel : IEquatable<ChatMessageViewModel>
    {
        public Guid ID { get; private set; }

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

        private ChatMessageViewModel(ChatMessageEventModel chatMessageEvent, UserViewModel user = null)
        {
            this.ChatMessageEvent = chatMessageEvent;
            this.ID = this.ChatMessageEvent.id;

            this.User = (user != null) ? user : new UserViewModel(this.ChatMessageEvent);
            this.IsInUsersChannel = ChannelSession.Channel.id.Equals(this.ChatMessageEvent.channel);

            this.TargetUsername = this.ChatMessageEvent.target;
            this.Message = string.Empty;
        }

        public static ChatMessageViewModel CreateChatMessageViewModel(ChatMessageEventModel chatMessageEvent, UserViewModel user = null)
        {
            ChatMessageViewModel newChatMessageViewModel = new ChatMessageViewModel(chatMessageEvent, user);
            
            foreach (ChatMessageDataModel message in newChatMessageViewModel.ChatMessageEvent.message.message)
            {
                newChatMessageViewModel.MessageComponents.Add(message);
                switch (message.type)
                {
                    case "emoticon":
                        // Special code here to process emoticons
                        ChannelSession.EnsureEmoticonForMessage(message);
                        newChatMessageViewModel.Message += message.text;
                        break;
                    case "link":
                        newChatMessageViewModel.ContainsLink = true;
                        newChatMessageViewModel.Message += message.text;
                        break;
                    case "image":
                        newChatMessageViewModel.Images[message.text] = message.url;
                        newChatMessageViewModel.Message += string.Format(" *{0}* ", message.text);
                        break;
                    case "text":
                    case "tag":
                    default:
                        newChatMessageViewModel.Message += message.text;
                        break;
                }
            }

            if (newChatMessageViewModel.ChatMessageEvent.message.ContainsSkill)
            {
                newChatMessageViewModel.ChatSkill = newChatMessageViewModel.ChatMessageEvent.message.Skill;
            }

            return newChatMessageViewModel;
        }

        public ChatMessageViewModel(string alertText, UserViewModel user = null, string foregroundBrush = null)
        {
            this.User = user;
            this.IsInUsersChannel = true;
            this.IsAlert = true;
            this.Message = "---  " + alertText + "  ---";
            this.AlertMessageBrush = ColorSchemes.GetColorCode(foregroundBrush);
            this.MessageComponents.Add(new ChatMessageDataModel() { type = "text", text = this.Message });
        }

        public ChatMessageViewModel(SkillInstanceModel skill, UserViewModel user)
        {
            this.User = user;
            this.IsInUsersChannel = true;
            this.Message = "---  \"" + skill.Skill.name + "\" Skill Used  ---";
            this.Skill = skill;
        }

        public string AlertMessageBrush { get; private set; }

        public bool IsWhisper { get { return !string.IsNullOrEmpty(this.TargetUsername); } }

        public bool IsUserTagged { get { return this.Message.Contains("@" + ChannelSession.User.username + " "); } }

        public bool ContainsImage { get { return this.Images.Count > 0; } }

        public bool IsChatSkill { get { return this.ChatSkill != null; } }

        public bool IsSkill { get { return this.Skill != null; } }

        public async Task<string> ShouldBeModerated()
        {
            if (this.IsWhisper)
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
    }
}
