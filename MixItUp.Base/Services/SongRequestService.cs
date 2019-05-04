using MixItUp.Base.Model.SongRequests;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
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

        Task Play(SongRequestModel song);
        Task Pause();
        Task Resume();
        Task PauseResume();
        Task Stop();

        Task SetVolume(int volume);
    }

    public interface ISongRequestService
    {
        List<SongRequestModel> RequestSongs { get; }

        SongRequestCurrentlyPlayingModel Status { get; }

        bool IsEnabled { get; }

        void AddProvider(ISongRequestProviderService provider);

        Task<bool> Initialize();

        Task Disable();

        Task SearchAndPickFirst(UserViewModel user, SongRequestServiceTypeEnum service, string identifier);
        Task SearchAndSelect(UserViewModel user, SongRequestServiceTypeEnum service, string identifier);

        Task Pause();
        Task Resume();
        Task PauseResume();
        Task Skip();

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

    public class SongRequestService : NotifyPropertyChangedBase, ISongRequestService
    {
        private static SemaphoreSlim songRequestLock = new SemaphoreSlim(1);

        private const int backgroundInterval = 2000;

        public List<SongRequestModel> RequestSongs { get; private set; } = new List<SongRequestModel>();
        private List<SongRequestModel> playlistSongs = new List<SongRequestModel>();

        public SongRequestCurrentlyPlayingModel Status
        {
            get { return this.status; }
            private set
            {
                this.status = value;
                this.NotifyPropertyChanged();
            }
        }
        private SongRequestCurrentlyPlayingModel status;

        private Task backgroundSongThread = null;
        private CancellationTokenSource backgroundSongThreadCancellationTokenSource;

        private Dictionary<UserViewModel, List<SongRequestModel>> lastUserSongSearches = new Dictionary<UserViewModel, List<SongRequestModel>>();

        private List<ISongRequestProviderService> allProviders = new List<ISongRequestProviderService>();
        private List<ISongRequestProviderService> enabledProviders = new List<ISongRequestProviderService>();

        public bool IsEnabled { get; private set; }

        private bool forceStateQuery = false;

        public SongRequestService() { }

        public void AddProvider(ISongRequestProviderService provider)
        {
            this.allProviders.Add(provider);
        }

        public async Task<bool> Initialize()
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
                                    + Environment.NewLine + Environment.NewLine + "This is required to be done everytime to let Spotify know that where to send our song requests to.");
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
                    await DialogHelper.ShowMessage("At least 1 song request service must be set");
                    return false;
                }

                await ChannelSession.SaveSettings();

                this.RequestSongs.Clear();
                this.playlistSongs.Clear();

                await this.RefreshVolumeInternal();

                await this.SkipInternal();

                this.IsEnabled = true;

                return true;
            });
        }

        public async Task Disable()
        {
            await SongRequestService.songRequestLock.WaitAndRelease(() =>
            {
                this.RequestSongs.Clear();

                if (this.backgroundSongThreadCancellationTokenSource != null)
                {
                    this.backgroundSongThreadCancellationTokenSource.Cancel();
                    this.backgroundSongThreadCancellationTokenSource = null;
                }

                this.IsEnabled = false;
                return Task.FromResult(0);
            });
            GlobalEvents.SongRequestsChangedOccurred();
        }

        public async Task SearchAndPickFirst(UserViewModel user, SongRequestServiceTypeEnum service, string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                await ChannelSession.Chat.Whisper(user.UserName, "You must specify your request with the command. For details on how to request songs, check out: https://github.com/SaviorXTanren/mixer-mixitup/wiki/Song-Requests#requesting-songs");
                return;
            }

            IEnumerable<SongRequestModel> results = await this.Search(service, identifier);
            if (results != null && results.Count() > 0)
            {
                await this.AddSongRequest(user, results.First());
            }
            else
            {
                await ChannelSession.Chat.Whisper(user.UserName, "We were unable to find a song that matched you request. For details on how to request songs, check out: https://github.com/SaviorXTanren/mixer-mixitup/wiki/Song-Requests#requesting-songs");
            }
        }

        public async Task SearchAndSelect(UserViewModel user, SongRequestServiceTypeEnum service, string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                await ChannelSession.Chat.Whisper(user.UserName, "You must specify your request with the command. For details on how to request songs, check out: https://github.com/SaviorXTanren/mixer-mixitup/wiki/Song-Requests#requesting-songs");
                return;
            }

            if (this.lastUserSongSearches.ContainsKey(user) && int.TryParse(identifier, out int songIndex) && songIndex >= 0 && songIndex < this.lastUserSongSearches[user].Count)
            {
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
                        await ChannelSession.Chat.Whisper(user.UserName, "Multiple results found, please re-run this command with a space & the number of the song: " + string.Join(",  ", resultsStrings));
                    }
                }
                else
                {
                    await ChannelSession.Chat.Whisper(user.UserName, "We were unable to find a song that matched you request. For details on how to request songs, check out: https://github.com/SaviorXTanren/mixer-mixitup/wiki/Song-Requests#requesting-songs");
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
                            this.forceStateQuery = true;
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
                            this.forceStateQuery = true;
                        }
                    }
                }
            });
            GlobalEvents.SongRequestsChangedOccurred();
        }

        public async Task PauseResume()
        {
            await SongRequestService.songRequestLock.WaitAndRelease(async () =>
            {
                if (this.Status != null)
                {
                    foreach (ISongRequestProviderService provider in this.enabledProviders)
                    {
                        if (this.Status.Type == provider.Type)
                        {
                            await provider.PauseResume();
                            this.forceStateQuery = true;
                        }
                    }
                }
            });
            GlobalEvents.SongRequestsChangedOccurred();
        }

        public async Task Skip()
        {
            await SongRequestService.songRequestLock.WaitAndRelease(async () =>
            {
                await this.SkipInternal();
            });
            GlobalEvents.SongRequestsChangedOccurred();
        }

        public async Task RefreshVolume()
        {
            await SongRequestService.songRequestLock.WaitAndRelease(async () =>
            {
                await this.RefreshVolumeInternal();
                this.forceStateQuery = true;
            });
            GlobalEvents.SongRequestsChangedOccurred();
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
                if (this.RequestSongs.Count > 0)
                {
                    return Task.FromResult(this.RequestSongs.FirstOrDefault());
                }
                else if (this.playlistSongs.Count > 0)
                {
                    return Task.FromResult(this.playlistSongs.FirstOrDefault());
                }
                else
                {
                    return null;
                }
            });
        }

        public async Task MoveUp(SongRequestModel song)
        {
            await SongRequestService.songRequestLock.WaitAndRelease(() =>
            {
                int index = this.RequestSongs.IndexOf(song) - 1;
                if (index >= 0)
                {
                    this.RequestSongs.Remove(song);
                    this.RequestSongs.Insert(index, song);
                }
                return Task.FromResult(0);
            });
            GlobalEvents.SongRequestsChangedOccurred();
        }

        public async Task MoveDown(SongRequestModel song)
        {
            await SongRequestService.songRequestLock.WaitAndRelease(() =>
            {
                int index = this.RequestSongs.IndexOf(song) + 1;
                if (index < this.RequestSongs.Count)
                {
                    this.RequestSongs.Remove(song);
                    this.RequestSongs.Insert(index, song);
                }
                return Task.FromResult(0);
            });
            GlobalEvents.SongRequestsChangedOccurred();
        }

        public async Task Remove(SongRequestModel song)
        {
            await SongRequestService.songRequestLock.WaitAndRelease(() =>
            {
                this.RequestSongs.Remove(song);
                return Task.FromResult(0);
            });
            GlobalEvents.SongRequestsChangedOccurred();
        }

        public async Task RemoveLastRequested()
        {
            SongRequestModel song = null;
            await SongRequestService.songRequestLock.WaitAndRelease(() =>
            {
                song = this.RequestSongs.LastOrDefault();
                return Task.FromResult(0);
            });

            if (song != null)
            {
                await this.Remove(song);
            }

            if (song != null)
            {
                await ChannelSession.Chat.SendMessage(string.Format("{0} was removed from the queue.", song.Name));
            }

            GlobalEvents.SongRequestsChangedOccurred();
        }

        public async Task RemoveLastRequested(UserViewModel user)
        {
            SongRequestModel song = null;
            await SongRequestService.songRequestLock.WaitAndRelease(() =>
            {
                song = this.RequestSongs.LastOrDefault(s => s.User.ID == user.ID);
                return Task.FromResult(0);
            });

            if (song != null)
            {
                await this.Remove(song);
            }

            if (song != null)
            {
                await ChannelSession.Chat.SendMessage(string.Format("{0} was removed from the queue.", song.Name));
            }
            else
            {
                await ChannelSession.Chat.Whisper(user.UserName, string.Format("You have no songs in the queue.", song.Name));
            }

            GlobalEvents.SongRequestsChangedOccurred();
        }

        public async Task ClearAll()
        {
            await SongRequestService.songRequestLock.WaitAndRelease(() =>
            {
                this.RequestSongs.Clear();
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

            bool isSongCurrentlyPlaying = false;
            await SongRequestService.songRequestLock.WaitAndRelease(() =>
            {
                isSongCurrentlyPlaying = (this.Status != null);

                if (ChannelSession.Settings.SongRequestSubPriority && user.IsMixerSubscriber)
                {
                    for (int i = 0; i < this.RequestSongs.Count; i++)
                    {
                        if (!this.RequestSongs[i].User.IsMixerSubscriber)
                        {
                            this.RequestSongs.Insert(i, song);
                            return Task.FromResult(0);
                        }
                    }
                }
                this.RequestSongs.Add(song);
                return Task.FromResult(0);
            });

            if (!isSongCurrentlyPlaying)
            {
                await this.Skip();
            }

            ChannelSession.Services.Telemetry.TrackSongRequest(song.Type);

            await ChannelSession.Chat.SendMessage(string.Format("{0} was added to the queue.", song.Name));
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
            if (this.RequestSongs.Count > 0)
            {
                newSong = this.RequestSongs.First();
                this.RequestSongs.RemoveAt(0);
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
                        await provider.Play(newSong);
                        this.Status = new SongRequestCurrentlyPlayingModel()
                        {
                            ID = newSong.ID,
                            URI = newSong.URI,
                            Name = newSong.Name,
                            AlbumImage = newSong.AlbumImage,
                            Type = newSong.Type,
                            IsFromBackupPlaylist = newSong.IsFromBackupPlaylist,
                            User = newSong.User,
                            State = SongRequestStateEnum.NotStarted,
                        };
                        this.forceStateQuery = true;

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
            foreach (ISongRequestProviderService provider in this.enabledProviders)
            {
                if (this.Status.Type == provider.Type)
                {
                    return await provider.GetStatus();
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
            bool failedToPlay = false;
            await BackgroundTaskWrapper.RunBackgroundTask(cancellationTokenSource, async (tokenSource) =>
            {
                await Task.Delay(backgroundInterval, tokenSource.Token);

                await SongRequestService.songRequestLock.WaitAndRelease(async () =>
                {
                    if (this.Status != null && this.Status.State == SongRequestStateEnum.Playing)
                    {
                        this.Status.Progress += backgroundInterval;
                    }

                    if (this.forceStateQuery || this.Status == null || (this.Status.Length > 0 && this.Status.Progress >= this.Status.Length))
                    {
                        this.forceStateQuery = false;
                        SongRequestCurrentlyPlayingModel newStatus = await this.GetStatus();
                        if (newStatus == null || newStatus.State == SongRequestStateEnum.NotStarted)
                        {
                            if (failedToPlay)
                            {
                                await this.SkipInternal();
                            }
                            else
                            {
                                failedToPlay = true;
                                this.forceStateQuery = true;
                            }
                        }
                        else
                        {
                            this.Status.State = newStatus.State;
                            this.Status.Progress = newStatus.Progress;
                            this.Status.Length = newStatus.Length;
                            this.Status.Volume = newStatus.Volume;

                            if (this.Status.State == SongRequestStateEnum.Ended)
                            {
                                await this.SkipInternal();
                            }
                            else if (this.Status.Volume != ChannelSession.Settings.SongRequestVolume)
                            {
                                await this.RefreshVolumeInternal();
                            }
                        }
                    }
                });
            });
        }
    }
}
