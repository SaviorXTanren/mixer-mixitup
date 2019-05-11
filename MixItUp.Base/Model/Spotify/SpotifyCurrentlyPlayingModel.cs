using Newtonsoft.Json.Linq;

namespace MixItUp.Base.Model.Spotify
{
    public class SpotifyCurrentlyPlayingModel : SpotifySongModel
    {
        public bool IsPlaying { get; set; }
        public long CurrentProgress { get; set; }
        public string ContextUri { get; set; }
        public int Volume { get; set; }

        public SpotifyCurrentlyPlayingModel() { }

        public SpotifyCurrentlyPlayingModel(JObject data)
            : base((data["item"] != null && data["item"] is JObject) ? (JObject)data["item"] : null)
        {
            if (data["item"] != null && data["item"] is JObject && data["is_playing"] != null)
            {
                this.IsPlaying = bool.Parse(data["is_playing"].ToString());
            }

            if (data["progress_ms"] != null)
            {
                this.CurrentProgress = long.Parse(data["progress_ms"].ToString());
            }

            if (data["context"] != null && data["context"] is JObject && data["context"]["uri"] != null)
            {
                this.ContextUri = data["context"]["uri"].ToString();
            }

            if (data.ContainsKey("device"))
            {
                var device = (JObject)data["device"];
                if (device.ContainsKey("volume_percent"))
                {
                    this.Volume = (int)device["volume_percent"];
                }
            }
        }
    }
}
