using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.Chat.Trovo;
using MixItUp.Base.ViewModel.Chat.YouTube;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public abstract class OverlayEventTrackingV3ModelBase : OverlayVisualTextV3ModelBase
    {
        [DataMember]
        public virtual bool Chatters { get; set; }

        [DataMember]
        public virtual bool ChatMessages { get; set; }

        [DataMember]
        public virtual bool Follows { get; set; }

        [DataMember]
        public virtual bool Raids { get; set; }

        [DataMember]
        public virtual bool TwitchSubscriptions { get; set; }
        [DataMember]
        public virtual bool TwitchBits { get; set; }

        [DataMember]
        public virtual bool YouTubeMemberships { get; set; }
        [DataMember]
        public virtual bool YouTubeSuperChats { get; set; }

        [DataMember]
        public virtual bool TrovoSubscriptions { get; set; }
        [DataMember]
        public virtual bool TrovoElixirSpells { get; set; }

        [DataMember]
        public virtual bool Donations { get; set; }

        public OverlayEventTrackingV3ModelBase(OverlayItemV3Type type) : base(type) { }

        protected override async Task WidgetEnableInternal()
        {
            await base.WidgetEnableInternal();

            this.RemoveEventHandlers();

            ChatService.OnChatUserBanned += OnChatUserBanned;

            if (this.Chatters || this.ChatMessages)
            {
                ChatService.OnChatMessageReceived += OnChatMessageReceived;
            }

            if (this.ChatMessages)
            {
                ChatService.OnChatMessageDeleted += OnChatMessageDeleted;
                ChatService.OnChatCleared += OnChatCleared;
            }

            if (this.Follows)
            {
                EventService.OnFollowOccurred += OnFollow;
            }

            if (this.Raids)
            {
                EventService.OnRaidOccurred += OnRaid;
            }

            if (this.TwitchSubscriptions)
            {
                EventService.OnSubscribeOccurred += OnSubscribe;
                EventService.OnResubscribeOccurred += OnSubscribe;
                EventService.OnSubscriptionGiftedOccurred += OnSubscribe;
                EventService.OnMassSubscriptionsGiftedOccurred += OnMassSubscription;
            }

            if (this.Donations)
            {
                EventService.OnDonationOccurred += OnDonation;
            }

            if (this.TwitchBits)
            {
                EventService.OnTwitchBitsCheeredOccurred += OnTwitchBits;
            }

            if (this.YouTubeSuperChats)
            {
                EventService.OnYouTubeSuperChatOccurred += OnYouTubeSuperChat;
            }

            if (this.TrovoElixirSpells)
            {
                EventService.OnTrovoSpellCastOccurred += EventService_OnTrovoSpellCastOccurred;
            }
        }

        protected override async Task WidgetDisableInternal()
        {
            await base.WidgetDisableInternal();

            this.RemoveEventHandlers();
        }

        protected virtual void OnChatUserBanned(object sender, UserV2ViewModel user) { }

        protected virtual void OnChatMessageReceived(object sender, ChatMessageViewModel message) { }

        protected virtual void OnChatMessageDeleted(object sender, string messageID) { }

        protected virtual void OnChatCleared(object sender, EventArgs e) { }

        protected virtual void OnFollow(object sender, UserV2ViewModel user) { }

        protected virtual void OnRaid(object sender, Tuple<UserV2ViewModel, int> raid) { }

        protected virtual void OnSubscribe(object sender, SubscriptionDetailsModel subscription) { }

        protected virtual void OnMassSubscription(object sender, IEnumerable<SubscriptionDetailsModel> subscriptions) { }

        protected virtual void OnDonation(object sender, UserDonationModel donation) { }

        protected virtual void OnTwitchBits(object sender, TwitchUserBitsCheeredModel bitsCheered) { }

        protected virtual void OnYouTubeSuperChat(object sender, YouTubeSuperChatViewModel superChat) { }

        protected virtual void OnTrovoSpell(object sender, TrovoChatSpellViewModel spell) { }

        private void EventService_OnTrovoSpellCastOccurred(object sender, TrovoChatSpellViewModel spell)
        {
            if (spell.IsElixir)
            {
                this.OnTrovoSpell(sender, spell);
            }
        }

        private void RemoveEventHandlers()
        {
            ChatService.OnChatUserBanned -= OnChatUserBanned;
            ChatService.OnChatMessageReceived -= OnChatMessageReceived;
            ChatService.OnChatMessageDeleted -= OnChatMessageDeleted;
            ChatService.OnChatCleared -= OnChatCleared;
            EventService.OnFollowOccurred -= OnFollow;
            EventService.OnRaidOccurred -= OnRaid;
            EventService.OnSubscribeOccurred -= OnSubscribe;
            EventService.OnResubscribeOccurred -= OnSubscribe;
            EventService.OnSubscriptionGiftedOccurred -= OnSubscribe;
            EventService.OnMassSubscriptionsGiftedOccurred -= OnMassSubscription;
            EventService.OnDonationOccurred -= OnDonation;
            EventService.OnTwitchBitsCheeredOccurred -= OnTwitchBits;
            EventService.OnYouTubeSuperChatOccurred -= OnYouTubeSuperChat;
            EventService.OnTrovoSpellCastOccurred -= EventService_OnTrovoSpellCastOccurred;
        }
    }
}
