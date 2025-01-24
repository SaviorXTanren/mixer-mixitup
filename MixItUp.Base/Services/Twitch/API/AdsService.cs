using MixItUp.Base.Model.Twitch.Ads;
using MixItUp.Base.Model.Twitch.User;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Twitch.API
{
    /// <summary>
    /// The APIs for Ads-based services.
    /// </summary>
    public class AdsService : NewTwitchAPIServiceBase
    {
        /// <summary>
        /// Creates an instance of the AdsService.
        /// </summary>
        /// <param name="connection">The Twitch connection to use</param>
        public AdsService(TwitchConnection connection) : base(connection) { }

        /// <summary>
        /// Runs an add for the specified channel.
        /// </summary>
        /// <param name="channel">The channel to run the add on</param>
        /// <param name="length">Desired length of the commercial in seconds. Valid options are 30, 60, 90, 120, 150, 180.</param>
        /// <returns></returns>
        public async Task<AdResponseModel> RunAd(UserModel channel, int length)
        {
            Validator.ValidateVariable(channel, "channel");
            Validator.Validate(length > 0, "length");

            JObject body = new JObject();
            body["broadcaster_id"] = channel.id;
            body["length"] = length;

            JObject result = await this.PostAsync<JObject>("channels/commercial", AdvancedHttpClient.CreateContentFromObject(body));
            if (result != null && result.ContainsKey("data"))
            {
                JArray array = (JArray)result["data"];
                IEnumerable<AdResponseModel> adResult = array.ToTypedArray<AdResponseModel>();
                if (adResult != null)
                {
                    return adResult.FirstOrDefault();
                }
            }
            return null;
        }

        /// <summary>
        /// If available, pushes back the timestamp of the upcoming automatic mid-roll ad by 5 minutes. This endpoint duplicates the snooze functionality in the creator dashboard’s Ads Manager.
        /// </summary>
        /// <param name="channel">The channel to attempt to snooze ads for</param>
        /// <returns>Information about the channel’s snoozes and next upcoming ad after successfully snoozing.</returns>
        public async Task<AdSnoozeResponseModel> SnoozeNextAd(UserModel channel)
        {
            Validator.ValidateVariable(channel, "channel");

            IEnumerable<AdSnoozeResponseModel> result = await this.PostDataResultAsync<AdSnoozeResponseModel>("channels/ads/schedule/snooze?broadcaster_id=" + channel.id);
            if (result != null && result.Count() > 0)
            {
                return result.FirstOrDefault();
            }
            return null;
        }

        /// <summary>
        /// This endpoint returns ad schedule related information, including snooze, when the last ad was run, when the next ad is scheduled, and if the channel is currently in pre-roll free time.
        /// 
        /// Note that a new ad cannot be run until 8 minutes after running a previous ad.
        /// </summary>
        /// <param name="channel">The channel to get the ad schedule for</param>
        /// <returns>Information related to the channel’s ad schedule.</returns>
        public async Task<AdScheduleModel> GetAdSchedule(UserModel channel)
        {
            Validator.ValidateVariable(channel, "channel");

            IEnumerable<AdScheduleModel> result = await this.GetDataResultAsync<AdScheduleModel>("channels/ads?broadcaster_id=" + channel.id);
            if (result != null && result.Count() > 0)
            {
                return result.FirstOrDefault();
            }
            return null;
        }
    }
}
