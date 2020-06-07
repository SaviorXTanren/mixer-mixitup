using MixItUp.Base;
using MixItUp.Base.Model.User;
using MixItUp.Base.ViewModel.Controls.MainControls;
using MixItUp.Base.ViewModel.Window;
using MixItUp.WPF.Windows.Users;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for UsersControl.xaml
    /// </summary>
    public partial class UsersControl : MainControlBase
    {
        private ObservableCollection<UserDataModel> userData = new ObservableCollection<UserDataModel>();

        private UsersMainControlViewModel viewModel;

        public UsersControl()
        {
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

        private void UsernameFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.viewModel.UsernameFilter = this.UsernameFilterTextBox.Text;
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
            this.viewModel.SortColumnIndex = this.UserDataGridView.Columns.IndexOf(column);
            this.viewModel.SortDirection = column.SortDirection.GetValueOrDefault();
            this.viewModel.RefreshUsers();
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
