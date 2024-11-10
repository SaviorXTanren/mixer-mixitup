namespace MixItUp.Base.Model.Twitch.Games
{
    /// <summary>
    /// Information about a game.
    /// </summary>
    public class GameModel
    {
        /// <summary>
        /// The ID of the game.
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// The name of the game.
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// The url to the game's box art.
        /// </summary>
        public string box_art_url { get; set; }
        /// <summary>
        /// The ID that IGDB uses to identify this game. If the IGDB ID is not available to Twitch, this field is set to an empty string.
        /// </summary>
        public string igdb_id { get; set; }
    }
}
