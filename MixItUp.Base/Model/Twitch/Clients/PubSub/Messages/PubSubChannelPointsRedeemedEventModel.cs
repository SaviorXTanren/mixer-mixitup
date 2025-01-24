using MixItUp.Base.Model.Twitch.User;
using Newtonsoft.Json.Linq;

namespace MixItUp.Base.Model.Twitch.Clients.PubSub.Messages
{
    /// <summary>
    /// Information about a channel points redemption event.
    /// </summary>
    public class PubSubChannelPointsRedemptionEventModel
    {
        /// <summary>
        /// The time when the event occurred.
        /// </summary>
        public string timestamp { get; set; }
        /// <summary>
        /// The redemption event data.
        /// </summary>
        public PubSubChannelPointsRedeemedEventModel redemption { get; set; }
    }

    /// <summary>
    /// Information about channel points redeemed.
    /// </summary>
    public class PubSubChannelPointsRedeemedEventModel
    {
        /// <summary>
        /// The ID of the redemption.
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// The user that performed the redemption.
        /// </summary>
        public UserModel user { get; set; }
        /// <summary>
        /// The ID of the channel the redemption was performed in.
        /// </summary>
        public string channel_id { get; set; }
        /// <summary>
        /// The time the redemption occurred.
        /// </summary>
        public string redeemed_at { get; set; }
        /// <summary>
        /// User text input for the redemption, if any.
        /// </summary>
        public string user_input { get; set; }
        /// <summary>
        /// The status of the redemption.
        /// </summary>
        public string status { get; set; }
        /// <summary>
        /// The reward redeemed.
        /// </summary>
        public PubSubChannelPointsRedeemedRewardEventModel reward { get; set; }
    }

    /// <summary>
    /// Information about the reward redeemed with channel points.
    /// </summary>
    public class PubSubChannelPointsRedeemedRewardEventModel
    {
        /// <summary>
        /// The ID of the reward.
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// The ID of the channel the reward was redeemed in.
        /// </summary>
        public string channel_id { get; set; }
        /// <summary>
        /// The title of the reward.
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// The text prompt of the reward.
        /// </summary>
        public string prompt { get; set; }
        /// <summary>
        /// The cost of the reward.
        /// </summary>
        public int cost { get; set; }
        /// <summary>
        /// Whether user input is required for the reward.
        /// </summary>
        public bool is_user_input_required { get; set; }
        /// <summary>
        /// Whether the reward is for subscribers only.
        /// </summary>
        public bool is_sub_only { get; set; }
        /// <summary>
        /// Image data for the reward
        /// </summary>
        public JObject image { get; set; }
        /// <summary>
        /// Default image data for the reward.
        /// </summary>
        public JObject default_image { get; set; }
        /// <summary>
        /// The background color of the reward.
        /// </summary>
        public string background_color { get; set; }
        /// <summary>
        /// Whether the reward is enabled.
        /// </summary>
        public bool is_enabled { get; set; }
        /// <summary>
        /// Whether the reward is paused.
        /// </summary>
        public bool is_paused { get; set; }
        /// <summary>
        /// Whether the reward is currently in stock.
        /// </summary>
        public bool is_in_stock { get; set; }
        /// <summary>
        /// Information about the max usage of the reward per stream.
        /// </summary>
        public JObject max_per_stream { get; set; }
        /// <summary>
        /// Whether redemptions should skip request queue.
        /// </summary>
        public bool should_redemptions_skip_request_queue { get; set; }
    }
}
