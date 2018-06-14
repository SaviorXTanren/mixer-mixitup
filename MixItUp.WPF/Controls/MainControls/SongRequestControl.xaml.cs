using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Desktop.Services;
using MixItUp.WPF.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for SongRequestControl.xaml
    /// </summary>
    public partial class SongRequestControl : MainControlBase, IDisposable
    {
        private static SemaphoreSlim songListLock = new SemaphoreSlim(1);

        private ObservableCollection<SongRequestItem> requests = new ObservableCollection<SongRequestItem>();

        private CancellationTokenSource backgroundThreadCancellationTokenSource = new CancellationTokenSource();

        public SongRequestControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            GlobalEvents.OnSongRequestsChangedOccurred += GlobalEvents_OnSongRequestsChangedOccurred;

            this.SongRequestsQueueListView.ItemsSource = this.requests;

            this.SpotifyToggleButton.IsChecked = ChannelSession.Settings.SongRequestServiceTypes.Contains(SongRequestServiceTypeEnum.Spotify);
            this.YouTubeToggleButton.IsChecked = ChannelSession.Settings.SongRequestServiceTypes.Contains(SongRequestServiceTypeEnum.YouTube);
            this.SoundCloudToggleButton.IsChecked = ChannelSession.Settings.SongRequestServiceTypes.Contains(SongRequestServiceTypeEnum.SoundCloud);

            this.SpotifyAllowExplicitSongToggleButton.IsChecked = ChannelSession.Settings.SpotifyAllowExplicit;

            await this.RefreshRequestsList();

            await base.InitializeInternal();
        }

        private async void EnableSongRequestsToggleButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                if (!this.SpotifyToggleButton.IsChecked.GetValueOrDefault() && !this.YouTubeToggleButton.IsChecked.GetValueOrDefault() &&
                    !this.SoundCloudToggleButton.IsChecked.GetValueOrDefault())
                {
                    await MessageBoxHelper.ShowMessageDialog("At least 1 song request service must be set");
                    this.EnableSongRequestsToggleButton.IsChecked = false;
                    return;
                }

                if (this.SpotifyToggleButton.IsChecked.GetValueOrDefault() && ChannelSession.Services.Spotify == null)
                {
                    await MessageBoxHelper.ShowMessageDialog("You must connect to your Spotify account in the Services area");
                    this.EnableSongRequestsToggleButton.IsChecked = false;
                    return;
                }

                if (this.YouTubeToggleButton.IsChecked.GetValueOrDefault() && ChannelSession.Services.OverlayServer == null)
                {
                    await MessageBoxHelper.ShowMessageDialog("You must enable & use the Mix It Up Overlay for YouTube song requests");
                    this.EnableSongRequestsToggleButton.IsChecked = false;
                    return;
                }

                if (this.SoundCloudToggleButton.IsChecked.GetValueOrDefault() && ChannelSession.Services.OverlayServer == null)
                {
                    await MessageBoxHelper.ShowMessageDialog("You must enable & use the Mix It Up Overlay for SoundCloud song requests");
                    this.EnableSongRequestsToggleButton.IsChecked = false;
                    return;
                }

                if (this.SpotifyToggleButton.IsChecked.GetValueOrDefault()) { ChannelSession.Settings.SongRequestServiceTypes.Add(SongRequestServiceTypeEnum.Spotify); }
                if (this.YouTubeToggleButton.IsChecked.GetValueOrDefault()) { ChannelSession.Settings.SongRequestServiceTypes.Add(SongRequestServiceTypeEnum.YouTube); }
                if (this.SoundCloudToggleButton.IsChecked.GetValueOrDefault()) { ChannelSession.Settings.SongRequestServiceTypes.Add(SongRequestServiceTypeEnum.SoundCloud); }

                ChannelSession.Settings.SpotifyAllowExplicit = this.SpotifyAllowExplicitSongToggleButton.IsChecked.GetValueOrDefault();

                await ChannelSession.SaveSettings();

                if (await ChannelSession.Services.SongRequestService.Initialize())
                {
                    await this.RefreshRequestsList();
                }
                else
                {
                    await MessageBoxHelper.ShowMessageDialog("We were unable to initialize the Song Request service, please try again.");
                }
            });
        }

        private async void EnableSongRequestsToggleButton_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            ChannelSession.Services.SongRequestService.Disable();

            await this.RefreshRequestsList();
        }

        private void SpotifyToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            this.SpotifyOptionsGrid.IsEnabled = this.SpotifyToggleButton.IsChecked.GetValueOrDefault();
        }

        private async void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                if (ChannelSession.Services.SongRequestService != null)
                {
                    await ChannelSession.Services.SongRequestService.PlayPauseCurrentSong();
                }
            });
        }

        private async void NextSongButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                if (ChannelSession.Services.SongRequestService != null)
                {
                    await ChannelSession.Services.SongRequestService.SkipToNextSong();
                }
            });
        }

        private async void ClearQueueButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                if (await MessageBoxHelper.ShowConfirmationDialog("Are you sure you want to clear the Song Request queue?"))
                {
                    await ChannelSession.Services.SongRequestService.ClearAllRequests();
                    await this.RefreshRequestsList();
                }
            });
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                Button button = (Button)sender;
                SongRequestItem songRequest = (SongRequestItem)button.DataContext;
                await ChannelSession.Services.SongRequestService.RemoveSongRequest(songRequest);
                await this.RefreshRequestsList();
            });
        }

        private async void GlobalEvents_OnSongRequestsChangedOccurred(object sender, EventArgs e)
        {
            await this.Dispatcher.InvokeAsync(async () =>
            {
                await this.RefreshRequestsList();
            });
        }

        private async Task RefreshRequestsList()
        {
            await SongRequestControl.songListLock.WaitAsync();

            this.EnableSongRequestsToggleButton.IsChecked = ChannelSession.Services.SongRequestService.IsEnabled;
            this.SongRequestServicesGrid.IsEnabled = !ChannelSession.Services.SongRequestService.IsEnabled;
            this.CurrentlyPlayingAndSongQueueGrid.IsEnabled = ChannelSession.Services.SongRequestService.IsEnabled;

            this.requests.Clear();
            foreach (SongRequestItem item in await ChannelSession.Services.SongRequestService.GetAllRequests())
            {
                this.requests.Add(item);
            }

            SongRequestControl.songListLock.Release();
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
                    this.backgroundThreadCancellationTokenSource.Dispose();
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
