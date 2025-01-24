using System;
using System.Collections.Generic;
using System.Linq;

namespace MixItUp.Base.Model.Twitch.Clients.Chat
{
    /// <summary>
    /// Information about a user list in chat.
    /// </summary>
    public class ChatUsersListPacketModel : ChatPacketModelBase
    {
        /// <summary>
        /// The command ID of the reply to the NAMES command.
        /// </summary>
        public const string CommandID = "353";

        /// <summary>
        /// The list of user logins.
        /// </summary>
        public List<string> UserLogins { get; set; }

        /// <summary>
        /// Creates a new instance of the ChatUsersListPacketModel class.
        /// </summary>
        /// <param name="packet">The Chat packet</param>
        public ChatUsersListPacketModel(ChatRawPacketModel packet)
            : base(packet)
        {
            this.UserLogins = packet.Parameters.Last().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }
    }
}
