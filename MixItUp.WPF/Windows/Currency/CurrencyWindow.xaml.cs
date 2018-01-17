using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Controls.Currency;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Command;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Windows.Currency
{
    /// <summary>
    /// Interaction logic for CurrencyWindow.xaml
    /// </summary>
    public partial class CurrencyWindow : LoadingWindowBase
    {
        private const string RankChangedCommandName = "User Rank Changed";

        private bool isRank = false;
        private UserCurrencyViewModel currency;

        private string specialIdentifier = null;

        private ObservableCollection<UserRankViewModel> ranks = new ObservableCollection<UserRankViewModel>();

        public CurrencyWindow(bool isRank)
            : this(new UserCurrencyViewModel())
        {
            this.isRank = isRank;
        }

        public CurrencyWindow(UserCurrencyViewModel currency)
        {
            this.currency = currency;
            this.isRank = this.currency.IsRank;

            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            if (isRank)
            {
                this.Title += "Rank";
                this.RankSpecialIdentifierGrid.Visibility = Visibility.Visible;
                this.RankGrid.Visibility = Visibility.Visible;

                this.RanksListView.ItemsSource = this.ranks;
                foreach (UserRankViewModel rank in this.currency.Ranks.OrderBy(r => r.MinimumPoints))
                {
                    this.ranks.Add(rank);
                }
                this.UpdateRankChangedCommand();
            }
            else
            {
                this.Title += "Currency";
            }

            this.ResetCurrencyComboBox.ItemsSource = EnumHelper.GetEnumNames<CurrencyResetRateEnum>();

            if (this.currency != null)
            {
                this.CurrencyNameTextBox.Text = this.currency.Name;
                this.CurrencyAmountTextBox.Text = this.currency.AcquireAmount.ToString();
                this.CurrencyTimeTextBox.Text = this.currency.AcquireInterval.ToString();
                this.CurrencySubscriberBonusTextBox.Text = this.currency.SubscriberBonus.ToString();

                this.CurrencyOnFollowBonusTextBox.Text = this.currency.OnFollowBonus.ToString();
                this.CurrencyOnHostBonusTextBox.Text = this.currency.OnHostBonus.ToString();
                this.CurrencyOnSubscribeBonusTextBox.Text = this.currency.OnSubscribeBonus.ToString();

                this.ResetCurrencyComboBox.SelectedItem = EnumHelper.GetEnumName(this.currency.ResetInterval);
            }

            await base.OnLoaded();
        }

        private void CurrencyNameTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.CurrencyNameTextBox.Text))
            {
                this.specialIdentifier = SpecialIdentifierStringBuilder.ConvertToSpecialIdentifier(this.CurrencyNameTextBox.Text);
                this.UserAmountSpecialIdentifierTextBlock.Text = string.Format("$user{0}", this.specialIdentifier);
                this.UserRankSpecialIdentifierTextBlock.Text = string.Format("$user{0}rank", this.specialIdentifier);
            }
            else
            {
                this.specialIdentifier = null;
                this.UserAmountSpecialIdentifierTextBlock.Text = "";
                this.UserRankSpecialIdentifierTextBlock.Text = "";
            }
        }

        private async void ResetCurrencyManuallyButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.currency != null)
            {
                await this.RunAsyncOperation(async () =>
                {
                    if (await MessageBoxHelper.ShowConfirmationDialog("Do you want to reset all currency?"))
                    {
                        this.currency.Reset();
                    }
                });
            }
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

        private async void SaveButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                if (string.IsNullOrEmpty(this.CurrencyNameTextBox.Text))
                {
                    await MessageBoxHelper.ShowMessageDialog("A currency name must be specified");
                    return;
                }

                int currencyAmount = 0;
                if (string.IsNullOrEmpty(this.CurrencyAmountTextBox.Text) || !int.TryParse(this.CurrencyAmountTextBox.Text, out currencyAmount) || currencyAmount < 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("The currency rate must be 0 or greater");
                    return;
                }

                int currencyTime = 0;
                if (string.IsNullOrEmpty(this.CurrencyTimeTextBox.Text) || !int.TryParse(this.CurrencyTimeTextBox.Text, out currencyTime) || currencyTime < 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("The currency interval must be 0 or greater");
                    return;
                }

                if ((currencyAmount == 0 && currencyTime != 0) || (currencyAmount != 0 && currencyTime == 0))
                {
                    await MessageBoxHelper.ShowMessageDialog("The currency rate and interval must be both greater than 0 or both equal to 0");
                    return;
                }

                int subscriberBonus = 0;
                if (string.IsNullOrEmpty(this.CurrencySubscriberBonusTextBox.Text) || !int.TryParse(this.CurrencySubscriberBonusTextBox.Text, out subscriberBonus) || subscriberBonus < 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("The Subscriber bonus must be 0 or greater");
                    return;
                }

                int onFollowBonus = 0;
                if (string.IsNullOrEmpty(this.CurrencyOnFollowBonusTextBox.Text) || !int.TryParse(this.CurrencyOnFollowBonusTextBox.Text, out onFollowBonus) || onFollowBonus < 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("The On Follow bonus must be 0 or greater");
                    return;
                }

                int onHostBonus = 0;
                if (string.IsNullOrEmpty(this.CurrencyOnHostBonusTextBox.Text) || !int.TryParse(this.CurrencyOnHostBonusTextBox.Text, out onHostBonus) || onHostBonus < 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("The On Host bonus must be 0 or greater");
                    return;
                }

                int onSubscribeBonus = 0;
                if (string.IsNullOrEmpty(this.CurrencyOnSubscribeBonusTextBox.Text) || !int.TryParse(this.CurrencyOnSubscribeBonusTextBox.Text, out onSubscribeBonus) || onSubscribeBonus < 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("The On Subscribe bonus must be 0 or greater");
                    return;
                }

                if (this.ResetCurrencyComboBox.SelectedIndex < 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("A reset frequency must be selected");
                    return;
                }

                if (string.IsNullOrEmpty(this.specialIdentifier))
                {
                    await MessageBoxHelper.ShowMessageDialog("A currency special identifier must exist. Please ensure your currency name contains letters or numbers.");
                    return;
                }

                if (this.isRank)
                {
                    if (this.ranks.Count() < 1)
                    {
                        await MessageBoxHelper.ShowMessageDialog("At least one rank must be created");
                        return;
                    }
                }

                this.currency.Name = this.CurrencyNameTextBox.Text;
                this.currency.AcquireAmount = currencyAmount;
                this.currency.AcquireInterval = currencyTime;
                this.currency.SubscriberBonus = subscriberBonus;

                this.currency.OnFollowBonus = onFollowBonus;
                this.currency.OnHostBonus = onHostBonus;
                this.currency.OnSubscribeBonus = onSubscribeBonus;

                this.currency.ResetInterval = EnumHelper.GetEnumValueFromString<CurrencyResetRateEnum>((string)this.ResetCurrencyComboBox.SelectedItem);

                this.currency.SpecialIdentifier = this.specialIdentifier;

                if (this.isRank)
                {
                    this.currency.Ranks = ranks.ToList();
                }

                if (!ChannelSession.Settings.Currencies.ContainsKey(this.currency.ID))
                {
                    ChannelSession.Settings.Currencies[this.currency.ID] = this.currency;
                }

                await ChannelSession.SaveSettings();

                this.Close();
            });
        }

        private void NewCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommandWindow window = new CommandWindow(new CustomCommandDetailsControl(new CustomCommand(CurrencyWindow.RankChangedCommandName)));
            window.CommandSaveSuccessfully += Window_CommandSaveSuccessfully;
            window.Closed += Window_Closed;
            window.Show();
        }

        private void CommandButtons_EditClicked(object sender, RoutedEventArgs e)
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

        private async void CommandButtons_DeleteClicked(object sender, RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
                CustomCommand command = commandButtonsControl.GetCommandFromCommandButtons<CustomCommand>(sender);
                if (command != null)
                {
                    this.currency = null;
                    await ChannelSession.SaveSettings();
                    this.UpdateRankChangedCommand();
                }
            });
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
