namespace MixItUp.Base.Model.Twitch.Predictions
{
    /// <summary>
    /// Information about a top predictor for a prediction outcome.
    /// </summary>
    public class PredictionOutcomeTopPredictorModel
    {
        /// <summary>
        /// The ID of the user
        /// </summary>
        public string user_id { get; set; }
        /// <summary>
        /// The display name of the user.
        /// </summary>
        public string user_login { get; set; }
        /// <summary>
        /// The login of the user.
        /// </summary>
        public string user_name { get; set; }
        /// <summary>
        /// The number of channel points bet.
        /// </summary>
        public int channel_points_used { get; set; }
        /// <summary>
        /// The number of channel points won.
        /// </summary>
        public int channel_points_won { get; set; }
    }
}
