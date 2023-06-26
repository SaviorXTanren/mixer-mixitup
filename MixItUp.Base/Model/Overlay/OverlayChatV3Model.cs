using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.Chat;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayChatV3Model : OverlayListV3ModelBase
    {
        [DataMember]
        public int DelayMessageTiming { get; set; }

        [DataMember]
        public int HideMessageTiming { get; set; }

        [DataMember]
        public bool ShowMaxMessagesOnly { get; set; }

        [DataMember]
        public bool ShowLatestMessagesAtTop { get; set; }

        [DataMember]
        public bool DontAnimateEmotes { get; set; }

        [DataMember]
        public bool HideBadgesAndRoles { get; set; }

        [DataMember]
        public bool HideAvatar { get; set; }

        [DataMember]
        public bool HideCommandMessages { get; set; }

        [DataMember]
        public bool HideSpecialtyExcludedUsers { get; set; }

        [DataMember]
        public HashSet<string> ExcludedUsernames { get; set; } = new HashSet<string>();

        public OverlayChatV3Model(int delayMessageTiming, int hideMessageTiming, bool showMaxMessagesOnly, bool showLatestMessagesAtTop, bool dontAnimateEmotes, bool hideBadgesAndRoles,
            bool hideAvatar, bool hideCommandMessages, bool hideSpecialtyExcludedUsers, HashSet<string> excludedUsernames)
            : base(OverlayItemV3Type.Chat)
        {
            this.DelayMessageTiming = delayMessageTiming;
            this.HideMessageTiming = hideMessageTiming;
            this.ShowMaxMessagesOnly = showMaxMessagesOnly;
            this.ShowLatestMessagesAtTop = showLatestMessagesAtTop;
            this.DontAnimateEmotes = dontAnimateEmotes;
            this.HideBadgesAndRoles = hideBadgesAndRoles;
            this.HideAvatar = hideAvatar;
            this.HideCommandMessages = hideCommandMessages;
            this.HideSpecialtyExcludedUsers = hideSpecialtyExcludedUsers;
            this.ExcludedUsernames = excludedUsernames;
        }

        [Obsolete]
        public OverlayChatV3Model() : base(OverlayItemV3Type.Chat) { }

        public override async Task Enable()
        {
            ChatService.OnChatMessageReceived += ChatService_OnChatMessageReceived;
            ChatService.OnChatMessageDeleted += ChatService_OnChatMessageDeleted;
            ChatService.OnChatCleared += ChatService_OnChatCleared;

            await base.Enable();
        }

        public override async Task Disable()
        {
            ChatService.OnChatMessageReceived -= ChatService_OnChatMessageReceived;
            ChatService.OnChatMessageDeleted -= ChatService_OnChatMessageDeleted;
            ChatService.OnChatCleared -= ChatService_OnChatCleared;

            await base.Disable();
        }

        public override async Task Test()
        {
            for (int i = 0; i < 5; i++)
            {
                ChatMessageViewModel message = new ChatMessageViewModel(Guid.NewGuid().ToString(), ChannelSession.Settings.DefaultStreamingPlatform, ChannelSession.User);
                message.AddStringMessagePart("This is test message #" + i);
                await this.SendMessage(message);
                await Task.Delay(1000);
            }
        }

        protected override async Task<OverlayOutputV3Model> GetProcessedItem(OverlayOutputV3Model item, CommandParametersModel parameters)
        {
            item = await base.GetProcessedItem(item, parameters);

            item.HTML = ReplaceProperty(item.HTML, nameof(this.ShowMaxMessagesOnly), this.ShowMaxMessagesOnly.ToString());
            item.CSS = ReplaceProperty(item.CSS, nameof(this.ShowMaxMessagesOnly), this.ShowMaxMessagesOnly.ToString());
            item.Javascript = ReplaceProperty(item.Javascript, nameof(this.ShowMaxMessagesOnly), this.ShowMaxMessagesOnly.ToString());

            if (this.ShowMaxMessagesOnly)
            {
                item.HTML = ReplaceProperty(item.HTML, "FullHeight", string.Empty);
                item.CSS = ReplaceProperty(item.CSS, "FullHeight", string.Empty);
                item.Javascript = ReplaceProperty(item.Javascript, "FullHeight", string.Empty);

                item.HTML = ReplaceProperty(item.HTML, "IndividualHeight", $"height: {this.ItemHeight}px;");
                item.CSS = ReplaceProperty(item.CSS, "IndividualHeight", $"height: {this.ItemHeight}px;");
                item.Javascript = ReplaceProperty(item.Javascript, "IndividualHeight", $"height: {this.ItemHeight}px;");
            }
            else
            {
                item.HTML = ReplaceProperty(item.HTML, "FullHeight", $"height: {this.Height}px;");
                item.CSS = ReplaceProperty(item.CSS, "FullHeight", $"height: {this.Height}px;");
                item.Javascript = ReplaceProperty(item.Javascript, "FullHeight", $"height: {this.Height}px;");

                item.HTML = ReplaceProperty(item.HTML, "IndividualHeight", string.Empty);
                item.CSS = ReplaceProperty(item.CSS, "IndividualHeight", string.Empty);
                item.Javascript = ReplaceProperty(item.Javascript, "IndividualHeight", string.Empty);
            }

            return item;
        }

        private async void ChatService_OnChatMessageReceived(object sender, ChatMessageViewModel message)
        {
            if (!message.IsWhisper && !message.IsDeleted)
            {
                await this.SendMessage(message);
            }
        }

        private async void ChatService_OnChatMessageDeleted(object sender, string messageID)
        {
            await this.Update("ChatDelete", new Dictionary<string, string>()
            {
                { "MessageID", messageID }
            },
            new CommandParametersModel());
        }

        private async void ChatService_OnChatCleared(object sender, EventArgs e)
        {
            await this.Update("ChatClear", new Dictionary<string, string>() { }, new CommandParametersModel());
        }

        private async Task SendMessage(ChatMessageViewModel message)
        {
            await this.Update("ChatAdd", new Dictionary<string, string>()
            {
                { "MessageID", message.ID },
                { nameof(message.Platform), message.Platform.ToString() },
                { nameof(message.MessageParts), JSONSerializerHelper.SerializeToString(message.MessageParts) }
            },
            new CommandParametersModel(message.User));
        }
    }
}
