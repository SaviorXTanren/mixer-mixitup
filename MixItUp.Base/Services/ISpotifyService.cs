using Mixer.Base.Model.OAuth;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public abstract class SpotifyItemBase : IEquatable<SpotifyItemBase>
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }

        public SpotifyItemBase() { }

        public SpotifyItemBase(JObject data)
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
            if (other is SpotifyItemBase)
            {
                return this.Equals((SpotifyItemBase)other);
            }
            return false;
        }

        public bool Equals(SpotifyItemBase other) { return this.ID.Equals(other.ID); }

        public override int GetHashCode() { return this.ID.GetHashCode(); }
    }

    public class SpotifyUserProfile : SpotifyItemBase
    {
        public bool IsPremium { get; set; }

        public SpotifyUserProfile() { }

        public SpotifyUserProfile(JObject data)
            : base(data)
        {
            if (data["product"] != null)
            {
                this.IsPremium = data["product"].ToString().Equals("premium");
            }
        }
    }

    public class SpotifyArtist : SpotifyItemBase
    {
        public SpotifyArtist() { }

        public SpotifyArtist(JObject data) : base(data) { }
    }

    public class SpotifyAlbum : SpotifyItemBase
    {
        public string ImageLink { get; set; }

        public SpotifyAlbum() { }

        public SpotifyAlbum(JObject data)
            : base(data)
        {
            IEnumerable<JObject> images = data["images"].Children().Select(c => (JObject)c);
            if (images.Count() > 0)
            {
                this.ImageLink = images.OrderByDescending(i => (int)i["width"]).First()["url"].ToString();
            }
        }
    }

    public class SpotifySong : SpotifyItemBase
    {
        public long Duration { get; set; }
        public bool IsLocal { get; set; }
        public bool Explicit { get; set; }

        public string Uri { get; set; }

        public SpotifyArtist Artist { get; set; }
        public SpotifyAlbum Album { get; set; }

        public SpotifySong() { }

        public SpotifySong(JObject data)
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
                    this.Artist = new SpotifyArtist((JObject)artists.First());
                }
                if (data["album"] != null)
                {
                    this.Album = new SpotifyAlbum((JObject)data["album"]);
                }
            }
        }

        public override string ToString()
        {
            return string.Format("\"{0}\" by {1}", this.Name, this.Artist.Name);
        }
    }

    public class SpotifyCurrentlyPlaying : SpotifySong
    {
        public bool IsPlaying { get; set; }
        public long CurrentProgress { get; set; }
        public string ContextUri { get; set; }

        public SpotifyCurrentlyPlaying() { }

        public SpotifyCurrentlyPlaying(JObject data)
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
        }
    }

    public class SpotifyPlaylist : SpotifyItemBase
    {
        public bool IsPublic { get; set; }
        public string Uri { get; set; }

        public SpotifyPlaylist() { }

        public SpotifyPlaylist(JObject data)
            : base(data)
        {
            this.IsPublic = bool.Parse(data["public"].ToString());
            this.Uri = data["uri"].ToString();
        }
    }

    public interface ISpotifyService
    {
        SpotifyUserProfile Profile { get; }

        Task<bool> Connect();

        Task Disconnect();

        Task<SpotifyUserProfile> GetCurrentProfile();

        Task<SpotifyUserProfile> GetProfile(string username);

        Task<IEnumerable<SpotifySong>> SearchSongs(string songName);

        Task<SpotifySong> GetSong(string songID);

        Task<IEnumerable<SpotifyPlaylist>> GetCurrentPlaylists();

        Task<SpotifyPlaylist> GetPlaylist(SpotifyPlaylist playlist);

        Task<IEnumerable<SpotifySong>> GetPlaylistSongs(SpotifyPlaylist playlist);

        Task<SpotifyPlaylist> CreatePlaylist(string name, string description);

        Task<bool> AddSongToPlaylist(SpotifyPlaylist playlist, SpotifySong song);

        Task RemoveSongFromPlaylist(SpotifyPlaylist playlist, SpotifySong song);

        Task RemoveSongsFromPlaylist(SpotifyPlaylist playlist, IEnumerable<SpotifySong> song);

        Task<SpotifyCurrentlyPlaying> GetCurrentlyPlaying();

        Task PlayCurrentlyPlaying();

        Task PauseCurrentlyPlaying();

        Task NextCurrentlyPlaying();

        Task PreviousCurrentlyPlaying();

        Task<bool> PlaySong(SpotifySong song);

        Task<bool> PlaySong(string uri);

        Task<bool> PlayPlaylist(SpotifyPlaylist playlist, bool random = false);

        Task SetVolume(int volume);

        Task<int> GetVolume();

        OAuthTokenModel GetOAuthTokenCopy();
    }
}
