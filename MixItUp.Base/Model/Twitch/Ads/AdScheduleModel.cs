using MixItUp.Base.Util;
using System;

namespace MixItUp.Base.Model.Twitch.Ads
{
    /// <summary>
    /// Information about a channel's ad schedule.
    /// </summary>
    public class AdScheduleModel
    {
        /// <summary>
        /// The number of snoozes available for the broadcaster.
        /// </summary>
        public int snooze_count { get; set; }
        /// <summary>
        /// The UTC timestamp when the broadcaster will gain an additional snooze, in Unix seconds timestamp format. Empty / 0 if the channel has the maximum number of snoozes.
        /// </summary>
        public string snooze_refresh_at { get; set; }
        /// <summary>
        /// The UTC timestamp of the broadcaster’s next scheduled ad, in Unix seconds timestamp format. Empty if the channel has no ad scheduled or is not live.
        /// </summary>
        public string next_ad_at { get; set; }
        /// <summary>
        /// The length in seconds of the scheduled upcoming ad break.
        /// </summary>
        public int duration { get; set; }
        /// <summary>
        /// The UTC timestamp of the broadcaster’s last ad-break, in Unix seconds timestamp format. Empty / 0 if the channel has not run an ad or is not live.
        /// </summary>
        public string last_ad_at { get; set; }
        /// <summary>
        /// The amount of pre-roll free time remaining for the channel in seconds. Returns 0 if they are currently not pre-roll free.
        /// </summary>
        public int preroll_free_time { get; set; }

        public DateTimeOffset NextAdTimestamp()
        {
            if (long.TryParse(this.next_ad_at, out long seconds) && seconds > 0)
            {
                return DateTimeOffsetExtensions.FromUTCUnixTimeSeconds(seconds);
            }
            return DateTimeOffset.MinValue;
        }

        public int NextAdMinutesFromNow()
        {
            DateTimeOffset nextAd = this.NextAdTimestamp();
            if (nextAd != DateTimeOffset.MinValue)
            {
                return (int)(nextAd - DateTimeOffset.Now).TotalMinutes;
            }
            return 0;
        }
    }
}
