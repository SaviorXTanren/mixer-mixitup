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
            this.AllowExplicitSongToggleButton.IsChecked = ChannelSession.Settings.SpotifyAllowExplicit;

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
                ChannelSession.Settings.SpotifyAllowExplicit = this.AllowExplicitSongToggleButton.IsChecked.GetValueOrDefault();

                this.ClearQueueButton.IsEnabled = true;

                await ChannelSession.Services.SongRequestService.Initialize(ChannelSession.Settings.SongRequestServiceType);

                await ChannelSession.SaveSettings();

                await this.RefreshRequestsList();
            });
        }

        private void EnableGameQueueToggleButton_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            ChannelSession.Services.SongRequestService.Disable();

            this.ClearQueueButton.IsEnabled = false;
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

        private async void ClearQueueButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                await ChannelSession.Services.SongRequestService.ClearAllRequests();
                await this.RefreshRequestsList();
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
