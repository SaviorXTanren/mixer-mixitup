using Newtonsoft.Json;

namespace MixItUp.Base.Model.Twitch.ChannelPoints
{
    /// <summary>
    /// Information about a reward redemption.
    /// </summary>
    public class CustomChannelPointRewardRedemptionModel
    {
        /// <summary>
        /// The unfulfilled status for a reward.
        /// </summary>
        public const string UNFULFILLED_STATUS = "UNFULFILLED";
        /// <summary>
        /// The fulfilled status for a reward.
        /// </summary>
        public const string FULFILLED_STATUS = "FULFILLED";
        /// <summary>
        /// The canceled status for a reward.
        /// </summary>
        public const string CANCELED_STATUS = "CANCELED";

        /// <summary>
        /// The id of the broadcaster that the reward belongs to.
        /// </summary>
        public string broadcaster_name { get; set; }
        /// <summary>
        /// The display name of the broadcaster that the reward belongs to.
        /// </summary>
        public string broadcaster_id { get; set; }
        /// <summary>
        /// The ID of the redemption.
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// The ID of the user that redeemed the reward
        /// </summary>
        public string user_id { get; set; }
        /// <summary>
        /// The display name of the user that redeemed the reward.
        /// </summary>
        public string user_name { get; set; }
        /// <summary>
        /// The user input provided. Empty string if not provided.
        /// </summary>
        public string user_input { get; set; }
        /// <summary>
        /// One of UNFULFILLED, FULFILLED or CANCELED
        /// </summary>
        public string status { get; set; }
        /// <summary>
        /// RFC3339 timestamp of when the reward was redeemed.
        /// </summary>
        public string redeemed_at { get; set; }
        /// <summary>
        /// Basic information about the Custom Reward that was redeemed at the time it was redeemed. { “id”: string, “title”: string, “prompt”: string, “cost”: int, }
        /// </summary>
        public CustomChannelPointRewardModel reward { get; set; }

        /// <summary>
        /// Whether the reward is fulfilled.
        /// </summary>
        [JsonIgnore]
        public bool IsUnfulfilled { get { return string.Equals(this.status, CustomChannelPointRewardRedemptionModel.UNFULFILLED_STATUS, System.StringComparison.InvariantCultureIgnoreCase); } }
        /// <summary>
        /// Whether the reward is fulfilled.
        /// </summary>
        [JsonIgnore]
        public bool IsFulfilled { get { return string.Equals(this.status, CustomChannelPointRewardRedemptionModel.FULFILLED_STATUS, System.StringComparison.InvariantCultureIgnoreCase); } }
        /// <summary>
        /// Whether the reward is fulfilled.
        /// </summary>
        [JsonIgnore]
        public bool IsCanceled { get { return string.Equals(this.status, CustomChannelPointRewardRedemptionModel.CANCELED_STATUS, System.StringComparison.InvariantCultureIgnoreCase); } }
    }
}
