using MixItUp.Base;
using MixItUp.Base.ViewModel;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.WPF.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Giveaway
{
    /// <summary>
    /// Interaction logic for GiveawayControl.xaml
    /// </summary>
    public partial class GiveawayControl : MainControlBase
    {
        private class PreviousWinnerModel
        {
            public uint ID { get; set; }
            public string Username { get; set; }
            public string Prize { get; set; }
        }

        private ObservableCollection<PreviousWinnerModel> previousWinners = new ObservableCollection<PreviousWinnerModel>();

        public GiveawayControl()
        {
            InitializeComponent();
        }

        protected override Task InitializeInternal()
        {
            this.GiveawayTypeComboBox.ItemsSource = new List<string>() { "Users", "Followers", "Subscribers" };
            this.PreviousWinnersListView.ItemsSource = this.previousWinners;

            return base.InitializeInternal();
        }

        private void EnableGiveawayButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.GiveawayItemTextBox.Text))
            {
                MessageBoxHelper.ShowDialog("An item to give away must be specified");
                return;
            }

            if (this.GiveawayTypeComboBox.SelectedIndex < 0)
            {
                MessageBoxHelper.ShowDialog("The allowed winners must be specified");
                return;
            }

            ChannelSession.Giveaway.IsEnabled = true;
            ChannelSession.Giveaway.Item = this.GiveawayItemTextBox.Text;
            ChannelSession.Giveaway.Type = (string)this.GiveawayTypeComboBox.SelectedItem;

            this.GiveawayWinnerTextBlock.Text = "";
            this.EnableGiveawayButton.Visibility = Visibility.Collapsed;
            this.DisableGiveawayButton.Visibility = Visibility.Visible;
            this.PerformGiveawayButton.IsEnabled = true;
        }

        private void DisableGiveawayButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ChannelSession.Giveaway.IsEnabled = false;

            this.GiveawayWinnerTextBlock.Text = "";
            this.DisableGiveawayButton.Visibility = Visibility.Collapsed;
            this.EnableGiveawayButton.Visibility = Visibility.Visible;
            this.PerformGiveawayButton.IsEnabled = false;
        }

        private void PerformGiveawayButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ChannelSession.Giveaway.IsEnabled = false;

            this.GiveawayWinnerTextBlock.Text = "";
            this.DisableGiveawayButton.Visibility = Visibility.Collapsed;
            this.EnableGiveawayButton.Visibility = Visibility.Visible;
            this.PerformGiveawayButton.IsEnabled = false;

            IEnumerable<UserViewModel> usersToSelectFrom = null;
            switch (ChannelSession.Giveaway.Type)
            {
                case "Users":
                    usersToSelectFrom = ChannelSession.ChatUsers.Values;
                    break;
                case "Followers":
                    usersToSelectFrom = ChannelSession.ChatUsers.Values.Where(u => u.Roles.Contains(UserRole.Follower));
                    break;
                case "Subscribers":
                    usersToSelectFrom = ChannelSession.ChatUsers.Values.Where(u => u.Roles.Contains(UserRole.Subscriber));
                    break;
            }

            usersToSelectFrom = usersToSelectFrom.Where(u => !this.previousWinners.Select(w => w.ID).Contains(u.ID));

            if (usersToSelectFrom.Count() > 0)
            {
                Random random = new Random();
                int index = random.Next(usersToSelectFrom.Count());
                UserViewModel winner = usersToSelectFrom.ElementAt(index);
                this.previousWinners.Add(new PreviousWinnerModel() { ID = winner.ID, Username = winner.UserName, Prize = ChannelSession.Giveaway.Item });

                this.GiveawayWinnerTextBlock.Text = winner.UserName;

                ChannelSession.BotChatClient.SendMessage(string.Format("Congratulations {0}, you won {1}! You'll find out how to get your prize momentarily!", winner.UserName, ChannelSession.Giveaway.Item));
            }
            else
            {
                MessageBoxHelper.ShowDialog("There are no users currently in chat that are either applicable to win or have not won already");
            }
        }
    }
}
