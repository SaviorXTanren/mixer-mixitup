using Mixer.Base;
using Mixer.Base.Model.OAuth;
using Mixer.Base.Web;
using MixItUp.Base.Model.Spotify;
using MixItUp.Base.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface ISpotifyService
    {
        SpotifyUserProfileModel Profile { get; }

        Task<bool> Connect();

        Task Disconnect();

        Task<SpotifyUserProfileModel> GetCurrentProfile();

        Task<IEnumerable<SpotifySongModel>> SearchSongs(string songName);

        Task<SpotifySongModel> GetSong(string songID);

        Task<SpotifyPlaylistModel> GetPlaylist(SpotifyPlaylistModel playlist);

        Task<IEnumerable<SpotifySongModel>> GetPlaylistSongs(SpotifyPlaylistModel playlist);

        Task<SpotifyCurrentlyPlayingModel> GetCurrentlyPlaying();

        Task PlayCurrentlyPlaying();

        Task PauseCurrentlyPlaying();

        Task NextCurrentlyPlaying();

        Task PreviousCurrentlyPlaying();

        Task<bool> PlaySong(SpotifySongModel song);

        Task<bool> PlaySong(string uri);

        Task SetVolume(int volume);

        OAuthTokenModel GetOAuthTokenCopy();
    }

    public class SpotifyService : OAuthServiceBase, ISpotifyService, IDisposable
    {
        private const string BaseAddress = "https://api.spotify.com/v1/";

        private const string ClientID = "94c9f9c67c864ae9a0f9f8f5bdf3e000";
        private const string StateKey = "V21C2J2RWE51CYSM";
        private const string AuthorizationUrl = "https://accounts.spotify.com/authorize?client_id={0}&redirect_uri=http://localhost:8919/&response_type=code&scope=playlist-read-private+playlist-modify-public+playlist-read-collaborative+user-top-read+user-read-recently-played+user-library-read+user-read-currently-playing+user-modify-playback-state+user-read-playback-state+streaming+user-read-private&state={1}";

        public SpotifyUserProfileModel Profile { get; private set; }

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

        public async Task<SpotifyUserProfileModel> GetCurrentProfile()
        {
            try
            {
                return new SpotifyUserProfileModel(await this.GetJObjectAsync("me"));
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        public async Task<IEnumerable<SpotifySongModel>> SearchSongs(string songName)
        {
            List<SpotifySongModel> songs = new List<SpotifySongModel>();
            try
            {
                JObject result = await this.GetJObjectAsync(string.Format("search?q={0}&type=track", this.EncodeString(songName)));
                if (result != null)
                {
                    JArray results = (JArray)result["tracks"]["items"];
                    foreach (JToken songResult in results)
                    {
                        songs.Add(new SpotifySongModel((JObject)songResult));
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return songs;
        }

        public async Task<SpotifySongModel> GetSong(string songID)
        {
            try
            {
                return new SpotifySongModel(await this.GetJObjectAsync(string.Format("tracks/" + songID)));
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        public async Task<SpotifyPlaylistModel> GetPlaylist(SpotifyPlaylistModel playlist)
        {
            try
            {
                if (playlist != null)
                {
                    return new SpotifyPlaylistModel(await this.GetJObjectAsync(string.Format("playlists/{0}", playlist.ID)));
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        public async Task<IEnumerable<SpotifySongModel>> GetPlaylistSongs(SpotifyPlaylistModel playlist)
        {
            List<SpotifySongModel> results = new List<SpotifySongModel>();
            try
            {
                if (playlist != null)
                {
                    foreach (JObject song in await this.GetPagedResult(string.Format("playlists/{0}/tracks", playlist.ID)))
                    {
                        results.Add(new SpotifySongModel((JObject)song["track"]));
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return results;
        }

        public async Task<SpotifyCurrentlyPlayingModel> GetCurrentlyPlaying()
        {
            try
            {
                return new SpotifyCurrentlyPlayingModel(await this.GetJObjectAsync("me/player"));
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        public async Task PlayCurrentlyPlaying()
        {
            await this.PutAsync("me/player/play");
        }

        public async Task PauseCurrentlyPlaying()
        {
            await this.PutAsync("me/player/pause");
        }

        public async Task NextCurrentlyPlaying()
        {
            await this.PostAsync("me/player/next");
        }

        public async Task PreviousCurrentlyPlaying()
        {
            await this.PostAsync("me/player/previous");
        }

        public async Task<bool> PlaySong(SpotifySongModel song) { return await this.PlaySong(song.Uri); }

        public async Task<bool> PlaySong(string uri)
        {
            JArray songArray = new JArray();
            songArray.Add(uri);
            JObject payload = new JObject();
            payload["uris"] = songArray;

            return await this.PutAsync("me/player/play", this.CreateContentFromObject(payload));
        }

        public async Task SetVolume(int volume)
        {
            await this.PutAsync("me/player/volume?volume_percent=" + volume);
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

        private new async Task<JObject> GetJObjectAsync(string url)
        {
            HttpResponseMessage response = await this.GetAsync(url);
            await this.LogSpotifyDiagnostic(response);
            return await this.ProcessJObjectResponse(response);
        }

        private async Task<bool> PostAsync(string url, HttpContent content = null)
        {
            try
            {
                HttpResponseMessage response = await base.PostAsync(url, content);
                await this.LogSpotifyDiagnostic(response);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return false;
        }

        private new async Task<bool> PutAsync(string url, HttpContent content = null)
        {
            try
            {
                HttpResponseMessage response = await base.PutAsync(url, content);
                await this.LogSpotifyDiagnostic(response);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return false;
        }

        private async Task DisableRepeat()
        {
            try
            {
                await this.PutAsync("me/player/repeat?state=off", null);
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        private async Task LogSpotifyDiagnostic(HttpResponseMessage response)
        {
            Logger.LogDiagnostic(string.Format("Spotify Log: {0} - {1} - {2}", response.RequestMessage.ToString(), response.StatusCode, await response.Content.ReadAsStringAsync()));
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
