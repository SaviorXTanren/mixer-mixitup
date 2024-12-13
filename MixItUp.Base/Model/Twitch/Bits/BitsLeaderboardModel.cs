using System.Collections.Generic;

namespace MixItUp.Base.Model.Twitch.Bits
{
    /// <summary>
    /// The period of time for a Bits leaderboard.
    /// </summary>
    public enum BitsLeaderboardPeriodEnum
    {
        /// <summary>
        /// Day
        /// </summary>
        Day,
        /// <summary>
        /// Week
        /// </summary>
        Week,
        /// <summary>
        /// Month
        /// </summary>
        Month,
        /// <summary>
        /// Year
        /// </summary>
        Year,
        /// <summary>
        /// All
        /// </summary>
        All
    }

    /// <summary>
    /// Information about a specific user on a channel's Bits leaderboard.
    /// </summary>
    public class BitsLeaderboardUserModel
    {
        /// <summary>
        /// The ID of the user.
        /// </summary>
        public string user_id { get; set; }
        /// <summary>
        /// The name of the user.
        /// </summary>
        public string user_name { get; set; }
        /// <summary>
        /// The rank of the user.
        /// </summary>
        public int rank { get; set; }
        /// <summary>
        /// The total bits score of the user.
        /// </summary>
        public long score { get; set; }
    }

    /// <summary>
    /// Information about a channel's Bits leaderboard.
    /// </summary>
    public class BitsLeaderboardModel
    {
        /// <summary>
        /// The date when the leaderboard started.
        /// </summary>
        public string started_at { get; set; }
        /// <summary>
        /// The date when the leaderboard ended.
        /// </summary>
        public string ended_at { get; set; }
        /// <summary>
        /// The users on the leaderboard.
        /// </summary>
        public List<BitsLeaderboardUserModel> users { get; set; }
    }
}
