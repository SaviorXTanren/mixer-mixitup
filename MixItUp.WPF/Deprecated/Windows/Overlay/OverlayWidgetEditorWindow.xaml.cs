using MixItUp.Base;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Overlay;
using MixItUp.WPF.Controls.Overlay;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Windows.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayWidgetEditorWindow.xaml
    /// </summary>
    [Obsolete]
    public partial class OverlayWidgetEditorWindow : LoadingWindowBase
    {
        private OverlayWidgetEditorWindowViewModel viewModel;

        private Dictionary<OverlayItemModelTypeEnum, OverlayItemControl> overlayTypeEditors = new Dictionary<OverlayItemModelTypeEnum, OverlayItemControl>();
        private OverlayItemControl overlayTypeEditor;

        private OverlayItemPositionViewModel position = new OverlayItemPositionViewModel();

        public OverlayWidgetEditorWindow(OverlayWidgetModel widget)
            : this()
        {
            this.viewModel = new OverlayWidgetEditorWindowViewModel(widget);

            this.position.SetPosition(this.viewModel.OverlayWidget.Item.Position);
        }

        public OverlayWidgetEditorWindow()
        {
            InitializeComponent();

            this.viewModel = new OverlayWidgetEditorWindowViewModel();

            this.ItemPosition.DataContext = this.position;

            this.Initialize(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            this.DataContext = this.viewModel;
            this.viewModel.OverlayTypeSelected += ViewModel_OverlayTypeSelected;
            await this.viewModel.OnOpen();

            if (this.viewModel.OverlayWidget != null)
            {
                if (this.viewModel.OverlayWidget.Item is OverlayChatMessagesListItemModel) { this.SetOverlayWidgetEditorControl(new OverlayChatMessagesListItemControl(new OverlayChatMessagesListItemViewModel((OverlayChatMessagesListItemModel)this.viewModel.OverlayWidget.Item))); }
                else if (this.viewModel.OverlayWidget.Item is OverlayEndCreditsItemModel) { this.SetOverlayWidgetEditorControl(new OverlayEndCreditsItemControl(new OverlayEndCreditsItemViewModel((OverlayEndCreditsItemModel)this.viewModel.OverlayWidget.Item))); }
                else if (this.viewModel.OverlayWidget.Item is OverlayEventListItemModel) { this.SetOverlayWidgetEditorControl(new OverlayEventListItemControl(new OverlayEventListItemViewModel((OverlayEventListItemModel)this.viewModel.OverlayWidget.Item))); }
                else if (this.viewModel.OverlayWidget.Item is OverlayGameQueueListItemModel) { this.SetOverlayWidgetEditorControl(new OverlayGameQueueListItemControl(new OverlayGameQueueListItemViewModel((OverlayGameQueueListItemModel)this.viewModel.OverlayWidget.Item))); }
                else if (this.viewModel.OverlayWidget.Item is OverlayProgressBarItemModel) { this.SetOverlayWidgetEditorControl(new OverlayProgressBarItemControl(new OverlayProgressBarItemViewModel((OverlayProgressBarItemModel)this.viewModel.OverlayWidget.Item))); }
                else if (this.viewModel.OverlayWidget.Item is OverlayImageItemModel) { this.SetOverlayWidgetEditorControl(new OverlayImageItemControl(new OverlayImageItemViewModel((OverlayImageItemModel)this.viewModel.OverlayWidget.Item))); }
                else if (this.viewModel.OverlayWidget.Item is OverlayHTMLItemModel) { this.SetOverlayWidgetEditorControl(new OverlayHTMLItemControl(new OverlayHTMLItemViewModel((OverlayHTMLItemModel)this.viewModel.OverlayWidget.Item))); }
                else if (this.viewModel.OverlayWidget.Item is OverlayLeaderboardListItemModel) { this.SetOverlayWidgetEditorControl(new OverlayLeaderboardListItemControl(new OverlayLeaderboardListItemViewModel((OverlayLeaderboardListItemModel)this.viewModel.OverlayWidget.Item))); }
                else if (this.viewModel.OverlayWidget.Item is OverlayStreamBossItemModel) { this.SetOverlayWidgetEditorControl(new OverlayStreamBossItemControl(new OverlayStreamBossItemViewModel((OverlayStreamBossItemModel)this.viewModel.OverlayWidget.Item))); }
                else if (this.viewModel.OverlayWidget.Item is OverlayTextItemModel) { this.SetOverlayWidgetEditorControl(new OverlayTextItemControl(new OverlayTextItemViewModel((OverlayTextItemModel)this.viewModel.OverlayWidget.Item))); }
                else if (this.viewModel.OverlayWidget.Item is OverlayTickerTapeListItemModel) { this.SetOverlayWidgetEditorControl(new OverlayTickerTapeListItemControl(new OverlayTickerTapeListItemViewModel((OverlayTickerTapeListItemModel)this.viewModel.OverlayWidget.Item))); }
                else if (this.viewModel.OverlayWidget.Item is OverlayTimerItemModel) { this.SetOverlayWidgetEditorControl(new OverlayTimerItemControl(new OverlayTimerItemViewModel((OverlayTimerItemModel)this.viewModel.OverlayWidget.Item))); }
                else if (this.viewModel.OverlayWidget.Item is OverlayTimerTrainItemModel) { this.SetOverlayWidgetEditorControl(new OverlayTimerTrainItemControl(new OverlayTimerTrainItemViewModel((OverlayTimerTrainItemModel)this.viewModel.OverlayWidget.Item))); }
                else if (this.viewModel.OverlayWidget.Item is OverlayVideoItemModel) { this.SetOverlayWidgetEditorControl(new OverlayVideoItemControl(new OverlayVideoItemViewModel((OverlayVideoItemModel)this.viewModel.OverlayWidget.Item))); }
                else if (this.viewModel.OverlayWidget.Item is OverlayWebPageItemModel) { this.SetOverlayWidgetEditorControl(new OverlayWebPageItemControl(new OverlayWebPageItemViewModel((OverlayWebPageItemModel)this.viewModel.OverlayWidget.Item))); }
                else if (this.viewModel.OverlayWidget.Item is OverlayYouTubeItemModel) { this.SetOverlayWidgetEditorControl(new OverlayYouTubeItemControl(new OverlayYouTubeItemViewModel((OverlayYouTubeItemModel)this.viewModel.OverlayWidget.Item))); }
            }
            else
            {
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.ChatMessages, new OverlayChatMessagesListItemControl(new OverlayChatMessagesListItemViewModel()));
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.EndCredits, new OverlayEndCreditsItemControl(new OverlayEndCreditsItemViewModel()));
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.EventList, new OverlayEventListItemControl(new OverlayEventListItemViewModel()));
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.GameQueue, new OverlayGameQueueListItemControl(new OverlayGameQueueListItemViewModel()));
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.ProgressBar, new OverlayProgressBarItemControl(new OverlayProgressBarItemViewModel()));
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.HTML, new OverlayHTMLItemControl(new OverlayHTMLItemViewModel()));
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.Image, new OverlayImageItemControl(new OverlayImageItemViewModel()));
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.Leaderboard, new OverlayLeaderboardListItemControl(new OverlayLeaderboardListItemViewModel()));
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.StreamBoss, new OverlayStreamBossItemControl(new OverlayStreamBossItemViewModel()));
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.Text, new OverlayTextItemControl(new OverlayTextItemViewModel()));
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.TickerTape, new OverlayTickerTapeListItemControl(new OverlayTickerTapeListItemViewModel()));
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.Timer, new OverlayTimerItemControl(new OverlayTimerItemViewModel()));
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.TimerTrain, new OverlayTimerTrainItemControl(new OverlayTimerTrainItemViewModel()));
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.Video, new OverlayVideoItemControl(new OverlayVideoItemViewModel()));
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.WebPage, new OverlayWebPageItemControl(new OverlayWebPageItemViewModel()));
                this.overlayTypeEditors.Add(OverlayItemModelTypeEnum.YouTube, new OverlayYouTubeItemControl(new OverlayYouTubeItemViewModel()));
            }
        }

        private void ViewModel_OverlayTypeSelected(object sender, OverlayTypeListing overlayType)
        {
            this.SetOverlayWidgetEditorControl(this.overlayTypeEditors[this.viewModel.SelectedOverlayType.Type]);
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            await this.RunAsyncOperation((System.Func<Task>)(async () =>
            {
                if (await this.viewModel.Validate())
                {
                    OverlayItemPositionModel position = this.position.GetPosition();
                    if (position == null)
                    {
                        await DialogHelper.ShowMessage(MixItUp.Base.Resources.InvalidPosition);
                        return;
                    }

                    OverlayItemModelBase overlayItem = overlayTypeEditor.GetItem();
                    if (overlayItem == null)
                    {
                        await DialogHelper.ShowMessage(MixItUp.Base.Resources.OverlayMissingDetails);
                        return;
                    }

                    overlayItem.Position = position;

                    OverlayWidgetModel widget = new OverlayWidgetModel(this.viewModel.Name, this.viewModel.SelectedOverlayEndpoint ?? string.Empty, overlayItem, (int)this.viewModel.RefreshTime);
                    if (this.viewModel.OverlayWidget != null)
                    {
                        await this.viewModel.OverlayWidget.Disable();
                        ChannelSession.Settings.OverlayWidgets.Remove(this.viewModel.OverlayWidget);
                        overlayItem.ID = this.viewModel.OverlayWidget.Item.ID;
                    }

                    if (widget.Item is OverlayEndCreditsItemModel)
                    {
                        widget.IsEnabled = false;
                    }

                    ChannelSession.Settings.OverlayWidgets.Add(widget);

                    this.Close();
                }
            }));
        }

        private void SetOverlayWidgetEditorControl(OverlayItemControl overlayTypeEditor)
        {
            this.MainContentControl.Content = this.overlayTypeEditor = overlayTypeEditor;

            OverlayItemViewModelBase itemViewModel = this.overlayTypeEditor.GetViewModel();
            this.viewModel.SupportsRefreshUpdating = itemViewModel.SupportsRefreshUpdating;
        }
    }
}
