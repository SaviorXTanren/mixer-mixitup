using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
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
    public partial class SongRequestControl : MainControlBase
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

            this.SongServiceTypeComboBox.ItemsSource = EnumHelper.GetEnumNames<SongRequestServiceTypeEnum>(new List<SongRequestServiceTypeEnum>() { SongRequestServiceTypeEnum.Spotify });

            this.SongServiceTypeComboBox.SelectedItem = EnumHelper.GetEnumName(ChannelSession.Settings.SongRequestServiceType);
            this.SpotifyAllowExplicitSongToggleButton.IsChecked = ChannelSession.Settings.SpotifyAllowExplicit;

            await this.RefreshRequestsList();

            await base.InitializeInternal();
        }

        private async void EnableGameQueueToggleButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.SongServiceTypeComboBox.SelectedIndex < 0)
            {
                await MessageBoxHelper.ShowMessageDialog("You must select a song service type.");
                this.EnableGameQueueToggleButton.IsChecked = false;
                return;
            }

            SongRequestServiceTypeEnum service = EnumHelper.GetEnumValueFromString<SongRequestServiceTypeEnum>((string)this.SongServiceTypeComboBox.SelectedItem);
            if (service == SongRequestServiceTypeEnum.Youtube)
            {

            }
            else if (service == SongRequestServiceTypeEnum.Spotify)
            {
                if (ChannelSession.Services.Spotify == null)
                {
                    await MessageBoxHelper.ShowMessageDialog("You must connect to your Spotify account in the Services area.");
                    this.EnableGameQueueToggleButton.IsChecked = false;
                    return;
                }
            }

            await this.Window.RunAsyncOperation(async () =>
            {
                ChannelSession.Settings.SongRequestServiceType = service;
                ChannelSession.Settings.SpotifyAllowExplicit = this.SpotifyAllowExplicitSongToggleButton.IsChecked.GetValueOrDefault();

                this.SongServiceTypeComboBox.IsEnabled = this.SpotifyOptionsGrid.IsEnabled = false;
                this.CurrentlyPlayingAndSongQueueGrid.IsEnabled = true;

                await ChannelSession.Services.SongRequestService.Initialize(ChannelSession.Settings.SongRequestServiceType);

                await ChannelSession.SaveSettings();

                await this.RefreshRequestsList();
            });
        }

        private void EnableGameQueueToggleButton_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            ChannelSession.Services.SongRequestService.Disable();

            this.SongServiceTypeComboBox.IsEnabled = this.SpotifyOptionsGrid.IsEnabled = true;
            this.CurrentlyPlayingAndSongQueueGrid.IsEnabled = false;
        }

        private void SongServiceTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            this.SpotifyOptionsGrid.Visibility = Visibility.Collapsed;

            if (this.SongServiceTypeComboBox.SelectedIndex >= 0)
            {
                SongRequestServiceTypeEnum service = EnumHelper.GetEnumValueFromString<SongRequestServiceTypeEnum>((string)this.SongServiceTypeComboBox.SelectedItem);
                if (service == SongRequestServiceTypeEnum.Youtube)
                {

                }
                else if (service == SongRequestServiceTypeEnum.Spotify)
                {
                    this.SpotifyOptionsGrid.Visibility = Visibility.Visible;
                }
            }
        }

        private async void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                if (ChannelSession.Services.SongRequestService.GetRequestService() == SongRequestServiceTypeEnum.Spotify && ChannelSession.Services.Spotify != null)
                {
                    if ((await ChannelSession.Services.Spotify.GetCurrentlyPlaying()).IsPlaying)
                    {
                        await ChannelSession.Services.Spotify.PauseCurrentlyPlaying();
                    }
                    else
                    {
                        await ChannelSession.Services.Spotify.PlayCurrentlyPlaying();
                    }
                }
            });
        }

        private async void NextSongButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                if (ChannelSession.Services.SongRequestService.GetRequestService() == SongRequestServiceTypeEnum.Spotify && ChannelSession.Services.Spotify != null)
                {
                    await ChannelSession.Services.Spotify.NextCurrentlyPlaying();
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

            this.requests.Clear();
            foreach (SongRequestItem item in await ChannelSession.Services.SongRequestService.GetAllRequests())
            {
                this.requests.Add(item);
            }

            SongRequestControl.songListLock.Release();
        }
    }
}
