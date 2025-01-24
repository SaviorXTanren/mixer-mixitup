using System;
using System.Collections.Generic;

namespace MixItUp.Base.Model.Twitch.Clients.Chat
{
    /// <summary>
    /// Information about a Chat message packet.
    /// </summary>
    public class ChatMessagePacketModel : ChatPacketModelBase
    {
        /// <summary>
        /// The ID of the command for a chat message.
        /// </summary>
        public const string CommandID = "PRIVMSG";

        /// <summary>
        /// The ID of the message.
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// The message text.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The user's ID.
        /// </summary>
        public string UserID { get; set; }

        /// <summary>
        /// The user's login name.
        /// </summary>
        public string UserLogin { get; set; }

        /// <summary>
        /// The user's display name.
        /// </summary>
        public string UserDisplayName { get; set; }

        /// <summary>
        /// The user's badge information.
        /// </summary>
        public string UserBadgeInfo { get; set; }

        /// <summary>
        /// The user's badges.
        /// </summary>
        public string UserBadges { get; set; }

        /// <summary>
        /// Indicates whether the user is a moderator.
        /// </summary>
        public bool Moderator { get; set; }

        /// <summary>
        /// Hexadecimal RGB color code of the message, if any.
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// Information to replace text in the message with emote images.
        /// </summary>
        public string Emotes { get; set; }

        /// <summary>
        /// The channel ID.
        /// </summary>
        public string RoomID { get; set; }

        /// <summary>
        /// The amount of bits associated with the message, if any.
        /// </summary>
        public string Bits { get; set; }

        /// <summary>
        /// The value of the Hype Chat sent by the user.
        /// </summary>
        public string PinnedChatPaidAmount { get; set; }

        /// <summary>
        /// The canonical value of the Hype Chat sent by the user.
        /// </summary>
        public string PinnedChatPaidCanonicalAmount { get; set; }

        /// <summary>
        /// The ISO 4217 alphabetic currency code the user has sent the Hype Chat in.
        /// </summary>
        public string PinnedChatPaidCurrency { get; set; }

        /// <summary>
        /// Indicates how many decimal points this currency represents partial amounts in. Decimal points start from the right side of the value defined in pinned-chat-paid-amount.
        /// </summary>
        public string PinnedChatPaidExponent { get; set; }

        /// <summary>
        /// A Boolean value that determines if the message sent with the Hype Chat was filled in by the system.
        /// If true (1), the user entered no message and the body message was automatically filled in by the system.
        /// If false (0), the user provided their own message to send with the Hype Chat.
        /// </summary>
        public string PinnedChatPaidIsSystemMessage { get; set; }

        /// <summary>
        /// The level of the Hype Chat, in English. Possible values are: ONE, TWO, THREE, FOUR, FIVE, SIX, SEVEN, EIGHT, NINE, TEN
        /// </summary>
        public string PinnedChatPaidLevel { get; set; }

        /// <summary>
        /// Timestamp when the server received the message.
        /// </summary>
        public string Timestamp { get; set; }

        /// <summary>
        /// Creates a new instance of the ChatMessagePacketModel class.
        /// </summary>
        /// <param name="packet">The Chat packet</param>
        public ChatMessagePacketModel(ChatRawPacketModel packet)
            : base(packet)
        {
            this.ID = packet.GetTagString("id");
            this.Message = packet.Get1SkippedParameterText;

            this.UserID = packet.GetTagString("user-id");
            this.UserLogin = packet.GetUserLogin;
            this.UserDisplayName = packet.GetTagString("display-name");
            this.UserBadgeInfo = packet.GetTagString("badge-info");
            this.UserBadges = packet.GetTagString("badges");
            this.Moderator = packet.GetTagBool("mod");

            this.Color = packet.GetTagString("color");
            this.Emotes = packet.GetTagString("emotes");
            this.RoomID = packet.GetTagString("room-id");

            this.Bits = packet.GetTagString("bits");

            this.PinnedChatPaidAmount = packet.GetTagString("pinned-chat-paid-amount");
            this.PinnedChatPaidCanonicalAmount = packet.GetTagString("pinned-chat-paid-canonical-amount");
            this.PinnedChatPaidCurrency = packet.GetTagString("pinned-chat-paid-currency");
            this.PinnedChatPaidExponent = packet.GetTagString("pinned-chat-paid-exponent");
            this.PinnedChatPaidIsSystemMessage = packet.GetTagString("pinned-chat-paid-is-system-message");
            this.PinnedChatPaidLevel = packet.GetTagString("pinned-chat-paid-level");

            this.Timestamp = packet.GetTagString("tmi-sent-ts");
        }


        /// <summary>
        /// A dictionary containing the user's badges and associated versions.
        /// </summary>
        public Dictionary<string, int> BadgeDictionary { get { return this.ParseBadgeDictionary(this.UserBadges); } }

        /// <summary>
        /// A dictionary containing the user's badges and associated versions.
        /// </summary>
        public Dictionary<string, int> BadgeInfoDictionary { get { return this.ParseBadgeDictionary(this.UserBadgeInfo); } }

        /// <summary>
        /// A dictionary containing the emote IDs used by the user in the message and their location in the message
        /// </summary>
        public Dictionary<string, List<Tuple<int, int>>> EmotesDictionary
        {
            get
            {
                Dictionary<string, List<Tuple<int, int>>> results = new Dictionary<string, List<Tuple<int, int>>>();
                if (!string.IsNullOrEmpty(this.Emotes))
                {
                    string[] splits = this.Emotes.Split(new char[] { '/', ':' });
                    if (splits != null && splits.Length > 0 && splits.Length % 2 == 0)
                    {
                        for (int i = 0; i < splits.Length; i = i + 2)
                        {
                            string setID = splits[i];
                            results[setID] = new List<Tuple<int, int>>();

                            string[] setSplits = splits[i + 1].Split(new char[] { '-', ',' });
                            if (setSplits != null && setSplits.Length > 0 && setSplits.Length % 2 == 0)
                            {
                                for (int j = 0; j < setSplits.Length; j = j + 2)
                                {
                                    if (int.TryParse(setSplits[j], out int start) && int.TryParse(setSplits[j + 1], out int end))
                                    {
                                        results[setID].Add(new Tuple<int, int>(start, end));
                                    }
                                }
                            }
                        }
                    }
                }
                return results;
            }
        }
    }
}
