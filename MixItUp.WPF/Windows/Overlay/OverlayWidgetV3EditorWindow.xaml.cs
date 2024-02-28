﻿using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Overlay.Widgets;
using MixItUp.Base.ViewModel.Overlay;
using MixItUp.WPF.Controls.Overlay;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MixItUp.WPF.Windows.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayWidgetV3EditorWindow.xaml
    /// </summary>
    public partial class OverlayWidgetV3EditorWindow : LoadingWindowBase
    {
        private OverlayWidgetV3ViewModel viewModel;

        public OverlayWidgetV3EditorWindow(OverlayItemV3Type type)
        {
            this.ViewModel = this.viewModel = new OverlayWidgetV3ViewModel(type);

            InitializeComponent();

            this.Initialize();
        }

        public OverlayWidgetV3EditorWindow(OverlayWidgetV3Model item)
        {
            this.ViewModel = this.viewModel = new OverlayWidgetV3ViewModel(item);

            InitializeComponent();

            this.Initialize();
        }

        protected override async Task OnLoaded()
        {
            this.DataContext = this.viewModel;

            this.AssignOverlayTypeControl(this.viewModel.Item.Type);

            await this.viewModel.OnOpen();
            await base.OnLoaded();
        }

        protected override async Task OnClosing()
        {
            await this.ViewModel.OnClosed();

            await base.OnClosing();
        }

        private void AssignOverlayTypeControl(OverlayItemV3Type type)
        {
            UserControl overlayControl = null;
            switch (type)
            {
                case OverlayItemV3Type.Text: overlayControl = new OverlayTextV3Control(); break;
                case OverlayItemV3Type.Image: overlayControl = new OverlayImageV3Control(); break;
                case OverlayItemV3Type.Video: overlayControl = new OverlayVideoV3Control(); break;
                case OverlayItemV3Type.YouTube: overlayControl = new OverlayYouTubeV3Control(); break;
                case OverlayItemV3Type.HTML: overlayControl = new OverlayHTMLV3Control(); break;
                case OverlayItemV3Type.Timer: overlayControl = new OverlayTimerV3Control(); break;
                case OverlayItemV3Type.TwitchClip: overlayControl = new OverlayTwitchClipV3Control(); break;
                case OverlayItemV3Type.Label: overlayControl = new OverlayLabelV3Control(); break;
                case OverlayItemV3Type.StreamBoss: overlayControl = new OverlayStreamBossV3Control(); break;
                case OverlayItemV3Type.Goal: overlayControl = new OverlayGoalV3Control(); break;
                case OverlayItemV3Type.PersistentTimer: overlayControl = new OverlayPersistentTimerV3Control(); break;
                case OverlayItemV3Type.Chat: overlayControl = new OverlayChatV3Control(); break;
            }

            if (overlayControl != null)
            {
                this.InnerContent.Content = overlayControl;
            }
        }

        private void Initialize()
        {
            this.Initialize(this.StatusBar);

            this.ViewModel.StartLoadingOperationOccurred += (sender, eventArgs) => { this.StartLoadingOperation(); };
            this.ViewModel.EndLoadingOperationOccurred += (sender, eventArgs) => { this.EndLoadingOperation(); };
            this.viewModel.OnCloseRequested += ViewModel_OnCloseRequested;
        }

        private void ViewModel_OnCloseRequested(object sender, System.EventArgs e)
        {
            this.Close();
        }
    }
}