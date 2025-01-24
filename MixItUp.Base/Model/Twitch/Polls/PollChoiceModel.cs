namespace MixItUp.Base.Model.Twitch.Polls
{
    /// <summary>
    /// Information about creating a poll choice.
    /// </summary>
    public class CreatePollChoiceModel
    {
        /// <summary>
        /// The title of the choice.
        /// </summary>
        public string title { get; set; }
    }

    /// <summary>
    /// Information about a poll choice.
    /// </summary>
    public class PollChoiceModel : CreatePollChoiceModel
    {
        /// <summary>
        /// The ID of the choice.
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// The total number of votes for the choice.
        /// </summary>
        public int votes { get; set; }
        /// <summary>
        /// The total number of votes via channel points for the choice.
        /// </summary>
        public int channel_points_votes { get; set; }
        /// <summary>
        /// The total number of votes via bits for the choice.
        /// </summary>
        public int bits_votes { get; set; }
    }
}
