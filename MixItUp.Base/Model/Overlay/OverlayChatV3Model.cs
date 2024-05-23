using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayChatV3Model : OverlayVisualTextV3ModelBase
    {
        public const string MessageIDProperty = "MessageID";
        public const string UsernameProperty = "Username";
        public const string MessageProperty = "Message";

        public const string MessagePartTypeProperty = "Type";
        public const string MessagePartContentProperty = "Content";

        public const string MessagePartTypeTextValue = "Text";
        public const string MessagePartTypeEmoteValue = "Emote";

        public const string FlexStart = "flex-start";
        public const string FlexEnd = "flex-end";

        public static readonly string DefaultHTML = OverlayResources.OverlayChatDefaultHTML;
        public static readonly string DefaultCSS = OverlayResources.OverlayChatDefaultCSS;
        public static readonly string DefaultJavascript = OverlayResources.OverlayChatDefaultJavascript;

        [DataMember]
        public string BackgroundColor { get; set; }
        [DataMember]
        public string BorderColor { get; set; }

        [DataMember]
        public int MessageDelayTime { get; set; }
        [DataMember]
        public int MessageRemovalTime { get; set; }
        [DataMember]
        public bool AddMessagesToTop { get; set; }

        [DataMember]
        public bool IgnoreSpecialtyExcludedUsers { get; set; }
        [DataMember]
        public List<string> UsernamesToIgnore { get; set; } = new List<string>();

        [DataMember]
        public bool ShowPlatformBadge { get; set; }
        [DataMember]
        public bool ShowRoleBadge { get; set; }
        [DataMember]
        public bool ShowSubscriberBadge { get; set; }
        [DataMember]
        public bool ShowSpecialtyBadge { get; set; }

        [DataMember]
        public HashSet<StreamingPlatformTypeEnum> ApplicableStreamingPlatforms { get; set; } = new HashSet<StreamingPlatformTypeEnum>();

        [DataMember]
        public OverlayAnimationV3Model MessageAddedAnimation { get; set; } = new OverlayAnimationV3Model();
        [DataMember]
        public OverlayAnimationV3Model MessageRemovedAnimation { get; set; } = new OverlayAnimationV3Model();

        [JsonIgnore]
        public string FlexAlignment { get { return this.AddMessagesToTop ? FlexStart : FlexEnd; } }

        public OverlayChatV3Model() : base(OverlayItemV3Type.Chat) { }

        public override Dictionary<string, object> GetGenerationProperties()
        {
            Dictionary<string, object> properties = base.GetGenerationProperties();

            properties[nameof(this.BackgroundColor)] = this.BackgroundColor;
            properties[nameof(this.BorderColor)] = this.BorderColor;

            properties[nameof(this.MessageDelayTime)] = this.MessageDelayTime.ToString();
            properties[nameof(this.MessageRemovalTime)] = this.MessageRemovalTime.ToString();
            properties[nameof(this.AddMessagesToTop)] = this.AddMessagesToTop.ToString().ToLower();
            properties[nameof(this.FlexAlignment)] = this.FlexAlignment;

            properties[nameof(this.ShowPlatformBadge)] = this.ShowPlatformBadge.ToString().ToLower();
            properties[nameof(this.ShowRoleBadge)] = this.ShowRoleBadge.ToString().ToLower();
            properties[nameof(this.ShowSubscriberBadge)] = this.ShowSubscriberBadge.ToString().ToLower();
            properties[nameof(this.ShowSpecialtyBadge)] = this.ShowSpecialtyBadge.ToString().ToLower();

            properties["MessageAddedAnimationFramework"] = this.MessageAddedAnimation.AnimationFramework;
            properties["MessageAddedAnimationName"] = this.MessageAddedAnimation.AnimationName;
            properties["MessageRemovedAnimationFramework"] = this.MessageRemovedAnimation.AnimationFramework;
            properties["MessageRemovedAnimationName"] = this.MessageRemovedAnimation.AnimationName;

            return properties;
        }

        public async Task AddMessage(ChatMessageViewModel message)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties[MessageIDProperty] = message.ID.ToString();
            properties[UserProperty] = JObject.FromObject(message.User);

            List<JObject> messageParts = new List<JObject>();
            foreach (var messagePart in message.MessageParts)
            {
                JObject part = new JObject();
                if (messagePart is string)
                {
                    part[MessagePartTypeProperty] = MessagePartTypeTextValue;
                    part[MessagePartContentProperty] = (string)messagePart;
                }
                else if (messagePart is ChatEmoteViewModelBase)
                {
                    part[MessagePartTypeProperty] = MessagePartTypeEmoteValue;
                    part[MessagePartContentProperty] = ((ChatEmoteViewModelBase)messagePart).ImageURL;
                }
                messageParts.Add(part);
            }
            properties[MessageProperty] = messageParts;

            await this.CallFunction("add", properties);
        }

        protected override async Task WidgetInitializeInternal()
        {
            await base.WidgetInitializeInternal();

            if (this.ApplicableStreamingPlatforms.Count == 0)
            {
                this.ApplicableStreamingPlatforms.AddRange(StreamingPlatforms.SupportedPlatforms);
            }
        }

        protected override async Task WidgetEnableInternal()
        {
            await base.WidgetEnableInternal();

            ChatService.OnChatMessageReceived += ChatService_OnChatMessageReceived;
            ChatService.OnChatMessageDeleted += ChatService_OnChatMessageDeleted;
            ChatService.OnChatUserTimedOut += ChatService_OnChatUserTimedOut;
            ChatService.OnChatUserBanned += ChatService_OnChatUserBanned;
            ChatService.OnChatCleared += ChatService_OnChatCleared;
        }

        protected override async Task WidgetDisableInternal()
        {
            await base.WidgetDisableInternal();

            ChatService.OnChatMessageReceived -= ChatService_OnChatMessageReceived;
            ChatService.OnChatMessageDeleted -= ChatService_OnChatMessageDeleted;
            ChatService.OnChatUserTimedOut -= ChatService_OnChatUserTimedOut;
            ChatService.OnChatUserBanned -= ChatService_OnChatUserBanned;
            ChatService.OnChatCleared -= ChatService_OnChatCleared;
        }

        private async void ChatService_OnChatMessageReceived(object sender, ChatMessageViewModel message)
        {
            if (message.IsWhisper || message.IsDeleted)
            {
                return;
            }

            if (this.IgnoreSpecialtyExcludedUsers && message.User.IsSpecialtyExcluded)
            {
                return;
            }

            if (this.UsernamesToIgnore.Contains(message.User.Username.ToLower()))
            {
                return;
            }

            if (!this.ApplicableStreamingPlatforms.Contains(message.Platform))
            {
                return;
            }

            await this.AddMessage(message);
        }

        private async void ChatService_OnChatMessageDeleted(object sender, string messageID)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties[MessageIDProperty] = messageID;
            await this.CallFunction("remove", properties);
        }

        private async void ChatService_OnChatUserTimedOut(object sender, UserV2ViewModel user)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties[UsernameProperty] = user.Username;
            await this.CallFunction("remove", properties);
        }

        private async void ChatService_OnChatUserBanned(object sender, UserV2ViewModel user)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties[UsernameProperty] = user.Username;
            await this.CallFunction("remove", properties);
        }

        private async void ChatService_OnChatCleared(object sender, EventArgs e)
        {
            await this.CallFunction("clear", new Dictionary<string, object>());
        }
    }
}
