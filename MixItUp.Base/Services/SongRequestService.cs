using Mixer.Base.Util;
using MixItUp.Base.Model.SongRequests;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface ISongRequestProviderService
    {
        SongRequestServiceTypeEnum Type { get; }

        Task<bool> Initialize();

        Task<IEnumerable<SongRequestModel>> GetPlaylist(string identifier);
        Task<IEnumerable<SongRequestModel>> Search(string identifier);

        Task<SongRequestCurrentlyPlayingModel> GetStatus();

        Task<SongRequestCurrentlyPlayingModel> Play(SongRequestModel song);
        Task Pause();
        Task Resume();
        Task Stop();

        Task SetVolume(int volume);
    }

    public interface ISongRequestService
    {
        IEnumerable<SongRequestModel> RequestSongs { get; }

        SongRequestCurrentlyPlayingModel Status { get; }

        bool IsEnabled { get; }

        void AddProvider(ISongRequestProviderService provider);

        Task Initialize();

        Task<bool> Enable();
        Task Disable();

        Task SearchAndPickFirst(UserViewModel user, SongRequestServiceTypeEnum service, string identifier);
        Task SearchAndSelect(UserViewModel user, SongRequestServiceTypeEnum service, string identifier);

        Task Pause();
        Task Resume();
        Task PauseResume();
        Task Skip();
        Task Ban();
        Task Ban(SongRequestModel song);

        Task RefreshVolume();

        Task<SongRequestModel> GetCurrent();
        Task<SongRequestModel> GetNext();

        Task MoveUp(SongRequestModel song);
        Task MoveDown(SongRequestModel song);

        Task Remove(SongRequestModel song);
        Task RemoveLastRequested();
        Task RemoveLastRequested(UserViewModel user);
        Task ClearAll();
    }

    public class SongRequestService : ISongRequestService
    {
        private static SemaphoreSlim songRequestLock = new SemaphoreSlim(1);

        private const int backgroundInterval = 1000;

        public IEnumerable<SongRequestModel> RequestSongs { get { return this.requestSongs.ToList(); } }
        private List<SongRequestModel> requestSongs = new List<SongRequestModel>();

        private List<SongRequestModel> playlistSongs = new List<SongRequestModel>();

        public SongRequestCurrentlyPlayingModel Status { get; private set; }

        private Task backgroundSongThread = null;
        private CancellationTokenSource backgroundSongThreadCancellationTokenSource;

        private Dictionary<UserViewModel, List<SongRequestModel>> lastUserSongSearches = new Dictionary<UserViewModel, List<SongRequestModel>>();

        private List<ISongRequestProviderService> allProviders = new List<ISongRequestProviderService>();
        private List<ISongRequestProviderService> enabledProviders = new List<ISongRequestProviderService>();

        private Dictionary<SongRequestServiceTypeEnum, List<string>> bannedSongsCache = new Dictionary<SongRequestServiceTypeEnum, List<string>>();

        public bool IsEnabled { get; private set; }

        public void AddProvider(ISongRequestProviderService provider)
        {
            this.allProviders.Add(provider);
        }

        public Task Initialize()
        {
            this.requestSongs.Clear();
            if (ChannelSession.Settings.SongRequestsSaveRequestQueue)
            {
                this.requestSongs.AddRange(ChannelSession.Settings.SongRequestsSavedRequestQueue);
            }

            bannedSongsCache.Clear();
            foreach (SongRequestServiceTypeEnum type in EnumHelper.GetEnumList<SongRequestServiceTypeEnum>())
            {
                bannedSongsCache[type] = new List<string>();
            }

            foreach (SongRequestModel bannedSong in ChannelSession.Settings.SongRequestsBannedSongs)
            {
                bannedSongsCache[bannedSong.Type].Add(bannedSong.ID);
            }

            GlobalEvents.SongRequestsChangedOccurred();
            return Task.FromResult(0);
        }

        public async Task<bool> Enable()
        {
            return await SongRequestService.songRequestLock.WaitAndRelease(async () =>
            {
                if (this.IsEnabled)
                {
                    return true;
                }

                this.enabledProviders.Clear();
                foreach (ISongRequestProviderService provider in this.allProviders)
                {
                    if (ChannelSession.Settings.SongRequestServiceTypes.Contains(provider.Type))
                    {
                        if (!await provider.Initialize())
                        {
                            if (provider.Type == SongRequestServiceTypeEnum.Spotify)
                            {
                                await DialogHelper.ShowMessage("You must connect to your Spotify account in the Services area. You must also have Spotify running on your computer and you have played at least one song in the Spotify app."
                                    + Environment.NewLine + Environment.NewLine + "This is required to be done every time to let Spotify know that where to send our song requests to.");
                            }
                            else
                            {
                                await DialogHelper.ShowMessage(string.Format("Failed to initialize the {0} service, please try again", provider.Type));
                            }
                            return false;
                        }
                        this.enabledProviders.Add(provider);
                    }
                }

                if (enabledProviders.Count() == 0)
                {
                    await DialogHelper.ShowMessage("At least 1 song request service must be enabled in the Settings menu");
                    return false;
                }

                await ChannelSession.SaveSettings();

                this.playlistSongs.Clear();

                await this.RefreshVolumeInternal();

                await this.SkipInternal();

                this.IsEnabled = true;

                return true;
            });
        }

        public async Task Disable()
        {
            await SongRequestService.songRequestLock.WaitAndRelease(async () =>
            {
                this.requestSongs.Clear();

                foreach (ISongRequestProviderService provider in this.enabledProviders)
                {
                    await provider.Stop();
                }

                if (this.backgroundSongThreadCancellationTokenSource != null)
                {
                    this.backgroundSongThreadCancellationTokenSource.Cancel();
                }
                this.backgroundSongThreadCancellationTokenSource = null;
                this.backgroundSongThread = null;

                this.IsEnabled = false;
                return Task.FromResult(0);
            });
            GlobalEvents.SongRequestsChangedOccurred();
        }

        public async Task SearchAndPickFirst(UserViewModel user, SongRequestServiceTypeEnum service, string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                await ChannelSession.Services.Chat.Whisper(user.UserName, "You must specify your request with the command. For details on how to request songs, check out: https://github.com/SaviorXTanren/mixer-mixitup/wiki/Song-Requests#requesting-songs");
                return;
            }

            IEnumerable<SongRequestModel> results = await this.Search(service, identifier);
            if (results != null && results.Count() > 0)
            {
                await this.AddSongRequest(user, results.First());
            }
            else
            {
                await ChannelSession.Services.Chat.Whisper(user.UserName, "We were unable to find a song that matched your request. For details on how to request songs, check out: https://github.com/SaviorXTanren/mixer-mixitup/wiki/Song-Requests#requesting-songs");
            }
        }

        public async Task SearchAndSelect(UserViewModel user, SongRequestServiceTypeEnum service, string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                await ChannelSession.Services.Chat.Whisper(user.UserName, "You must specify your request with the command. For details on how to request songs, check out: https://github.com/SaviorXTanren/mixer-mixitup/wiki/Song-Requests#requesting-songs");
                return;
            }

            if (this.lastUserSongSearches.ContainsKey(user) && int.TryParse(identifier, out int songIndex) && songIndex > 0 && songIndex <= this.lastUserSongSearches[user].Count)
            {
                songIndex = songIndex - 1;
                await this.AddSongRequest(user, this.lastUserSongSearches[user][songIndex]);
                this.lastUserSongSearches.Remove(user);
            }
            else
            {
                IEnumerable<SongRequestModel> results = await this.Search(service, identifier);
                if (results != null && results.Count() > 0)
                {
                    if (results.Count() == 1)
                    {
                        await this.AddSongRequest(user, results.First());
                    }
                    else
                    {
                        this.lastUserSongSearches[user] = new List<SongRequestModel>();
                        foreach (SongRequestModel song in results.Take(5))
                        {
                            this.lastUserSongSearches[user].Add(song);
                        }

                        List<string> resultsStrings = new List<string>();
                        for (int i = 0; i < this.lastUserSongSearches[user].Count; i++)
                        {
                            resultsStrings.Add((i + 1) + ". " + this.lastUserSongSearches[user][i].Name);
                        }
                        await ChannelSession.Services.Chat.Whisper(user.UserName, "Multiple results found, please re-run this command with a space & the number of the song: " + string.Join(",  ", resultsStrings));
                    }
                }
                else
                {
                    await ChannelSession.Services.Chat.Whisper(user.UserName, "We were unable to find a song that matched your request. For details on how to request songs, check out: https://github.com/SaviorXTanren/mixer-mixitup/wiki/Song-Requests#requesting-songs");
                }
            }
        }

        public async Task Pause()
        {
            await SongRequestService.songRequestLock.WaitAndRelease(async () =>
            {
                if (this.Status != null)
                {
                    foreach (ISongRequestProviderService provider in this.enabledProviders)
                    {
                        if (this.Status.Type == provider.Type)
                        {
                            await provider.Pause();
                            this.Status.State = SongRequestStateEnum.Paused;
                        }
                    }
                }
            });
            GlobalEvents.SongRequestsChangedOccurred();
        }

        public async Task Resume()
        {
            await SongRequestService.songRequestLock.WaitAndRelease(async () =>
            {
                if (this.Status != null)
                {
                    foreach (ISongRequestProviderService provider in this.enabledProviders)
                    {
                        if (this.Status.Type == provider.Type)
                        {
                            await provider.Resume();
                            this.Status.State = SongRequestStateEnum.Playing;
                        }
                    }
                }
            });
            GlobalEvents.SongRequestsChangedOccurred();
        }

        public async Task PauseResume()
        {
            bool isPlaying = false;
            await SongRequestService.songRequestLock.WaitAndRelease(() =>
            {
                if (this.Status != null && this.Status.State == SongRequestStateEnum.Playing)
                {
                    isPlaying = true;
                }
                return Task.FromResult(0);
            });

            if (isPlaying)
            {
                await this.Pause();
            }
            else
            {
                await this.Resume();
            }
        }

        public async Task Skip()
        {
            await SongRequestService.songRequestLock.WaitAndRelease(async () =>
            {
                await this.SkipInternal();
            });
            GlobalEvents.SongRequestsChangedOccurred();
        }

        public async Task Ban()
        {
            if (this.Status != null)
            {
                await this.Ban(this.Status);
                await this.Skip();
            }
        }

        public async Task Ban(SongRequestModel song)
        {
            await SongRequestService.songRequestLock.WaitAndRelease(() =>
            {
                ChannelSession.Settings.SongRequestsBannedSongs.Add(song);
                bannedSongsCache[song.Type].Add(song.ID);
                return Task.FromResult(0);
            });
            await this.Remove(song);
        }

        public async Task RefreshVolume()
        {
            await SongRequestService.songRequestLock.WaitAndRelease(async () =>
            {
                await this.RefreshVolumeInternal();
            });
        }

        public async Task<SongRequestModel> GetCurrent()
        {
            return await SongRequestService.songRequestLock.WaitAndRelease(() =>
            {
                return Task.FromResult(this.Status);
            });
        }

        public async Task<SongRequestModel> GetNext()
        {
            return await SongRequestService.songRequestLock.WaitAndRelease(() =>
            {
                if (this.requestSongs.Count > 0)
                {
                    return Task.FromResult(this.requestSongs.FirstOrDefault());
                }
                else if (this.playlistSongs.Count > 0)
                {
                    return Task.FromResult(this.playlistSongs.FirstOrDefault());
                }
                else
                {
                    return Task.FromResult<SongRequestModel>(null);
                }
            });
        }

        public async Task MoveUp(SongRequestModel song)
        {
            await SongRequestService.songRequestLock.WaitAndRelease(() =>
            {
                this.requestSongs.MoveUp(song);
                return Task.FromResult(0);
            });
            GlobalEvents.SongRequestsChangedOccurred();
        }

        public async Task MoveDown(SongRequestModel song)
        {
            await SongRequestService.songRequestLock.WaitAndRelease(() =>
            {
                this.requestSongs.MoveDown(song);
                return Task.FromResult(0);
            });
            GlobalEvents.SongRequestsChangedOccurred();
        }

        public async Task Remove(SongRequestModel song)
        {
            await SongRequestService.songRequestLock.WaitAndRelease(() =>
            {
                this.requestSongs.Remove(song);
                return Task.FromResult(0);
            });

            if (ChannelSession.Settings.SongRemovedCommand != null)
            {
                Dictionary<string, string> specialIdentifiers = new Dictionary<string, string>()
                {
                    { "songtitle", song.Name },
                    { "songalbumimage", song.AlbumImage }
                };
                await ChannelSession.Settings.SongRemovedCommand.Perform(song.User, arguments: null, extraSpecialIdentifiers: specialIdentifiers);
            }

            GlobalEvents.SongRequestsChangedOccurred();
        }

        public async Task RemoveLastRequested()
        {
            SongRequestModel song = null;
            await SongRequestService.songRequestLock.WaitAndRelease(() =>
            {
                song = this.requestSongs.LastOrDefault();
                return Task.FromResult(0);
            });

            if (song != null)
            {
                await this.Remove(song);
            }

            GlobalEvents.SongRequestsChangedOccurred();
        }

        public async Task RemoveLastRequested(UserViewModel user)
        {
            SongRequestModel song = null;
            await SongRequestService.songRequestLock.WaitAndRelease(() =>
            {
                song = this.requestSongs.LastOrDefault(s => s.User.ID == user.ID);
                return Task.FromResult(0);
            });

            if (song != null)
            {
                await this.Remove(song);
            }
            else
            {
                await ChannelSession.Services.Chat.Whisper(user.UserName, string.Format("You have no songs in the queue.", song.Name));
            }

            GlobalEvents.SongRequestsChangedOccurred();
        }

        public async Task ClearAll()
        {
            await SongRequestService.songRequestLock.WaitAndRelease(() =>
            {
                this.requestSongs.Clear();
                return Task.FromResult(0);
            });
            GlobalEvents.SongRequestsChangedOccurred();
        }

        private async Task<IEnumerable<SongRequestModel>> Search(SongRequestServiceTypeEnum service, string identifier)
        {
            List<Task<IEnumerable<SongRequestModel>>> searchTasks = new List<Task<IEnumerable<SongRequestModel>>>();
            foreach (ISongRequestProviderService provider in this.enabledProviders)
            {
                if (service == SongRequestServiceTypeEnum.All || service == provider.Type)
                {
                    searchTasks.Add(provider.Search(identifier));
                }
            }

            IEnumerable<SongRequestModel>[] taskResults = await Task.WhenAll(searchTasks);
            return taskResults.SelectMany(r => r);
        }

        private async Task AddSongRequest(UserViewModel user, SongRequestModel song)
        {
            song.User = user;

            if (bannedSongsCache[song.Type].Contains(song.ID))
            {
                await ChannelSession.Services.Chat.Whisper(user.UserName, "This song is banned from being requested.");
                return;
            }

            bool isSongCurrentlyPlaying = false;
            await SongRequestService.songRequestLock.WaitAndRelease(() =>
            {
                isSongCurrentlyPlaying = (this.Status != null);

                if (ChannelSession.Settings.SongRequestSubPriority && user.IsMixerSubscriber)
                {
                    for (int i = 0; i < this.requestSongs.Count; i++)
                    {
                        if (!this.requestSongs[i].User.IsMixerSubscriber)
                        {
                            this.requestSongs.Insert(i, song);
                            return Task.FromResult(0);
                        }
                    }
                }
                this.requestSongs.Add(song);

                if (ChannelSession.Settings.SongRequestsSaveRequestQueue)
                {
                    ChannelSession.Settings.SongRequestsSavedRequestQueue = this.requestSongs.ToList();
                }

                return Task.FromResult(0);
            });

            if (!isSongCurrentlyPlaying)
            {
                await this.Skip();
            }
            else
            {
                if (ChannelSession.Settings.SongAddedCommand != null)
                {
                    Dictionary<string, string> specialIdentifiers = new Dictionary<string, string>()
                    {
                        { "songtitle", song.Name },
                        { "songalbumimage", song.AlbumImage }
                    };
                    await ChannelSession.Settings.SongAddedCommand.Perform(song.User, arguments: null, extraSpecialIdentifiers: specialIdentifiers);
                }
            }

            ChannelSession.Services.Telemetry.TrackSongRequest(song.Type);

            GlobalEvents.SongRequestsChangedOccurred();
        }

        private async Task SkipInternal()
        {
            if (this.Status != null)
            {
                foreach (ISongRequestProviderService provider in this.enabledProviders)
                {
                    if (this.Status.Type == provider.Type)
                    {
                        await provider.Stop();
                    }
                }
            }
            this.Status = null;

            if (this.backgroundSongThread != null)
            {
                this.backgroundSongThreadCancellationTokenSource.Cancel();
                this.backgroundSongThreadCancellationTokenSource = null;
                this.backgroundSongThread = null;
            }

            SongRequestModel newSong = null;
            if (this.requestSongs.Count > 0)
            {
                newSong = this.requestSongs.First();
                this.requestSongs.RemoveAt(0);

                if (ChannelSession.Settings.SongRequestsSaveRequestQueue)
                {
                    ChannelSession.Settings.SongRequestsSavedRequestQueue = this.requestSongs.ToList();
                }
            }
            else
            {
                if (this.playlistSongs.Count == 0)
                {
                    await this.LoadBackupPlaylistSongs();
                }

                if (this.playlistSongs.Count > 0)
                {
                    newSong = this.playlistSongs.First();
                    this.playlistSongs.Remove(newSong);
                }
            }

            if (newSong != null)
            {
                foreach (ISongRequestProviderService provider in this.enabledProviders)
                {
                    if (newSong.Type == provider.Type)
                    {
                        this.Status = await provider.Play(newSong);

                        if (ChannelSession.Settings.SongPlayedCommand != null)
                        {
                            Dictionary<string, string> specialIdentifiers = new Dictionary<string, string>()
                            {
                                { "songtitle", newSong.Name },
                                { "songalbumimage", newSong.AlbumImage }
                            };
                            await ChannelSession.Settings.SongPlayedCommand.Perform(newSong.User, arguments: null, extraSpecialIdentifiers: specialIdentifiers);
                        }

                        this.backgroundSongThreadCancellationTokenSource = new CancellationTokenSource();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        this.backgroundSongThread = Task.Run(async () => { await this.BackgroundSongMaintainer(this.backgroundSongThreadCancellationTokenSource); }, this.backgroundSongThreadCancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    }
                }
            }
            GlobalEvents.SongRequestsChangedOccurred();
        }

        private async Task RefreshVolumeInternal()
        {
            foreach (ISongRequestProviderService provider in this.enabledProviders)
            {
                await provider.SetVolume(ChannelSession.Settings.SongRequestVolume);
            }
            GlobalEvents.SongRequestsChangedOccurred();
        }

        private async Task<SongRequestCurrentlyPlayingModel> GetStatus()
        {
            if (this.Status != null)
            {
                foreach (ISongRequestProviderService provider in this.enabledProviders)
                {
                    if (this.Status.Type == provider.Type)
                    {
                        return await provider.GetStatus();
                    }
                }
            }
            return null;
        }

        private async Task LoadBackupPlaylistSongs()
        {
            this.playlistSongs.Clear();
            if (!string.IsNullOrEmpty(ChannelSession.Settings.DefaultPlaylist))
            {
                List<Task<IEnumerable<SongRequestModel>>> playlistTasks = new List<Task<IEnumerable<SongRequestModel>>>();
                foreach (ISongRequestProviderService provider in this.enabledProviders)
                {
                    playlistTasks.Add(provider.GetPlaylist(ChannelSession.Settings.DefaultPlaylist));
                }

                IEnumerable<SongRequestModel>[] taskResults = await Task.WhenAll(playlistTasks);
                IEnumerable<SongRequestModel> results = taskResults.SelectMany(r => r);
                if (results != null)
                {
                    foreach (SongRequestModel song in results.OrderBy(s => Guid.NewGuid()))
                    {
                        song.IsFromBackupPlaylist = true;
                        this.playlistSongs.Add(song);
                    }
                }
            }
        }

        private async Task BackgroundSongMaintainer(CancellationTokenSource cancellationTokenSource)
        {
            int totalLoops = 0;
            await BackgroundTaskWrapper.RunBackgroundTask(cancellationTokenSource, async (tokenSource) =>
            {
                await Task.Delay(backgroundInterval, tokenSource.Token);

                await SongRequestService.songRequestLock.WaitAndRelease(async () =>
                {
                    if (this.Status != null && this.Status.Length > 0)
                    {
                        totalLoops++;
                        if (this.Status.State == SongRequestStateEnum.Playing)
                        {
                            this.Status.Progress += backgroundInterval;
                            if (totalLoops >= 30)
                            {
                                SongRequestCurrentlyPlayingModel newStatus = await this.GetStatus();
                                if (newStatus != null)
                                {
                                    this.Status.State = newStatus.State;
                                    this.Status.Progress = newStatus.Progress;
                                    this.Status.Length = newStatus.Length;
                                    this.Status.Volume = newStatus.Volume;
                                }
                                totalLoops = 0;
                            }
                        }

                        if (this.Status.Progress >= this.Status.Length || this.Status.State == SongRequestStateEnum.Ended)
                        {
                            await this.SkipInternal();
                        }
                    }
                    else
                    {
                        SongRequestCurrentlyPlayingModel newStatus = await this.GetStatus();
                        if (newStatus != null)
                        {
                            this.Status.State = newStatus.State;
                            this.Status.Progress = newStatus.Progress;
                            this.Status.Length = newStatus.Length;
                            this.Status.Volume = newStatus.Volume;
                        }
                    }
                });
            });
        }
    }
}
