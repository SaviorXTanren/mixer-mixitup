namespace MixItUp.Base.Model.Twitch.Clients.Chat
{
    /// <summary>
    /// Information about a chat user notice packet.
    /// </summary>
    public class ChatUserNoticePacketModel : ChatUserPacketModelBase
    {
        /// <summary>
        /// The ID of the command for a chat user notice.
        /// </summary>
        public const string CommandID = "USERNOTICE";

        /// <summary>
        /// The user’s ID.
        /// </summary>
        public long UserID { get; set; }

        /// <summary>
        /// The name of the user who sent the notice.
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// The display name of the user who sent the notice.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Indicates whether the user is a moderator.
        /// </summary>
        public bool Moderator { get; set; }

        /// <summary>
        /// A unique ID for the message.
        /// </summary>
        public string MessageID { get; set; }

        /// <summary>
        /// The type of notice (not the ID). Valid values: sub, resub, subgift, anonsubgift, submysterygift, giftpaidupgrade, rewardgift, anongiftpaidupgrade, raid, unraid, ritual, bitsbadgetier.
        /// </summary>
        public string MessageTypeID { get; set; }

        /// <summary>
        /// The message printed in chat along with this notice.
        /// </summary>
        public string SystemMessage { get; set; }

        /// <summary>
        /// The channel ID.
        /// </summary>
        public string RoomID { get; set; }

        /// <summary>
        /// Information to replace text in the message with emote images. This can be empty. Syntax:
        /// &lt;emote ID&gt;:&lt;first index&gt;-&lt;last index&gt;,&lt;another first index&gt;-&lt;another last index&gt;/&lt;another emote ID&gt;:&lt;first index&gt;-&lt;last index&gt;...
        ///     emote ID – The number to use in this URL: http://static-cdn.jtvnw.net/emoticons/v1/:&lt;emote ID&gt;/:&lt;size&gt; (size is 1.0, 2.0 or 3.0.)
        ///     first index, last index – Character indexes. \001ACTION does not count. Indexing starts from the first character that is part of the user’s actual message.See the example (normal message) below.
        /// </summary>
        public string Emotes { get; set; }

        /// <summary>
        /// (Sent only on sub, resub) The total number of months the user has subscribed. This is the same as msg-param-months but sent for different types of user notices.
        /// </summary>
        public int SubCumulativeMonths { get; set; }

        /// <summary>
        /// (Sent only on sub, resub, subgift, anonsubgift) The type of subscription plan being used.
        /// 
        /// Valid values: Prime, 1000, 2000, 3000. 1000, 2000, and 3000 refer to the first, second, and third levels of paid subscriptions, respectively (currently $4.99, $9.99, and $24.99).
        /// </summary>
        public string SubPlan { get; set; }

        /// <summary>
        /// (Sent only on sub, resub, subgift, anonsubgift) The display name of the subscription plan. This may be a default name or one created by the channel owner.
        /// </summary>
        public string SubPlanDisplayName { get; set; }

        /// <summary>
        /// (Sent only on sub, resub) Boolean indicating whether users want their streaks to be shared.
        /// </summary>
        public bool SubShareStreakMonths { get; set; }

        /// <summary>
        /// (Sent only on sub, resub) The number of consecutive months the user has subscribed. This is 0 if msg-param-should-share-streak is 0.
        /// </summary>
        public int SubStreakMonths { get; set; }

        /// <summary>
        /// (Sent only on giftpaidupgrade) The login of the user who gifted the subscription.
        /// </summary>
        public string SubGiftSenderLogin { get; set; }

        /// <summary>
        /// (Sent only on giftpaidupgrade) The display name of the user who gifted the subscription.
        /// </summary>
        public string SubGiftSenderDisplayName { get; set; }

        /// <summary>
        /// (Sent only on subgift, anonsubgift) The total number of months the user has subscribed. This is the same as msg-param-cumulative-months but sent for different types of user notices.
        /// </summary>
        public int SubGiftMonths { get; set; }

        /// <summary>
        /// Sent for extendsub. Indiciates the month number to which the sub was extended.
        /// </summary>
        public int SubBenefitEndMonth { get; set; }

        /// <summary>
        /// (Sent only on subgift, anonsubgift) The user ID of the subscription gift recipient.
        /// </summary>
        public string SubGiftRecipientID { get; set; }

        /// <summary>
        /// (Sent only on subgift, anonsubgift) The user name of the subscription gift recipient.
        /// </summary>
        public string SubGiftRecipientLogin { get; set; }

        /// <summary>
        /// (Sent only on subgift, anonsubgift) The display name of the subscription gift recipient.
        /// </summary>
        public string SubGiftRecipientDisplayName { get; set; }

        /// <summary>
        /// (Sent only on submysterygift) The total number of subscriptions gifted.
        /// </summary>
        public int SubTotalGifted { get; set; }

        /// <summary>
        /// (Sent only on submysterygift) The total number of subscriptions gifted by the sender throughout the lifetime of the channel.
        /// </summary>
        public int SubTotalGiftedLifetime { get; set; }

        /// <summary>
        /// (Sent only on anongiftpaidupgrade, giftpaidupgrade) The subscriptions promo, if any, that is ongoing; e.g. Subtember 2018.
        /// </summary>
        public string SubPromoName { get; set; }

        /// <summary>
        /// (Sent only on anongiftpaidupgrade, giftpaidupgrade) The number of gifts the gifter has given during the promo indicated by msg-param-promo-name.
        /// </summary>
        public int SubPromoTotalGifts { get; set; }

        /// <summary>
        /// (Sent on only raid) The name of the source user raiding this channel.
        /// </summary>
        public string RaidUserLogin { get; set; }

        /// <summary>
        /// (Sent only on raid) The display name of the source user raiding this channel.
        /// </summary>
        public string RaidUserDisplayName { get; set; }

        /// <summary>
        /// (Sent only on raid) The number of viewers watching the source channel raiding this channel.
        /// </summary>
        public int RaidViewerCount { get; set; }

        /// <summary>
        /// (Sent only on bitsbadgetier) The tier of the bits badge the user just earned; e.g. 100, 1000, 10000.
        /// </summary>
        public long BitsTierThreshold { get; set; }

        /// <summary>
        /// (Sent only on ritual) The name of the ritual this notice is for. Valid value: new_chatter.
        /// </summary>
        public string RitualName { get; set; }

        /// <summary>
        /// (Sent only on celebration) The name of the effect shown.
        /// </summary>
        public string CelebrationEffect { get; set; }

        /// <summary>
        /// (Sent only on celebration) The intensity of the celebration.
        /// </summary>
        public string CelebrationIntensity { get; set; }

        /// <summary>
        /// (Sent on CommunityPayForward) Display name of the person who originally gifted the sub.
        /// </summary>
        public string PriorGifterDisplayName { get; set; }

        /// <summary>
        /// (Sent on CommunityPayForward) Username of the person who originally gifted the sub.
        /// </summary>
        public string PriorGifterUserName { get; set; }


        /// <summary>
        /// Timestamp when the server received the message.
        /// </summary>
        public long Timestamp { get; set; }

        /// <summary>
        /// Creates a new instance of the ChatUserNoticePacketModel class.
        /// </summary>
        /// <param name="packet">The Chat packet</param>
        public ChatUserNoticePacketModel(ChatRawPacketModel packet)
            : base(packet)
        {
            this.UserID = packet.GetTagLong("user-id");
            this.Login = packet.GetTagString("login");
            this.DisplayName = packet.GetTagString("display-name");
            this.Moderator = packet.GetTagBool("mod");
            this.MessageID = packet.GetTagString("id");
            this.MessageTypeID = packet.GetTagString("msg-id");
            this.SystemMessage = packet.GetTagString("system-msg");
            this.RoomID = packet.GetTagString("room-id");
            this.Emotes = packet.GetTagString("emotes");
            this.SubCumulativeMonths = packet.GetTagInt("msg-param-cumulative-months");
            this.SubPlan = packet.GetTagString("msg-param-sub-plan");
            this.SubPlanDisplayName = packet.GetTagString("msg-param-sub-plan-name");
            this.SubShareStreakMonths = packet.GetTagBool("msg-param-should-share-streak");
            this.SubStreakMonths = packet.GetTagInt("msg-param-streak-months");
            this.SubGiftSenderLogin = packet.GetTagString("msg-param-sender-login");
            this.SubGiftSenderDisplayName = packet.GetTagString("msg-param-sender-name");
            this.SubGiftMonths = packet.GetTagInt("msg-param-months");
            this.SubGiftRecipientID = packet.GetTagString("msg-param-recipient-id");
            this.SubGiftRecipientLogin = packet.GetTagString("msg-param-recipient-user-name");
            this.SubGiftRecipientDisplayName = packet.GetTagString("msg-param-recipient-display-name");
            this.SubTotalGifted = packet.GetTagInt("msg-param-mass-gift-count");
            this.SubTotalGiftedLifetime = packet.GetTagInt("msg-param-sender-count");
            this.SubPromoName = packet.GetTagString("msg-param-promo-name");
            this.SubPromoTotalGifts = packet.GetTagInt("msg-param-promo-gift-total");
            this.RaidUserLogin = packet.GetTagString("msg-param-login");
            this.RaidUserDisplayName = packet.GetTagString("msg-param-displayName");
            this.RaidViewerCount = packet.GetTagInt("msg-param-viewerCount");
            this.RitualName = packet.GetTagString("msg-param-ritual-name");
            this.BitsTierThreshold = packet.GetTagLong("msg-param-threshold");
            this.SubBenefitEndMonth = packet.GetTagInt("msg-param-sub-benefit-end-month");
            this.CelebrationEffect = packet.GetTagString("msg-param-effect");
            this.CelebrationIntensity = packet.GetTagString("msg-param-intensity");
            this.PriorGifterDisplayName = packet.GetTagString("msg-param-prior-gifter-display-name");
            this.PriorGifterUserName = packet.GetTagString("msg-param-prior-gifter-user-name");
            this.Timestamp = packet.GetTagLong("tmi-sent-ts");
        }
    }
}
