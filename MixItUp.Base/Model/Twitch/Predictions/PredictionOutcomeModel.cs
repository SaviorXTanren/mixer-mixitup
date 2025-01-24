using System.Collections.Generic;

namespace MixItUp.Base.Model.Twitch.Predictions
{
    /// <summary>
    /// Information about creating a prediction outcome.
    /// </summary>
    public class CreatePredictionOutcomeModel
    {
        /// <summary>
        /// The title of the choice.
        /// </summary>
        public string title { get; set; }
    }

    /// <summary>
    /// Information about a prediction outcome.
    /// </summary>
    public class PredictionOutcomeModel : CreatePredictionOutcomeModel
    {
        /// <summary>
        /// The ID of the choice.
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// The unique number of users who voted for the choice.
        /// </summary>
        public int users { get; set; }
        /// <summary>
        /// The total number of channel points used for the outcome.
        /// </summary>
        public int channel_points { get; set; }
        /// <summary>
        /// The color of the outcome
        /// </summary>
        public string color { get; set; }
        /// <summary>
        /// The top predictors of the outcome.
        /// </summary>
        public List<PredictionOutcomeTopPredictorModel> top_predictors { get; set; } = new List<PredictionOutcomeTopPredictorModel>();
    }
}
