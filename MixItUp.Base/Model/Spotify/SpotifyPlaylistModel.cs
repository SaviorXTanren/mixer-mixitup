using Newtonsoft.Json.Linq;

namespace MixItUp.Base.Model.Spotify
{
    public class SpotifyPlaylistModel : SpotifyItemModelBase
    {
        public bool IsPublic { get; set; }
        public string Uri { get; set; }

        public SpotifyPlaylistModel() { }

        public SpotifyPlaylistModel(JObject data)
            : base(data)
        {
            this.IsPublic = bool.Parse(data["public"].ToString());
            this.Uri = data["uri"].ToString();
        }
    }
}
