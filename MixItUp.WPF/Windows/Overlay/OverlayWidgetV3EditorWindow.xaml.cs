using MixItUp.Base;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Overlay;
using MixItUp.WPF.Controls.Overlay;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Windows.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayWidgetV3EditorWindow.xaml
    /// </summary>
    public partial class OverlayWidgetV3EditorWindow : LoadingWindowBase
    {
        public static readonly DependencyProperty ContentControlProperty =
            DependencyProperty.Register(
            "ContentControl",
            typeof(object),
            typeof(OverlayItemContainerV3Control),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.AffectsRender,
                new PropertyChangedCallback(OnContentControlChanged)
            ));

        public object ContentControl
        {
            get { return (object)GetValue(ContentControlProperty); }
            set { SetValue(ContentControlProperty, value); }
        }

        private OverlayWidgetV3EditorWindowViewModel viewModel;

        public OverlayWidgetV3EditorWindow(OverlayItemV3Type type)
        {
            this.viewModel = new OverlayWidgetV3EditorWindowViewModel(type);

            this.AssignOverlayTypeControl(type);

            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        public OverlayWidgetV3EditorWindow(OverlayItemV3ModelBase item)
        {
            this.viewModel = new OverlayWidgetV3EditorWindowViewModel(item);

            this.AssignOverlayTypeControl(item.Type);

            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            this.DataContext = this.viewModel;
            await this.viewModel.OnOpen();
            await base.OnLoaded();
        }

        private void AssignOverlayTypeControl(OverlayItemV3Type type)
        {
            switch (type)
            {
                case OverlayItemV3Type.Text:
                    this.ContentControl = new OverlayTextV3Control();
                    break;
                case OverlayItemV3Type.Image:
                    this.ContentControl = new OverlayImageV3Control();
                    break;
                case OverlayItemV3Type.Video:
                    this.ContentControl = new OverlayVideoV3Control();
                    break;
                case OverlayItemV3Type.YouTube:
                    this.ContentControl = new OverlayYouTubeV3Control();
                    break;
                case OverlayItemV3Type.HTML:
                    this.ContentControl = new OverlayHTMLV3Control();
                    break;
                case OverlayItemV3Type.WebPage:
                    this.ContentControl = new OverlayWebPageV3Control();
                    break;
                case OverlayItemV3Type.Timer:
                    this.ContentControl = new OverlayTimerV3Control();
                    break;
                //case OverlayItemV3Type.Label:
                //    this.ContentControl = new OverlayLabelV3Control();
                //    break;
                //case OverlayItemV3Type.EventList:
                //    this.ContentControl = new OverlayEventListV3Control();
                //    break;
                //case OverlayItemV3Type.Goal:
                //    this.ContentControl = new OverlayGoalV3Control();
                //    break;
            }
        }

        private async void SaveButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Result result = this.viewModel.Validate();
            if (!result.Success)
            {
                await DialogHelper.ShowFailedResult(result);
                return;
            }

            OverlayItemV3ModelBase item = this.viewModel.GetItem();
            if (item != null)
            {
                if (this.viewModel.oldItem != null)
                {
                    ChannelSession.Settings.OverlayWidgetsV3.Remove(this.viewModel.oldItem);
                }
                ChannelSession.Settings.OverlayWidgetsV3.Add(item);
            }
        }

        private static void OnContentControlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            OverlayWidgetV3EditorWindow control = (OverlayWidgetV3EditorWindow)d;
            if (control != null)
            {
                control.InnerContent.Content = control.ContentControl;
            }
        }
    }
}
