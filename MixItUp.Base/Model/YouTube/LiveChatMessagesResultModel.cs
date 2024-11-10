using Google.Apis.YouTube.v3.Data;
using System.Collections.Generic;

namespace MixItUp.Base.Model.YouTube
{
    /// <summary>
    /// Information about a live chat messages query.
    /// 
    /// https://developers.google.com/youtube/v3/live/docs/liveChatMessages#resource
    /// </summary>
    public class LiveChatMessagesResultModel
    {
        /// <summary>
        /// The chat messages.
        /// </summary>
        public List<LiveChatMessage> Messages { get; set; } = new List<LiveChatMessage>();

        /// <summary>
        /// The amount of time in milliseconds that the new chat messages query can be made in.
        /// </summary>
        public long PollingInterval { get; set; }

        /// <summary>
        /// Token used for querying the messages that come after these.
        /// </summary>
        public string NextResultsToken { get; set; }

        /// <summary>
        /// Creates a new instance of the LiveChatMessagesResultModel class.
        /// </summary>
        /// <param name="response">The response from a chat message query</param>
        public LiveChatMessagesResultModel(LiveChatMessageListResponse response)
        {
            this.Messages.AddRange(response.Items);
            this.PollingInterval = response.PollingIntervalMillis.GetValueOrDefault();
            this.NextResultsToken = response.NextPageToken;
        }
    }
}
