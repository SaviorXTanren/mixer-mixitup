using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.Chat;
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

            await base.Enable();
        }

        public override async Task Disable()
        {
            ChatService.OnChatMessageReceived -= ChatService_OnChatMessageReceived;
            ChatService.OnChatMessageDeleted -= ChatService_OnChatMessageDeleted;

            await base.Disable();
        }

        protected override async Task TestInternal()
        {
            for (int i = 0; i < 5; i++)
            {
                ChatMessageViewModel message = new ChatMessageViewModel(Guid.NewGuid().ToString(), ChannelSession.Settings.DefaultStreamingPlatform, ChannelSession.User);
                message.AddStringMessagePart("This is test message #" + i);
                this.ChatService_OnChatMessageReceived(this, message);
                await Task.Delay(1000);
            }
        }

        protected override async Task<OverlayOutputV3Model> GetProcessedItem(OverlayOutputV3Model item, OverlayEndpointService overlayEndpointService, CommandParametersModel parameters)
        {
            item = await base.GetProcessedItem(item, overlayEndpointService, parameters);

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

        private void ChatService_OnChatMessageReceived(object sender, ChatMessageViewModel message)
        {

        }

        private void ChatService_OnChatMessageDeleted(object sender, string messageID)
        {

        }
    }
}
