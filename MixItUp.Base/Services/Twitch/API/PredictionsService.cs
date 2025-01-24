using MixItUp.Base.Model.Twitch;
using MixItUp.Base.Model.Twitch.Predictions;
using MixItUp.Base.Model.Twitch.User;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Twitch.API
{
    /// <summary>
    /// The APIs for Prediction-based services.
    /// </summary>
    public class PredictionsService : NewTwitchAPIServiceBase
    {
        /// <summary>
        /// Creates an instance of the PredictionsService
        /// </summary>
        /// <param name="connection">The Twitch connection to use</param>
        public PredictionsService(TwitchConnection connection) : base(connection) { }

        /// <summary>
        /// Creates a prediction.
        /// </summary>
        /// <param name="prediction">The prediction to create</param>
        /// <returns>The created prediction</returns>
        public async Task<PredictionModel> CreatePrediction(CreatePredictionModel prediction)
        {
            Validator.ValidateVariable(prediction, "prediction");
            return (await this.PostDataResultAsync<PredictionModel>("predictions", AdvancedHttpClient.CreateContentFromObject(prediction)))?.FirstOrDefault();
        }

        /// <summary>
        /// Gets the prediction for the specified broadcaster and poll ID.
        /// </summary>
        /// <param name="broadcaster">The broadcaster to get the prediction for</param>
        /// <param name="id">The ID of the prediction to get</param>
        /// <returns>The prediction</returns>
        public async Task<PredictionModel> GetPrediction(UserModel broadcaster, string id)
        {
            Validator.ValidateVariable(broadcaster, "broadcaster");
            Validator.ValidateString(id, "id");
            return (await this.GetDataResultAsync<PredictionModel>("predictions?broadcaster_id=" + broadcaster.id + "&id=" + id))?.FirstOrDefault();
        }

        /// <summary>
        /// Ends the prediction for the specified broadcaster and poll ID.
        /// </summary>
        /// <param name="broadcaster">The broadcaster to get the prediction for</param>
        /// <param name="id">The ID of the prediction to get</param>
        /// <param name="outcomeID">The ID of the selected outcome</param>
        /// <returns>The prediction</returns>
        public async Task<PredictionModel> EndPrediction(UserModel broadcaster, string id, string outcomeID)
        {
            Validator.ValidateVariable(broadcaster, "broadcaster");
            Validator.ValidateString(id, "id");
            Validator.ValidateString(outcomeID, "outcomeID");

            JObject jobj = new JObject();
            jobj["broadcaster_id"] = broadcaster.id;
            jobj["id"] = id;
            jobj["status"] = "RESOLVED";
            jobj["winning_outcome_id"] = outcomeID;

            return (await this.PatchAsync<NewTwitchAPIDataRestResult<PredictionModel>>("predictions", AdvancedHttpClient.CreateContentFromObject(jobj)))?.data?.FirstOrDefault();
        }
    }
}
