using Newtonsoft.Json.Linq;
using System.Linq;

namespace MixItUp.Base.Model.Spotify
{
    public class SpotifySongModel : SpotifyItemModelBase
    {
        public long Duration { get; set; }
        public bool IsLocal { get; set; }
        public bool Explicit { get; set; }

        public string Uri { get; set; }

        public SpotifyArtistModel Artist { get; set; }
        public SpotifyAlbumModel Album { get; set; }

        public SpotifySongModel() { }

        public SpotifySongModel(JObject data)
            : base(data)
        {
            if (data != null)
            {
                this.Duration = long.Parse(data["duration_ms"].ToString());
                if (data["is_local"] != null)
                {
                    this.IsLocal = bool.Parse(data["is_local"].ToString());
                }
                if (data["explicit"] != null)
                {
                    this.Explicit = bool.Parse(data["explicit"].ToString());
                }

                if (data["uri"] != null)
                {
                    this.Uri = data["uri"].ToString();
                }

                if (data["artists"] != null)
                {
                    JArray artists = (JArray)data["artists"];
                    this.Artist = new SpotifyArtistModel((JObject)artists.First());
                }
                if (data["album"] != null)
                {
                    this.Album = new SpotifyAlbumModel((JObject)data["album"]);
                }
            }
        }

        public override string ToString()
        {
            return string.Format("\"{0}\" by {1}", this.Name, this.Artist.Name);
        }
    }
}
