using MixItUp.Base;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Overlay;
using System.Threading.Tasks;

namespace MixItUp.WPF.Windows.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayWidgetV3EditorWindow.xaml
    /// </summary>
    public partial class OverlayWidgetV3EditorWindow : LoadingWindowBase
    {
        private OverlayWidgetV3EditorWindowViewModel viewModel;

        public OverlayWidgetV3EditorWindow(OverlayItemV3Type type)
        {
            this.viewModel = new OverlayWidgetV3EditorWindowViewModel(type);

            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        public OverlayWidgetV3EditorWindow(OverlayItemV3ModelBase item)
        {
            this.viewModel = new OverlayWidgetV3EditorWindowViewModel(item);

            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            this.DataContext = this.viewModel;
            await this.viewModel.OnOpen();
            await base.OnLoaded();
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
    }
}
