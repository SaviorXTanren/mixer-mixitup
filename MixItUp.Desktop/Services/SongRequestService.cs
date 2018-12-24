using Mixer.Base.Web;
using MixItUp.Base;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
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
using System.Xml;

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

    public class SongRequestService : ISongRequestService
    {
        private const string SpotifyLinkPrefix = "https://open.spotify.com/track/";
        private const string SpotifyTrackPrefix = "spotify:track:";

        private const string SpotifyPlaylistLinkRegex = @"https://open.spotify.com/user/\w+/playlist/\w+";
        private const string SpotifyPlaylistRegex = @"spotify:user:\w+:playlist:";
        private const string SpotifyPlaylistUriFormat = "spotify:user:{0}:playlist:{1}";

        private const string YouTubeFullLinkPrefix = "https://www.youtube.com/watch?v=";
        private const string YouTubeFullLinkWithTimePattern = @"https://www.youtube.com/watch\?t=\w+&v=";
        private const string YouTubeLongLinkPrefix = "www.youtube.com/watch?v=";
        private const string YouTubeShortLinkPrefix = "youtu.be/";
        private const string YouTubeHost = "youtube.com";

        private static SemaphoreSlim songRequestLock = new SemaphoreSlim(1);

        private CancellationTokenSource backgroundThreadCancellationTokenSource;

        private List<SongRequestItem> allRequests = new List<SongRequestItem>();
        private List<SongRequestItem> playlistItems = new List<SongRequestItem>();

        private SongRequestItem currentSong = null;
        private SongRequestItem spotifyStatus = null;
        private SongRequestItem youTubeStatus = null;

        private Dictionary<UserViewModel, List<SpotifySong>> lastUserSpotifySongSearches = new Dictionary<UserViewModel, List<SpotifySong>>();
        private Dictionary<UserViewModel, List<SongRequestItem>> lastUserYouTubeSongSearches = new Dictionary<UserViewModel, List<SongRequestItem>>();

        public bool IsEnabled { get; private set; }

        public async Task<bool> Initialize()
        {
            if (ChannelSession.Settings.SongRequestServiceTypes.Count == 0)
            {
                return false;
            }

            if (ChannelSession.Settings.SongRequestServiceTypes.Contains(SongRequestServiceTypeEnum.Spotify) && (ChannelSession.Services.Spotify == null || (await ChannelSession.Services.Spotify.GetCurrentlyPlaying()) == null))
            {
                return false;
            }

            if (ChannelSession.Settings.SongRequestServiceTypes.Contains(SongRequestServiceTypeEnum.YouTube) && ChannelSession.Services.OverlayServer == null)
            {
                return false;
            }

            this.playlistItems.Clear();
            if (!string.IsNullOrEmpty(ChannelSession.Settings.DefaultPlaylist))
            {
                if (Regex.IsMatch(ChannelSession.Settings.DefaultPlaylist, SpotifyPlaylistRegex, RegexOptions.IgnoreCase) ||
                    Regex.IsMatch(ChannelSession.Settings.DefaultPlaylist, SpotifyPlaylistLinkRegex, RegexOptions.IgnoreCase))
                {
                    if (ChannelSession.Services.Spotify != null)
                    {
                        string uri = ChannelSession.Settings.DefaultPlaylist;
                        if (Regex.IsMatch(ChannelSession.Settings.DefaultPlaylist, SpotifyPlaylistLinkRegex, RegexOptions.IgnoreCase))
                        {
                            string playlistID = ChannelSession.Settings.DefaultPlaylist.Split(new char[] { '/' }).Last();
                            if (!string.IsNullOrEmpty(playlistID))
                            {
                                if (playlistID.Contains("?"))
                                {
                                    playlistID = playlistID.Substring(0, playlistID.IndexOf("?"));
                                }
                                uri = string.Format(SpotifyPlaylistUriFormat, ChannelSession.Services.Spotify.Profile.ID, playlistID);
                            }
                        }

                        string id = uri.Split(new string[] { ":playlist:" }, StringSplitOptions.RemoveEmptyEntries).Last();
                        if (!string.IsNullOrEmpty(id))
                        {
                            IEnumerable<SpotifySong> songs = await ChannelSession.Services.Spotify.GetPlaylistSongs(new SpotifyPlaylist { ID = id });
                            if (songs != null)
                            {
                                foreach (SpotifySong song in songs)
                                {
                                    this.playlistItems.Add(new SongRequestItem()
                                    {
                                        Type = SongRequestServiceTypeEnum.Spotify,
                                        ID = song.ID,
                                        Name = song.ToString(),
                                        Length = song.Duration,
                                        AlbumImage = song.Album?.ImageLink
                                    });
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (ChannelSession.Services.OverlayServer != null)
                    {
                        try
                        {
                            Uri uri = new Uri(ChannelSession.Settings.DefaultPlaylist);
                            if (uri.Host.EndsWith(YouTubeHost, StringComparison.InvariantCultureIgnoreCase))
                            {
                                NameValueCollection queryParameteters = HttpUtility.ParseQueryString(uri.Query);
                                if (!string.IsNullOrEmpty(queryParameteters["list"]))
                                {
                                    using (HttpClientWrapper client = new HttpClientWrapper("https://www.googleapis.com/"))
                                    {
                                        string pageToken = null;
                                        do
                                        {
                                            string restURL = string.Format("youtube/v3/playlistItems?playlistId={0}&maxResults=50&part=snippet,contentDetails&key={1}", HttpUtility.UrlEncode(queryParameteters["list"]), ChannelSession.SecretManager.GetSecret("YouTubeKey"));
                                            if (!string.IsNullOrEmpty(pageToken))
                                            {
                                                restURL += "&pageToken=" + pageToken;
                                            }
                                            HttpResponseMessage response = await client.GetAsync(restURL);
                                            if (response.StatusCode == HttpStatusCode.OK)
                                            {
                                                string content = await response.Content.ReadAsStringAsync();
                                                JObject jobj = JObject.Parse(content);
                                                if (jobj["nextPageToken"] != null)
                                                {
                                                    pageToken = jobj["nextPageToken"].ToString();
                                                }
                                                else
                                                {
                                                    pageToken = null;
                                                }

                                                if (jobj["items"] != null && jobj["items"] is JArray)
                                                {
                                                    JArray items = (JArray)jobj["items"];
                                                    foreach (JToken item in items)
                                                    {
                                                        if (item["kind"] != null && item["kind"].ToString().Equals("youtube#playlistItem"))
                                                        {
                                                            this.playlistItems.Add(new SongRequestItem()
                                                            {
                                                                ID = item["contentDetails"]["videoId"].ToString(),
                                                                Name = item["snippet"]["title"].ToString(),
                                                                Type = SongRequestServiceTypeEnum.YouTube,
                                                                AlbumImage = item["snippet"]?["thumbnails"]?["high"]?["url"]?.ToString()
                                                            });
                                                        }
                                                    }
                                                }
                                            }
                                        } while (!string.IsNullOrEmpty(pageToken));
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(ex);
                        }
                    }
                }

                if (this.playlistItems.Count > 0)
                {
                    this.playlistItems = this.playlistItems.OrderBy(s => Guid.NewGuid()).ToList();
                }
            }

            await this.ClearAllRequests();

            this.backgroundThreadCancellationTokenSource = new CancellationTokenSource();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () => { await this.PlayerBackground(); }, this.backgroundThreadCancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            this.IsEnabled = true;

            return true;
        }

        public async Task Disable()
        {
            await this.ClearAllRequests();

            if (this.backgroundThreadCancellationTokenSource != null)
            {
                this.backgroundThreadCancellationTokenSource.Cancel();
            }

            this.IsEnabled = false;
        }

        #region Request Methods

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
                    await SongRequestService.songRequestLock.WaitAndRelease(() =>
                    {
                        this.allRequests.Add(requestSearch.SongRequest);
                        return Task.FromResult(0);
                    });

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

        public async Task<IEnumerable<SongRequestItem>> GetAllRequests()
        {
            return await SongRequestService.songRequestLock.WaitAndRelease(() =>
            {
                return Task.FromResult(this.allRequests.ToList());
            });
        }

        public async Task RemoveSongRequest(SongRequestItem song)
        {
            await SongRequestService.songRequestLock.WaitAndRelease(() =>
            {
                this.allRequests.Remove(song);
                return Task.FromResult(0);
            });

            GlobalEvents.SongRequestsChangedOccurred();
        }

        public async Task RemoveLastSongRequestedByUser(UserViewModel user)
        {
            SongRequestItem song = null;
            await SongRequestService.songRequestLock.WaitAndRelease(() =>
            {
                song = this.allRequests.LastOrDefault(s => s.User.ID == user.ID);
                if (song != null)
                {
                    this.allRequests.Remove(song);
                }
                return Task.FromResult(0);
            });

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

        public async Task ClearAllRequests()
        {
            await SongRequestService.songRequestLock.WaitAndRelease(() =>
            {
                this.allRequests.Clear();
                return Task.FromResult(0);
            });

            GlobalEvents.SongRequestsChangedOccurred();
        }

        #endregion Request Methods

        #region Interaction Methods

        public async Task PlayPauseCurrentSong()
        {
            await SongRequestService.songRequestLock.WaitAndRelease(async () =>
            {
                await this.PlayPauseCurrentSongInternal();
            });
            GlobalEvents.SongRequestsChangedOccurred();
        }

        public async Task SkipToNextSong()
        {
            await SongRequestService.songRequestLock.WaitAndRelease(async () =>
            {
                await this.SkipToNextSongInternal();
            });
            GlobalEvents.SongRequestsChangedOccurred();
        }

        public async Task RefreshVolume()
        {
            await SongRequestService.songRequestLock.WaitAndRelease(async () =>
            {
                await this.RefreshVolumeInternal();
            });
            GlobalEvents.SongRequestsChangedOccurred();
        }

        public Task StatusUpdate(SongRequestItem item)
        {
            if (item != null)
            {
                if (item.Type == SongRequestServiceTypeEnum.YouTube && !string.IsNullOrEmpty(item.ID))
                {
                    this.youTubeStatus = item;
                    this.youTubeStatus.ID = Regex.Replace(this.youTubeStatus.ID, YouTubeFullLinkWithTimePattern, "");
                    this.youTubeStatus.ID = this.youTubeStatus.ID.Replace(YouTubeFullLinkPrefix, "");
                }
            }
            return Task.FromResult(0);
        }

        #endregion Interaction Methods

        #region Get Methods

        public async Task<SongRequestItem> GetCurrentlyPlaying()
        {
            return await SongRequestService.songRequestLock.WaitAndRelease(() =>
            {
                return Task.FromResult(this.currentSong);
            });
        }

        public async Task<SongRequestItem> GetNextTrack()
        {
            return await SongRequestService.songRequestLock.WaitAndRelease(() =>
            {
                if (this.allRequests.Count == 1 && this.playlistItems.Count > 0)
                {
                    return Task.FromResult(this.playlistItems.FirstOrDefault());
                }
                if (this.playlistItems.Count > 0 && this.playlistItems.First().Equals(this.currentSong) && this.allRequests.Count > 0)
                {
                    return Task.FromResult(this.allRequests.FirstOrDefault());
                }
                else if (this.allRequests.Count > 1)
                {
                    return Task.FromResult(this.allRequests.Skip(1).FirstOrDefault());
                }
                else if (this.playlistItems.Count > 1)
                {
                    return Task.FromResult(this.playlistItems.Skip(1).FirstOrDefault());
                }
                else
                {
                    return null;
                }
            });
        }

        #endregion Get Methods

        private async Task PlayerBackground()
        {
            await BackgroundTaskWrapper.RunBackgroundTask(this.backgroundThreadCancellationTokenSource, async (tokenSource) =>
            {
                tokenSource.Token.ThrowIfCancellationRequested();

                bool changeOccurred = false;

                await SongRequestService.songRequestLock.WaitAndRelease(async () =>
                {
                    Logger.LogDiagnostic("Current Song: " + this.currentSong);
                    Logger.LogDiagnostic("Spotify Status: " + this.spotifyStatus);
                    Logger.LogDiagnostic("YouTube Status: " + this.youTubeStatus);

                    if (this.currentSong == null)
                    {
                        await this.SkipToNextSongInternal();
                        changeOccurred = true;
                    }
                    else
                    {
                        SongRequestItem status = null;
                        if (this.currentSong.Type == SongRequestServiceTypeEnum.Spotify)
                        {
                            status = this.spotifyStatus = await this.GetSpotifyStatus();
                        }
                        else if (this.currentSong.Type == SongRequestServiceTypeEnum.YouTube)
                        {
                            status = await this.GetYouTubeStatus();
                        }

                        if (status != null)
                        {
                            if (status.Volume != ChannelSession.Settings.SongRequestVolume)
                            {
                                await this.RefreshVolumeInternal();
                            }

                            if (this.currentSong.Type == status.Type && this.currentSong.ID.Equals(status.ID))
                            {
                                this.currentSong.Progress = status.Progress;
                                this.currentSong.State = status.State;
                            }
                        }

                        if (currentSong.State == SongRequestStateEnum.NotStarted)
                        {
                            await this.RefreshVolumeInternal();
                            await this.PlaySongInternal(this.currentSong);
                            changeOccurred = true;
                        }
                        else if (this.currentSong.State == SongRequestStateEnum.Ended)
                        {
                            await this.RefreshVolumeInternal();
                            await this.SkipToNextSongInternal();
                            changeOccurred = true;
                        }
                    }
                });

                if (changeOccurred)
                {
                    GlobalEvents.SongRequestsChangedOccurred();
                    await Task.Delay(2000, tokenSource.Token);
                }

                await Task.Delay(1000, tokenSource.Token);
            });
        }

        #region Get Request Internal Methods

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
                                        searchItems.Add(new SongRequestItemSearch(new SongRequestItem()
                                        {
                                            ID = item["id"]["videoId"].ToString(),
                                            Name = item["snippet"]["title"].ToString(),
                                            User = user,
                                            Type = SongRequestServiceTypeEnum.YouTube,
                                            AlbumImage = item["snippet"]?["thumbnails"]?["high"]?["url"]?.ToString()
                                        }));
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
                using (HttpClientWrapper client = new HttpClientWrapper("https://www.googleapis.com/"))
                {
                    HttpResponseMessage response = await client.GetAsync(string.Format("youtube/v3/videos?id={0}&part=snippet,contentDetails&key={1}", HttpUtility.UrlEncode(identifier), ChannelSession.SecretManager.GetSecret("YouTubeKey")));
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        string content = await response.Content.ReadAsStringAsync();
                        JObject jobj = JObject.Parse(content);
                        if (jobj["items"] != null && jobj["items"] is JArray)
                        {
                            JArray items = (JArray)jobj["items"];
                            if (items.Count > 0)
                            {
                                JToken item = items.First;
                                if (item["id"] != null && item["snippet"] != null && item["contentDetails"] != null && item["kind"].ToString().Equals("youtube#video"))
                                {
                                    string length = item["contentDetails"]["duration"].ToString();
                                    TimeSpan timespan = XmlConvert.ToTimeSpan(length);

                                    return new SongRequestItemSearch(new SongRequestItem()
                                    {
                                        ID = item["id"].ToString(),
                                        Name = item["snippet"]["title"].ToString(),
                                        Length = (int)timespan.TotalSeconds, User = user,
                                        Type = SongRequestServiceTypeEnum.YouTube,
                                        AlbumImage = item["snippet"]?["thumbnails"]?["high"]?["url"]?.ToString()
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception) { }

            return new SongRequestItemSearch(SongRequestServiceTypeEnum.YouTube, "The link you specified is not a valid YouTube video");
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
                            return new SongRequestItemSearch(new SongRequestItem()
                            {
                                ID = song.ID,
                                Name = song.ToString(),
                                Length = song.Duration,
                                User = user, Type = SongRequestServiceTypeEnum.Spotify,
                                AlbumImage = song.Album?.ImageLink
                            });
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

        #endregion Get Request Methods

        #region Interaction Internal Methods

        private async Task PlayPauseCurrentSongInternal()
        {
            if (this.currentSong != null)
            {
                if (this.currentSong.Type == SongRequestServiceTypeEnum.Spotify)
                {
                    await this.PlayPauseSpotifySong();
                }
                else if (this.currentSong.Type == SongRequestServiceTypeEnum.YouTube)
                {
                    await this.PlayPauseOverlaySong(this.currentSong.Type);
                }
            }
        }

        private async Task SkipToNextSongInternal()
        {
            if (this.allRequests.Count > 0 && this.allRequests.First().Equals(this.currentSong))
            {
                this.allRequests.RemoveAt(0);
            }
            else if (this.playlistItems.Count > 0 && this.playlistItems.First().Equals(this.currentSong))
            {
                this.playlistItems.RemoveAt(0);              
            }

            if (this.currentSong != null)
            {
                if (this.currentSong.Type == SongRequestServiceTypeEnum.Spotify)
                {
                    await ChannelSession.Services.Spotify.PauseCurrentlyPlaying();
                    this.spotifyStatus = null;
                }
                else if (this.currentSong.Type == SongRequestServiceTypeEnum.YouTube)
                {
                    if (ChannelSession.Services.OverlayServer != null)
                    {
                        await ChannelSession.Services.OverlayServer.SendSongRequest(new OverlaySongRequest() { Type = SongRequestServiceTypeEnum.YouTube.ToString(), Action = "stop", Volume = ChannelSession.Settings.SongRequestVolume });
                        this.youTubeStatus = null;
                    }
                }
                this.currentSong.State = SongRequestStateEnum.Ended;
                this.currentSong = null;
            }

            SongRequestItem newSong = null;
            if (this.allRequests.Count > 0)
            {
                newSong = this.allRequests.First();
            }
            else if (this.playlistItems.Count > 0)
            {
                newSong = this.playlistItems.First();
            }

            if (newSong != null)
            {
                await this.PlaySongInternal(newSong);
            }
        }

        private async Task RefreshVolumeInternal()
        {
            if (ChannelSession.Services.Spotify != null)
            {
                await ChannelSession.Services.Spotify.SetVolume(ChannelSession.Settings.SongRequestVolume);
            }
            if (ChannelSession.Services.OverlayServer != null)
            {
                await ChannelSession.Services.OverlayServer.SendSongRequest(new OverlaySongRequest() { Type = SongRequestServiceTypeEnum.YouTube.ToString(), Action = "volume", Volume = ChannelSession.Settings.SongRequestVolume });
            }
        }

        private async Task PlaySongInternal(SongRequestItem item)
        {
            if (item != null)
            {
                if (item.Type == SongRequestServiceTypeEnum.Spotify)
                {
                    if (ChannelSession.Services.Spotify != null)
                    {
                        SpotifySong song = await ChannelSession.Services.Spotify.GetSong(item.ID);
                        if (song != null)
                        {
                            await ChannelSession.Services.Spotify.PlaySong(song);
                            this.currentSong = item;
                        }
                    }
                }
                else if (item.Type == SongRequestServiceTypeEnum.YouTube)
                {
                    if (ChannelSession.Services.OverlayServer != null)
                    {
                        await ChannelSession.Services.OverlayServer.SendSongRequest(new OverlaySongRequest() { Type = SongRequestServiceTypeEnum.YouTube.ToString(), Action = "song", Source = item.ID, Volume = ChannelSession.Settings.SongRequestVolume });
                        this.currentSong = item;
                    }
                }
            }
        }

        private async Task PlayPauseSpotifySong()
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

        private async Task PlayPauseOverlaySong(SongRequestServiceTypeEnum type)
        {
            if (ChannelSession.Services.OverlayServer != null)
            {
                await ChannelSession.Services.OverlayServer.SendSongRequest(new OverlaySongRequest() { Type = type.ToString(), Action = "playpause", Volume = ChannelSession.Settings.SongRequestVolume });
            }
        }

        private async Task<SongRequestItem> GetSpotifyStatus()
        {
            if (ChannelSession.Services.Spotify != null)
            {
                SpotifyCurrentlyPlaying currentlyPlaying = await ChannelSession.Services.Spotify.GetCurrentlyPlaying();
                if (currentlyPlaying != null && currentlyPlaying.ID != null)
                {
                    SongRequestItem result = new SongRequestItem()
                    {
                        Type = SongRequestServiceTypeEnum.Spotify,
                        ID = currentlyPlaying.ID,
                        Progress = currentlyPlaying.CurrentProgress,
                        Length = currentlyPlaying.Duration,
                        AlbumImage = currentlyPlaying.Album?.ImageLink
                    };

                    if (currentlyPlaying.IsPlaying)
                    {
                        result.State = SongRequestStateEnum.Playing;
                    }
                    else if (currentlyPlaying.CurrentProgress > 0)
                    {
                        result.State = SongRequestStateEnum.Paused;
                    }
                    else
                    {
                        result.State = SongRequestStateEnum.Ended;
                    }

                    result.Volume = await ChannelSession.Services.Spotify.GetVolume();

                    return result;
                }
            }
            return null;
        }

        private async Task<SongRequestItem> GetYouTubeStatus()
        {
            if (ChannelSession.Services.OverlayServer != null)
            {
                this.youTubeStatus = null;
                await ChannelSession.Services.OverlayServer.SendSongRequest(new OverlaySongRequest() { Type = SongRequestServiceTypeEnum.YouTube.ToString(), Action = "status" });
                for (int i = 0; i < 10 && this.youTubeStatus == null; i++)
                {
                    await Task.Delay(500);
                }
            }
            return this.youTubeStatus;
        }

        #endregion Interaction Internal Methods
    }
}
