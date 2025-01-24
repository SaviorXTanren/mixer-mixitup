using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public static Dictionary<string, object> GetMessageProperties(ChatMessageViewModel message)
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
                    part[MessagePartContentProperty] = ((ChatEmoteViewModelBase)messagePart).OverlayAnimatedOrStaticImageURL;
                }
                messageParts.Add(part);
            }
            properties[MessageProperty] = messageParts;

            return properties;
        }

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
        public bool HideExclamationMessages { get; set; }
        [DataMember]
        public bool DisplayAlejoPronouns { get; set; }

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

        public override async Task Initialize()
        {
            await base.Initialize();

            if (this.ApplicableStreamingPlatforms.Count == 0)
            {
                this.ApplicableStreamingPlatforms.AddRange(StreamingPlatforms.SupportedPlatforms);
            }

            this.RemoveEventHandlers();

            ChatService.OnChatMessageReceived += ChatService_OnChatMessageReceived;
            ChatService.OnChatMessageDeleted += ChatService_OnChatMessageDeleted;
            ChatService.OnChatUserTimedOut += ChatService_OnChatUserTimedOut;
            ChatService.OnChatUserBanned += ChatService_OnChatUserBanned;
            ChatService.OnChatCleared += ChatService_OnChatCleared;
        }

        public override async Task Uninitialize()
        {
            await base.Uninitialize();

            this.RemoveEventHandlers();
        }

        public override Dictionary<string, object> GetGenerationProperties()
        {
            Dictionary<string, object> properties = base.GetGenerationProperties();

            properties[nameof(this.BackgroundColor)] = this.BackgroundColor;
            properties[nameof(this.BorderColor)] = this.BorderColor;

            properties[nameof(this.MessageDelayTime)] = this.MessageDelayTime.ToString();
            properties[nameof(this.MessageRemovalTime)] = this.MessageRemovalTime.ToString();
            properties[nameof(this.AddMessagesToTop)] = this.AddMessagesToTop.ToString().ToLower();
            properties[nameof(this.DisplayAlejoPronouns)] = this.DisplayAlejoPronouns.ToString().ToLower();
            properties[nameof(this.FlexAlignment)] = this.FlexAlignment;

            properties[nameof(this.ShowPlatformBadge)] = this.ShowPlatformBadge.ToString().ToLower();
            properties[nameof(this.ShowRoleBadge)] = this.ShowRoleBadge.ToString().ToLower();
            properties[nameof(this.ShowSubscriberBadge)] = this.ShowSubscriberBadge.ToString().ToLower();
            properties[nameof(this.ShowSpecialtyBadge)] = this.ShowSpecialtyBadge.ToString().ToLower();

            this.MessageAddedAnimation.AddAnimationProperties(properties, nameof(this.MessageAddedAnimation));
            this.MessageRemovedAnimation.AddAnimationProperties(properties, nameof(this.MessageRemovedAnimation));

            return properties;
        }

        public async Task AddMessage(ChatMessageViewModel message)
        {
            await this.CallFunction("add", OverlayChatV3Model.GetMessageProperties(message));
        }

        private async void ChatService_OnChatMessageReceived(object sender, ChatMessageViewModel message)
        {
            if (message.IsWhisper || message.IsDeleted || string.IsNullOrWhiteSpace(message.PlainTextMessage))
            {
                return;
            }

            if (this.HideExclamationMessages && message.PlainTextMessage.StartsWith("!"))
            {
                return;
            }

            if (this.IgnoreSpecialtyExcludedUsers && message.User.IsSpecialtyExcluded)
            {
                return;
            }

            if (this.UsernamesToIgnore.Any(u => string.Equals(u, message.User.Username, StringComparison.OrdinalIgnoreCase)))
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
            properties[UserProperty] = user;
            await this.CallFunction("remove", properties);
        }

        private async void ChatService_OnChatUserBanned(object sender, UserV2ViewModel user)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties[UsernameProperty] = user.Username;
            properties[UserProperty] = user;
            await this.CallFunction("remove", properties);
        }

        private async void ChatService_OnChatCleared(object sender, EventArgs e)
        {
            await this.CallFunction("clear", new Dictionary<string, object>());
        }

        private void RemoveEventHandlers()
        {
            ChatService.OnChatMessageReceived -= ChatService_OnChatMessageReceived;
            ChatService.OnChatMessageDeleted -= ChatService_OnChatMessageDeleted;
            ChatService.OnChatUserTimedOut -= ChatService_OnChatUserTimedOut;
            ChatService.OnChatUserBanned -= ChatService_OnChatUserBanned;
            ChatService.OnChatCleared -= ChatService_OnChatCleared;
        }
    }
}
