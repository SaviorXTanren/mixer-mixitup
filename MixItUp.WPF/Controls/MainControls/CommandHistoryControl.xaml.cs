using MixItUp.Base.Util;
using MixItUp.Base.ViewModel;
using MixItUp.Base.ViewModel.MainControls;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for CommandHistoryControl.xaml
    /// </summary>
    public partial class CommandHistoryControl : MainControlBase
    {
        private CommandHistoryMainControlViewModel viewModel;

        private Timer textChangedTimer;

        public CommandHistoryControl()
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            textChangedTimer = new Timer((e) => UpdateText(), null, Timeout.Infinite, Timeout.Infinite);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            InitializeComponent();
        }

        protected override Task InitializeInternal()
        {
            this.DataContext = this.viewModel = new CommandHistoryMainControlViewModel((MainWindowViewModel)this.Window.ViewModel);

            this.viewModel.UncheckSelectAll += ViewModel_UncheckSelectAll;

            return Task.CompletedTask;
        }

        private async Task UpdateText()
        {
            this.viewModel.RefreshList();
            await DispatcherHelper.Dispatcher.InvokeAsync(() =>
            {
                this.UsernameFilterTextBox.Focus();
                return Task.CompletedTask;
            });
        }

        private void UsernameFilterTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            this.viewModel.UsernameFilter = this.UsernameFilterTextBox.Text;
            textChangedTimer.Change(500, Timeout.Infinite);
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
