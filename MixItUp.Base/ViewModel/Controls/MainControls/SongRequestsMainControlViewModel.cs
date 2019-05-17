using MixItUp.Base.Model.SongRequests;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Window;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.MainControls
{
    public class SongRequestsMainControlViewModel : MainControlViewModelBase
    {
        public bool IsEnabled { get { return ChannelSession.Services.SongRequestService.IsEnabled; } }

        public string EnableDisableButtonText { get { return (this.IsEnabled) ? "Disable" : "Enable"; } }

        public string SongListType { get { return (this.currentlyPlaying != null && this.currentlyPlaying.IsFromBackupPlaylist) ? "Playlist Song:" : "Request Song:"; } }
        public string SongName { get { return (this.currentlyPlaying != null) ? this.currentlyPlaying.Name : "None"; } }
        private SongRequestModel currentlyPlaying;

        public int Volume
        {
            get { return ChannelSession.Settings.SongRequestVolume; }
            set
            {
                ChannelSession.Settings.SongRequestVolume = value;
                this.NotifyPropertyChanged();
            }
        }

        public ObservableCollection<SongRequestModel> RequestSongs { get; private set; } = new ObservableCollection<SongRequestModel>();

        public ICommand EnableDisableCommand { get; private set; }
        public ICommand PauseResumeCommand { get; private set; }
        public ICommand NextCommand { get; private set; }
        public ICommand BanCurrentCommand { get; private set; }
        public ICommand MoveUpCommand { get; private set; }
        public ICommand MoveDownCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand BanCommand { get; private set; }
        public ICommand ClearQueueCommand { get; private set; }

        public SongRequestsMainControlViewModel(MainWindowViewModel windowViewModel)
            : base(windowViewModel)
        {
            GlobalEvents.OnSongRequestsChangedOccurred += GlobalEvents_OnSongRequestsChangedOccurred;

            this.EnableDisableCommand = this.CreateCommand(async (x) =>
            {
                if (this.IsEnabled)
                {
                    await ChannelSession.Services.SongRequestService.Disable();
                }
                else
                {
                    await ChannelSession.Services.SongRequestService.Enable();
                }
                this.NotifyPropertyChanges();
            });

            this.PauseResumeCommand = this.CreateCommand(async (x) =>
            {
                await ChannelSession.Services.SongRequestService.PauseResume();
                this.NotifyPropertyChanges();
            });

            this.NextCommand = this.CreateCommand(async (x) =>
            {
                await ChannelSession.Services.SongRequestService.Skip();
                this.NotifyPropertyChanges();
            });

            this.BanCurrentCommand = this.CreateCommand(async (x) =>
            {
                if (await DialogHelper.ShowConfirmation("Are you sure you want to ban this song?"))
                {
                    await ChannelSession.Services.SongRequestService.Ban();
                    this.NotifyPropertyChanges();
                }
            });

            this.MoveUpCommand = this.CreateCommand(async (song) =>
            {
                await ChannelSession.Services.SongRequestService.MoveUp((SongRequestModel)song);
                this.NotifyPropertyChanges();
            });

            this.MoveDownCommand = this.CreateCommand(async (song) =>
            {
                await ChannelSession.Services.SongRequestService.MoveDown((SongRequestModel)song);
                this.NotifyPropertyChanges();
            });

            this.DeleteCommand = this.CreateCommand(async (song) =>
            {
                await ChannelSession.Services.SongRequestService.Remove((SongRequestModel)song);
                this.NotifyPropertyChanges();
            });

            this.BanCommand = this.CreateCommand(async (song) =>
            {
                if (await DialogHelper.ShowConfirmation("Are you sure you want to ban this song?"))
                {
                    await ChannelSession.Services.SongRequestService.Ban((SongRequestModel)song);
                    this.NotifyPropertyChanges();
                }
            });

            this.ClearQueueCommand = this.CreateCommand(async (x) =>
            {
                if (await DialogHelper.ShowConfirmation("Are you sure you want to clear the Song Request queue?"))
                {
                    await ChannelSession.Services.SongRequestService.ClearAll();
                    this.NotifyPropertyChanges();
                }
            });
        }

        protected override async Task OnLoadedInternal()
        {
            await ChannelSession.Services.SongRequestService.Initialize();
        }

        private async void GlobalEvents_OnSongRequestsChangedOccurred(object sender, EventArgs e)
        {
            this.currentlyPlaying = await ChannelSession.Services.SongRequestService.GetCurrent();

            await DispatcherHelper.InvokeDispatcher(() =>
            {
                this.RequestSongs.Clear();
                foreach (SongRequestModel songRequest in ChannelSession.Services.SongRequestService.RequestSongs.ToList())
                {
                    this.RequestSongs.Add(songRequest);
                }
                return Task.FromResult(0);
            });

            this.NotifyPropertyChanges();
        }

        private void NotifyPropertyChanges()
        {
            this.NotifyPropertyChanged("IsEnabled");
            this.NotifyPropertyChanged("EnableDisableButtonText");
            this.NotifyPropertyChanged("SongListType");
            this.NotifyPropertyChanged("SongName");
            this.NotifyPropertyChanged("Volume");
        }
    }
}
