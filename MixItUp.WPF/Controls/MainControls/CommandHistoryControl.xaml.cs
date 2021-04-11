using MixItUp.Base.ViewModel;
using MixItUp.Base.ViewModel.MainControls;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for CommandHistoryControl.xaml
    /// </summary>
    public partial class CommandHistoryControl : MainControlBase
    {
        private CommandHistoryMainControlViewModel viewModel;

        public CommandHistoryControl()
        {
            InitializeComponent();
        }

        protected override Task InitializeInternal()
        {
            this.DataContext = this.viewModel = new CommandHistoryMainControlViewModel((MainWindowViewModel)this.Window.ViewModel);
            this.viewModel.UncheckSelectAll += ViewModel_UncheckSelectAll;
            return Task.FromResult(0);
        }

        private void ViewModel_UncheckSelectAll(object sender, System.EventArgs e)
        {
            this.SelectAllCheckBox.IsChecked = false;
        }

        private void SelectAllCheckBox_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            this.viewModel.SetSelectedStateForAll(state: true);
        }

        private void SelectAllCheckBox_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            this.viewModel.SetSelectedStateForAll(state: false);
        }
    }
}
