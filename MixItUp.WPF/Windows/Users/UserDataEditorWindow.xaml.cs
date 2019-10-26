using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Controls.Currency;
using MixItUp.WPF.Windows.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Windows.Users
{
    /// <summary>
    /// Interaction logic for UserDataEditorWindow.xaml
    /// </summary>
    public partial class UserDataEditorWindow : LoadingWindowBase
    {
        private const string UserEntranceCommandName = "Entrance Command";

        private UserViewModel user;

        private ObservableCollection<ChatCommand> userOnlyCommands = new ObservableCollection<ChatCommand>();

        public UserDataEditorWindow(UserDataViewModel userData)
        {
            this.user = new UserViewModel(userData);

            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            this.UserOnlyChatCommandsListView.ItemsSource = this.userOnlyCommands;

            this.CurrencyRankExemptToggleButton.IsChecked = this.user.Data.IsCurrencyRankExempt;
            this.SparkExemptToggleButton.IsChecked = this.user.Data.IsSparkExempt;

            await this.RefreshData();
        }

        private async Task RefreshData()
        {
            this.DataContext = null;

            await this.user.RefreshDetails(force: true);

            this.CurrencyRankStackPanel.Children.Clear();
            foreach (UserCurrencyViewModel currency in ChannelSession.Settings.Currencies.Values.ToList())
            {
                UserCurrencyDataViewModel currencyData = this.user.Data.GetCurrency(currency);
                this.CurrencyRankStackPanel.Children.Add(new UserCurrencyIndividualEditorControl(currencyData));
            }

            this.InventoryStackPanel.Children.Clear();
            foreach (UserInventoryViewModel inventory in ChannelSession.Settings.Inventories.Values.ToList())
            {
                UserInventoryDataViewModel inventoryData = this.user.Data.GetInventory(inventory);
                this.InventoryStackPanel.Children.Add(new UserInventoryEditorControl(inventory, inventoryData));
            }

            this.UserOnlyChatCommandsListView.Visibility = Visibility.Collapsed;
            this.userOnlyCommands.Clear();
            if (this.user.Data.CustomCommands.Count > 0)
            {
                this.UserOnlyChatCommandsListView.Visibility = Visibility.Visible;
                foreach (ChatCommand command in this.user.Data.CustomCommands)
                {
                    this.userOnlyCommands.Add(command);
                }
            }

            if (this.user.Data.EntranceCommand != null)
            {
                this.NewEntranceCommandButton.Visibility = Visibility.Collapsed;
                this.ExistingEntranceCommandButtons.Visibility = Visibility.Visible;
                this.ExistingEntranceCommandButtons.DataContext = this.user.Data.EntranceCommand;
            }
            else
            {
                this.NewEntranceCommandButton.Visibility = Visibility.Visible;
                this.ExistingEntranceCommandButtons.Visibility = Visibility.Collapsed;
            }

            if (ChannelSession.Services.Patreon != null)
            {
                this.PatreonUserComboBox.IsEnabled = true;
                this.PatreonUserComboBox.ItemsSource = ChannelSession.Services.Patreon.CampaignMembers;
                this.PatreonUserComboBox.SelectedItem = this.user.PatreonUser;
            }

            List<Tuple<string, string>> userMetricsList1 = new List<Tuple<string, string>>();
            userMetricsList1.Add(new Tuple<string, string>("Streams Watched:", user.Data.TotalStreamsWatched.ToString()));
            userMetricsList1.Add(new Tuple<string, string>("Tagged In Chat:", user.Data.TotalTimesTagged.ToString()));
            userMetricsList1.Add(new Tuple<string, string>("Sparks Spent:", user.Data.TotalSparksSpent.ToString()));
            userMetricsList1.Add(new Tuple<string, string>("Skills Used:", user.Data.TotalSkillsUsed.ToString()));
            userMetricsList1.Add(new Tuple<string, string>("Subs Gifted:", user.Data.TotalSubsGifted.ToString()));
            userMetricsList1.Add(new Tuple<string, string>("Cumulative Months Subbed:", user.Data.TotalMonthsSubbed.ToString()));
            this.UserMetricsItemsControl1.ItemsSource = userMetricsList1;

            List<Tuple<string, string>> userMetricsList2 = new List<Tuple<string, string>>();
            userMetricsList2.Add(new Tuple<string, string>("Chat Messages Sent:", user.Data.TotalChatMessageSent.ToString()));
            userMetricsList2.Add(new Tuple<string, string>("Commands Run:", user.Data.TotalCommandsRun.ToString()));
            userMetricsList2.Add(new Tuple<string, string>("Embers Spent:", user.Data.TotalEmbersSpent.ToString()));
            userMetricsList2.Add(new Tuple<string, string>("Amount Donated:", string.Format("{0:C}", Math.Round(user.Data.TotalAmountDonated, 2))));
            userMetricsList2.Add(new Tuple<string, string>("Subs Received:", user.Data.TotalSubsReceived.ToString()));
            this.UserMetricsItemsControl2.ItemsSource = userMetricsList2;

            this.DataContext = this.user;
        }

        private void AddUserOnlyCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommandWindow window = new CommandWindow(new ChatCommandDetailsControl(autoAddToChatCommands: false));
            window.CommandSaveSuccessfully += NewUserOnlyCommandWindow_CommandSaveSuccessfully;
            window.Closed += Window_Closed;
            window.Show();
        }

        private void NewUserOnlyCommandWindow_CommandSaveSuccessfully(object sender, CommandBase e)
        {
            this.user.Data.CustomCommands.Add((ChatCommand)e);
        }

        private void UserOnlyChatCommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
            ChatCommand command = commandButtonsControl.GetCommandFromCommandButtons<ChatCommand>(sender);
            if (command != null)
            {
                CommandWindow window = new CommandWindow(new ChatCommandDetailsControl(command, autoAddToChatCommands: false));
                window.Closed += Window_Closed;
                window.Show();
            }
        }

        private async void UserOnlyChatCommandButtons_DeleteClicked(object sender, RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
                ChatCommand command = commandButtonsControl.GetCommandFromCommandButtons<ChatCommand>(sender);
                if (command != null)
                {
                    this.user.Data.CustomCommands.Remove(command);
                    await ChannelSession.SaveSettings();
                    await this.RefreshData();
                }
            });
        }

        private void ExistingEntranceCommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
            CustomCommand command = commandButtonsControl.GetCommandFromCommandButtons<CustomCommand>(sender);
            if (command != null)
            {
                CommandWindow window = new CommandWindow(new CustomCommandDetailsControl(command));
                window.Closed += Window_Closed;
                window.Show();
            }
        }

        private async void ExistingEntranceCommandButtons_DeleteClicked(object sender, RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
                CustomCommand command = commandButtonsControl.GetCommandFromCommandButtons<CustomCommand>(sender);
                if (command != null)
                {
                    this.user.Data.EntranceCommand = null;
                    await ChannelSession.SaveSettings();
                    await this.RefreshData();
                }
            });
        }

        private void NewEntranceCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommandWindow window = new CommandWindow(new CustomCommandDetailsControl(new CustomCommand(UserEntranceCommandName)));
            window.CommandSaveSuccessfully += NewEntranceCommandWindow_CommandSaveSuccessfully;
            window.Closed += Window_Closed;
            window.Show();
        }

        private void NewEntranceCommandWindow_CommandSaveSuccessfully(object sender, CommandBase e)
        {
            this.user.Data.EntranceCommand = (CustomCommand)e;
        }

        private async void Window_Closed(object sender, System.EventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                await this.RefreshData();
            });
        }

        private void CurrencyRankExemptToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            this.user.Data.IsCurrencyRankExempt = this.CurrencyRankExemptToggleButton.IsChecked.GetValueOrDefault();
            foreach (UserCurrencyViewModel currency in ChannelSession.Settings.Currencies.Values)
            {
                this.user.Data.ResetCurrencyAmount(currency);
            }
            ChannelSession.Settings.UserData.ManualValueChanged(this.user.ID);
        }

        private void SparkExemptToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            this.user.Data.IsSparkExempt = this.SparkExemptToggleButton.IsChecked.GetValueOrDefault();
        }

        private void PatreonUserComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            this.user.Data.PatreonUserID = null;
            if (this.PatreonUserComboBox.SelectedIndex >= 0)
            {
                PatreonCampaignMember campaignMember = (PatreonCampaignMember)this.PatreonUserComboBox.SelectedItem;
                if (campaignMember != null)
                {
                    this.user.Data.PatreonUserID = campaignMember.UserID;
                }
            }
        }
    }
}
