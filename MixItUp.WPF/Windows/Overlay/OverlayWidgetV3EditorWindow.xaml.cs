using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Overlay;
using MixItUp.WPF.Controls.Overlay;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.WPF.Windows.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayWidgetV3EditorWindow.xaml
    /// </summary>
    public partial class OverlayWidgetV3EditorWindow : LoadingWindowBase
    {
        private OverlayWidgetV3EditorWindowViewModel viewModel;

        public OverlayWidgetV3EditorWindow()
        {
            this.viewModel = new OverlayWidgetV3EditorWindowViewModel();

            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        public OverlayWidgetV3EditorWindow(OverlayWidgetV3Model item)
        {
            this.viewModel = new OverlayWidgetV3EditorWindowViewModel(item);

            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            this.DataContext = this.viewModel;

            if (this.viewModel.IsTypeSelected)
            {
                this.AssignOverlayTypeControl(this.viewModel.SelectedType);
            }

            await this.viewModel.OnOpen();
            await base.OnLoaded();
        }

        private void AssignOverlayTypeControl(OverlayItemV3Type type)
        {
            switch (type)
            {
                case OverlayItemV3Type.Text:
                    this.InnerContent.Content = new OverlayTextV3Control();
                    break;
                case OverlayItemV3Type.Image:
                    this.InnerContent.Content = new OverlayImageV3Control();
                    break;
                case OverlayItemV3Type.Video:
                    this.InnerContent.Content = new OverlayVideoV3Control();
                    break;
                case OverlayItemV3Type.YouTube:
                    this.InnerContent.Content = new OverlayYouTubeV3Control();
                    break;
                case OverlayItemV3Type.HTML:
                    this.InnerContent.Content = new OverlayHTMLV3Control();
                    break;
                case OverlayItemV3Type.Timer:
                    this.InnerContent.Content = new OverlayTimerV3Control();
                    break;
                case OverlayItemV3Type.Label:
                    this.InnerContent.Content = new OverlayLabelV3Control();
                    break;
            }
        }

        private async void TypeSelectedButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.viewModel.TypeSelected();
            this.AssignOverlayTypeControl(this.viewModel.SelectedType);
        }

        private async void SaveButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Result result = this.viewModel.Validate();
            if (!result.Success)
            {
                await DialogHelper.ShowFailedResult(result);
                return;
            }

            await this.viewModel.Save();

            this.Close();
        }

        private async void TestButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Result result = this.viewModel.Validate();
            if (!result.Success)
            {
                await DialogHelper.ShowFailedResult(result);
                return;
            }

            OverlayItemV3ModelBase item = this.viewModel.GetItem();

            CommandParametersModel parameters = CommandParametersModel.GetTestParameters(new Dictionary<string, string>());
            parameters = await DialogHelper.ShowEditTestCommandParametersDialog(parameters);
            if (parameters == null)
            {
                return;
            }

            OverlayWidgetV3Model widget = new OverlayWidgetV3Model(item);
            await widget.Test();
        }
    }
}
