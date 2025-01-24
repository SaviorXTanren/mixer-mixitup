using System.Collections.Generic;

namespace MixItUp.Base.Model.Twitch.Predictions
{
    /// <summary>
    /// Information about creating a prediction.
    /// </summary>
    public class CreatePredictionModel
    {
        /// <summary>
        /// The ID of the broadcaster for the prediction.
        /// </summary>
        public string broadcaster_id { get; set; }
        /// <summary>
        /// The title of the prediction.
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// The duration of the prediction guessing in seconds.
        /// </summary>
        public int prediction_window { get; set; }
        /// <summary>
        /// The outcomes of the prediction.
        /// </summary>
        public List<CreatePredictionOutcomeModel> outcomes { get; set; } = new List<CreatePredictionOutcomeModel>();
    }

    /// <summary>
    /// Information about a prediction.
    /// </summary>
    public class PredictionModel : CreatePredictionModel
    {
        /// <summary>
        /// The ID of the prediction.
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// The display name of the broadcaster for the prediction.
        /// </summary>
        public string broadcaster_name { get; set; }
        /// <summary>
        /// The login of the broadcaster for the prediction.
        /// </summary>
        public string broadcaster_login { get; set; }
        /// <summary>
        /// The status of the prediction:
        /// - ACTIVE: Poll is currently in progress.
        /// - RESOLVED: A winning outcome has been chosen and the Channel Points have been distributed to the users who guessed the correct outcome.
        /// - CANCELED: The Prediction has been canceled and the Channel Points have been refunded to participants.
        /// - LOCKED: The Prediction has been locked and viewers can no longer make predictions.
        /// </summary>
        public string status { get; set; }
        /// <summary>
        /// The ID of the winning prediction outcome.
        /// </summary>
        public string winning_outcome_id { get; set; }
        /// <summary>
        /// The UTC created date time of the prediction.
        /// </summary>
        public string created_at { get; set; }
        /// <summary>
        /// The UTC locked date time of the prediction.
        /// </summary>
        public string locked_at { get; set; }
        /// <summary>
        /// The UTC end date time of the prediction.
        /// </summary>
        public string ended_at { get; set; }
        /// <summary>
        /// The outcomes of the prediction.
        /// </summary>
        public new List<PredictionOutcomeModel> outcomes { get; set; } = new List<PredictionOutcomeModel>();
    }
}
