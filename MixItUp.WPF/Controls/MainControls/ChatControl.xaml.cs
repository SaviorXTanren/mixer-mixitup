using MixItUp.Base.ViewModel.Controls.MainControls;
using MixItUp.Base.ViewModel.User;
using MixItUp.Base.ViewModel.Window;
using MixItUp.WPF.Controls.Chat;
using MixItUp.WPF.Windows.Dashboard;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for ChatControl.xaml
    /// </summary>
    public partial class ChatControl : MainControlBase
    {
        private ChatMainControlViewModel viewModel;

        public ChatControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            await this.ChatList.Initialize(this.Window);

            this.viewModel = new ChatMainControlViewModel((MainWindowViewModel)this.Window.ViewModel);

            await this.viewModel.OnLoaded();
            this.DataContext = this.viewModel;
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.viewModel.OnVisible();
            await base.OnVisibilityChanged();
        }

        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            DashboardWindow window = new DashboardWindow();
            window.Show();
        }

        private async void UserList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.UserList.SelectedItem != null && this.UserList.SelectedItem is UserViewModel)
            {
                UserViewModel user = (UserViewModel)this.UserList.SelectedItem;
                this.UserList.SelectedIndex = -1;
                await ChatUserDialogControl.ShowUserDialog(user);
            }
        }
    }
}
