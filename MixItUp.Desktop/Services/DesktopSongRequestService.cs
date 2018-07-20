using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Mixer.Base.Web;
using MixItUp.Base;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;

namespace MixItUp.Desktop.Services
{
    public class SongRequestItemSearch
    {
        public SongRequestItem SongRequest { get; set; }

        public bool MultipleResults { get; set; }

        public SongRequestServiceTypeEnum Type { get; set; }
        public List<string> ErrorMessages { get; set; }

        public SongRequestItemSearch(SongRequestItem songRequest)
        {
            this.SongRequest = songRequest;
            this.Type = this.SongRequest.Type;
        }

        public SongRequestItemSearch(SongRequestServiceTypeEnum type, string errorMessage, bool multipleResults = false) : this(type, new string[] { errorMessage }, multipleResults) { }

        public SongRequestItemSearch(SongRequestServiceTypeEnum type, IEnumerable<string> errorMessages, bool multipleResults = false)
        {
            this.Type = type;
            this.ErrorMessages = new List<string>(errorMessages);
            this.MultipleResults = multipleResults;
        }

        public bool FoundSingleResult { get { return this.SongRequest != null; } }
    }

    public class DesktopSongRequestService : ISongRequestService
    {
        private const string DefaultSongId = "DEFAULT";

        private const string SpotifyLinkPrefix = "https://open.spotify.com/track/";
        private const string SpotifyTrackPrefix = "spotify:track:";
        private const string SpotifyPlaylistRegex = @"spotify:user:\w+:playlist:";

        private const string YouTubeLongLinkPrefix = "www.youtube.com/watch?v=";
        private const string YouTubeShortLinkPrefix = "youtu.be/";
        private const string YouTubeHost = "youtube.com";

        private const string SoundCloudPublicLinkFormat = "https://soundcloud.com/.+/.+";
        private const string SoundCloudHost = "soundcloud.com";

        private static SemaphoreSlim songRequestLock = new SemaphoreSlim(1);

        public bool IsEnabled { get; private set; }

        private CancellationTokenSource backgroundThreadCancellationTokenSource;

        private SongRequestItem currentSong;
        private readonly List<SongRequestItem> allRequests = new List<SongRequestItem>();

        private Dictionary<UserViewModel, List<SpotifySong>> lastUserSpotifySongSearches = new Dictionary<UserViewModel, List<SpotifySong>>();
        private Dictionary<UserViewModel, List<SongRequestItem>> lastUserYouTubeSongSearches = new Dictionary<UserViewModel, List<SongRequestItem>>();

        public DesktopSongRequestService()
        {
        }

        public async Task<bool> Initialize()
        {
            if (ChannelSession.Settings.SongRequestServiceTypes.Count == 0)
            {
                return false;
            }

            if (ChannelSession.Settings.SongRequestServiceTypes.Contains(SongRequestServiceTypeEnum.Spotify) && ChannelSession.Services.Spotify == null)
            {
                return false;
            }

            if (ChannelSession.Settings.SongRequestServiceTypes.Contains(SongRequestServiceTypeEnum.YouTube) && ChannelSession.Services.OverlayServer == null)
            {
                return false;
            }

            await this.ClearAllRequests();

            this.backgroundThreadCancellationTokenSource = new CancellationTokenSource();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () => { await this.PlayerBackground(); }, this.backgroundThreadCancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            this.IsEnabled = true;

            return true;
        }

        public void Disable()
        {
            this.ClearAllRequests();

            if (this.backgroundThreadCancellationTokenSource != null)
            {
                this.backgroundThreadCancellationTokenSource.Cancel();
            }

            this.IsEnabled = false;
        }

        public async Task AddSongRequest(UserViewModel user, SongRequestServiceTypeEnum service, string identifier, bool pickFirst = false)
        {
            if (!this.IsEnabled)
            {
                await ChannelSession.Chat.Whisper(user.UserName, "Song Requests are not currently enabled");
                return;
            }

            if (string.IsNullOrEmpty(identifier))
            {
                await ChannelSession.Chat.Whisper(user.UserName, "You must specify your request with the command. For details on how to request songs, check out: https://github.com/SaviorXTanren/mixer-mixitup/wiki/Song-Requests#requesting-songs");
                return;
            }

            List<Task<SongRequestItemSearch>> allTaskSearches = new List<Task<SongRequestItemSearch>>();
            if (ChannelSession.Settings.SongRequestServiceTypes.Contains(SongRequestServiceTypeEnum.YouTube) && (service == SongRequestServiceTypeEnum.All || service == SongRequestServiceTypeEnum.YouTube))
            {
                allTaskSearches.Add(this.GetYouTubeSongRequest(user, identifier, pickFirst));
            }
            if (ChannelSession.Settings.SongRequestServiceTypes.Contains(SongRequestServiceTypeEnum.SoundCloud) && (service == SongRequestServiceTypeEnum.All || service == SongRequestServiceTypeEnum.SoundCloud))
            {
                allTaskSearches.Add(this.GetSoundCloudSongRequest(user, identifier, pickFirst));
            }
            if (ChannelSession.Settings.SongRequestServiceTypes.Contains(SongRequestServiceTypeEnum.Spotify) && (service == SongRequestServiceTypeEnum.All || service == SongRequestServiceTypeEnum.Spotify))
            {
                allTaskSearches.Add(this.GetSpotifySongRequest(user, identifier, pickFirst));
            }

            List<SongRequestItemSearch> allSearches = new List<SongRequestItemSearch>(await Task.WhenAll(allTaskSearches));

            SongRequestItemSearch requestSearch = null;
            if (service != SongRequestServiceTypeEnum.All)
            {
                if (allSearches.First(s => s.Type == service).FoundSingleResult)
                {
                    requestSearch = allSearches.First(s => s.Type == service);
                }
            }

            if (requestSearch == null)
            {
                foreach (SongRequestItemSearch search in allSearches)
                {
                    if (search.FoundSingleResult)
                    {
                        requestSearch = search;
                        break;
                    }
                }
            }

            if (requestSearch == null)
            {
                foreach (SongRequestItemSearch search in allSearches)
                {
                    if (search.MultipleResults)
                    {
                        requestSearch = search;
                        break;
                    }
                }
            }

            if (requestSearch != null)
            {
                if (requestSearch.FoundSingleResult)
                {
                    await DesktopSongRequestService.songRequestLock.WaitAsync();
                    if (this.currentSong == null)
                    {
                        await this.PlaySong(requestSearch.SongRequest);
                    }
                    else
                    {
                        this.allRequests.Add(requestSearch.SongRequest);
                    }
                    DesktopSongRequestService.songRequestLock.Release();

                    ChannelSession.Services?.Telemetry?.TrackSongRequest(requestSearch.Type);

                    await ChannelSession.Chat.SendMessage(string.Format("{0} was added to the queue.", requestSearch.SongRequest.Name));
                    GlobalEvents.SongRequestsChangedOccurred();

                    return;
                }
                else
                {
                    foreach (string errorMessage in requestSearch.ErrorMessages)
                    {
                        await ChannelSession.Chat.Whisper(user.UserName, errorMessage);
                    }
                    return;
                }
            }

            await ChannelSession.Chat.Whisper(user.UserName, "We were unable to find a song that matched you request. For details on how to request songs, check out: https://github.com/SaviorXTanren/mixer-mixitup/wiki/Song-Requests#requesting-songs");
            return;
        }

        public async Task RemoveSongRequest(SongRequestItem song)
        {
            await DesktopSongRequestService.songRequestLock.WaitAsync();
            this.allRequests.Remove(song);
            DesktopSongRequestService.songRequestLock.Release();

            GlobalEvents.SongRequestsChangedOccurred();
        }

        public async Task RemoveLastSongRequestedByUser(UserViewModel user)
        {
            await DesktopSongRequestService.songRequestLock.WaitAsync();
            SongRequestItem song = this.allRequests.LastOrDefault(s => s.User.ID == user.ID);
            if (song != null)
            {
                this.allRequests.Remove(song);
            }
            DesktopSongRequestService.songRequestLock.Release();

            if (song != null)
            {
                await ChannelSession.Chat.SendMessage(string.Format("{0} was removed from the queue.", song.Name));
                GlobalEvents.SongRequestsChangedOccurred();
            }
            else
            {
                await ChannelSession.Chat.Whisper(user.UserName, string.Format("You have no songs in the queue.", song.Name));
            }
        }

        public async Task PlayPauseCurrentSong()
        {
            await DesktopSongRequestService.songRequestLock.WaitAsync();
            if (this.currentSong != null)
            {
                if (this.currentSong.Type == SongRequestServiceTypeEnum.Spotify)
                {
                    if (ChannelSession.Services.Spotify != null)
                    {
                        SpotifyCurrentlyPlaying currentlyPlaying = await ChannelSession.Services.Spotify.GetCurrentlyPlaying();
                        if (currentlyPlaying != null && currentlyPlaying.ID != null)
                        {
                            if (currentlyPlaying.IsPlaying)
                            {
                                await ChannelSession.Services.Spotify.PauseCurrentlyPlaying();
                            }
                            else
                            {
                                await ChannelSession.Services.Spotify.PlayCurrentlyPlaying();
                            }
                        }
                        else
                        {
                            SpotifySong song = await ChannelSession.Services.Spotify.GetSong(this.currentSong.ID);
                            if (song != null)
                            {
                                await ChannelSession.Services.Spotify.PlaySong(song);
                            }
                        }
                    }
                }
                else if (this.currentSong.Type == SongRequestServiceTypeEnum.YouTube || this.currentSong.Type == SongRequestServiceTypeEnum.SoundCloud)
                {
                    await this.PlayPauseOverlaySong(this.currentSong.Type);
                }
            }
            DesktopSongRequestService.songRequestLock.Release();
        }

        public async Task SkipToNextSong()
        {
            await DesktopSongRequestService.songRequestLock.WaitAsync();

            if (this.currentSong != null)
            {
                if (this.currentSong.ID.StartsWith(DefaultSongId, StringComparison.InvariantCultureIgnoreCase))
                {
                    // Current playlist is default, just go to next song
                    if (this.currentSong.Type == SongRequestServiceTypeEnum.Spotify)
                    {
                        await ChannelSession.Services.Spotify.NextCurrentlyPlaying();
                    }
                    else if (this.currentSong.Type == SongRequestServiceTypeEnum.YouTube || this.currentSong.Type == SongRequestServiceTypeEnum.SoundCloud)
                    {
                        await this.NextOverlay(this.currentSong.Type);
                    }
                }
                else
                {
                    // Otherwise, pause and clear current song so we select a new one
                    if (this.currentSong.Type == SongRequestServiceTypeEnum.Spotify)
                    {
                        await ChannelSession.Services.Spotify.PauseCurrentlyPlaying();
                    }
                    else if (this.currentSong.Type == SongRequestServiceTypeEnum.YouTube || this.currentSong.Type == SongRequestServiceTypeEnum.SoundCloud)
                    {
                        await this.StopOverlaySong(this.currentSong.Type);
                    }

                    this.currentSong = null;
                }
            }

            if (this.currentSong == null)
            {
                SongRequestItem nextSong = this.allRequests.FirstOrDefault();
                if (nextSong != null)
                {
                    this.allRequests.RemoveAt(0);
                }

                if (nextSong == null)
                {
                    await this.PlayDefaultPlaylist();
                }
                else
                {
                    await this.PlaySong(nextSong);
                }
            }

            DesktopSongRequestService.songRequestLock.Release();

            GlobalEvents.SongRequestsChangedOccurred();
        }

        public async Task RefreshVolume()
        {
            if (this.currentSong != null)
            {
                if (this.currentSong.Type == SongRequestServiceTypeEnum.Spotify)
                {
                    await ChannelSession.Services.Spotify.RefreshVolume();
                }
                else if (this.currentSong.Type == SongRequestServiceTypeEnum.YouTube || this.currentSong.Type == SongRequestServiceTypeEnum.SoundCloud)
                {
                    await this.RefreshOverlayVolume(this.currentSong.Type);
                }
            }
        }

        public Task<SongRequestItem> GetCurrentlyPlaying()
        {
            return Task.FromResult(this.currentSong);
        }

        public Task<SongRequestItem> GetNextTrack()
        {
            return Task.FromResult(this.allRequests.FirstOrDefault());
        }

        public Task<IEnumerable<SongRequestItem>> GetAllRequests()
        {
            return Task.FromResult<IEnumerable<SongRequestItem>>(this.allRequests.ToArray());
        }

        public Task ClearAllRequests()
        {
            this.allRequests.Clear();
            GlobalEvents.SongRequestsChangedOccurred();
            return Task.FromResult(0);
        }

        public void OverlaySongFinished() { this.currentSong = null; }

        private async Task PlayerBackground()
        {
            await BackgroundTaskWrapper.RunBackgroundTask(this.backgroundThreadCancellationTokenSource, async (tokenSource) =>
            {
                tokenSource.Token.ThrowIfCancellationRequested();

                bool shouldSkip = false;

                await DesktopSongRequestService.songRequestLock.WaitAsync();

                if (this.currentSong != null)
                {
                    if  (this.currentSong.ID.StartsWith(DefaultSongId, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (this.allRequests.Count > 0 || this.currentSong.ID != $"{DefaultSongId}:{ChannelSession.Settings.DefaultPlaylist}")
                        {
                            // If we are playing the default playlist, but there is a song in the queue
                            // clear this default song and skip to next song
                            if (this.currentSong.Type == SongRequestServiceTypeEnum.Spotify)
                            {
                                await ChannelSession.Services.Spotify.PauseCurrentlyPlaying();
                            }
                            else if (this.currentSong.Type == SongRequestServiceTypeEnum.YouTube || this.currentSong.Type == SongRequestServiceTypeEnum.SoundCloud)
                            {
                                await this.StopOverlaySong(this.currentSong.Type);
                            }
                            this.currentSong = null;
                        }
                    }
                    else if (this.currentSong.Type == SongRequestServiceTypeEnum.Spotify && ChannelSession.Services.Spotify != null)
                    {
                        SpotifyCurrentlyPlaying currentlyPlaying = await ChannelSession.Services.Spotify.GetCurrentlyPlaying();
                        if (currentlyPlaying == null || currentlyPlaying.ID == null || !currentlyPlaying.ID.Equals(this.currentSong.ID) || (!currentlyPlaying.IsPlaying && currentlyPlaying.CurrentProgress == 0))
                        {
                            // Spotify decided to move on to a new song
                            await ChannelSession.Services.Spotify.PauseCurrentlyPlaying();
                            this.currentSong = null;
                        }
                    }
                }

                shouldSkip = this.currentSong == null;

                DesktopSongRequestService.songRequestLock.Release();

                if (shouldSkip)
                {
                    await this.SkipToNextSong();
                }

                await Task.Delay(2000, tokenSource.Token);
            });
        }

        private async Task PlaySong(SongRequestItem request)
        {
            if (request != null)
            {
                this.currentSong = request;
                if (request.Type == SongRequestServiceTypeEnum.Spotify)
                {
                    if (ChannelSession.Services.Spotify != null)
                    {
                        SpotifySong song = await ChannelSession.Services.Spotify.GetSong(request.ID);
                        if (song != null)
                        {
                            await ChannelSession.Services.Spotify.PlaySong(song);
                        }
                    }
                }
                else if (request.Type == SongRequestServiceTypeEnum.YouTube || request.Type == SongRequestServiceTypeEnum.SoundCloud)
                {
                    await this.PlayOverlaySong(request);
                }
            }
        }

        private async Task<SongRequestItemSearch> GetYouTubeSongRequest(UserViewModel user, string identifier, bool pickFirst = false)
        {
            if (this.lastUserYouTubeSongSearches.ContainsKey(user) && int.TryParse(identifier, out int songIndex))
            {
                if (songIndex > 0 && this.lastUserYouTubeSongSearches.ContainsKey(user) && this.lastUserYouTubeSongSearches[user].Count > 0 && songIndex <= this.lastUserYouTubeSongSearches[user].Count)
                {
                    identifier = this.lastUserYouTubeSongSearches[user][songIndex - 1].ID;
                }
                else
                {
                    return new SongRequestItemSearch(SongRequestServiceTypeEnum.YouTube, "The song index you specified was not valid, please try your search again");
                }
            }
            else
            {
                identifier = identifier.Replace("https://", "");
                if (identifier.Contains(YouTubeLongLinkPrefix) || identifier.Contains(YouTubeShortLinkPrefix))
                {
                    identifier = identifier.Replace(YouTubeLongLinkPrefix, "");
                    identifier = identifier.Replace(YouTubeShortLinkPrefix, "");
                    if (identifier.Contains("&"))
                    {
                        identifier = identifier.Substring(0, identifier.IndexOf("&"));
                    }
                }
                else
                {
                    List<SongRequestItemSearch> searchItems = new List<SongRequestItemSearch>();
                    using (HttpClientWrapper client = new HttpClientWrapper("https://www.googleapis.com/"))
                    {
                        HttpResponseMessage response = await client.GetAsync(string.Format("youtube/v3/search?q={0}&maxResults=5&part=snippet&key={1}", HttpUtility.UrlEncode(identifier), ChannelSession.SecretManager.GetSecret("YouTubeKey")));
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            string content = await response.Content.ReadAsStringAsync();
                            JObject jobj = JObject.Parse(content);
                            if (jobj["items"] != null && jobj["items"] is JArray)
                            {
                                JArray items = (JArray)jobj["items"];
                                foreach (JToken item in items)
                                {
                                    if (item["id"] != null && item["id"]["kind"] != null && item["id"]["kind"].ToString().Equals("youtube#video"))
                                    {
                                        searchItems.Add(new SongRequestItemSearch(new SongRequestItem() { ID = item["id"]["videoId"].ToString(), Name = item["snippet"]["title"].ToString(), User = user, Type = SongRequestServiceTypeEnum.YouTube }));
                                    }
                                }
                            }
                        }
                    }

                    if (searchItems.Count > 0)
                    {
                        if (pickFirst || searchItems.Count == 1)
                        {
                            identifier = searchItems.First().SongRequest.ID;
                        }
                        else
                        {
                            this.lastUserYouTubeSongSearches[user] = new List<SongRequestItem>();
                            foreach (var item in searchItems)
                            {
                                this.lastUserYouTubeSongSearches[user].Add(item.SongRequest);
                            }

                            List<string> artistsStrings = new List<string>();
                            for (int i = 0; i < this.lastUserYouTubeSongSearches[user].Count; i++)
                            {
                                artistsStrings.Add((i + 1) + ". " + this.lastUserYouTubeSongSearches[user][i].Name);
                            }
                            string artistsText = string.Join(",  ", artistsStrings);
                            return new SongRequestItemSearch(SongRequestServiceTypeEnum.YouTube,
                                new List<string>() { "Multiple results found, please re-run this command with a space & the number of the song:", artistsText }, multipleResults: true);
                        }
                    }
                }
            }

            try
            {
                using (HttpClientWrapper client = new HttpClientWrapper("https://www.youtube.com/"))
                {
                    HttpResponseMessage response = await client.GetAsync(string.Format("oembed?url=http://www.youtube.com/watch?v={0}&format=json", identifier));
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        string content = await response.Content.ReadAsStringAsync();
                        JObject jobj = JObject.Parse(content);
                        if (jobj["title"] != null)
                        {
                            return new SongRequestItemSearch(new SongRequestItem() { ID = identifier, Name = jobj["title"].ToString(), User = user, Type = SongRequestServiceTypeEnum.YouTube });
                        }
                    }
                }
            }
            catch (Exception) { }

            return new SongRequestItemSearch(SongRequestServiceTypeEnum.YouTube, "The link you specified is not a valid YouTube video");
        }

        private async Task<SongRequestItemSearch> GetSoundCloudSongRequest(UserViewModel user, string identifier, bool pickFirst = false)
        {
            if (Regex.IsMatch(identifier, SoundCloudPublicLinkFormat))
            {
                try
                {
                    using (HttpClientWrapper client = new HttpClientWrapper("https://soundcloud.com"))
                    {
                        HttpResponseMessage response = await client.GetAsync("http://soundcloud.com/oembed?format=js&url=" + HttpUtility.UrlEncode(identifier) + "&iframe=true");
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            string content = await response.Content.ReadAsStringAsync();
                            content = content.Trim(new char[] { '(', ')', ';' });
                            JObject jobj = JObject.Parse(content);
                            if (jobj["title"] != null)
                            {
                                return new SongRequestItemSearch(new SongRequestItem() { ID = identifier, Name = jobj["title"].ToString(), User = user, Type = SongRequestServiceTypeEnum.SoundCloud });
                            }
                        }
                    }
                }
                catch (Exception ex) { Logger.Log(ex); }
            }
            return new SongRequestItemSearch(SongRequestServiceTypeEnum.SoundCloud, "The link you specified is not a valid SoundCloud song");
        }

        private async Task<SongRequestItemSearch> GetSpotifySongRequest(UserViewModel user, string identifier, bool pickFirst = false)
        {
            if (ChannelSession.Services.Spotify != null)
            {
                string songID = null;

                if (identifier.StartsWith(SpotifyTrackPrefix) || identifier.StartsWith(SpotifyLinkPrefix))
                {
                    songID = identifier;
                    songID = songID.Replace(SpotifyTrackPrefix, "");
                    songID = songID.Replace(SpotifyLinkPrefix, "");
                    if (songID.Contains('?'))
                    {
                        songID = songID.Substring(0, songID.IndexOf('?'));
                    }
                }
                else if (user != null)
                {
                    Dictionary<string, SpotifySong> artistToSong = new Dictionary<string, SpotifySong>();

                    if (this.lastUserSpotifySongSearches.ContainsKey(user) && int.TryParse(identifier, out int artistIndex))
                    {
                        if (artistIndex > 0 && this.lastUserSpotifySongSearches.ContainsKey(user) && this.lastUserSpotifySongSearches[user].Count > 0 && artistIndex <= this.lastUserSpotifySongSearches[user].Count)
                        {
                            songID = this.lastUserSpotifySongSearches[user][artistIndex - 1].ID;
                        }
                        else
                        {
                            return new SongRequestItemSearch(SongRequestServiceTypeEnum.Spotify, "The artist index you specified was not valid, please try your search again");
                        }
                    }
                    else
                    {
                        foreach (SpotifySong song in await ChannelSession.Services.Spotify.SearchSongs(identifier))
                        {
                            if (!artistToSong.ContainsKey(song.Artist.Name))
                            {
                                artistToSong[song.Artist.Name] = song;
                            }
                        }

                        this.lastUserSpotifySongSearches[user] = new List<SpotifySong>();
                        foreach (var kvp in artistToSong)
                        {
                            this.lastUserSpotifySongSearches[user].Add(kvp.Value);
                        }

                        if (artistToSong.Count == 0)
                        {
                            return new SongRequestItemSearch(SongRequestServiceTypeEnum.Spotify,
                                "We could not find any songs with the name specified. You can visit https://open.spotify.com/search and search for the song you want. Once you have found it, right click on the song and select \"Copy Song Link\" and run this command with that link.");
                        }
                        else if (artistToSong.Count == 1 || pickFirst)
                        {
                            songID = artistToSong.First().Value.ID;
                        }
                        else
                        {
                            List<string> artistsStrings = new List<string>();
                            for (int i = 0; i < artistToSong.Count; i++)
                            {
                                artistsStrings.Add((i + 1) + ". " + artistToSong.ElementAt(i).Key);
                            }
                            string artistsText = string.Join(",  ", artistsStrings);
                            return new SongRequestItemSearch(SongRequestServiceTypeEnum.Spotify,
                                new List<string>() { "Multiple results found, please re-run this command with a space & the number of the artist:", artistsText }, multipleResults: true);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(songID))
                {
                    SpotifySong song = await ChannelSession.Services.Spotify.GetSong(songID);
                    if (song != null)
                    {
                        if (song.Explicit && !ChannelSession.Settings.SpotifyAllowExplicit)
                        {
                            return new SongRequestItemSearch(SongRequestServiceTypeEnum.Spotify, "Explicit content is currently blocked for song requests");
                        }
                        else
                        {
                            return new SongRequestItemSearch(new SongRequestItem() { ID = song.ID, Name = song.ToString(), User = user, Type = SongRequestServiceTypeEnum.Spotify });
                        }
                    }
                    else
                    {
                        return new SongRequestItemSearch(SongRequestServiceTypeEnum.Spotify, "We could not find a valid song for your request. If this is valid song, please try again in a little bit.");
                    }
                }
            }
            return new SongRequestItemSearch(SongRequestServiceTypeEnum.Spotify, "Spotify is not currently enabled. Please inform the Streamer that re-enable it.");
        }

        private async Task PlayOverlaySong(SongRequestItem request)
        {
            if (ChannelSession.Services.OverlayServer != null)
            {
                await ChannelSession.Services.OverlayServer.SendSongRequest(new OverlaySongRequest() { Type = request.Type.ToString(), Action = "song", Source = request.ID, Volume = ChannelSession.Settings.SongRequestVolume });
            }
        }

        private async Task PlayPauseOverlaySong(SongRequestServiceTypeEnum type)
        {
            if (ChannelSession.Services.OverlayServer != null)
            {
                await ChannelSession.Services.OverlayServer.SendSongRequest(new OverlaySongRequest() { Type = type.ToString(), Action = "playpause", Volume = ChannelSession.Settings.SongRequestVolume });
            }
        }

        private async Task StopOverlaySong(SongRequestServiceTypeEnum type)
        {
            if (ChannelSession.Services.OverlayServer != null)
            {
                await ChannelSession.Services.OverlayServer.SendSongRequest(new OverlaySongRequest() { Type = type.ToString(), Action = "stop", Volume = ChannelSession.Settings.SongRequestVolume });
            }
        }

        private async Task NextOverlay(SongRequestServiceTypeEnum type)
        {
            if (ChannelSession.Services.OverlayServer != null)
            {
                await ChannelSession.Services.OverlayServer.SendSongRequest(new OverlaySongRequest() { Type = type.ToString(), Action = "next", Volume = ChannelSession.Settings.SongRequestVolume });
            }
        }

        private async Task RefreshOverlayVolume(SongRequestServiceTypeEnum type)
        {
            if (ChannelSession.Services.OverlayServer != null)
            {
                await ChannelSession.Services.OverlayServer.SendSongRequest(new OverlaySongRequest() { Type = type.ToString(), Action = "volume", Volume = ChannelSession.Settings.SongRequestVolume });
            }
        }

        private async Task PlayDefaultPlaylist()
        {
            if (!string.IsNullOrEmpty(ChannelSession.Settings.DefaultPlaylist))
            {
                if (ChannelSession.Services.Spotify != null && Regex.IsMatch(ChannelSession.Settings.DefaultPlaylist, SpotifyPlaylistRegex, RegexOptions.IgnoreCase))
                {
                    this.currentSong = new SongRequestItem
                    {
                        ID = $"{DefaultSongId}:{ChannelSession.Settings.DefaultPlaylist}",
                        Name = "Default Spotify Playlist",
                        User = ChannelSession.GetCurrentUser(),
                        Type = SongRequestServiceTypeEnum.Spotify
                    };
                    await ChannelSession.Services.Spotify.PlayPlaylist(new SpotifyPlaylist { Uri = ChannelSession.Settings.DefaultPlaylist });
                }
                else if (ChannelSession.Services.OverlayServer != null)
                {
                    try
                    {
                        Uri uri = new Uri(ChannelSession.Settings.DefaultPlaylist);
                        if (uri.Host.EndsWith(YouTubeHost, StringComparison.InvariantCultureIgnoreCase))
                        {
                            NameValueCollection queryParameteters = HttpUtility.ParseQueryString(uri.Query);
                            if (!string.IsNullOrEmpty(queryParameteters["list"]))
                            {
                                this.currentSong = new SongRequestItem
                                {
                                    ID = $"{DefaultSongId}:{ChannelSession.Settings.DefaultPlaylist}",
                                    Name = "Default YouTube Playlist",
                                    User = ChannelSession.GetCurrentUser(),
                                    Type = SongRequestServiceTypeEnum.YouTube
                                };
                                await ChannelSession.Services.OverlayServer.SendSongRequest(new OverlaySongRequest() { Type = SongRequestServiceTypeEnum.YouTube.ToString(), Action = "playlist", Source = queryParameteters["list"], Volume = ChannelSession.Settings.SongRequestVolume });
                            }
                        }
                        else if (uri.Host.EndsWith(SoundCloudHost, StringComparison.InvariantCultureIgnoreCase) && uri.Segments.Length == 4 && uri.Segments[2].Equals("sets/", StringComparison.InvariantCultureIgnoreCase))
                        {
                            this.currentSong = new SongRequestItem
                            {
                                ID = $"{DefaultSongId}:{ChannelSession.Settings.DefaultPlaylist}",
                                Name = "Default SoundCloud Playlist",
                                User = ChannelSession.GetCurrentUser(),
                                Type = SongRequestServiceTypeEnum.SoundCloud
                            };
                            await ChannelSession.Services.OverlayServer.SendSongRequest(new OverlaySongRequest() { Type = SongRequestServiceTypeEnum.SoundCloud.ToString(), Action = "playlist", Source = ChannelSession.Settings.DefaultPlaylist, Volume = ChannelSession.Settings.SongRequestVolume });
                        }
                    }
                    catch { }
                }
            }
        }
    }
}
