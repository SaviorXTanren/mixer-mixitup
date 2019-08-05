using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace MixItUp.Base.Model.Spotify
{
    public class SpotifyAlbumModel : SpotifyItemModelBase
    {
        public string ImageLink { get; set; }

        public SpotifyAlbumModel() { }

        public SpotifyAlbumModel(JObject data)
            : base(data)
        {
            IEnumerable<JObject> images = data["images"].Children().Select(c => (JObject)c);
            if (images.Count() > 0)
            {
                this.ImageLink = images.OrderByDescending(i => (int)i["width"]).First()["url"].ToString();
            }
        }
    }
}
