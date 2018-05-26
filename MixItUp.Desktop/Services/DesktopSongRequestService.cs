using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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

        public SongRequestServiceTypeEnum Type { get; set; }
        public List<string> ErrorMessages { get; set; }

        public SongRequestItemSearch(SongRequestServiceTypeEnum type, SongRequestItem songRequest)
        {
            this.Type = type;
            this.SongRequest = songRequest;
        }

        public SongRequestItemSearch(SongRequestServiceTypeEnum type, string errorMessage) : this(type, new string[] { errorMessage }) { }

        public SongRequestItemSearch(SongRequestServiceTypeEnum type, IEnumerable<string> errorMessages)
        {
            this.Type = type;
            this.ErrorMessages = new List<string>(errorMessages);
        }
    }

    public class DesktopSongRequestService : ISongRequestService
    {
        private const string SpotifyLinkPrefix = "https://open.spotify.com/track/";
        private const string SpotifyTrackPrefix = "spotify:track:";

        private const string YouTubeLongLinkPrefix = "https://www.youtube.com/watch?v=";
        private const string YouTubeShortLinkPrefix = "https://youtu.be/";

        private static SemaphoreSlim songRequestLock = new SemaphoreSlim(1);

        public bool IsEnabled { get; private set; }

        private CancellationTokenSource backgroundThreadCancellationTokenSource;

        private List<SongRequestItem> allRequests;

        private Dictionary<UserViewModel, List<SpotifySong>> lastUserSongSearches = new Dictionary<UserViewModel, List<SpotifySong>>();
        private bool overlaySongFinished = false;

        public DesktopSongRequestService()
        {
            this.allRequests = new List<SongRequestItem>();
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

            if (ChannelSession.Settings.SongRequestServiceTypes.Contains(SongRequestServiceTypeEnum.Youtube) && ChannelSession.Services.OverlayServer == null)
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

        public async Task AddSongRequest(UserViewModel user, string identifier)
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

            List<Task<SongRequestItemSearch>> allRequests = new List<Task<SongRequestItemSearch>>();
            if (ChannelSession.Settings.SongRequestServiceTypes.Contains(SongRequestServiceTypeEnum.Youtube)) { allRequests.Add(this.GetYouTubeSongRequest(identifier)); }
            if (ChannelSession.Settings.SongRequestServiceTypes.Contains(SongRequestServiceTypeEnum.Spotify)) { allRequests.Add(this.GetSpotifySongRequest(user, identifier)); }

            SongRequestItemSearch requestSearch = null;
            foreach (SongRequestItemSearch search in await Task.WhenAll(allRequests))
            {
                requestSearch = search;
                if (requestSearch.SongRequest != null)
                {
                    break;
                }
            }

            if (requestSearch != null)
            {
                if (requestSearch.SongRequest != null)
                {
                    await DesktopSongRequestService.songRequestLock.WaitAsync();
                    if (this.allRequests.Count == 0)
                    {
                        await this.PlaySong(requestSearch.SongRequest);
                    }
                    this.allRequests.Add(requestSearch.SongRequest);
                    DesktopSongRequestService.songRequestLock.Release();

                    await ChannelSession.Chat.SendMessage(string.Format("{0} was added to the queue", requestSearch.SongRequest.Name));
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

        public async Task PlayPauseCurrentSong()
        {
            if (this.allRequests.Count > 0)
            {
                await DesktopSongRequestService.songRequestLock.WaitAsync();

                SongRequestItem request = this.allRequests.FirstOrDefault();
                if (request.Type == SongRequestServiceTypeEnum.Spotify)
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
                    }
                }
                else if (request.Type == SongRequestServiceTypeEnum.Youtube)
                {
                    if (ChannelSession.Services.OverlayServer != null)
                    {
                        await ChannelSession.Services.OverlayServer.SendSongRequest(new OverlaySongRequest() { Type = "YouTube", Action = "playpause" });
                    }
                }

                DesktopSongRequestService.songRequestLock.Release();
            }
        }

        public async Task SkipToNextSong()
        {
            await DesktopSongRequestService.songRequestLock.WaitAsync();

            if (this.allRequests.Count > 0)
            {
                SongRequestItem request = this.allRequests.FirstOrDefault();
                if (request.Type == SongRequestServiceTypeEnum.Spotify)
                {
                    await ChannelSession.Services.Spotify.PauseCurrentlyPlaying();
                }
                else if (request.Type == SongRequestServiceTypeEnum.Youtube)
                {
                    if (ChannelSession.Services.OverlayServer != null)
                    {
                        await ChannelSession.Services.OverlayServer.SendSongRequest(new OverlaySongRequest() { Type = "YouTube", Action = "stop" });
                    }
                }

                this.allRequests.RemoveAt(0);
            }

            if (this.allRequests.Count > 0)
            {
                await this.PlaySong(this.allRequests.FirstOrDefault());
            }

            DesktopSongRequestService.songRequestLock.Release();

            GlobalEvents.SongRequestsChangedOccurred();
        }

        public Task<SongRequestItem> GetCurrentlyPlaying()
        {
            return Task.FromResult(this.allRequests.FirstOrDefault());
        }

        public Task<SongRequestItem> GetNextTrack()
        {
            SongRequestItem result = null;
            if (this.allRequests.Count > 1)
            {
                result = this.allRequests.Skip(1).FirstOrDefault();
            }
            return Task.FromResult(result);
        }

        public Task<IEnumerable<SongRequestItem>> GetAllRequests()
        {
            return Task.FromResult<IEnumerable<SongRequestItem>>(this.allRequests.ToList());
        }

        public Task ClearAllRequests()
        {
            this.allRequests.Clear();
            GlobalEvents.SongRequestsChangedOccurred();
            return Task.FromResult(0);
        }

        public void OverlaySongFinished() { this.overlaySongFinished = true; }

        private async Task PlayerBackground()
        {
            await BackgroundTaskWrapper.RunBackgroundTask(this.backgroundThreadCancellationTokenSource, async (tokenSource) =>
            {
                tokenSource.Token.ThrowIfCancellationRequested();

                bool shouldSkip = false;

                await DesktopSongRequestService.songRequestLock.WaitAsync();

                SongRequestItem current = this.allRequests.FirstOrDefault();
                if (current != null)
                {
                    if (current.Type == SongRequestServiceTypeEnum.Spotify)
                    {
                        if (ChannelSession.Services.Spotify != null)
                        {
                            SpotifyCurrentlyPlaying currentlyPlaying = await ChannelSession.Services.Spotify.GetCurrentlyPlaying();
                            if (currentlyPlaying == null || currentlyPlaying.ID == null || !currentlyPlaying.ID.Equals(current.ID) || (!currentlyPlaying.IsPlaying && currentlyPlaying.CurrentProgress == 0))
                            {
                                shouldSkip = true;
                            }
                        }
                    }
                    else if (current.Type == SongRequestServiceTypeEnum.Youtube)
                    {
                        if (this.overlaySongFinished)
                        {
                            this.overlaySongFinished = false;
                            shouldSkip = true;
                        }
                    }
                }

                DesktopSongRequestService.songRequestLock.Release();

                if (shouldSkip)
                {
                    await this.SkipToNextSong();
                }

                await Task.Delay(2000);
            });
        }

        private async Task PlaySong(SongRequestItem request)
        {
            if (request != null)
            {
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
                else if (request.Type == SongRequestServiceTypeEnum.Youtube)
                {
                    await this.PlayYouTubeSong(request.ID);
                }
            }
        }

        private async Task<SongRequestItemSearch> GetYouTubeSongRequest(string identifier)
        {
            identifier = identifier.Replace(YouTubeLongLinkPrefix, "");
            identifier = identifier.Replace(YouTubeShortLinkPrefix, "");
            if (identifier.Contains("&"))
            {
                identifier = identifier.Substring(0, identifier.IndexOf("&"));
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
                            return new SongRequestItemSearch(SongRequestServiceTypeEnum.Spotify, new SongRequestItem() { ID = identifier, Name = jobj["title"].ToString(), Type = SongRequestServiceTypeEnum.Youtube });
                        }
                    }
                }
            }
            catch (Exception) { }

            return new SongRequestItemSearch(SongRequestServiceTypeEnum.Youtube, "The link you specified is not a valid YouTube video");
        }

        private async Task<SongRequestItemSearch> GetSpotifySongRequest(UserViewModel user, string identifier)
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
                else
                {
                    Dictionary<string, SpotifySong> artistToSong = new Dictionary<string, SpotifySong>();

                    if (this.lastUserSongSearches.ContainsKey(user) && int.TryParse(identifier, out int artistIndex))
                    {
                        if (artistIndex > 0 && this.lastUserSongSearches.ContainsKey(user) && this.lastUserSongSearches[user].Count > 0 && artistIndex <= this.lastUserSongSearches[user].Count)
                        {
                            songID = this.lastUserSongSearches[user][artistIndex - 1].ID;
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

                        this.lastUserSongSearches[user] = new List<SpotifySong>();
                        foreach (var kvp in artistToSong)
                        {
                            this.lastUserSongSearches[user].Add(kvp.Value);
                        }

                        if (artistToSong.Count == 0)
                        {
                            return new SongRequestItemSearch(SongRequestServiceTypeEnum.Spotify,
                                "We could not find any songs with the name specified. You can visit https://open.spotify.com/search and search for the song you want. Once you have found it, right click on the song and select \"Copy Song Link\" and run this command with that link.");
                        }
                        else if (artistToSong.Count == 1)
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
                                new List<string>() { "Multiple results came back, please re-run this command with a space & the number of the artist:", artistsText });
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
                            return new SongRequestItemSearch(SongRequestServiceTypeEnum.Spotify, new SongRequestItem() { ID = song.ID, Name = song.ToString(), Type = SongRequestServiceTypeEnum.Spotify });
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

        private async Task PlayYouTubeSong(string identifier)
        {
            if (ChannelSession.Services.OverlayServer != null)
            {
                await ChannelSession.Services.OverlayServer.SendSongRequest(new OverlaySongRequest() { Type = "YouTube", Action = "song", Source = identifier });
            }
        }
    }
}
