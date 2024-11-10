using System.Collections.Generic;

namespace MixItUp.Base.Model.Twitch.Clients.Chat
{
    /// <summary>
    /// Information about a chat user state packet.
    /// </summary>
    public class ChatUserStatePacketModel : ChatUserPacketModelBase
    {
        /// <summary>
        /// The ID of the command for a chat user state.
        /// </summary>
        public const string CommandID = "USERSTATE";

        /// <summary>
        /// Indicates whether the user is a moderator.
        /// </summary>
        public bool Moderator { get; set; }

        /// <summary>
        /// Information to the user's emote sets.
        /// </summary>
        public string EmoteSets { get; set; }

        /// <summary>
        /// Creates a new instance of the ChatUserStatePacketModel class.
        /// </summary>
        /// <param name="packet">The Chat packet</param>
        public ChatUserStatePacketModel(ChatRawPacketModel packet)
            : base(packet)
        {
            this.Moderator = packet.GetTagBool("mod");
            this.EmoteSets = packet.GetTagString("emote-sets");
        }

        /// <summary>
        /// A list containing the user's available emote set IDs.
        /// </summary>
        public IEnumerable<string> EmoteSetsDictionary
        {
            get
            {
                HashSet<string> results = new HashSet<string>();
                if (!string.IsNullOrEmpty(this.EmoteSets))
                {
                    string[] splits = this.EmoteSets.Split(new char[] { ',' });
                    if (splits != null)
                    {
                        results = new HashSet<string>(splits);
                        results.Remove("0");
                    }
                }
                return results;
            }
        }
    }
}
