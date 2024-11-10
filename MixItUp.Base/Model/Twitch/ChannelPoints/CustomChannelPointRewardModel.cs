using System;
using System.Collections.Generic;
using System.Text;

namespace MixItUp.Base.Model.Twitch.ChannelPoints
{
    /// <summary>
    /// Base information about a custom Channel Point Reward.
    /// </summary>
    public class CustomChannelPointRewardModelBase
    {
        /// <summary>
        /// The title of the reward.
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// The cost of the reward.
        /// </summary>
        public int cost { get; set; }
        /// <summary>
        /// Whether the reward is enabled.
        /// </summary>
        public bool is_enabled { get; set; }
        /// <summary>
        /// The HTML background color of the reward.
        /// </summary>
        public string background_color { get; set; }
        /// <summary>
        /// The text prompt for the user when redeeming the reward.
        /// </summary>
        public string prompt { get; set; }
        /// <summary>
        /// Whether user input is required when redeeming the reward.
        /// </summary>
        public bool is_user_input_required { get; set; }
        /// <summary>
        /// Should redemptions be set to FULFILLED status immediately when redeemed and skip the request queue instead of the normal UNFULFILLED status.
        /// </summary>
        public bool should_redemptions_skip_request_queue { get; set; }
        /// <summary>
        /// Whether the reward is currently paused.
        /// </summary>
        public bool is_paused { get; set; }
    }

    /// <summary>
    /// Updatable information about a custom Channel Point Reward.
    /// </summary>
    public class UpdatableCustomChannelPointRewardModel : CustomChannelPointRewardModelBase
    {
        /// <summary>
        /// Whether a maximum per stream is enabled. Defaults to false.
        /// </summary>
        public bool is_max_per_stream_enabled { get; set; }
        /// <summary>
        /// The maximum number per stream if enabled.
        /// </summary>
        public int max_per_stream { get; set; }
        /// <summary>
        /// Whether a maximum per user per stream is enabled. Defaults to false.
        /// </summary>
        public bool is_max_per_user_per_stream_enabled { get; set; }
        /// <summary>
        /// The maximum number per user per stream if enabled.
        /// </summary>
        public int max_per_user_per_stream { get; set; }
        /// <summary>
        ///	Whether a cooldown is enabled. Defaults to false.
        /// </summary>
        public bool is_global_cooldown_enabled { get; set; }
        /// <summary>
        /// The cooldown in seconds if enabled.
        /// </summary>
        public int global_cooldown_seconds { get; set; }
    }

    /// <summary>
    /// Information about a custom Channel Point Reward.
    /// </summary>
    public class CustomChannelPointRewardModel : CustomChannelPointRewardModelBase
    {
        /// <summary>
        /// ID of the channel the reward is for.
        /// </summary>
        public string broadcaster_name { get; set; }
        /// <summary>
        /// Display name of the channel the reward is for.
        /// </summary>
        public string broadcaster_id { get; set; }
        /// <summary>
        /// The ID of the reward.
        /// </summary>
        public Guid id { get; set; }
        /// <summary>
        /// The custom image of the reward.
        /// </summary>
        public CustomChannelPointRewardImageModel image { get; set; }
        /// <summary>
        /// The maximum per-stream redemption settings for the reward
        /// </summary>
        public CustomChannelPointRewardMaxPerStreamSettingModel max_per_stream_setting { get; set; }
        /// <summary>
        /// The maximum per-user, per-stream redemption settings for the reward.
        /// </summary>
        public CustomChannelPointRewardMaxPerUserPerStreamSettingModel max_per_user_per_stream_setting { get; set; }
        /// <summary>
        /// The global cooldown for the reward.
        /// </summary>
        public CustomChannelPointRewardGlobalCooldownSettingModel global_cooldown_setting { get; set; }
        /// <summary>
        /// Whether the reward currently currently has stock to be redeemed.
        /// </summary>
        public bool is_in_stock { get; set; }
        /// <summary>
        /// The default image of the Channel Point Reward.
        /// </summary>
        public CustomChannelPointRewardImageModel default_image { get; set; }
        /// <summary>
        /// The number of redemptions redeemed during the current live stream. Counts against the max_per_stream_setting limit. Null if the broadcasters stream isn’t live or max_per_stream_setting isn’t enabled.
        /// </summary>
        public int? redemptions_redeemed_current_stream { get; set; }
        /// <summary>
        /// Timestamp of the cooldown expiration. Null if the reward isn’t on cooldown.
        /// </summary>
        public string cooldown_expires_at { get; set; }
    }

    /// <summary>
    /// Information about a custom Channel Point Reward's max per-stream settings.
    /// </summary>
    public class CustomChannelPointRewardMaxPerStreamSettingModel
    {
        /// <summary>
        /// Whether it is enabled.
        /// </summary>
        public bool is_enabled { get; set; }
        /// <summary>
        /// The maximum times per stream this reward can be redeemed.
        /// </summary>
        public int max_per_stream { get; set; }
    }

    /// <summary>
    /// Information about a custom Channel Point Reward's max per-user, per-stream settings.
    /// </summary>
    public class CustomChannelPointRewardMaxPerUserPerStreamSettingModel
    {
        /// <summary>
        /// Whether it is enabled.
        /// </summary>
        public bool is_enabled { get; set; }
        /// <summary>
        /// The maximum times a individual users can redeem this reward per stream.
        /// </summary>
        public int max_per_user_per_stream { get; set; }
    }

    /// <summary>
    /// Information about a custom Channel Point Reward's global cooldown settings.
    /// </summary>
    public class CustomChannelPointRewardGlobalCooldownSettingModel
    {
        /// <summary>
        /// Whether it is enabled.
        /// </summary>
        public bool is_enabled { get; set; }
        /// <summary>
        /// The amount of seconds for the global cooldown.
        /// </summary>
        public int global_cooldown_seconds { get; set; }
    }

    /// <summary>
    /// Information about a custom Channel Point Reward's image.
    /// </summary>
    public class CustomChannelPointRewardImageModel
    {
        /// <summary>
        /// The 1x image URL.
        /// </summary>
        public string url_1x { get; set; }
        /// <summary>
        /// The 2x image URL.
        /// </summary>
        public string url_2x { get; set; }
        /// <summary>
        /// The 4x image URL.
        /// </summary>
        public string url_4x { get; set; }
    }
}
