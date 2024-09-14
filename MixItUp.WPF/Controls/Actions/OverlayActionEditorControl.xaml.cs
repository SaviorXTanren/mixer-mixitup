using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.Actions;
using MixItUp.WPF.Controls.Overlay;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for OverlayActionEditorControl.xaml
    /// </summary>
    public partial class OverlayActionEditorControl : ActionEditorControlBase
    {
        private OverlayActionEditorControlViewModel viewModel;

        public OverlayActionEditorControl()
        {
            InitializeComponent();

            this.Loaded += OverlayActionEditorControl_Loaded;
            this.DataContextChanged += OverlayActionEditorControl_DataContextChanged;
        }

        private void OverlayActionEditorControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.AnimationsMayNotWork.Visibility = SystemParameters.ClientAreaAnimation ? Visibility.Collapsed : Visibility.Visible;
        }

        private void OverlayActionEditorControl_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            this.viewModel = (OverlayActionEditorControlViewModel)this.DataContext;
        }

        private void ActionTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.viewModel != null && this.viewModel.ShowItem)
            {
                UserControl overlayControl = null;
                if (this.viewModel.SelectedActionType == OverlayActionTypeEnum.Text)
                {
                    overlayControl = new OverlayTextV3Control();
                }
                else if (this.viewModel.SelectedActionType == OverlayActionTypeEnum.Image)
                {
                    overlayControl = new OverlayImageV3Control();
                }
                else if (this.viewModel.SelectedActionType == OverlayActionTypeEnum.Video)
                {
                    overlayControl = new OverlayVideoV3Control();
                }
                else if (this.viewModel.SelectedActionType == OverlayActionTypeEnum.YouTube)
                {
                    overlayControl = new OverlayYouTubeV3Control();
                }
                else if (this.viewModel.SelectedActionType == OverlayActionTypeEnum.HTML)
                {
                    overlayControl = new OverlayHTMLV3Control();
                }
                else if (this.viewModel.SelectedActionType == OverlayActionTypeEnum.Timer)
                {
                    overlayControl = new OverlayTimerV3Control();
                }
                else if (this.viewModel.SelectedActionType == OverlayActionTypeEnum.TwitchClip)
                {
                    overlayControl = new OverlayTwitchClipV3Control();
                }
                else if (this.viewModel.SelectedActionType == OverlayActionTypeEnum.EmoteEffect)
                {
                    overlayControl = new OverlayEmoteEffectV3Control();
                }

                if (overlayControl != null)
                {
                    overlayControl.DataContext = this.viewModel.Item;
                    this.InnerContent.Content = overlayControl;
                }
            }
        }
    }
}
