using MixItUp.Base.Model.Twitch.Games;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Twitch.API
{
    /// <summary>
    /// The APIs for Games-based services.
    /// </summary>
    public class GamesService : NewTwitchAPIServiceBase
    {
        /// <summary>
        /// Creates an instance of the GamesService.
        /// </summary>
        /// <param name="connection">The Twitch connection to use</param>
        public GamesService(TwitchConnection connection) : base(connection) { }

        /// <summary>
        /// Gets the top games.
        /// </summary>
        /// <param name="maxResults">The maximum number of results. Will be either that amount or slightly more</param>
        /// <returns>The games information</returns>
        public async Task<IEnumerable<GameModel>> GetTopGames(int maxResults = 20)
        {
            return await this.GetPagedDataResultAsync<GameModel>("games/top", maxResults);
        }

        /// <summary>
        /// Gets a game by it's ID.
        /// </summary>
        /// <param name="id">The ID of the game</param>
        /// <returns>The game information</returns>
        public async Task<GameModel> GetGameByID(string id)
        {
            Validator.ValidateString(id, "id");
            IEnumerable<GameModel> games = await this.GetDataResultAsync<GameModel>("games?id=" + id);
            return (games != null) ? games.FirstOrDefault() : null;
        }

        /// <summary>
        /// Gets a set of games by their IDs.
        /// </summary>
        /// <param name="ids">The IDs of the gamse</param>
        /// <returns>The set of game information</returns>
        public async Task<IEnumerable<GameModel>> GetGamesByID(IEnumerable<string> ids)
        {
            Validator.ValidateList(ids, "ids");
            return await this.GetDataResultAsync<GameModel>("games?id=" + string.Join("&id=", ids));
        }

        /// <summary>
        /// Gets a set of games by the specified name
        /// </summary>
        /// <param name="name">The name of the game</param>
        /// <returns>The games information</returns>
        public async Task<IEnumerable<GameModel>> GetGamesByName(string name)
        {
            Validator.ValidateString(name, "name");
            return await this.GetDataResultAsync<GameModel>("games?name=" + AdvancedHttpClient.URLEncodeString(name));
        }
    }
}
