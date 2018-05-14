using MixItUp.Base;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Currency;
using System.Collections.ObjectModel;
using System.Linq;
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
        private ObservableCollection<UserDataViewModel> userData = new ObservableCollection<UserDataViewModel>();

        public UsersControl()
        {
            InitializeComponent();

            this.UserDataGridView.ItemsSource = this.userData;
        }

        protected override async Task InitializeInternal()
        {
            this.RefreshList();
            await base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            this.RefreshList();
            await base.OnVisibilityChanged();
        }

        private void RefreshList()
        {
            string filter = this.UsernameFilterTextBox.Text;
            if (!string.IsNullOrEmpty(filter))
            {
                filter = filter.ToLower();
            }

            this.LimitingResultsMessage.Visibility = Visibility.Collapsed;
            this.userData.Clear();

            foreach (var userData in ChannelSession.Settings.UserData.Values.ToList())
            {
                if (string.IsNullOrEmpty(filter) || userData.UserName.ToLower().Contains(filter))
                {
                    this.userData.Add(userData);
                }

                if (this.userData.Count >= 200)
                {
                    this.LimitingResultsMessage.Visibility = Visibility.Visible;
                    break;
                }
            }
        }

        private void UsernameFilterTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            this.RefreshList();
        }

        private void UserEditButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            UserDataViewModel userData = (UserDataViewModel)button.DataContext;
            UserDataEditorWindow window = new UserDataEditorWindow(userData);
            window.Closed += Window_Closed;
            window.Show();
        }

        private async void UserDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            UserDataViewModel userData = (UserDataViewModel)button.DataContext;
            if (await MessageBoxHelper.ShowConfirmationDialog("This will delete this user's data, which includes their Hours, Currency, Rank, & Custom User Commands. Are you sure you want to do this?"))
            {
                ChannelSession.Settings.UserData.Remove(userData.ID);
            }
            this.RefreshList();
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            this.RefreshList();
        }
    }
}
