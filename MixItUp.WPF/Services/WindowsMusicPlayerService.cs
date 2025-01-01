using Id3;
using MixItUp.Base;
using MixItUp.Base.Model;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.WPF.Services
{
    public class WindowsMusicPlayerService : IMusicPlayerService
    {
        public event EventHandler SongChanged = delegate { };

        public MusicPlayerState State { get; private set; }

        public int Volume
        {
            get { return ChannelSession.Settings.MusicPlayerVolume; }
            set { ChannelSession.Settings.MusicPlayerVolume = value; }
        }

        public MusicPlayerSong CurrentSong
        {
            get
            {
                if (0 <= this.currentSongIndex && this.currentSongIndex < this.songs.Count)
                {
                    return this.songs[this.currentSongIndex];
                }
                return null;
            }
        }

        public ThreadSafeObservableCollection<MusicPlayerSong> Songs { get { return this.songs; } }

        private ThreadSafeObservableCollection<MusicPlayerSong> songs = new ThreadSafeObservableCollection<MusicPlayerSong>();
        private int currentSongIndex = 0;

        private CancellationTokenSource backgroundPlayThreadTokenSource = new CancellationTokenSource();
        private WaveOutEvent currentWaveOutEvent;
        private WaveStream currentWaveStream;

        private SemaphoreSlim sempahore = new SemaphoreSlim(1); 

        public async Task Play()
        {
            if (this.songs.Count == 0)
            {
                await this.LoadSongs();
            }

            if (this.songs.Count > 0)
            {
                if (this.State == MusicPlayerState.Paused)
                {
                    try
                    {
                        await this.sempahore.WaitAsync();

                        this.State = MusicPlayerState.Playing;
                        if (this.currentWaveOutEvent != null)
                        {
                            this.currentWaveOutEvent.Play();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                    }
                    finally
                    {
                        this.sempahore.Release();
                    }
                }
                else if (this.State == MusicPlayerState.Stopped)
                {
                    try
                    {
                        await this.sempahore.WaitAsync();

                        this.State = MusicPlayerState.Playing;
                        this.PlayInternal(this.CurrentSong.FilePath);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                    }
                    finally
                    {
                        this.sempahore.Release();
                    }

                    this.SongChanged.Invoke(this, new EventArgs());

                    await ServiceManager.Get<CommandService>().Queue(ChannelSession.Settings.MusicPlayerOnSongChangedCommandID, new CommandParametersModel(ChannelSession.User, platform: StreamingPlatformTypeEnum.All));
                }
            }
        }

        public async Task Pause()
        {
            if (this.State == MusicPlayerState.Playing)
            {
                try
                {
                    await this.sempahore.WaitAsync();

                    this.State = MusicPlayerState.Paused;
                    if (this.currentWaveOutEvent != null)
                    {
                        this.currentWaveOutEvent.Pause();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
                finally
                {
                    this.sempahore.Release();
                }
            }
        }

        public async Task Stop()
        {
            try
            {
                await this.sempahore.WaitAsync();

                this.State = MusicPlayerState.Stopped;

                if (this.currentWaveOutEvent != null)
                {
                    this.currentWaveOutEvent.Stop();
                }
                this.currentWaveOutEvent = null;

                if (this.backgroundPlayThreadTokenSource != null)
                {
                    this.backgroundPlayThreadTokenSource.Cancel();
                }
                this.backgroundPlayThreadTokenSource = null;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            finally
            {
                this.sempahore.Release();
            }
        }

        public async Task Next()
        {
            await this.Stop();

            try
            {
                await this.sempahore.WaitAsync();

                this.currentSongIndex++;
                if (this.currentSongIndex >= this.songs.Count)
                {
                    this.currentSongIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            finally
            {
                this.sempahore.Release();
            }

            await this.Play();
        }

        public async Task Previous()
        {
            await this.Stop();

            try
            {
                await this.sempahore.WaitAsync();

                this.currentSongIndex--;
                if (this.currentSongIndex < 0)
                {
                    this.currentSongIndex = Math.Max(this.songs.Count - 1, 0);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            finally
            {
                this.sempahore.Release();
            }

            await this.Play();
        }

        public async Task ChangeVolume(int amount)
        {
            try
            {
                await this.sempahore.WaitAsync();

                this.Volume = amount;
                if (this.currentWaveStream != null && this.currentWaveStream is AudioFileReader)
                {
                    ((AudioFileReader)this.currentWaveStream).Volume = (ServiceManager.Get<IAudioService>() as WindowsAudioService).ConvertVolumeAmount(this.Volume);
                }
                else if (this.currentWaveOutEvent != null)
                {
                    this.currentWaveOutEvent.Volume = (ServiceManager.Get<IAudioService>() as WindowsAudioService).ConvertVolumeAmount(this.Volume);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            finally
            {
                this.sempahore.Release();
            }
        }

        public async Task ChangeFolder(string folderPath)
        {
            ChannelSession.Settings.MusicPlayerFolders.Clear();
            ChannelSession.Settings.MusicPlayerFolders.Add(folderPath);

            await ServiceManager.Get<IMusicPlayerService>().LoadSongs();
        }

        public async Task LoadSongs()
        {
            await Task.Run(async () =>
            {
                ISet<string> allowedFileExtensions = ServiceManager.Get<IAudioService>().ApplicableAudioFileExtensions;
                WindowsFileService fileService = ServiceManager.Get<IFileService>() as WindowsFileService;
                List<string> files = new List<string>();
                foreach (string folder in ChannelSession.Settings.MusicPlayerFolders)
                {
                    await this.AddFilesFromDirectory(fileService, allowedFileExtensions, files, folder);
                }

                List<MusicPlayerSong> tempSongs = new List<MusicPlayerSong>();
                foreach (string file in files)
                {
                    MusicPlayerSong song = null;
                    try
                    {
                        using (var mp3 = new Mp3(file))
                        {
                            var v2Tags = mp3.GetTag(Id3TagFamily.Version2X);
                            if (v2Tags != null)
                            {
                                song = new MusicPlayerSong()
                                {
                                    FilePath = file,
                                    Title = v2Tags.Title.Value,
                                    Length = v2Tags.Length.IsAssigned ? (int)v2Tags.Length.Value.TotalSeconds : 0
                                };

                                if (v2Tags.Artists.IsAssigned && v2Tags.Artists.Value.Count > 0)
                                {
                                    song.Artist = string.Join(", ", v2Tags.Artists.Value);
                                }
                                else if (v2Tags.Band.IsAssigned)
                                {
                                    song.Artist = v2Tags.Band.Value;
                                }
                                else if (v2Tags.Composers.IsAssigned && v2Tags.Composers.Value.Count > 0)
                                {
                                    song.Artist = string.Join(", ", v2Tags.Artists.Value);
                                }
                            }
                            else
                            {
                                var v1Tags = mp3.GetTag(Id3TagFamily.Version1X);
                                if (v1Tags != null)
                                {
                                    song = new MusicPlayerSong()
                                    {
                                        FilePath = file,
                                        Title = v1Tags.Title.Value,
                                        Length = v1Tags.Length.IsAssigned ? (int)v1Tags.Length.Value.TotalSeconds : 0
                                    };

                                    if (v1Tags.Artists.IsAssigned && v1Tags.Artists.Value.Count > 0)
                                    {
                                        song.Artist = string.Join(", ", v1Tags.Artists.Value);
                                    }
                                    else if (v1Tags.Band.IsAssigned)
                                    {
                                        song.Artist = v1Tags.Band.Value;
                                    }
                                    else if (v1Tags.Composers.IsAssigned && v1Tags.Composers.Value.Count > 0)
                                    {
                                        song.Artist = string.Join(", ", v1Tags.Artists.Value);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                    }

                    if (song == null)
                    {
                        song = new MusicPlayerSong() { FilePath = file, Title = Path.GetFileNameWithoutExtension(file) };
                    }
                    tempSongs.Add(song);
                }

                try
                {
                    await this.sempahore.WaitAsync();

                    this.songs.Clear();
                    foreach (MusicPlayerSong song in tempSongs.Shuffle())
                    {
                        this.songs.Add(song);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
                finally
                {
                    this.sempahore.Release();
                }
            });
        }

        public async Task<MusicPlayerSong> SearchAndPlaySong(string searchText)
        {
            MusicPlayerSong song = null;

            var songs = this.songs.Where(s => s.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase));
            if (songs != null && songs.Count() > 0)
            {
                song = songs.OrderBy(s => s.Title.Length).First();
            }

            if (song != null)
            {
                await this.Stop();
                this.currentSongIndex = this.songs.IndexOf(song);
                await this.Play();
            }

            return song;
        }

        private void PlayInternal(string filePath)
        {
            if (this.backgroundPlayThreadTokenSource != null)
            {
                this.backgroundPlayThreadTokenSource.Cancel();
            }
            this.backgroundPlayThreadTokenSource = new CancellationTokenSource();

            WindowsAudioService audioService = ServiceManager.Get<IAudioService>() as WindowsAudioService;
            Tuple<WaveOutEvent, WaveStream> output = audioService.PlayWithOutput(filePath, this.Volume, ChannelSession.Settings.MusicPlayerAudioOutput);
            this.currentWaveOutEvent = output.Item1;
            this.currentWaveStream = output.Item2;
            Task backgroundPlayThreadTask = Task.Run(async () => await this.PlayBackground(this.currentWaveOutEvent, this.currentSongIndex), this.backgroundPlayThreadTokenSource.Token);
        }

        private async Task PlayBackground(WaveOutEvent waveOutEvent, int songIndex)
        {
            using (waveOutEvent)
            {
                while (waveOutEvent != null && (waveOutEvent.PlaybackState == PlaybackState.Playing || waveOutEvent.PlaybackState == PlaybackState.Paused))
                {
                    await Task.Delay(500);
                }
                waveOutEvent.Dispose();

                if (this.currentSongIndex == songIndex && this.State == MusicPlayerState.Playing)
                {
                    await this.Next();
                }
            }
        }

        private async Task AddFilesFromDirectory(WindowsFileService fileService, ISet<string> allowedFileExtensions, List<string> files, string path)
        {
            foreach (string file in await fileService.GetFilesInDirectory(path))
            {
                string extension = Path.GetExtension(file).ToLower();
                if (allowedFileExtensions.Contains(extension))
                {
                    files.Add(file);
                }
            }

            foreach (string subFolder in await fileService.GetFoldersInDirectory(path))
            {
                await this.AddFilesFromDirectory(fileService, allowedFileExtensions, files, subFolder);
            }
        }
    }
}
