using Newtonsoft.Json.Linq;

namespace MixItUp.Base.Model.Spotify
{
    public class SpotifyArtistModel : SpotifyItemModelBase
    {
        public SpotifyArtistModel() { }

        public SpotifyArtistModel(JObject data) : base(data) { }
    }
}
