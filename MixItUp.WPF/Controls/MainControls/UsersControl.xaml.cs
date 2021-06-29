using MixItUp.Base.Model.User;
using MixItUp.Base.ViewModel.MainControls;
using MixItUp.Base.ViewModel;
using MixItUp.WPF.Windows.Users;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Threading;
using MixItUp.Base.Util;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for UsersControl.xaml
    /// </summary>
    public partial class UsersControl : MainControlBase
    {
        private UsersMainControlViewModel viewModel;
        private Timer textChangedTimer;

        public UsersControl()
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            textChangedTimer = new Timer((e) => UpdateText(), null, Timeout.Infinite, Timeout.Infinite);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.DataContext = this.viewModel = new UsersMainControlViewModel((MainWindowViewModel)this.Window.ViewModel);
            await this.viewModel.OnLoaded();
            await base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.viewModel.OnVisible();
        }

        private async Task UpdateText()
        {
            UIElement elementToFocus = null;
            await DispatcherHelper.Dispatcher.InvokeAsync(() =>
            {
                if (this.UsernameFilterTextBox.IsFocused) { elementToFocus = this.UsernameFilterTextBox; }
                else if (this.WatchTimeAmountSearchFilterTextBox.IsFocused) { elementToFocus = this.WatchTimeAmountSearchFilterTextBox; }
                else if (this.ConsumablesAmountSearchFilterTextBox.IsFocused) { elementToFocus = this.ConsumablesAmountSearchFilterTextBox; }
                return Task.CompletedTask;
            });

            await this.viewModel.RefreshUsersAsync();

            await DispatcherHelper.Dispatcher.InvokeAsync(() =>
            {
                if (elementToFocus != null)
                {
                    elementToFocus.Focus();
                }
                return Task.CompletedTask;
            });
        }

        private void UsernameFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.viewModel.UsernameFilter = this.UsernameFilterTextBox.Text;
            textChangedTimer.Change(500, Timeout.Infinite);
        }

        private void SearchFilterTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.WatchTimeAmountSearchFilterTextBox.Text = null;
            this.ConsumablesAmountSearchFilterTextBox.Text = null;
        }

        private void WatchTimeAmountSearchFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(this.WatchTimeAmountSearchFilterTextBox.Text, out int amount))
            {
                this.viewModel.WatchTimeAmountSearchFilter = amount;
                textChangedTimer.Change(500, Timeout.Infinite);
            }
        }

        private void ConsumablesAmountSearchFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(this.ConsumablesAmountSearchFilterTextBox.Text, out int amount))
            {
                this.viewModel.ConsumablesAmountSearchFilter = amount;
                textChangedTimer.Change(500, Timeout.Infinite);
            }
        }

        private void UserEditButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            UserDataModel userData = (UserDataModel)button.DataContext;
            UserDataEditorWindow window = new UserDataEditorWindow(userData);
            window.Closed += Window_Closed;
            window.Show();
        }

        private async void UserDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            UserDataModel userData = (UserDataModel)button.DataContext;
            await this.viewModel.DeleteUser(userData);
        }

        private void UserDataGridView_Sorted(object sender, DataGridColumn column)
        {
            this.viewModel.SetSortColumnIndexAndDirection(this.UserDataGridView.Columns.IndexOf(column), column.SortDirection.GetValueOrDefault());
        }

        private void ImportUserDataButton_Click(object sender, RoutedEventArgs e)
        {
            UserDataImportWindow window = new UserDataImportWindow();
            window.Closed += Window_Closed;
            window.Show();
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            this.viewModel.RefreshUsers();
        }
    }
}
