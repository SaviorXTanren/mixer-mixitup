using MixItUp.Base.Model.Twitch.Teams;
using MixItUp.Base.Model.Twitch.User;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Twitch.API
{
    /// <summary>
    /// The APIs for Teams-based services.
    /// </summary>
    public class TeamsService : NewTwitchAPIServiceBase
    {
        /// <summary>
        /// Creates an instance of the TeamsService.
        /// </summary>
        /// <param name="connection">The Twitch connection to use</param>
        public TeamsService(TwitchConnection connection) : base(connection) { }

        /// <summary>
        /// Gets all the teams that the specified broadcaster is a part of.
        /// </summary>
        /// <param name="broadcaster">The broadcaster to get teams for</param>
        /// <returns>A list of teams</returns>
        public async Task<IEnumerable<TeamModel>> GetChannelTeams(UserModel broadcaster)
        {
            Validator.ValidateVariable(broadcaster, "broadcaster");
            return await this.GetDataResultAsync<TeamModel>("teams/channel?broadcaster_id=" + broadcaster.id);
        }

        /// <summary>
        /// Gets the details for a specific Team.
        /// </summary>
        /// <param name="id">The ID of the team</param>
        /// <returns>A list of teams</returns>
        public async Task<TeamDetailsModel> GetTeam(string id)
        {
            Validator.ValidateString(id, "id");
            IEnumerable<TeamDetailsModel> results = await this.GetDataResultAsync<TeamDetailsModel>("teams?id=" + id);
            return results.FirstOrDefault();
        }
    }
}
