using MixItUp.Base.Model.SongRequests;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        Task<SongRequestModel> GetStatus();

        Task Play(SongRequestModel song);
        Task Pause();
        Task Resume();
        Task PauseResume();
        Task Stop();

        Task SetVolume(int volume);
    }

    public interface ISongRequestService
    {
        ObservableCollection<SongRequestModel> RequestSongs { get; }
        ObservableCollection<SongRequestModel> PlaylistSongs { get; }

        SongRequestModel CurrentSong { get; }

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

        Task<SongRequestModel> GetCurrentlyPlaying();
        Task<SongRequestModel> GetNextTrack();

        Task RemoveSongRequest(SongRequestModel song);
        Task RemoveLastSongRequested();
        Task RemoveLastSongRequestedByUser(UserViewModel user);
        Task ClearAllRequests();
    }

    public class SongRequestService : NotifyPropertyChangedBase, ISongRequestService
    {
        private static SemaphoreSlim songRequestLock = new SemaphoreSlim(1);

        public ObservableCollection<SongRequestModel> RequestSongs { get; private set; } = new ObservableCollection<SongRequestModel>();
        public ObservableCollection<SongRequestModel> PlaylistSongs { get; private set; } = new ObservableCollection<SongRequestModel>();

        public SongRequestModel CurrentSong
        {
            get { return this.currentSong; }
            private set
            {
                this.currentSong = value;
                this.NotifyPropertyChanged();
            }
        }
        private SongRequestModel currentSong;

        private CancellationTokenSource backgroundThreadCancellationTokenSource;

        private Dictionary<UserViewModel, List<SongRequestModel>> lastUserSongSearches = new Dictionary<UserViewModel, List<SongRequestModel>>();

        private List<ISongRequestProviderService> allProviders = new List<ISongRequestProviderService>();
        private List<ISongRequestProviderService> enabledProviders = new List<ISongRequestProviderService>();

        public bool IsEnabled { get; private set; }

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
                            return false;
                        }
                        this.enabledProviders.Add(provider);
                    }
                }

                if (enabledProviders.Count() == 0)
                {
                    return false;
                }

                this.RequestSongs.Clear();
                this.PlaylistSongs.Clear();

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
                            this.PlaylistSongs.Add(song);
                        }
                    }
                }

                this.backgroundThreadCancellationTokenSource = new CancellationTokenSource();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Run(async () => { await this.PlayerBackground(); }, this.backgroundThreadCancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                this.IsEnabled = true;

                return true;
            });
        }

        public async Task Disable()
        {
            await SongRequestService.songRequestLock.WaitAndRelease(() =>
            {
                this.RequestSongs.Clear();

                if (this.backgroundThreadCancellationTokenSource != null)
                {
                    this.backgroundThreadCancellationTokenSource.Cancel();
                    this.backgroundThreadCancellationTokenSource = null;
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

        public async Task Pause()
        {
            await SongRequestService.songRequestLock.WaitAndRelease(async () =>
            {
                if (this.currentSong != null)
                {
                    foreach (ISongRequestProviderService provider in this.enabledProviders)
                    {
                        if (this.currentSong.Type == provider.Type)
                        {
                            await provider.Pause();
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
                if (this.currentSong != null)
                {
                    foreach (ISongRequestProviderService provider in this.enabledProviders)
                    {
                        if (this.currentSong.Type == provider.Type)
                        {
                            await provider.Resume();
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
                if (this.currentSong != null)
                {
                    foreach (ISongRequestProviderService provider in this.enabledProviders)
                    {
                        if (this.currentSong.Type == provider.Type)
                        {
                            await provider.PauseResume();
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
            });
            GlobalEvents.SongRequestsChangedOccurred();
        }

        public async Task<SongRequestModel> GetCurrentlyPlaying()
        {
            return await SongRequestService.songRequestLock.WaitAndRelease(() =>
            {
                return Task.FromResult(this.currentSong);
            });
        }

        public async Task<SongRequestModel> GetNextTrack()
        {
            return await SongRequestService.songRequestLock.WaitAndRelease(() =>
            {
                if (this.RequestSongs.Count == 1 && this.PlaylistSongs.Count > 0)
                {
                    return Task.FromResult(this.PlaylistSongs.FirstOrDefault());
                }
                if (this.PlaylistSongs.Count > 0 && this.PlaylistSongs.First().Equals(this.currentSong) && this.RequestSongs.Count > 0)
                {
                    return Task.FromResult(this.RequestSongs.FirstOrDefault());
                }
                else if (this.RequestSongs.Count > 1)
                {
                    return Task.FromResult(this.RequestSongs.Skip(1).FirstOrDefault());
                }
                else if (this.PlaylistSongs.Count > 1)
                {
                    return Task.FromResult(this.PlaylistSongs.Skip(1).FirstOrDefault());
                }
                else
                {
                    return null;
                }
            });
        }

        public async Task RemoveSongRequest(SongRequestModel song)
        {
            await SongRequestService.songRequestLock.WaitAndRelease(() =>
            {
                this.RequestSongs.Remove(song);
                return Task.FromResult(0);
            });

            GlobalEvents.SongRequestsChangedOccurred();
        }

        public async Task RemoveLastSongRequested()
        {
            SongRequestModel song = null;
            await SongRequestService.songRequestLock.WaitAndRelease(() =>
            {
                song = this.RequestSongs.LastOrDefault();
                if (song != null)
                {
                    this.RequestSongs.Remove(song);
                }
                return Task.FromResult(0);
            });

            GlobalEvents.SongRequestsChangedOccurred();

            if (song != null)
            {
                await ChannelSession.Chat.SendMessage(string.Format("{0} was removed from the queue.", song.Name));
                GlobalEvents.SongRequestsChangedOccurred();
            }
        }

        public async Task RemoveLastSongRequestedByUser(UserViewModel user)
        {
            SongRequestModel song = null;
            await SongRequestService.songRequestLock.WaitAndRelease(() =>
            {
                song = this.RequestSongs.LastOrDefault(s => s.User.ID == user.ID);
                if (song != null)
                {
                    this.RequestSongs.Remove(song);
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
                this.RequestSongs.Clear();
                return Task.FromResult(0);
            });

            GlobalEvents.SongRequestsChangedOccurred();
        }

        private async Task PlayerBackground()
        {
            await BackgroundTaskWrapper.RunBackgroundTask(this.backgroundThreadCancellationTokenSource, async (tokenSource) =>
            {
                tokenSource.Token.ThrowIfCancellationRequested();

                bool changeOccurred = false;

                await SongRequestService.songRequestLock.WaitAndRelease(async () =>
                {
                    if (this.currentSong == null)
                    {
                        await this.SkipInternal();
                        changeOccurred = true;
                    }
                    else
                    {
                        SongRequestModel status = null;
                        foreach (ISongRequestProviderService provider in this.enabledProviders)
                        {
                            if (this.currentSong.Type == provider.Type)
                            {
                                status = await provider.GetStatus();
                            }
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
                            await this.SkipInternal();
                            changeOccurred = true;
                        }
                    }
                });

                if (changeOccurred)
                {
                    GlobalEvents.SongRequestsChangedOccurred();
                    await Task.Delay(2500, tokenSource.Token);
                }

                await Task.Delay(2000, tokenSource.Token);
            });
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

            await SongRequestService.songRequestLock.WaitAndRelease(() =>
            {
                this.RequestSongs.Add(song);
                return Task.FromResult(0);
            });

            ChannelSession.Services.Telemetry.TrackSongRequest(song.Type);

            await ChannelSession.Chat.SendMessage(string.Format("{0} was added to the queue.", song.Name));
            GlobalEvents.SongRequestsChangedOccurred();
        }

        private async Task PlaySongInternal(SongRequestModel song)
        {
            if (song != null)
            {
                foreach (ISongRequestProviderService provider in this.enabledProviders)
                {
                    if (this.currentSong.Type == provider.Type)
                    {
                        await provider.Play(song);
                    }
                }
            }
        }

        private async Task SkipInternal()
        {
            if (this.RequestSongs.Count > 0 && this.RequestSongs.First().Equals(this.currentSong))
            {
                this.RequestSongs.RemoveAt(0);
            }
            else if (this.PlaylistSongs.Count > 0 && this.PlaylistSongs.First().Equals(this.currentSong))
            {
                this.PlaylistSongs.RemoveAt(0);              
            }

            if (this.currentSong != null)
            {
                foreach (ISongRequestProviderService provider in this.enabledProviders)
                {
                    if (this.currentSong.Type == provider.Type)
                    {
                        await provider.Stop();
                    }
                }
                this.currentSong = null;
            }

            SongRequestModel newSong = null;
            if (this.RequestSongs.Count > 0)
            {
                newSong = this.RequestSongs.First();
            }
            else if (this.PlaylistSongs.Count > 0)
            {
                newSong = this.PlaylistSongs.First();
            }

            if (newSong != null)
            {
                await this.PlaySongInternal(newSong);
            }
        }

        private async Task RefreshVolumeInternal()
        {
            foreach (ISongRequestProviderService provider in this.enabledProviders)
            {
                await provider.SetVolume(ChannelSession.Settings.SongRequestVolume);
            }
        }
    }
}
