using MixItUp.Base;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.GameQueue
{
    /// <summary>
    /// Interaction logic for GameQueueControl.xaml
    /// </summary>
    public partial class GameQueueControl : MainControlBase
    {
        private ObservableCollection<UserViewModel> gameQueueUsers = new ObservableCollection<UserViewModel>();

        public GameQueueControl()
        {
            InitializeComponent();
        }

        protected override Task InitializeInternal()
        {
            this.GameQueueUsersListView.ItemsSource = gameQueueUsers;

            ChannelSession.OnGameQueueUpdated += ChannelSession_OnGameQueueUpdated;

            return Task.FromResult(0);
        }

        private void EnableGameQueueToggleButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            ChannelSession.GameQueueEnabled = this.EnableGameQueueToggleButton.IsChecked.GetValueOrDefault();
        }

        private void MoveUpButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Button button = (Button)sender;
            UserViewModel user = (UserViewModel)button.DataContext;

            int index = ChannelSession.GameQueue.IndexOf(user);
            index = MathHelper.Clamp((index - 1), 0, ChannelSession.GameQueue.Count - 1);

            ChannelSession.GameQueue.Remove(user);
            ChannelSession.GameQueue.Insert(index, user);

            ChannelSession.GameQueueUpdated();
        }

        private void MoveDownButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Button button = (Button)sender;
            UserViewModel user = (UserViewModel)button.DataContext;

            int index = ChannelSession.GameQueue.IndexOf(user);
            index = MathHelper.Clamp((index + 1), 0, ChannelSession.GameQueue.Count - 1);

            ChannelSession.GameQueue.Remove(user);
            ChannelSession.GameQueue.Insert(index, user);

            ChannelSession.GameQueueUpdated();
        }

        private void DeleteButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Button button = (Button)sender;
            UserViewModel user = (UserViewModel)button.DataContext;
            ChannelSession.GameQueue.Remove(user);

            ChannelSession.GameQueueUpdated();
        }

        private void ChannelSession_OnGameQueueUpdated(object sender, System.EventArgs e)
        {
            gameQueueUsers.Clear();
            foreach (UserViewModel user in ChannelSession.GameQueue)
            {
                gameQueueUsers.Add(user);
            }
        }
    }
}
