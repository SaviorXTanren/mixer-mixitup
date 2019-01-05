using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.MainControls
{
    public class QueueUser
    {
        public UserViewModel user { get; set; }

        public int QueuePosition { get; set; }

        public string UserName { get { return this.user.UserName; } }

        public string PrimaryRole { get { return EnumHelper.GetEnumName(this.user.PrimaryRole); } }

        public QueueUser(UserViewModel user, int queuePosition)
        {
            this.user = user;
            this.QueuePosition = queuePosition;
        }
    }

    /// <summary>
    /// Interaction logic for GameQueueControl.xaml
    /// </summary>
    public partial class GameQueueControl : MainControlBase
    {
        private ObservableCollection<QueueUser> gameQueueUsers = new ObservableCollection<QueueUser>();

        public GameQueueControl()
        {
            InitializeComponent();
        }

        protected override Task InitializeInternal()
        {
            this.GameQueueUsersListView.ItemsSource = this.gameQueueUsers;

            this.SubPriorityToggleButton.IsChecked = ChannelSession.Settings.GameQueueSubPriority;

            this.Requirement.HideSettingsRequirement();
            this.Requirement.SetRequirements(ChannelSession.Settings.GameQueueRequirements);

            GlobalEvents.OnGameQueueUpdated += ChannelSession_OnGameQueueUpdated;

            return Task.FromResult(0);
        }

        protected override Task OnVisibilityChanged()
        {
            this.RefreshQueueList();
            return Task.FromResult(0);
        }

        private void RefreshQueueList()
        {
            this.gameQueueUsers.Clear();
            for (int i = 0; i < ChannelSession.GameQueue.Count; i++)
            {
                this.gameQueueUsers.Add(new QueueUser(ChannelSession.GameQueue[i], (i + 1)));
            }
        }

        private void SubPriorityToggleButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            ChannelSession.Settings.GameQueueSubPriority = this.SubPriorityToggleButton.IsChecked.GetValueOrDefault();
        }

        private async void EnableGameQueueToggleButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!await this.Requirement.Validate())
            {
                this.EnableGameQueueToggleButton.IsChecked = false;
                return;
            }

            ChannelSession.Settings.GameQueueRequirements = this.Requirement.GetRequirements();

            ChannelSession.GameQueueEnabled = true;
            this.SubPriorityToggleButton.IsEnabled = this.Requirement.IsEnabled = false;
            this.ClearQueueButton.IsEnabled = true;
        }

        private void EnableGameQueueToggleButton_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            ChannelSession.GameQueueEnabled = false;
            this.ClearQueueButton.IsEnabled = this.SubPriorityToggleButton.IsEnabled = this.Requirement.IsEnabled = true;
            this.ClearQueueButton.IsEnabled = false;
        }

        private void ClearQueueButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ChannelSession.GameQueue.Clear();
            GlobalEvents.GameQueueUpdated();
        }

        private void MoveUpButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Button button = (Button)sender;
            QueueUser user = (QueueUser)button.DataContext;

            int index = ChannelSession.GameQueue.IndexOf(user.user);
            index = MathHelper.Clamp((index - 1), 0, ChannelSession.GameQueue.Count - 1);

            ChannelSession.GameQueue.Remove(user.user);
            ChannelSession.GameQueue.Insert(index, user.user);

            GlobalEvents.GameQueueUpdated();
        }

        private void MoveDownButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Button button = (Button)sender;
            QueueUser user = (QueueUser)button.DataContext;

            int index = ChannelSession.GameQueue.IndexOf(user.user);
            index = MathHelper.Clamp((index + 1), 0, ChannelSession.GameQueue.Count - 1);

            ChannelSession.GameQueue.Remove(user.user);
            ChannelSession.GameQueue.Insert(index, user.user);

            GlobalEvents.GameQueueUpdated();
        }

        private void DeleteButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Button button = (Button)sender;
            QueueUser user = (QueueUser)button.DataContext;
            ChannelSession.GameQueue.Remove(user.user);

            GlobalEvents.GameQueueUpdated();
        }

        private void ChannelSession_OnGameQueueUpdated(object sender, System.EventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.EnableGameQueueToggleButton.IsChecked = ChannelSession.GameQueueEnabled;
                this.SubPriorityToggleButton.IsEnabled = this.Requirement.IsEnabled = (!ChannelSession.GameQueueEnabled);
                this.ClearQueueButton.IsEnabled = ChannelSession.GameQueueEnabled;
                this.RefreshQueueList();
            }));
        }
    }
}
