using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Controls.MainControls;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Currency
{
    /// <summary>
    /// Interaction logic for CurrencyDataControl.xaml
    /// </summary>
    public partial class CurrencyDataControl : MainCommandControlBase
    {
        private const int MinimizedGroupBoxHeight = 35;
        private const string RankChangedCommandName = "User Rank Changed";

        private CurrencyAndRankControl currencyControl = null;
        private UserCurrencyViewModel currency = null;
        private ObservableCollection<UserRankViewModel> ranks = new ObservableCollection<UserRankViewModel>();

        public CurrencyDataControl(CurrencyAndRankControl currencyControl, UserCurrencyViewModel currency)
        {
            this.currencyControl = currencyControl;
            this.DataContext = this.currency = currency;

            InitializeComponent();
        }

        public void Minimize() { this.GroupBox.Height = MinimizedGroupBoxHeight; }

        protected override Task InitializeInternal()
        {
            this.ResetCurrencyComboBox.ItemsSource = new List<string>() { "Never", "Yearly", "Monthly", "Weekly", "Daily" };

            this.RanksListView.ItemsSource = this.ranks;
            this.ranks.Clear();

            if (this.currency != null)
            {
                this.HeaderTextBlock.Text = this.currency.Name;
                foreach (UserRankViewModel rank in this.currency.Ranks.OrderBy(r => r.MinimumPoints))
                {
                    this.ranks.Add(rank);
                }

                this.CurrencyNameTextBox.Text = this.currency.Name;
                this.CurrencyAmountTextBox.Text = this.currency.AcquireAmount.ToString();
                this.CurrencyTimeTextBox.Text = this.currency.AcquireInterval.ToString();
                this.CurrencyFollowBonusTextBox.Text = this.currency.FollowBonus.ToString();
                this.CurrencyHostBonusTextBox.Text = this.currency.HostBonus.ToString();
                this.CurrencySubscribeBonusTextBox.Text = this.currency.SubscribeBonus.ToString();
                this.ResetCurrencyComboBox.SelectedItem = this.currency.ResetInterval;
                this.CurrencyGrid.IsEnabled = !this.currency.Enabled;

                this.CurrencyToggleSwitch.IsChecked = this.currency.Enabled;
                if (this.currency.Ranks.Count > 0)
                {
                    this.IsRankToggleSwitch.IsChecked = true;
                }

                if (this.currency.ShouldBeReset())
                {
                    this.ResetCurrency();
                }

                this.UpdateRankChangedCommand();
            }

            return base.InitializeInternal();
        }

        private void GroupBoxHeader_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (this.GroupBox.Height == MinimizedGroupBoxHeight)
            {
                this.GroupBox.Height = Double.NaN;
            }
            else
            {
                this.Minimize();
            }
        }

        private async void DeleteCurrencyButton_Click(object sender, RoutedEventArgs e)
        {
            await this.currencyControl.DeleteCurrency(this.currency);
        }

        private async void CurrencyToggleSwitch_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                if (string.IsNullOrEmpty(this.CurrencyNameTextBox.Text))
                {
                    await MessageBoxHelper.ShowMessageDialog("A currency name must be specified");
                    this.CurrencyToggleSwitch.IsChecked = false;
                    return;
                }

                int currencyAmount = 0;
                if (string.IsNullOrEmpty(this.CurrencyAmountTextBox.Text) || !int.TryParse(this.CurrencyAmountTextBox.Text, out currencyAmount) || currencyAmount < 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("The currency rate must be 0 or greater");
                    this.CurrencyToggleSwitch.IsChecked = false;
                    return;
                }

                int currencyTime = 0;
                if (string.IsNullOrEmpty(this.CurrencyTimeTextBox.Text) || !int.TryParse(this.CurrencyTimeTextBox.Text, out currencyTime) || currencyTime < 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("The currency interval must be 0 or greater");
                    this.CurrencyToggleSwitch.IsChecked = false;
                    return;
                }

                if ((currencyAmount == 0 && currencyTime != 0) || (currencyAmount != 0 && currencyTime == 0))
                {
                    await MessageBoxHelper.ShowMessageDialog("The currency rate and interval must be both greater than 0 or both equal to 0");
                    this.CurrencyToggleSwitch.IsChecked = false;
                    return;
                }

                int followBonus = 0;
                if (string.IsNullOrEmpty(this.CurrencyFollowBonusTextBox.Text) || !int.TryParse(this.CurrencyFollowBonusTextBox.Text, out followBonus) || followBonus < 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("The Follow bonus must be 0 or greater");
                    this.CurrencyToggleSwitch.IsChecked = false;
                    return;
                }

                int hostBonus = 0;
                if (string.IsNullOrEmpty(this.CurrencyHostBonusTextBox.Text) || !int.TryParse(this.CurrencyHostBonusTextBox.Text, out hostBonus) || hostBonus < 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("The Host bonus must be 0 or greater");
                    this.CurrencyToggleSwitch.IsChecked = false;
                    return;
                }

                int subscribeBonus = 0;
                if (string.IsNullOrEmpty(this.CurrencySubscribeBonusTextBox.Text) || !int.TryParse(this.CurrencyHostBonusTextBox.Text, out subscribeBonus) || subscribeBonus < 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("The Subscribe bonus must be 0 or greater");
                    this.CurrencyToggleSwitch.IsChecked = false;
                    return;
                }

                if (this.IsRankToggleSwitch.IsChecked.GetValueOrDefault() && this.ranks.Count() < 1)
                {
                    await MessageBoxHelper.ShowMessageDialog("At least one rank must be created");
                    this.CurrencyToggleSwitch.IsChecked = false;
                    return;
                }

                this.HeaderTextBlock.Text = this.currency.Name = this.CurrencyNameTextBox.Text;
                this.currency.AcquireAmount = currencyAmount;
                this.currency.AcquireInterval = currencyTime;
                this.currency.FollowBonus = followBonus;
                this.currency.HostBonus = hostBonus;
                this.currency.SubscribeBonus = subscribeBonus;
                this.currency.ResetInterval = (string)this.ResetCurrencyComboBox.SelectedItem;
                this.currency.Enabled = true;

                if (!ChannelSession.Settings.Currencies.ContainsKey(this.currency.Name))
                {
                    ChannelSession.Settings.Currencies[this.currency.Name] = this.currency;
                }

                await ChannelSession.SaveSettings();

                this.CurrencyGrid.IsEnabled = false;
                this.IsRankToggleSwitch.IsEnabled = false;
            });
        }

        private async void CurrencyToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                this.currency.Enabled = false;

                await ChannelSession.SaveSettings();
            });

            this.CurrencyGrid.IsEnabled = true;
            this.IsRankToggleSwitch.IsEnabled = true;
        }

        private void IsRankToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            this.RankGrid.Visibility = (this.IsRankToggleSwitch.IsChecked.GetValueOrDefault()) ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void ResetCurrencyManuallyButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                if (await MessageBoxHelper.ShowConfirmationDialog("Do you want to reset all currency?"))
                {
                    this.ResetCurrency();
                }
            });
        }

        private void ResetCurrency()
        {
            foreach (var kvp in ChannelSession.Settings.UserData)
            {
                kvp.Value.ResetCurrency(this.currency);
            }
            this.currency.LastReset = new DateTimeOffset(DateTimeOffset.Now.Date);
        }

        private void DeleteRankButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            UserRankViewModel rank = (UserRankViewModel)button.DataContext;
            this.currency.Ranks.Remove(rank);
            this.ranks.Remove(rank);
        }

        private async void AddRankButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.RankNameTextBox.Text))
            {
                await MessageBoxHelper.ShowMessageDialog("A rank name must be specified");
                return;
            }

            int rankAmount = 0;
            if (string.IsNullOrEmpty(this.RankAmountTextBox.Text) || !int.TryParse(this.RankAmountTextBox.Text, out rankAmount) || rankAmount < 0)
            {
                await MessageBoxHelper.ShowMessageDialog("A rank amount must be specified");
                return;
            }

            if (this.ranks.Any(r => r.Name.Equals(this.RankNameTextBox.Text) || r.MinimumPoints == rankAmount))
            {
                await MessageBoxHelper.ShowMessageDialog("Every rank must have a unique name and minimum amount");
                return;
            }

            UserRankViewModel newRank = new UserRankViewModel(this.RankNameTextBox.Text, rankAmount);
            this.currency.Ranks.Add(newRank);

            this.ranks.Clear();
            foreach (UserRankViewModel rank in this.currency.Ranks.OrderBy(r => r.MinimumPoints))
            {
                this.ranks.Add(rank);
            }

            this.currency.Ranks = this.ranks.ToList();

            this.RankNameTextBox.Clear();
            this.RankAmountTextBox.Clear();
        }

        private void NewCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommandWindow window = new CommandWindow(new CustomCommandDetailsControl(new CustomCommand(CurrencyDataControl.RankChangedCommandName)));
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
                    this.currency = null;
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
            this.currency.RankChangedCommand = (CustomCommand)e;
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            this.UpdateRankChangedCommand();
        }

        private void UpdateRankChangedCommand()
        {
            if (this.currency.RankChangedCommand != null)
            {
                this.NewCommandButton.Visibility = Visibility.Collapsed;
                this.CommandButtons.Visibility = Visibility.Visible;
                this.CommandButtons.DataContext = this.currency.RankChangedCommand;
            }
            else
            {
                this.NewCommandButton.Visibility = Visibility.Visible;
                this.CommandButtons.Visibility = Visibility.Collapsed;
            }
        }
    }
}
