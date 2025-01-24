namespace MixItUp.Base.Model.Twitch.Ads
{
    /// <summary>
    /// Information about the channel’s snoozes and next upcoming ad after successfully snoozing.
    /// </summary>
    public class AdSnoozeResponseModel
    {
        /// <summary>
        /// The number of snoozes available for the broadcaster.
        /// </summary>
        public int snooze_count { get; set; }
        /// <summary>
        /// The UTC timestamp when the broadcaster will gain an additional snooze, in Unix seconds timestamp format.
        /// </summary>
        public string snooze_refresh_at { get; set; }
        /// <summary>
        /// The UTC timestamp of the broadcaster’s next scheduled ad, in Unix seconds timestamp format.
        /// </summary>
        public string next_ad_at { get; set; }
    }
}
