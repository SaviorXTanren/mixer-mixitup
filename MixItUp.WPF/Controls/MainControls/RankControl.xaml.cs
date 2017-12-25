using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for RankControl.xaml
    /// </summary>
    public partial class RankControl : MainCommandControlBase
    {
        private const string RankChangedCommandName = "User Rank Changed";

        private ObservableCollection<UserRankViewModel> ranks = new ObservableCollection<UserRankViewModel>();

        public RankControl()
        {
            InitializeComponent();
        }

        protected override Task InitializeInternal()
        {
            this.RanksListView.ItemsSource = this.ranks;
            this.ranks.Clear();
            foreach (UserRankViewModel rank in ChannelSession.Settings.Ranks.OrderBy(r => r.MinimumPoints))
            {
                this.ranks.Add(rank);
            }

            this.ResetRankComboBox.ItemsSource = new List<string>() { "Never", "Yearly", "Monthly", "Weekly", "Daily" };

            this.RankToggleSwitch.IsChecked = ChannelSession.Settings.RankAcquisition.Enabled;
            this.RankPointsNameTextBox.Text = ChannelSession.Settings.RankAcquisition.Name;
            this.RankPointsAmountTextBox.Text = ChannelSession.Settings.RankAcquisition.AcquireAmount.ToString();
            this.RankPointsTimeTextBox.Text = ChannelSession.Settings.RankAcquisition.AcquireInterval.ToString();
            this.RankFollowBonusTextBox.Text = ChannelSession.Settings.RankAcquisition.FollowBonus.ToString();
            this.RankHostBonusTextBox.Text = ChannelSession.Settings.RankAcquisition.HostBonus.ToString();
            this.RankSubscribeBonusTextBox.Text = ChannelSession.Settings.RankAcquisition.SubscribeBonus.ToString();
            this.ResetRankComboBox.SelectedItem = ChannelSession.Settings.RankAcquisition.ResetInterval;
            this.RankGrid.IsEnabled = !ChannelSession.Settings.RankAcquisition.Enabled;

            if (ChannelSession.Settings.RankAcquisition.ShouldBeReset())
            {
                this.ResetRanks();
            }

            this.UpdateRankChangedCommand();

            return base.InitializeInternal();
        }

        private async void RankToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.RankPointsNameTextBox.Text))
            {
                await MessageBoxHelper.ShowMessageDialog("A points name must be specified");
                this.RankToggleSwitch.IsChecked = false;
                return;
            }

            int rankAmount = 0;
            if (string.IsNullOrEmpty(this.RankPointsAmountTextBox.Text) || !int.TryParse(this.RankPointsAmountTextBox.Text, out rankAmount) || rankAmount < 0)
            {
                await MessageBoxHelper.ShowMessageDialog("The points rate must be 0 or greater");
                this.RankToggleSwitch.IsChecked = false;
                return;
            }

            int rankTime = 0;
            if (string.IsNullOrEmpty(this.RankPointsTimeTextBox.Text) || !int.TryParse(this.RankPointsTimeTextBox.Text, out rankTime) || rankTime < 1)
            {
                await MessageBoxHelper.ShowMessageDialog("The points interval be greater than 1");
                this.RankToggleSwitch.IsChecked = false;
                return;
            }

            int followBonus = 0;
            if (string.IsNullOrEmpty(this.RankFollowBonusTextBox.Text) || !int.TryParse(this.RankFollowBonusTextBox.Text, out followBonus) || followBonus < 0)
            {
                await MessageBoxHelper.ShowMessageDialog("The Follow bonus must be 0 or greater");
                this.RankToggleSwitch.IsChecked = false;
                return;
            }

            int hostBonus = 0;
            if (string.IsNullOrEmpty(this.RankHostBonusTextBox.Text) || !int.TryParse(this.RankHostBonusTextBox.Text, out hostBonus) || hostBonus < 0)
            {
                await MessageBoxHelper.ShowMessageDialog("The Host bonus must be 0 or greater");
                this.RankToggleSwitch.IsChecked = false;
                return;
            }

            int subscribeBonus = 0;
            if (string.IsNullOrEmpty(this.RankSubscribeBonusTextBox.Text) || !int.TryParse(this.RankSubscribeBonusTextBox.Text, out subscribeBonus) || subscribeBonus < 0)
            {
                await MessageBoxHelper.ShowMessageDialog("The Subscribe bonus must be 0 or greater");
                this.RankToggleSwitch.IsChecked = false;
                return;
            }

            if (this.ranks.Count() < 1)
            {
                await MessageBoxHelper.ShowMessageDialog("At least one rank must be created");
                this.RankToggleSwitch.IsChecked = false;
                return;
            }

            await this.Window.RunAsyncOperation(async () =>
            {
                ChannelSession.Settings.RankAcquisition.Name = this.RankPointsNameTextBox.Text;
                ChannelSession.Settings.RankAcquisition.AcquireAmount = rankAmount;
                ChannelSession.Settings.RankAcquisition.AcquireInterval = rankTime;
                ChannelSession.Settings.RankAcquisition.FollowBonus = followBonus;
                ChannelSession.Settings.RankAcquisition.HostBonus = hostBonus;
                ChannelSession.Settings.RankAcquisition.SubscribeBonus = subscribeBonus;
                ChannelSession.Settings.RankAcquisition.ResetInterval = (string)this.ResetRankComboBox.SelectedItem;
                ChannelSession.Settings.RankAcquisition.Enabled = true;

                await ChannelSession.SaveSettings();
            });

            this.RankGrid.IsEnabled = false;
        }

        private async void RankToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                ChannelSession.Settings.RankAcquisition.Enabled = false;

                await ChannelSession.SaveSettings();
            });

            this.RankGrid.IsEnabled = true;
        }

        private void DeleteRankButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            UserRankViewModel rank = (UserRankViewModel)button.DataContext;
            ChannelSession.Settings.Ranks.Remove(rank);
            this.ranks.Remove(rank);
        }

        private async void AddRankButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.RankNameTextBox.Text))
            {
                await MessageBoxHelper.ShowMessageDialog("A rank name must be specified");
                this.RankToggleSwitch.IsChecked = false;
                return;
            }

            int rankAmount = 0;
            if (string.IsNullOrEmpty(this.RankAmountTextBox.Text) || !int.TryParse(this.RankAmountTextBox.Text, out rankAmount) || rankAmount < 0)
            {
                await MessageBoxHelper.ShowMessageDialog("A rank amount must be specified");
                this.RankToggleSwitch.IsChecked = false;
                return;
            }

            if (this.ranks.Any(r => r.Name.Equals(this.RankNameTextBox.Text) || r.MinimumPoints == rankAmount))
            {
                await MessageBoxHelper.ShowMessageDialog("Every rank must have a unique name and minimum amount");
                this.RankToggleSwitch.IsChecked = false;
                return;
            }

            UserRankViewModel newRank = new UserRankViewModel(this.RankNameTextBox.Text, rankAmount);
            ChannelSession.Settings.Ranks.Add(newRank);

            this.ranks.Clear();
            foreach (UserRankViewModel rank in ChannelSession.Settings.Ranks.OrderBy(r => r.MinimumPoints))
            {
                this.ranks.Add(rank);
            }

            ChannelSession.Settings.Ranks = this.ranks.ToList();

            this.RankNameTextBox.Clear();
            this.RankAmountTextBox.Clear();
        }

        private void NewInteractiveCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommandWindow window = new CommandWindow(new CustomCommandDetailsControl(new CustomCommand(RankControl.RankChangedCommandName)));
            window.CommandSaveSuccessfully += Window_CommandSaveSuccessfully;
            window.Closed += Window_Closed;
            window.Show();
        }

        private async void CommandButtons_PlayClicked(object sender, RoutedEventArgs e)
        {
            await this.HandleCommandPlay(sender);
        }

        private void CommandButtons_StopClicked(object sender, RoutedEventArgs e)
        {
            this.HandleCommandStop(sender);
        }

        private void CommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            CustomCommand command = this.GetCommandFromCommandButtons<CustomCommand>(sender);
            if (command != null)
            {
                CommandWindow window = new CommandWindow(new CustomCommandDetailsControl(command));
                window.Closed += Window_Closed;
                window.Show();
            }
        }

        private async void CommandButtons_DeleteClicked(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                CustomCommand command = this.GetCommandFromCommandButtons<CustomCommand>(sender);
                if (command != null)
                {
                    ChannelSession.Settings.RankChangedCommand = null;
                    await ChannelSession.SaveSettings();
                    this.UpdateRankChangedCommand();
                }
            });
        }

        private void CommandButtons_EnableDisableToggled(object sender, RoutedEventArgs e)
        {
            this.HandleCommandEnableDisable(sender);
        }

        private void Window_CommandSaveSuccessfully(object sender, CommandBase e)
        {
            ChannelSession.Settings.RankChangedCommand = (CustomCommand)e;
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            this.UpdateRankChangedCommand();
        }

        private void UpdateRankChangedCommand()
        {
            if (ChannelSession.Settings.RankChangedCommand != null)
            {
                this.NewInteractiveCommandButton.Visibility = Visibility.Collapsed;
                this.CommandButtons.Visibility = Visibility.Visible;
                this.CommandButtons.DataContext = ChannelSession.Settings.RankChangedCommand;
            }
            else
            {
                this.NewInteractiveCommandButton.Visibility = Visibility.Visible;
                this.CommandButtons.Visibility = Visibility.Collapsed;
            }
        }

        private async void ResetRankManuallyButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                if (await MessageBoxHelper.ShowConfirmationDialog("Do you want to reset all ranks?"))
                {
                    this.ResetRanks();
                }
            });
        }

        private void ResetRanks()
        {
            foreach (var kvp in ChannelSession.Settings.UserData)
            {
                kvp.Value.ResetRank();
            }
            ChannelSession.Settings.RankAcquisition.LastReset = new DateTimeOffset(DateTimeOffset.Now.Date);
        }
    }
}

