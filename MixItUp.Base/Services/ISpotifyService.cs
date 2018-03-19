using Mixer.Base.Model.OAuth;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public abstract class SpotifyItemBase
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }

        public SpotifyItemBase() { }

        public SpotifyItemBase(JObject data)
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
            this.Link = data["external_urls"]["spotify"].ToString();
        }
    }

    public class SpotifyUserProfile : SpotifyItemBase
    {
        public SpotifyUserProfile() { }

        public SpotifyUserProfile(JObject data) : base(data) { }
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
            this.ImageLink = images.OrderByDescending(i => (int)i["width"]).First()["url"].ToString();
        }
    }

    public class SpotifySong : SpotifyItemBase
    {
        public int Duration { get; set; }
        public bool IsLocal { get; set; }
        public bool Explicit { get; set; }

        public SpotifyArtist Artist { get; set; }
        public SpotifyAlbum Album { get; set; }

        public SpotifySong() { }

        public SpotifySong(JObject data)
            : base(data)
        {
            this.Duration = int.Parse(data["duration_ms"].ToString());
            if (data["is_local"] != null)
            {
                this.IsLocal = bool.Parse(data["is_local"].ToString());
            }
            if (data["explicit"] != null)
            {
                this.Explicit = bool.Parse(data["explicit"].ToString());
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

    public class SpotifyCurrentlyPlaying : SpotifySong
    {
        public bool IsPlaying { get; set; }
        public int CurrentProgress { get; set; }

        public SpotifyCurrentlyPlaying() { }

        public SpotifyCurrentlyPlaying(JObject data)
            : base((JObject)data["item"])
        {
            this.IsPlaying = bool.Parse(data["is_playing"].ToString());
            this.CurrentProgress = int.Parse(data["progress_ms"].ToString());
        }
    }

    public class SpotifyPlaylist : SpotifyItemBase
    {
        public bool IsPublic { get; set; }

        public SpotifyPlaylist() { }

        public SpotifyPlaylist(JObject data)
            : base(data)
        {
            this.IsPublic = bool.Parse(data["public"].ToString());
        }
    }

    public interface ISpotifyService
    {
        Task<bool> Connect();

        Task Disconnect();

        Task<SpotifyUserProfile> GetCurrentProfile();

        Task<SpotifyUserProfile> GetProfile(string username);

        Task<IEnumerable<SpotifySong>> SearchSongs(string songName);

        Task<SpotifySong> GetSong(string songID);

        Task<IEnumerable<SpotifyPlaylist>> GetCurrentPlaylists();

        Task<SpotifyPlaylist> GetPlaylist(string playlistID);

        Task<IEnumerable<SpotifySong>> GetPlaylistSongs(string playlistID);

        Task<SpotifyPlaylist> CreatePlaylist(string name, string description);

        Task AddSongToPlaylist(string songID);

        Task RemoveSongToPlaylist(IEnumerable<string> songID);

        Task RemoveSongToPlaylist(string songID);

        Task<SpotifyCurrentlyPlaying> GetCurrentlyPlaying();

        Task PlayCurrentlyPlaying();

        Task PauseCurrentlyPlaying();

        Task NextCurrentlyPlaying();

        Task PreviousCurrentlyPlaying();

        OAuthTokenModel GetOAuthTokenCopy();
    }
}
