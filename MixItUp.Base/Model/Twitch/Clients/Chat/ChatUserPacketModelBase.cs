using System.Collections.Generic;

namespace MixItUp.Base.Model.Twitch.Clients.Chat
{
    /// <summary>
    /// Base class for user-based chat packets.
    /// </summary>
    public class ChatUserPacketModelBase : ChatPacketModelBase
    {
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
        /// Hexadecimal RGB color code of the message, if any.
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// Creates a new instance of the ChatUserStatePacketModel class.
        /// </summary>
        /// <param name="packet">The Chat packet</param>
        public ChatUserPacketModelBase(ChatRawPacketModel packet)
            : base(packet)
        {
            this.UserDisplayName = packet.GetTagString("display-name");
            this.UserBadgeInfo = packet.GetTagString("badge-info");
            this.UserBadges = packet.GetTagString("badges");
            this.Color = packet.GetTagString("color");
        }

        /// <summary>
        /// A dictionary containing the user's badges and associated versions.
        /// </summary>
        public Dictionary<string, int> BadgeDictionary { get { return this.ParseBadgeDictionary(this.UserBadges); } }

        /// <summary>
        /// A dictionary containing the user's badges and associated versions.
        /// </summary>
        public Dictionary<string, int> BadgeInfoDictionary { get { return this.ParseBadgeDictionary(this.UserBadgeInfo); } }
    }
}
