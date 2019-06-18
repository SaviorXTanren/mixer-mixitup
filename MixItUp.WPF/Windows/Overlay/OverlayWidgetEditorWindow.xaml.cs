using MixItUp.Base;
using MixItUp.Base.Model.Overlay;
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

        private Dictionary<OverlayWidgetTypeEnum, OverlayItemControl> overlayTypeEditors = new Dictionary<OverlayWidgetTypeEnum, OverlayItemControl>();
        private OverlayItemControl overlayTypeEditor;

        public OverlayWidgetEditorWindow(OverlayWidget widget)
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
                this.ItemPosition.SetPosition(this.viewModel.OverlayWidget.Position);

                if (this.viewModel.OverlayWidget.Item is OverlayChatMessages) { this.SetGameEditorControl(new OverlayChatMessagesControl((OverlayChatMessages)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayEventList) { this.SetGameEditorControl(new OverlayEventListControl((OverlayEventList)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayGameQueue) { this.SetGameEditorControl(new OverlayGameQueueControl((OverlayGameQueue)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayProgressBar) { this.SetGameEditorControl(new OverlayProgressBarControl((OverlayProgressBar)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayHTMLItem) { this.SetGameEditorControl(new OverlayHTMLItemControl((OverlayHTMLItem)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayImageItem) { this.SetGameEditorControl(new OverlayImageItemControl((OverlayImageItem)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayLeaderboard) { this.SetGameEditorControl(new OverlayLeaderboardControl((OverlayLeaderboard)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayMixerClip) { this.SetGameEditorControl(new OverlayMixerClipControl((OverlayMixerClip)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlaySongRequests) { this.SetGameEditorControl(new OverlaySongRequestsControl((OverlaySongRequests)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayStreamBoss) { this.SetGameEditorControl(new OverlayStreamBossControl((OverlayStreamBoss)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayTextItem) { this.SetGameEditorControl(new OverlayTextItemControl((OverlayTextItem)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayTimer) { this.SetGameEditorControl(new OverlayTimerControl((OverlayTimer)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayTimerTrain) { this.SetGameEditorControl(new OverlayTimerTrainControl((OverlayTimerTrain)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayVideoItem) { this.SetGameEditorControl(new OverlayVideoItemControl((OverlayVideoItem)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayWebPageItem) { this.SetGameEditorControl(new OverlayWebPageItemControl((OverlayWebPageItem)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayYouTubeItem) { this.SetGameEditorControl(new OverlayYouTubeItemControl((OverlayYouTubeItem)this.viewModel.OverlayWidget.Item)); }
            }
            else
            {
                this.overlayTypeEditors.Add(OverlayWidgetTypeEnum.ChatMessages, new OverlayChatMessagesControl());
                this.overlayTypeEditors.Add(OverlayWidgetTypeEnum.EventList, new OverlayEventListControl());
                this.overlayTypeEditors.Add(OverlayWidgetTypeEnum.GameQueue, new OverlayGameQueueControl());
                this.overlayTypeEditors.Add(OverlayWidgetTypeEnum.ProgressBar, new OverlayProgressBarControl());
                this.overlayTypeEditors.Add(OverlayWidgetTypeEnum.HTML, new OverlayHTMLItemControl());
                this.overlayTypeEditors.Add(OverlayWidgetTypeEnum.Image, new OverlayImageItemControl());
                this.overlayTypeEditors.Add(OverlayWidgetTypeEnum.Leaderboard, new OverlayLeaderboardControl());
                this.overlayTypeEditors.Add(OverlayWidgetTypeEnum.MixerClip, new OverlayMixerClipControl());
                this.overlayTypeEditors.Add(OverlayWidgetTypeEnum.SongRequests, new OverlaySongRequestsControl());
                this.overlayTypeEditors.Add(OverlayWidgetTypeEnum.StreamBoss, new OverlayStreamBossControl());
                this.overlayTypeEditors.Add(OverlayWidgetTypeEnum.Text, new OverlayTextItemControl());
                this.overlayTypeEditors.Add(OverlayWidgetTypeEnum.Timer, new OverlayTimerControl());
                this.overlayTypeEditors.Add(OverlayWidgetTypeEnum.TimerTrain, new OverlayTimerTrainControl());
                this.overlayTypeEditors.Add(OverlayWidgetTypeEnum.Video, new OverlayVideoItemControl());
                this.overlayTypeEditors.Add(OverlayWidgetTypeEnum.WebPage, new OverlayWebPageItemControl());
                this.overlayTypeEditors.Add(OverlayWidgetTypeEnum.YouTube, new OverlayYouTubeItemControl());
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
                    OverlayItemPosition position = this.ItemPosition.GetPosition();
                    if (position == null)
                    {
                        await DialogHelper.ShowMessage("A valid position for this overlay widget must be selected");
                        return;
                    }

                    OverlayItemBase overlayItem = overlayTypeEditor.GetItem();
                    if (overlayItem == null)
                    {
                        await DialogHelper.ShowMessage("There are missing details for the overlay item");
                        return;
                    }

                    if (this.viewModel.OverlayWidget == null)
                    {
                        OverlayWidget widget = new OverlayWidget(this.viewModel.Name, this.viewModel.SelectedOverlayEndpoint, overlayItem, position, this.viewModel.DontRefresh);
                        ChannelSession.Settings.OverlayWidgets.Add(widget);
                    }
                    else
                    {
                        overlayItem.ID = this.viewModel.OverlayWidget.Item.ID;

                        this.viewModel.OverlayWidget.Name = this.viewModel.Name;
                        this.viewModel.OverlayWidget.OverlayName = this.viewModel.SelectedOverlayEndpoint;
                        this.viewModel.OverlayWidget.Item = overlayItem;
                        this.viewModel.OverlayWidget.Position = position;
                        this.viewModel.OverlayWidget.DontRefresh = this.viewModel.DontRefresh;
                    }

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
