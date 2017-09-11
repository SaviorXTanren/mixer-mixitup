using MixItUp.Base;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.WPF.Util;
using System;
using System.Collections.Generic;
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
        public List<ChatUserViewModel> previousWinners = new List<ChatUserViewModel>();

        public GiveawayControl()
        {
            InitializeComponent();
        }

        protected override Task InitializeInternal()
        {
            this.GiveawayTypeComboBox.ItemsSource = new List<string>() { "Users", "Followers", "Subscribers" };

            return base.InitializeInternal();
        }

        private void EnableGiveawayButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.GiveawayItemTextBox.Text))
            {
                MessageBoxHelper.ShowError("An item to give away must be specified");
                return;
            }

            if (this.GiveawayTypeComboBox.SelectedIndex < 0)
            {
                MessageBoxHelper.ShowError("The allowed winners must be specified");
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

            IEnumerable<ChatUserViewModel> usersToSelectFrom = null;
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

            usersToSelectFrom = usersToSelectFrom.Where(u => !this.previousWinners.Contains(u));

            if (usersToSelectFrom.Count() > 0)
            {
                Random random = new Random();
                int index = random.Next(usersToSelectFrom.Count());
                ChatUserViewModel winner = usersToSelectFrom.ElementAt(index);
                this.previousWinners.Add(winner);

                this.GiveawayWinnerTextBlock.Text = winner.UserName;

                ChannelSession.BotChatClient.SendMessage(string.Format("Congratulations {0}, your won {1}! You'll find out how to get your prize momentarily!", winner.UserName, ChannelSession.Giveaway.Item));
            }
            else
            {
                MessageBoxHelper.ShowError("There are no users currently in chat that are either applicable to win or have not won already");
            }
        }
    }
}
