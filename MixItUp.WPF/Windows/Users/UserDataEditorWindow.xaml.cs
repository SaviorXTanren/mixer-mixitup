using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Controls.Command;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MixItUp.WPF.Windows.Command;
using MixItUp.Base;
using MixItUp.WPF.Controls.Currency;

namespace MixItUp.WPF.Windows.Users
{
    /// <summary>
    /// Interaction logic for UserDataEditorWindow.xaml
    /// </summary>
    public partial class UserDataEditorWindow : LoadingWindowBase
    {
        private const string UserEntranceCommandName = "Entrance Command";

        private UserViewModel user;

        private ObservableCollection<UserCurrencyIndividualEditorControl> currencies = new ObservableCollection<UserCurrencyIndividualEditorControl>();
        private ObservableCollection<UserCurrencyIndividualEditorControl> ranks = new ObservableCollection<UserCurrencyIndividualEditorControl>();

        private ObservableCollection<ChatCommand> userOnlyCommands = new ObservableCollection<ChatCommand>();

        public UserDataEditorWindow(UserDataViewModel userData)
        {
            this.user = new UserViewModel(userData);

            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            this.CurrencyDataGrid.ItemsSource = this.currencies;
            this.RankDataGrid.ItemsSource = this.ranks;

            this.UserOnlyChatCommandsListView.ItemsSource = this.userOnlyCommands;

            this.CurrencyRankExemptToggleButton.IsChecked = this.user.Data.IsCurrencyRankExempt;
            this.SparkExemptToggleButton.IsChecked = this.user.Data.IsSparkExempt;

            await this.RefreshData();
        }

        private async Task RefreshData()
        {
            await this.user.RefreshDetails();

            this.ranks.Clear();
            this.currencies.Clear();
            foreach (UserCurrencyViewModel currency in ChannelSession.Settings.Currencies.Values.ToList())
            {
                UserCurrencyDataViewModel currencyData = this.user.Data.GetCurrency(currency);
                if (currencyData.Currency.IsRank)
                {
                    this.ranks.Add(new UserCurrencyIndividualEditorControl(currencyData));
                }
                else
                {
                    this.currencies.Add(new UserCurrencyIndividualEditorControl(currencyData));
                }
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

            this.DataContext = null;
            this.DataContext = this.user;
        }

        private void CurrencyAmountTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            UserCurrencyDataViewModel currencyData = (UserCurrencyDataViewModel)textBox.DataContext;
            if (!string.IsNullOrEmpty(textBox.Text) && int.TryParse(textBox.Text, out int amount) && amount >= 0)
            {
                this.user.Data.SetCurrencyAmount(currencyData.Currency, amount);
            }
        }

        private void RankAmountTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            UserCurrencyDataViewModel currencyData = (UserCurrencyDataViewModel)textBox.DataContext;
            if (!string.IsNullOrEmpty(textBox.Text) && int.TryParse(textBox.Text, out int amount) && amount >= 0)
            {
                this.user.Data.SetCurrencyAmount(currencyData.Currency, amount);
            }
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
        }

        private void SparkExemptToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            this.user.Data.IsSparkExempt = this.SparkExemptToggleButton.IsChecked.GetValueOrDefault();
        }
    }
}
