using MixItUp.Base.ViewModel.Accounts;

namespace MixItUp.WPF.Controls.Accounts
{
    /// <summary>
    /// Interaction logic for StreamingPlatformAccountControl.xaml
    /// </summary>
    public partial class StreamingPlatformAccountControl : LoadingControlBase
    {
        private StreamingPlatformAccountControlViewModel viewModel;

        public StreamingPlatformAccountControl()
        {
            InitializeComponent();

            this.Loaded += StreamingPlatformAccountControl_Loaded;
        }

        private async void StreamingPlatformAccountControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.viewModel = (StreamingPlatformAccountControlViewModel)this.DataContext;
            if (this.viewModel != null)
            {
                await this.viewModel.OnOpen();
            }
        }
    }
}
