using MixItUp.Base.Model.Twitch.Bits;
using MixItUp.Base.Model.Twitch.User;
using MixItUp.Base.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Twitch.API
{
    /// <summary>
    /// The APIs for Bits-based services.
    /// </summary>
    public class BitsService : NewTwitchAPIServiceBase
    {
        /// <summary>
        /// Creates an instance of the BitsService.
        /// </summary>
        /// <param name="connection">The Twitch connection to use</param>
        public BitsService(TwitchConnection connection) : base(connection) { }

        /// <summary>
        /// Retrieves the list of available Cheermotes, animated emotes to which viewers can assign Bits, to cheer in chat. Cheermotes returned are available throughout Twitch, in all Bits-enabled channels.
        /// </summary>
        /// <param name="channel">Optional channel to include specialized cheermotes for if they exist</param>
        /// <returns>The list of available cheermotes</returns>
        public async Task<IEnumerable<BitsCheermoteModel>> GetCheermotes(UserModel channel = null)
        {
            return await this.GetDataResultAsync<BitsCheermoteModel>("bits/cheermotes" + ((channel != null) ? "?broadcaster_id=" + channel.id : ""));
        }

        /// <summary>
        /// Gets the Bits leaderboard for the current channel.
        /// </summary>
        /// <param name="startedAt">The date when the leaderboard should start</param>
        /// <param name="period">The period to get the leaderboard for</param>
        /// <param name="userID">An optional user to get bits leaderboard data specifically for</param>
        /// <param name="count">The total amount of users to include</param>
        /// <returns>The Bits leaderboard</returns>
        public async Task<BitsLeaderboardModel> GetBitsLeaderboard(DateTimeOffset? startedAt = null, BitsLeaderboardPeriodEnum period = BitsLeaderboardPeriodEnum.All, string userID = null, int count = 10)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (startedAt != null)
            {
                parameters.Add("started_at", startedAt.GetValueOrDefault().ToRFC3339String());
            }
            if (userID != null)
            {
                parameters.Add("user_id", userID);
            }
            parameters.Add("period", period.ToString().ToLower());
            parameters.Add("count", count.ToString());

            string parameterString = string.Join("&", parameters.Select(kvp => kvp.Key + "=" + kvp.Value));
            JObject jobj = await this.GetJObjectAsync("bits/leaderboard?" + parameterString);
            if (jobj != null)
            {
                BitsLeaderboardModel result = new BitsLeaderboardModel();
                result.users = ((JArray)jobj["data"]).ToTypedArray<BitsLeaderboardUserModel>();
                result.started_at = jobj["date_range"]["started_at"].ToString();
                result.ended_at = jobj["date_range"]["ended_at"].ToString();
                return result;
            }
            return null;
        }
    }
}
