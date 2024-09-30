using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.MainControls
{
    public class MusicPlayerMainControlViewModel : WindowControlViewModelBase
    {
        public ThreadSafeObservableCollection<MusicPlayerSong> Songs { get { return ServiceManager.Get<IMusicPlayerService>().Songs; } }

        public bool MusicLoaded { get { return this.Songs != null && this.Songs.Count > 0; } }

        public bool MusicNotLoaded { get { return !this.MusicLoaded; } }

        public string CurrentlyPlayingSong
        {
            get
            {
                MusicPlayerSong song = ServiceManager.Get<IMusicPlayerService>().CurrentSong;
                if (song != null)
                {
                    return song.ToString();
                }
                else
                {
                    return Resources.None;
                }
            }
        }

        public ICommand PreviousCommand { get; private set; }
        public ICommand PlayPauseCommand { get; private set; }
        public ICommand StopCommand { get; private set; }
        public ICommand NextCommand { get; private set; }

        public int Volume
        {
            get { return ServiceManager.Get<IMusicPlayerService>().Volume; }
            set
            {
                ServiceManager.Get<IMusicPlayerService>().ChangeVolume(value).Wait();
                this.NotifyPropertyChanged();
            }
        }

        public ObservableCollection<string> AudioDevices { get; set; } = new ObservableCollection<string>();

        public string SelectedAudioDevice
        {
            get { return ChannelSession.Settings.MusicPlayerAudioOutput; }
            set
            {
                ChannelSession.Settings.MusicPlayerAudioOutput = value;
                this.NotifyPropertyChanged();
            }
        }

        public ICommand SetFolderCommand { get; private set; }

        public CommandModelBase OnSongChangedCommand
        {
            get { return this.onSongChangedCommand; }
            set
            {
                this.onSongChangedCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CommandModelBase onSongChangedCommand;

        public MusicPlayerMainControlViewModel(MainWindowViewModel windowViewModel)
            : base(windowViewModel)
        {
            ServiceManager.Get<IMusicPlayerService>().SongChanged += MusicPlayerMainControlViewModel_SongChanged;

            this.OnSongChangedCommand = ChannelSession.Settings.GetCommand(ChannelSession.Settings.MusicPlayerOnSongChangedCommandID);

            this.AudioDevices.AddRange(ServiceManager.Get<IAudioService>().GetSelectableAudioDevices());
            this.AudioDevices.Remove(ServiceManager.Get<IAudioService>().MixItUpOverlay);
            this.SelectedAudioDevice = (ChannelSession.Settings.MusicPlayerAudioOutput != null) ? ChannelSession.Settings.MusicPlayerAudioOutput : ServiceManager.Get<IAudioService>().DefaultAudioDevice;

            this.PreviousCommand = this.CreateCommand(async () =>
            {
                await ServiceManager.Get<IMusicPlayerService>().Previous();
            });

            this.PlayPauseCommand = this.CreateCommand(async () =>
            {
                if (ServiceManager.Get<IMusicPlayerService>().State == MusicPlayerState.Playing)
                {
                    await ServiceManager.Get<IMusicPlayerService>().Pause();
                }
                else
                {
                    await ServiceManager.Get<IMusicPlayerService>().Play();
                }
            });

            this.StopCommand = this.CreateCommand(async () =>
            {
                await ServiceManager.Get<IMusicPlayerService>().Stop();
            });

            this.NextCommand = this.CreateCommand(async () =>
            {
                await ServiceManager.Get<IMusicPlayerService>().Next();
            });

            this.SetFolderCommand = this.CreateCommand(async () =>
            {
                string folderPath = ServiceManager.Get<IFileService>().ShowOpenFolderDialog();

                await ServiceManager.Get<IMusicPlayerService>().ChangeFolder(folderPath);

                this.NotifyPropertyChanged(nameof(this.MusicLoaded));
                this.NotifyPropertyChanged(nameof(this.MusicNotLoaded));
            });
        }

        private void MusicPlayerMainControlViewModel_SongChanged(object sender, System.EventArgs e)
        {
            this.NotifyPropertyChanged(nameof(this.CurrentlyPlayingSong));
            this.NotifyPropertyChanged(nameof(this.MusicLoaded));
            this.NotifyPropertyChanged(nameof(this.MusicNotLoaded));
        }

        protected override async Task OnOpenInternal()
        {
            await base.OnOpenInternal();
        }

        protected override async Task OnVisibleInternal()
        {
            await base.OnVisibleInternal();
        }
    }
}
