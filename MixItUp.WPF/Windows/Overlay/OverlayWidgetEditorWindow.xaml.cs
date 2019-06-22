using MixItUp.Base;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Window.Overlay;
using MixItUp.WPF.Controls.Overlay;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Windows.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayWidgetEditorWindow.xaml
    /// </summary>
    public partial class OverlayWidgetEditorWindow : LoadingWindowBase
    {
        private OverlayWidgetEditorWindowViewModel viewModel;

        private Dictionary<OverlayItemModelTypeEnum, OverlayItemControl> overlayTypeEditors = new Dictionary<OverlayItemModelTypeEnum, OverlayItemControl>();
        private OverlayItemControl overlayTypeEditor;

        public OverlayWidgetEditorWindow(OverlayWidgetModel widget)
            : this()
        {
            this.viewModel = new OverlayWidgetEditorWindowViewModel(widget);
        }

        public OverlayWidgetEditorWindow()
        {
            InitializeComponent();

            this.viewModel = new OverlayWidgetEditorWindowViewModel();

            this.Initialize(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            this.DataContext = this.viewModel;
            this.viewModel.OverlayTypeSelected += ViewModel_OverlayTypeSelected;
            await this.viewModel.OnLoaded();

            if (this.viewModel.OverlayWidget != null)
            {
                this.ItemPosition.SetItemPosition(this.viewModel.OverlayWidget.Item.Position);

                if (this.viewModel.OverlayWidget.Item is OverlayHTMLItemModel) { this.SetGameEditorControl(new OverlayHTMLItemControl((OverlayHTMLItemModel)this.viewModel.OverlayWidget.Item)); }
                //else if (this.viewModel.OverlayWidget.Item is OverlayChatMessages) { this.SetGameEditorControl(new OverlayChatMessagesControl((OverlayChatMessages)this.viewModel.OverlayWidget.Item)); }
                //else if (this.viewModel.OverlayWidget.Item is OverlayEventList) { this.SetGameEditorControl(new OverlayEventListControl((OverlayEventList)this.viewModel.OverlayWidget.Item)); }
                //else if (this.viewModel.OverlayWidget.Item is OverlayGameQueue) { this.SetGameEditorControl(new OverlayGameQueueControl((OverlayGameQueue)this.viewModel.OverlayWidget.Item)); }
                //else if (this.viewModel.OverlayWidget.Item is OverlayProgressBar) { this.SetGameEditorControl(new OverlayProgressBarControl((OverlayProgressBar)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayImageItemModel) { this.SetGameEditorControl(new OverlayImageItemControl((OverlayImageItemModel)this.viewModel.OverlayWidget.Item)); }
                //else if (this.viewModel.OverlayWidget.Item is OverlayLeaderboard) { this.SetGameEditorControl(new OverlayLeaderboardControl((OverlayLeaderboard)this.viewModel.OverlayWidget.Item)); }
                //else if (this.viewModel.OverlayWidget.Item is OverlayMixerClip) { this.SetGameEditorControl(new OverlayMixerClipControl((OverlayMixerClip)this.viewModel.OverlayWidget.Item)); }
                //else if (this.viewModel.OverlayWidget.Item is OverlaySongRequests) { this.SetGameEditorControl(new OverlaySongRequestsControl((OverlaySongRequests)this.viewModel.OverlayWidget.Item)); }
                //else if (this.viewModel.OverlayWidget.Item is OverlayStreamBoss) { this.SetGameEditorControl(new OverlayStreamBossControl((OverlayStreamBoss)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayTextItemModel) { this.SetGameEditorControl(new OverlayTextItemControl((OverlayTextItemModel)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayTimerItemModel) { this.SetGameEditorControl(new OverlayTimerControl((OverlayTimerItemModel)this.viewModel.OverlayWidget.Item)); }
                //else if (this.viewModel.OverlayWidget.Item is OverlayTimerTrain) { this.SetGameEditorControl(new OverlayTimerTrainControl((OverlayTimerTrain)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayVideoItemModel) { this.SetGameEditorControl(new OverlayVideoItemControl((OverlayVideoItemModel)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayWebPageItemModel) { this.SetGameEditorControl(new OverlayWebPageItemControl((OverlayWebPageItemModel)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayYouTubeItemModel) { this.SetGameEditorControl(new OverlayYouTubeItemControl((OverlayYouTubeItemModel)this.viewModel.OverlayWidget.Item)); }
            }
            else
            {
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.ChatMessages, new OverlayChatMessagesControl());
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.EventList, new OverlayEventListControl());
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.GameQueue, new OverlayGameQueueControl());
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.ProgressBar, new OverlayProgressBarControl());
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.HTML, new OverlayHTMLItemControl());
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.Image, new OverlayImageItemControl());
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.Leaderboard, new OverlayLeaderboardControl());
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.StreamClip, new OverlayMixerClipControl());
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.SongRequests, new OverlaySongRequestsControl());
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.StreamBoss, new OverlayStreamBossControl());
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.Text, new OverlayTextItemControl());
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.Timer, new OverlayTimerControl());
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.TimerTrain, new OverlayTimerTrainControl());
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.Video, new OverlayVideoItemControl());
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.WebPage, new OverlayWebPageItemControl());
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.YouTube, new OverlayYouTubeItemControl());
            }
        }

        private void ViewModel_OverlayTypeSelected(object sender, OverlayTypeListing overlayType)
        {
            this.SetGameEditorControl(this.overlayTypeEditors[this.viewModel.SelectedOverlayType.Type]);
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                if (await this.viewModel.Validate())
                {
                    OverlayItemPositionModel position = this.ItemPosition.GetItemPosition();
                    if (position == null)
                    {
                        await DialogHelper.ShowMessage("A valid position for this overlay widget must be selected");
                        return;
                    }

                    OverlayItemModelBase overlayItem = overlayTypeEditor.GetOverlayItem();
                    if (overlayItem == null)
                    {
                        await DialogHelper.ShowMessage("There are missing details for the overlay item");
                        return;
                    }

                    overlayItem.Position = position;

                    OverlayWidgetModel widget = new OverlayWidgetModel(this.viewModel.Name, this.viewModel.SelectedOverlayEndpoint, overlayItem, this.viewModel.DontRefresh);
                    if (this.viewModel.OverlayWidget != null)
                    {
                        overlayItem.ID = this.viewModel.OverlayWidget.Item.ID;
                        ChannelSession.Settings.OverlayWidgets.Remove(this.viewModel.OverlayWidget);
                        await this.viewModel.OverlayWidget.HideItem();
                    }
                    ChannelSession.Settings.OverlayWidgets.Add(widget);

                    this.Close();
                }
            });
        }

        private void SetGameEditorControl(OverlayItemControl overlayTypeEditor)
        {
            this.MainContentControl.Content = this.overlayTypeEditor = overlayTypeEditor;
        }
    }
}
