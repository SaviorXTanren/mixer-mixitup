using System.Collections.Generic;

namespace MixItUp.Base.Model.Twitch.Channels
{
    /// <summary>
    /// Information for a channel Hype Train event.
    /// </summary>
    public class ChannelHypeTrainModel
    {
        /// <summary>
        /// The distinct ID of the event
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// Displays hypetrain.{event_name}, currently only hypetrain.progression
        /// </summary>
        public string event_type { get; set; }
        /// <summary>
        /// RFC3339 formatted timestamp of event
        /// </summary>
        public string event_timestamp { get; set; }
        /// <summary>
        /// Returns the version of the endpoint
        /// </summary>
        public string version { get; set; }
        /// <summary>
        /// Event data for the Hype Train.
        /// </summary>
        public ChannelHypeTrainEventDataModel event_data { get; set; }
    }

    /// <summary>
    /// Information about a channel Hype Train event data.
    /// </summary>
    public class ChannelHypeTrainEventDataModel
    {
        /// <summary>
        /// The distinct ID of this Hype Train
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// Channel ID of which Hype Train events the clients are interested in
        /// </summary>
        public string broadcaster_id { get; set; }
        /// <summary>
        /// RFC3339 formatted timestamp of when this Hype Train started
        /// </summary>
        public string started_at { get; set; }
        /// <summary>
        /// RFC3339 formatted timestamp of the expiration time of this Hype Train
        /// </summary>
        public string expires_at { get; set; }
        /// <summary>
        /// RFC3339 formatted timestamp of when another Hype Train can be started again
        /// </summary>
        public string cooldown_end_time { get; set; }
        /// <summary>
        /// The highest level (in the scale of 1-5) reached of the Hype Train
        /// </summary>
        public int level { get; set; }
        /// <summary>
        /// The total score so far towards completing the level goal above
        /// </summary>
        public int total { get; set; }
        /// <summary>
        /// The goal value of the level above
        /// </summary>
        public int goal { get; set; }
        /// <summary>
        /// An array of top contribution objects, one object for each type.  For example, one object would represent top contributor of BITS, by aggregate, and one would represent top contributor of SUBS by count.
        /// </summary>
        public List<ChannelHypeTrainContributorModel> top_contributions { get; set; } = new List<ChannelHypeTrainContributorModel>();
        /// <summary>
        /// An object that represents the most recent contribution
        /// </summary>
        public ChannelHypeTrainContributorModel last_contribution { get; set; }
    }

    /// <summary>
    /// Information about a Channel Hype Train contributor.
    /// </summary>
    public class ChannelHypeTrainContributorModel
    {
        private const string BitsType = "BITS";
        private const string SubsType = "SUBS";

        /// <summary>
        /// ID of the contributing user
        /// </summary>
        public string user { get; set; }
        /// <summary>
        /// Identifies the contribution method, either BITS or SUBS
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// Total aggregated amount of all contributions by the top contributor. If type is BITS, total represents aggregate amount of bits used. If type is SUBS, aggregate total where 500, 1000, or 2500 represent tier 1, 2, or 3 subscriptions respectively.  For example, if top contributor has gifted a tier 1, 2, and 3 subscription, total would be 4000.
        /// </summary>
        public int total { get; set; }

        /// <summary>
        /// Identifies the contribution method is bits.
        /// </summary>
        public bool IsBits { get { return string.Equals(BitsType, this.type); } }
        /// <summary>
        /// Identifies the contribution method is subs.
        /// </summary>
        public bool IsSubs { get { return string.Equals(SubsType, this.type); } }
    }
}