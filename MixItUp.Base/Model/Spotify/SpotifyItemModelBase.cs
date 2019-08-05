using Newtonsoft.Json.Linq;
using System;

namespace MixItUp.Base.Model.Spotify
{
    public abstract class SpotifyItemModelBase : IEquatable<SpotifyItemModelBase>
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }

        public SpotifyItemModelBase() { }

        public SpotifyItemModelBase(JObject data)
        {
            if (data != null)
            {
                this.ID = data["id"].ToString();
                if (data["name"] != null)
                {
                    this.Name = data["name"].ToString();
                }
                else
                {
                    this.Name = data["display_name"].ToString();
                }

                if (data["external_urls"] != null && data["external_urls"]["spotify"] != null)
                {
                    this.Link = data["external_urls"]["spotify"].ToString();
                }
            }
        }

        public override bool Equals(object other)
        {
            if (other is SpotifyItemModelBase)
            {
                return this.Equals((SpotifyItemModelBase)other);
            }
            return false;
        }

        public bool Equals(SpotifyItemModelBase other) { return this.ID.Equals(other.ID); }

        public override int GetHashCode() { return this.ID.GetHashCode(); }
    }
}
