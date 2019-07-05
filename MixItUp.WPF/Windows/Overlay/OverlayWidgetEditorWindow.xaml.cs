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
                this.ItemPosition.SetPosition(this.viewModel.OverlayWidget.Item.Position);

                if (this.viewModel.OverlayWidget.Item is OverlayHTMLItemModel) { this.SetGameEditorControl(new OverlayHTMLItemControl((OverlayHTMLItemModel)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayChatMessagesListItemModel) { this.SetGameEditorControl(new OverlayChatMessagesControl((OverlayChatMessagesListItemModel)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayEventListItemModel) { this.SetGameEditorControl(new OverlayEventListControl((OverlayEventListItemModel)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayGameQueueListItemModel) { this.SetGameEditorControl(new OverlayGameQueueControl((OverlayGameQueueListItemModel)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayProgressBarItemModel) { this.SetGameEditorControl(new OverlayProgressBarControl((OverlayProgressBarItemModel)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayImageItemModel) { this.SetGameEditorControl(new OverlayImageItemControl((OverlayImageItemModel)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayLeaderboardListItemModel) { this.SetGameEditorControl(new OverlayLeaderboardControl((OverlayLeaderboardListItemModel)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayStreamClipItemModel) { this.SetGameEditorControl(new OverlayStreamClipControl((OverlayStreamClipItemModel)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlaySongRequestsListItemModel) { this.SetGameEditorControl(new OverlaySongRequestsControl((OverlaySongRequestsListItemModel)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayStreamBossItemModel) { this.SetGameEditorControl(new OverlayStreamBossControl((OverlayStreamBossItemModel)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayTextItemModel) { this.SetGameEditorControl(new OverlayTextItemControl((OverlayTextItemModel)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayTimerItemModel) { this.SetGameEditorControl(new OverlayTimerControl((OverlayTimerItemModel)this.viewModel.OverlayWidget.Item)); }
                else if (this.viewModel.OverlayWidget.Item is OverlayTimerTrainItemModel) { this.SetGameEditorControl(new OverlayTimerTrainControl((OverlayTimerTrainItemModel)this.viewModel.OverlayWidget.Item)); }
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
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.StreamClip, new OverlayStreamClipControl());
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
            await this.RunAsyncOperation((System.Func<Task>)(async () =>
            {
                if (await this.viewModel.Validate())
                {
                    OverlayItemPositionModel position = this.ItemPosition.GetPosition();
                    if (position == null)
                    {
                        await DialogHelper.ShowMessage("A valid position for this overlay widget must be selected");
                        return;
                    }

                    OverlayItemModelBase overlayItem = overlayTypeEditor.GetItem();
                    if (overlayItem == null)
                    {
                        await DialogHelper.ShowMessage("There are missing details for the overlay item");
                        return;
                    }

                    overlayItem.Position = position;

                    if (this.viewModel.OverlayWidget == null)
                    {
                        OverlayWidgetModel widget = new OverlayWidgetModel(this.viewModel.Name, this.viewModel.SelectedOverlayEndpoint, overlayItem, (int)this.viewModel.RefreshTime);
                        ChannelSession.Settings.OverlayWidgets.Add(widget);
                    }
                    else
                    {
                        await this.viewModel.OverlayWidget.HideItem();

                        overlayItem.ID = this.viewModel.OverlayWidget.Item.ID;

                        this.viewModel.OverlayWidget.Name = this.viewModel.Name;
                        this.viewModel.OverlayWidget.OverlayName = this.viewModel.SelectedOverlayEndpoint;
                        this.viewModel.OverlayWidget.Item = overlayItem;
                        this.viewModel.OverlayWidget.RefreshTime = this.viewModel.RefreshTime;
                    }

                    this.Close();
                }
            }));
        }

        private void SetGameEditorControl(OverlayItemControl overlayTypeEditor)
        {
            this.MainContentControl.Content = this.overlayTypeEditor = overlayTypeEditor;
        }
    }
}
