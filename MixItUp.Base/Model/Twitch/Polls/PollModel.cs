using System.Collections.Generic;

namespace MixItUp.Base.Model.Twitch.Polls
{
    /// <summary>
    /// Information about creating a poll.
    /// </summary>
    public class CreatePollModel
    {
        /// <summary>
        /// The ID of the broadcaster for the poll.
        /// </summary>
        public string broadcaster_id { get; set; }
        /// <summary>
        /// The title of the poll.
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// The duration of the poll in seconds.
        /// </summary>
        public int duration { get; set; }
        /// <summary>
        /// Whether voting with bits is enabled.
        /// </summary>
        public bool bits_voting_enabled { get; set; }
        /// <summary>
        /// The number of bits required to vote.
        /// </summary>
        public int bits_per_vote { get; set; }
        /// <summary>
        /// Whether voting with channel points is enabled.
        /// </summary>
        public bool channel_points_voting_enabled { get; set; }
        /// <summary>
        /// The number of channel points required to vote.
        /// </summary>
        public int channel_points_per_vote { get; set; }
        /// <summary>
        /// The choices of the poll.
        /// </summary>
        public List<CreatePollChoiceModel> choices { get; set; } = new List<CreatePollChoiceModel>();
    }

    /// <summary>
    /// Information about a poll
    /// </summary>
    public class PollModel : CreatePollModel
    {
        /// <summary>
        /// The ID of the poll.
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// The display name of the broadcaster for the poll.
        /// </summary>
        public string broadcaster_name { get; set; }
        /// <summary>
        /// The login of the broadcaster for the poll.
        /// </summary>
        public string broadcaster_login { get; set; }
        /// <summary>
        /// The status of the poll:
        /// - ACTIVE: Poll is currently in progress.
        /// - COMPLETED: Poll has reached its ended_at time.
        /// - TERMINATED: Poll has been manually terminated before its ended_at time.
        /// - MODERATED: Poll is no longer visible to any user on Twitch.
        /// - INVALID: Something went wrong determining the state.
        /// </summary>
        public string status { get; set; }
        /// <summary>
        /// The UTC start date time of the poll.
        /// </summary>
        public string started_at { get; set; }
        /// <summary>
        /// The choices of the poll.
        /// </summary>
        public new List<PollChoiceModel> choices { get; set; } = new List<PollChoiceModel>();
    }
}
