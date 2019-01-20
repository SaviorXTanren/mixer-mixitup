using Mixer.Base;
using Mixer.Base.Model.OAuth;
using Mixer.Base.Web;
using MixItUp.Base;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Desktop.Services
{
    public class SpotifyService : OAuthServiceBase, ISpotifyService, IDisposable
    {
        private const string BaseAddress = "https://api.spotify.com/v1/";

        private const string ClientID = "94c9f9c67c864ae9a0f9f8f5bdf3e000";
        private const string StateKey = "V21C2J2RWE51CYSM";
        private const string AuthorizationUrl = "https://accounts.spotify.com/authorize?client_id={0}&redirect_uri=http://localhost:8919/&response_type=code&scope=playlist-read-private+playlist-modify-public+playlist-read-collaborative+user-top-read+user-read-recently-played+user-library-read+user-read-currently-playing+user-modify-playback-state+user-read-playback-state+streaming+user-read-private&state={1}";

        public SpotifyUserProfile Profile { get; private set; }

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public SpotifyService() : base(SpotifyService.BaseAddress) { }

        public SpotifyService(OAuthTokenModel token) : base(SpotifyService.BaseAddress, token) { }

        public async Task<bool> Connect()
        {
            if (this.token != null)
            {
                try
                {
                    await this.RefreshOAuthToken();

                    if (await this.InitializeInternal())
                    {
                        return true;
                    }
                }
                catch (Exception ex) { Logger.Log(ex); }
            }

            string authorizationCode = await this.ConnectViaOAuthRedirect(string.Format(SpotifyService.AuthorizationUrl, SpotifyService.ClientID, SpotifyService.StateKey));
            if (!string.IsNullOrEmpty(authorizationCode))
            {
                var body = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("redirect_uri", MixerConnection.DEFAULT_OAUTH_LOCALHOST_URL),
                    new KeyValuePair<string, string>("code", authorizationCode),
                };
                this.token = await this.GetWWWFormUrlEncodedOAuthToken("https://accounts.spotify.com/api/token", SpotifyService.ClientID, ChannelSession.SecretManager.GetSecret("SpotifySecret"), body);

                if (this.token != null)
                {
                    this.token.authorizationCode = authorizationCode;

                    return await this.InitializeInternal();
                }
            }

            return false;
        }

        public Task Disconnect()
        {
            this.token = null;
            this.cancellationTokenSource.Cancel();
            return Task.FromResult(0);
        }

        public async Task<SpotifyUserProfile> GetCurrentProfile()
        {
            try
            {
                JObject result = await this.GetJObjectAsync("me");
                return new SpotifyUserProfile(result);
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        public async Task<SpotifyUserProfile> GetProfile(string username)
        {
            try
            {
                JObject result = await this.GetJObjectAsync("users/" + username);
                return new SpotifyUserProfile(result);
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        public async Task<IEnumerable<SpotifySong>> SearchSongs(string songName)
        {
            List<SpotifySong> songs = new List<SpotifySong>();
            try
            {
                JObject result = await this.GetJObjectAsync(string.Format("search?q={0}&type=track", this.EncodeString(songName)));
                if (result != null)
                {
                    JArray results = (JArray)result["tracks"]["items"];
                    foreach (JToken songResult in results)
                    {
                        songs.Add(new SpotifySong((JObject)songResult));
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return songs;
        }

        public async Task<SpotifySong> GetSong(string songID)
        {
            try
            {
                JObject result = await this.GetJObjectAsync(string.Format("tracks/" + songID));
                return new SpotifySong(result);
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        public async Task<IEnumerable<SpotifyPlaylist>> GetCurrentPlaylists()
        {
            List<SpotifyPlaylist> playlists = new List<SpotifyPlaylist>();
            try
            {
                foreach (JObject playlist in await this.GetPagedResult("me/playlists"))
                {
                    playlists.Add(new SpotifyPlaylist(playlist));
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return playlists;
        }

        public async Task<SpotifyPlaylist> GetPlaylist(SpotifyPlaylist playlist)
        {
            try
            {
                if (playlist != null)
                {
                    JObject result = await this.GetJObjectAsync(string.Format("playlists/{0}", playlist.ID));
                    return new SpotifyPlaylist(result);
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        public async Task<IEnumerable<SpotifySong>> GetPlaylistSongs(SpotifyPlaylist playlist)
        {
            List<SpotifySong> results = new List<SpotifySong>();
            try
            {
                if (playlist != null)
                {
                    foreach (JObject song in await this.GetPagedResult(string.Format("playlists/{0}/tracks", playlist.ID)))
                    {
                        results.Add(new SpotifySong((JObject)song["track"]));
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return results;
        }

        public async Task<SpotifyPlaylist> CreatePlaylist(string name, string description)
        {
            try
            {
                JObject payload = new JObject();
                payload["name"] = name;
                payload["description"] = description;
                payload["public"] = "true";

                HttpResponseMessage response = await this.PostAsync("playlists", this.CreateContentFromObject(payload));
                string responseString = await response.Content.ReadAsStringAsync();
                JObject jobj = JObject.Parse(responseString);
                return new SpotifyPlaylist(jobj);
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        public async Task<bool> AddSongToPlaylist(SpotifyPlaylist playlist, SpotifySong song)
        {
            try
            {
                if (playlist != null && song != null)
                {
                    HttpResponseMessage response = await this.PostAsync(string.Format("playlists/{0}/tracks?uris=spotify:track:" + song.ID, playlist.ID), null);
                    return (response.StatusCode == HttpStatusCode.Created);
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return false;
        }

        public async Task RemoveSongFromPlaylist(SpotifyPlaylist playlist, SpotifySong song)
        {
            await this.RemoveSongsFromPlaylist(playlist, new List<SpotifySong>() { song });
        }

        public async Task RemoveSongsFromPlaylist(SpotifyPlaylist playlist, IEnumerable<SpotifySong> songs)
        {
            try
            {
                if (playlist != null)
                {
                    for (int i = 0; i < songs.Count(); i += 50)
                    {
                        JArray songsToDeleteArray = new JArray();
                        foreach (SpotifySong songToDelete in songs.Skip(i).Take(50))
                        {
                            JObject songPayload = new JObject();
                            songPayload["uri"] = "spotify:track:" + songToDelete.ID;
                            songsToDeleteArray.Add(songPayload);
                        }

                        JObject payload = new JObject();
                        payload["tracks"] = songsToDeleteArray;

                        using (HttpClientWrapper client = await this.GetHttpClient())
                        {
                            HttpMethod method = new HttpMethod("DELETE");
                            HttpRequestMessage request = new HttpRequestMessage(method, string.Format("playlists/{0}/tracks", playlist.ID))
                            { Content = this.CreateContentFromObject(payload) };
                            await client.SendAsync(request);
                        }
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        public async Task<SpotifyCurrentlyPlaying> GetCurrentlyPlaying()
        {
            try
            {
                HttpResponseMessage response = await this.GetAsync("me/player/currently-playing");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string responseString = await response.Content.ReadAsStringAsync();
                    JObject jobj = JObject.Parse(responseString);
                    return new SpotifyCurrentlyPlaying(jobj);
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        public async Task PlayCurrentlyPlaying()
        {
            try
            {
                await this.PutAsync("me/player/play", null);
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        public async Task PauseCurrentlyPlaying()
        {
            try
            {
                await this.PutAsync("me/player/pause", null);
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        public async Task NextCurrentlyPlaying()
        {
            try
            {
                await this.PostAsync("me/player/next", null);
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        public async Task PreviousCurrentlyPlaying()
        {
            try
            {
                await this.PostAsync("me/player/previous", null);
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        public async Task<bool> PlaySong(SpotifySong song) { return await this.PlaySong(song.Uri); }

        public async Task<bool> PlayPlaylist(SpotifyPlaylist playlist, bool random = false)
        {
            try
            {
                JObject payload = new JObject();
                payload["context_uri"] = playlist.Uri;

                JObject position = new JObject();
                position["position"] = 0;
                payload["offset"] = position;

                if (random)
                {
                    IEnumerable<SpotifySong> playlistSongs = await this.GetPlaylistSongs(playlist);
                    if (playlistSongs != null && playlistSongs.Count() > 0)
                    {
                        position["position"] = RandomHelper.GenerateRandomNumber(playlistSongs.Count());
                    }
                }

                await this.PutAsync("me/player/shuffle?state=true", null);
                await Task.Delay(250);

                HttpResponseMessage playResponse = await this.PutAsync("me/player/play", this.CreateContentFromObject(payload));
                await Task.Delay(250);

                await this.DisableRepeat();

                return (playResponse.StatusCode == HttpStatusCode.NoContent);
            }
            catch (Exception ex) { Logger.Log(ex); }
            return false;
        }

        public async Task<bool> PlaySong(string uri)
        {
            try
            {
                JArray songArray = new JArray();
                songArray.Add(uri);
                JObject payload = new JObject();
                payload["uris"] = songArray;

                HttpResponseMessage response = await this.PutAsync("me/player/play", this.CreateContentFromObject(payload));
                await Task.Delay(250);

                await this.DisableRepeat();

                return (response.StatusCode == HttpStatusCode.NoContent);
            }
            catch (Exception ex) { Logger.Log(ex); }
            return false;
        }

        public async Task SetVolume(int volume)
        {
            try
            {
                await this.PutAsync("me/player/volume?volume_percent=" + volume, null);
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        public async Task<int> GetVolume()
        {
            try
            {
                JObject jobj = await this.GetJObjectAsync("me/player");
                if (jobj != null && jobj.ContainsKey("device"))
                {
                    jobj = (JObject)jobj["device"];
                    if (jobj.ContainsKey("volume_percent"))
                    {
                        return (int)jobj["volume_percent"];
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return -1;
        }

        protected override async Task RefreshOAuthToken()
        {
            if (this.token != null)
            {
                var body = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("refresh_token", this.token.refreshToken),
                };
                OAuthTokenModel token = await this.GetWWWFormUrlEncodedOAuthToken("https://accounts.spotify.com/api/token", SpotifyService.ClientID, ChannelSession.SecretManager.GetSecret("SpotifySecret"), body);
                token.refreshToken = this.token.refreshToken;
                this.token = token;
            }
        }

        private async Task<bool> InitializeInternal()
        {
            this.Profile = await this.GetCurrentProfile();
            if (this.Profile != null)
            {
                await this.DisableRepeat();
                return true;
            }
            return false;
        }

        private async Task<IEnumerable<JObject>> GetPagedResult(string endpointURL)
        {
            List<JObject> results = new List<JObject>();

            int total = 1;
            while (results.Count < total)
            {
                JObject result = await this.GetJObjectAsync(endpointURL + "?offset=" + results.Count);
                if (result == null)
                {
                    break;
                }

                total = int.Parse(result["total"].ToString());

                JArray arrayResults = (JArray)result["items"];
                foreach (JToken arrayResult in arrayResults)
                {
                    results.Add((JObject)arrayResult);
                }
            }

            return results;
        }

        private async Task DisableRepeat()
        {
            try
            {
                await this.PutAsync("me/player/repeat?state=off", null);
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                    this.cancellationTokenSource.Dispose();
                }

                // Free unmanaged resources (unmanaged objects) and override a finalizer below.
                // Set large fields to null.

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
