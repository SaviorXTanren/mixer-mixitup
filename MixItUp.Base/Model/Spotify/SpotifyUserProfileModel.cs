using Newtonsoft.Json.Linq;

namespace MixItUp.Base.Model.Spotify
{
    public class SpotifyUserProfileModel : SpotifyItemModelBase
    {
        public bool IsPremium { get; set; }

        public SpotifyUserProfileModel() { }

        public SpotifyUserProfileModel(JObject data)
            : base(data)
        {
            if (data["product"] != null)
            {
                this.IsPremium = data["product"].ToString().Equals("premium");
            }
        }
    }
}