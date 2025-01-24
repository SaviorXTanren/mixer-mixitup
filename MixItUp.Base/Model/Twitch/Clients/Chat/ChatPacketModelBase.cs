using System.Collections.Generic;

namespace MixItUp.Base.Model.Twitch.Clients.Chat
{
    /// <summary>
    /// Information about a base chat packet.
    /// </summary>
    public class ChatPacketModelBase
    {
        /// <summary>
        /// The raw packet information.
        /// </summary>
        public ChatRawPacketModel RawPacket { get; set; }

        /// <summary>
        /// Creates a new instance of the ChatPacketModelBase class.
        /// </summary>
        public ChatPacketModelBase() { }

        /// <summary>
        /// Creates a new instance of the ChatPacketModelBase class.
        /// </summary>
        /// <param name="rawPacket">The raw Chat packet</param>
        public ChatPacketModelBase(ChatRawPacketModel rawPacket)
        {
            this.RawPacket = rawPacket;
        }

        /// <summary>
        /// Creates a dictionary for badge information from a text list.
        /// </summary>
        /// <param name="list">The text list of badge information</param>
        /// <returns>The dictionary of badge information</returns>
        protected Dictionary<string, int> ParseBadgeDictionary(string list)
        {
            Dictionary<string, int> results = new Dictionary<string, int>();
            if (!string.IsNullOrEmpty(list))
            {
                string[] splits = list.Split(new char[] { ',', '/' });
                if (splits != null && splits.Length > 0 && splits.Length % 2 == 0)
                {
                    for (int i = 0; i < splits.Length; i = i + 2)
                    {
                        if (int.TryParse(splits[i + 1], out int version))
                        {
                            results[splits[i]] = version;
                        }
                    }
                }
            }
            return results;
        }
    }
}
