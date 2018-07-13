using Mixer.Base.Model.User;
using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Controls.Dialogs;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Windows.Currency
{
    public enum CurrencyAcquireRateTypeEnum
    {
        [Name("1 Per Minute")]
        Minutes,
        [Name("1 Per Hour")]
        Hours,
        Custom,
        Disabled,
    }

    /// <summary>
    /// Interaction logic for CurrencyWindow.xaml
    /// </summary>
    public partial class CurrencyWindow : LoadingWindowBase
    {
        private const string RankChangedCommandName = "User Rank Changed";

        private UserCurrencyViewModel currency;
        private CustomCommand rankChangedCommand;

        private Dictionary<UserDataViewModel, int> userImportData = new Dictionary<UserDataViewModel, int>();

        private ObservableCollection<UserRankViewModel> ranks = new ObservableCollection<UserRankViewModel>();

        public CurrencyWindow()
        {
            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        public CurrencyWindow(UserCurrencyViewModel currency)
        {
            this.currency = currency;
            this.rankChangedCommand = this.currency.RankChangedCommand;

            InitializeComponent();

            this.ImportFromFileButton.IsEnabled = true;

            this.Initialize(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            this.RanksListView.ItemsSource = this.ranks;

            this.OnlineRateComboBox.ItemsSource = EnumHelper.GetEnumNames<CurrencyAcquireRateTypeEnum>();
            this.OfflineRateComboBox.ItemsSource = EnumHelper.GetEnumNames<CurrencyAcquireRateTypeEnum>();

            this.AutomaticResetComboBox.ItemsSource = EnumHelper.GetEnumNames<CurrencyResetRateEnum>();

            if (this.currency != null)
            {
                this.NameTextBox.Text = this.currency.Name;

                if (this.currency.MaxAmount != int.MaxValue)
                {
                    this.MaxAmountTextBox.Text = this.currency.MaxAmount.ToString();
                }

                if (this.currency.IsOnlineIntervalMinutes)
                {
                    this.OnlineRateComboBox.SelectedItem = EnumHelper.GetEnumName(CurrencyAcquireRateTypeEnum.Minutes);
                }
                else if (this.currency.IsOnlineIntervalHours)
                {
                    this.OnlineRateComboBox.SelectedItem = EnumHelper.GetEnumName(CurrencyAcquireRateTypeEnum.Hours);
                }
                else if (this.currency.IsOnlineIntervalDisabled)
                {
                    this.OnlineRateComboBox.SelectedItem = EnumHelper.GetEnumName(CurrencyAcquireRateTypeEnum.Disabled);
                }
                else
                {
                    this.OnlineRateComboBox.SelectedItem = EnumHelper.GetEnumName(CurrencyAcquireRateTypeEnum.Custom);
                }
                this.OnlineAmountRateTextBox.Text = this.currency.AcquireAmount.ToString();
                this.OnlineTimeRateTextBox.Text = this.currency.AcquireInterval.ToString();

                if (this.currency.IsOfflineIntervalMinutes)
                {
                    this.OfflineRateComboBox.SelectedItem = EnumHelper.GetEnumName(CurrencyAcquireRateTypeEnum.Minutes);
                }
                else if (this.currency.IsOfflineIntervalHours)
                {
                    this.OfflineRateComboBox.SelectedItem = EnumHelper.GetEnumName(CurrencyAcquireRateTypeEnum.Hours);
                }
                else if (this.currency.IsOfflineIntervalDisabled)
                {
                    this.OfflineRateComboBox.SelectedItem = EnumHelper.GetEnumName(CurrencyAcquireRateTypeEnum.Disabled);
                }
                else
                {
                    this.OfflineRateComboBox.SelectedItem = EnumHelper.GetEnumName(CurrencyAcquireRateTypeEnum.Custom);
                }
                this.OfflineAmountRateTextBox.Text = this.currency.OfflineAcquireAmount.ToString();
                this.OfflineTimeRateTextBox.Text = this.currency.OfflineAcquireInterval.ToString();

                this.SubscriberBonusTextBox.Text = this.currency.SubscriberBonus.ToString();

                this.OnFollowBonusTextBox.Text = this.currency.OnFollowBonus.ToString();
                this.OnHostBonusTextBox.Text = this.currency.OnHostBonus.ToString();
                this.OnSubscribeBonusTextBox.Text = this.currency.OnSubscribeBonus.ToString();

                this.AutomaticResetComboBox.SelectedItem = EnumHelper.GetEnumName(this.currency.ResetInterval);

                if (this.currency.IsRank)
                {
                    this.IsRankToggleButton.IsChecked = true;
                    foreach (UserRankViewModel rank in this.currency.Ranks.OrderBy(r => r.MinimumPoints))
                    {
                        this.ranks.Add(rank);
                    }
                    this.UpdateRankChangedCommand();
                }
            }
            else
            {
                this.OnlineRateComboBox.SelectedItem = EnumHelper.GetEnumName(CurrencyAcquireRateTypeEnum.Minutes);
                this.OfflineRateComboBox.SelectedItem = EnumHelper.GetEnumName(CurrencyAcquireRateTypeEnum.Disabled);

                this.SubscriberBonusTextBox.Text = "0";
                this.OnFollowBonusTextBox.Text = "0";
                this.OnHostBonusTextBox.Text = "0";
                this.OnSubscribeBonusTextBox.Text = "0";

                this.AutomaticResetComboBox.SelectedItem = EnumHelper.GetEnumName(CurrencyResetRateEnum.Never);
            }

            await base.OnLoaded();
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/SaviorXTanren/mixer-mixitup/wiki/Currency-&-Rank");
        }

        private void IsRankToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            this.RankListGrid.Visibility = (this.IsRankToggleButton.IsChecked.GetValueOrDefault()) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OnlineRateComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.OnlineRateComboBox.SelectedIndex >= 0)
            {
                CurrencyAcquireRateTypeEnum acquireRate = EnumHelper.GetEnumValueFromString<CurrencyAcquireRateTypeEnum>((string)this.OnlineRateComboBox.SelectedItem);
                this.OnlineAmountRateTextBox.IsEnabled = (acquireRate == CurrencyAcquireRateTypeEnum.Custom);
                this.OnlineTimeRateTextBox.IsEnabled = (acquireRate == CurrencyAcquireRateTypeEnum.Custom);

                if (acquireRate == CurrencyAcquireRateTypeEnum.Minutes || acquireRate == CurrencyAcquireRateTypeEnum.Hours)
                {
                    this.OnlineAmountRateTextBox.Text = "1";
                    if (acquireRate == CurrencyAcquireRateTypeEnum.Minutes)
                    {
                        this.OnlineTimeRateTextBox.Text = "1";
                    }
                    else if (acquireRate == CurrencyAcquireRateTypeEnum.Hours)
                    {
                        this.OnlineTimeRateTextBox.Text = "60";
                    }
                }
                else
                {
                    this.OnlineAmountRateTextBox.Text = "0";
                    this.OnlineTimeRateTextBox.Text = "0";
                }
            }
        }

        private void OfflineRateComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.OfflineRateComboBox.SelectedIndex >= 0)
            {
                CurrencyAcquireRateTypeEnum acquireRate = EnumHelper.GetEnumValueFromString<CurrencyAcquireRateTypeEnum>((string)this.OfflineRateComboBox.SelectedItem);
                this.OfflineAmountRateTextBox.IsEnabled = (acquireRate == CurrencyAcquireRateTypeEnum.Custom);
                this.OfflineTimeRateTextBox.IsEnabled = (acquireRate == CurrencyAcquireRateTypeEnum.Custom);

                if (acquireRate == CurrencyAcquireRateTypeEnum.Minutes || acquireRate == CurrencyAcquireRateTypeEnum.Hours)
                {
                    this.OfflineAmountRateTextBox.Text = "1";
                    if (acquireRate == CurrencyAcquireRateTypeEnum.Minutes)
                    {
                        this.OfflineTimeRateTextBox.Text = "1";
                    }
                    else if (acquireRate == CurrencyAcquireRateTypeEnum.Hours)
                    {
                        this.OfflineTimeRateTextBox.Text = "60";
                    }
                }
                else
                {
                    this.OfflineAmountRateTextBox.Text = "0";
                    this.OfflineTimeRateTextBox.Text = "0";
                }
            }
        }

        private void DeleteRankButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            UserRankViewModel rank = (UserRankViewModel)button.DataContext;
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
                await MessageBoxHelper.ShowMessageDialog("A minimum amount must be specified");
                return;
            }

            if (this.ranks.Any(r => r.Name.Equals(this.RankNameTextBox.Text) || r.MinimumPoints == rankAmount))
            {
                await MessageBoxHelper.ShowMessageDialog("Every rank must have a unique name and minimum amount");
                return;
            }

            UserRankViewModel newRank = new UserRankViewModel(this.RankNameTextBox.Text, rankAmount);
            this.ranks.Add(newRank);

            var tempRanks = this.ranks.ToList();

            this.ranks.Clear();
            foreach (UserRankViewModel rank in tempRanks.OrderBy(r => r.MinimumPoints))
            {
                this.ranks.Add(rank);
            }

            this.RankNameTextBox.Clear();
            this.RankAmountTextBox.Clear();
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
                    this.rankChangedCommand = null;
                    await ChannelSession.SaveSettings();
                    this.UpdateRankChangedCommand();
                }
            });
        }

        private async void ManualResetButton_Click(object sender, RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                if (await MessageBoxHelper.ShowConfirmationDialog("Do you want to reset all currency?"))
                {
                    if (this.currency != null)
                    {
                        this.currency.Reset();
                    }
                }
            });
        }

        private async void ImportFromFileButton_Click(object sender, RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                this.userImportData.Clear();

                if (await MessageBoxHelper.ShowConfirmationDialog("This will allow you to import the total amounts that each user had, assign them to this currency/rank, and will overwrite any amounts that each user has." +
                    Environment.NewLine + Environment.NewLine + "This process may take some time; are you sure you wish to do this?"))
                {
                    try
                    {
                        string filePath = ChannelSession.Services.FileService.ShowOpenFileDialog();
                        if (!string.IsNullOrEmpty(filePath))
                        {
                            string fileContents = await ChannelSession.Services.FileService.ReadFile(filePath);
                            string[] lines = fileContents.Split(new string[] { Environment.NewLine, "\n" }, StringSplitOptions.RemoveEmptyEntries);
                            if (lines.Count() > 0)
                            {
                                foreach (string line in lines)
                                {
                                    UserModel user = null;
                                    uint id = 0;
                                    string username = null;
                                    int amount = 0;

                                    string[] segments = line.Split(new string[] { " ", "\t", "," }, StringSplitOptions.RemoveEmptyEntries);
                                    if (segments.Count() == 2)
                                    {
                                        if (!int.TryParse(segments[1], out amount))
                                        {
                                            throw new InvalidOperationException("File is not in the correct format");
                                        }

                                        if (!uint.TryParse(segments[0], out id))
                                        {
                                            username = segments[0];
                                        }
                                    }
                                    else if (segments.Count() == 3)
                                    {
                                        if (!uint.TryParse(segments[0], out id))
                                        {
                                            throw new InvalidOperationException("File is not in the correct format");
                                        }

                                        if (!int.TryParse(segments[2], out amount))
                                        {
                                            throw new InvalidOperationException("File is not in the correct format");
                                        }
                                    }
                                    else
                                    {
                                        throw new InvalidOperationException("File is not in the correct format");
                                    }

                                    if (amount > 0)
                                    {
                                        if (id > 0)
                                        {
                                            user = await ChannelSession.Connection.GetUser(id);
                                        }
                                        else if (!string.IsNullOrEmpty(username))
                                        {
                                            user = await ChannelSession.Connection.GetUser(username);
                                        }
                                    }

                                    if (user != null)
                                    {
                                        UserDataViewModel data = ChannelSession.Settings.UserData.GetValueIfExists(user.id, new UserDataViewModel(user));
                                        if (!this.userImportData.ContainsKey(data))
                                        {
                                            this.userImportData[data] = amount;
                                        }
                                        this.userImportData[data] = Math.Max(this.userImportData[data], amount);
                                        this.ImportFromFileButton.Content = string.Format("{0} Imported...", this.userImportData.Count());
                                    }
                                }

                                foreach (var kvp in this.userImportData)
                                {
                                    kvp.Key.SetCurrencyAmount(this.currency, kvp.Value);
                                }

                                this.ImportFromFileButton.Content = "Import From File";
                                return;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Base.Util.Logger.Log(ex);
                    }

                    await MessageBoxHelper.ShowMessageDialog("We were unable to import the data. Please ensure your file is in one of the following formats:" +
                        Environment.NewLine + Environment.NewLine + "<USERNAME> <AMOUNT>" +
                        Environment.NewLine + Environment.NewLine + "<USER ID> <AMOUNT>" +
                        Environment.NewLine + Environment.NewLine + "<USER ID> <USERNAME> <AMOUNT>");

                    this.ImportFromFileButton.Content = "Import From File";
                }
            });
        }

        private async void ExportToFileButton_Click(object sender, RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                string filePath = ChannelSession.Services.FileService.ShowSaveFileDialog(this.currency.Name + " Data.txt");
                if (!string.IsNullOrEmpty(filePath))
                {
                    StringBuilder fileContents = new StringBuilder();
                    foreach (UserDataViewModel userData in ChannelSession.Settings.UserData.Values.ToList())
                    {
                        fileContents.AppendLine(string.Format("{0} {1} {2}", userData.ID, userData.UserName, userData.GetCurrencyAmount(this.currency)));
                    }

                    await ChannelSession.Services.FileService.SaveFile(filePath, fileContents.ToString());
                }
            });
        }

        private async void SaveButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                if (string.IsNullOrEmpty(this.NameTextBox.Text))
                {
                    await MessageBoxHelper.ShowMessageDialog("A currency name must be specified");
                    return;
                }

                UserCurrencyViewModel dupeCurrency = ChannelSession.Settings.Currencies.Values.FirstOrDefault(c => c.Name.Equals(this.NameTextBox.Text));
                if (dupeCurrency != null && (this.currency == null || !this.currency.ID.Equals(dupeCurrency.ID)))
                {
                    await MessageBoxHelper.ShowMessageDialog("There already exists a currency or rank system with this name");
                    return;
                }

                int maxAmount = int.MaxValue;
                if (!string.IsNullOrEmpty(this.MaxAmountTextBox.Text))
                {
                    if (!int.TryParse(this.MaxAmountTextBox.Text, out maxAmount) || maxAmount <= 0)
                    {
                        await MessageBoxHelper.ShowMessageDialog("The max amount must be greater than 0 or can be left empty for no max amount");
                        return;
                    }
                }

                if (string.IsNullOrEmpty(this.OnlineAmountRateTextBox.Text) || !int.TryParse(this.OnlineAmountRateTextBox.Text, out int onlineAmount) || onlineAmount < 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("The online amount must be 0 or greater");
                    return;
                }

                if (string.IsNullOrEmpty(this.OnlineTimeRateTextBox.Text) || !int.TryParse(this.OnlineTimeRateTextBox.Text, out int onlineTime) || onlineTime < 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("The online minutes must be 0 or greater");
                    return;
                }

                if (string.IsNullOrEmpty(this.OfflineAmountRateTextBox.Text) || !int.TryParse(this.OfflineAmountRateTextBox.Text, out int offlineAmount) || offlineAmount < 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("The offline amount must be 0 or greater");
                    return;
                }

                if (string.IsNullOrEmpty(this.OfflineTimeRateTextBox.Text) || !int.TryParse(this.OfflineTimeRateTextBox.Text, out int offlineTime) || offlineTime < 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("The offline minutes must be 0 or greater");
                    return;
                }

                if (onlineAmount > 0 && onlineTime == 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("The online time can not be 0 if the online amount is greater than 0");
                    return;
                }

                if (offlineAmount > 0 && offlineTime == 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("The offline time can not be 0 if the offline amount is greater than 0");
                    return;
                }

                int subscriberBonus = 0;
                if (string.IsNullOrEmpty(this.SubscriberBonusTextBox.Text) || !int.TryParse(this.SubscriberBonusTextBox.Text, out subscriberBonus) || subscriberBonus < 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("The Subscriber bonus must be 0 or greater");
                    return;
                }

                int onFollowBonus = 0;
                if (string.IsNullOrEmpty(this.OnFollowBonusTextBox.Text) || !int.TryParse(this.OnFollowBonusTextBox.Text, out onFollowBonus) || onFollowBonus < 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("The On Follow bonus must be 0 or greater");
                    return;
                }

                int onHostBonus = 0;
                if (string.IsNullOrEmpty(this.OnHostBonusTextBox.Text) || !int.TryParse(this.OnHostBonusTextBox.Text, out onHostBonus) || onHostBonus < 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("The On Host bonus must be 0 or greater");
                    return;
                }

                int onSubscribeBonus = 0;
                if (string.IsNullOrEmpty(this.OnSubscribeBonusTextBox.Text) || !int.TryParse(this.OnSubscribeBonusTextBox.Text, out onSubscribeBonus) || onSubscribeBonus < 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("The On Subscribe bonus must be 0 or greater");
                    return;
                }

                if (this.IsRankToggleButton.IsChecked.GetValueOrDefault())
                {
                    if (this.ranks.Count() < 1)
                    {
                        await MessageBoxHelper.ShowMessageDialog("At least one rank must be created");
                        return;
                    }
                }

                bool isNew = false;
                if (this.currency == null)
                {
                    isNew = true;
                    this.currency = new UserCurrencyViewModel();
                    ChannelSession.Settings.Currencies[this.currency.ID] = this.currency;
                }

                this.currency.Name = this.NameTextBox.Text;
                this.currency.MaxAmount = maxAmount;

                this.currency.AcquireAmount = onlineAmount;
                this.currency.AcquireInterval = onlineTime;
                this.currency.OfflineAcquireAmount = offlineAmount;
                this.currency.OfflineAcquireInterval = offlineTime;

                this.currency.SubscriberBonus = subscriberBonus;
                this.currency.OnFollowBonus = onFollowBonus;
                this.currency.OnHostBonus = onHostBonus;
                this.currency.OnSubscribeBonus = onSubscribeBonus;

                this.currency.ResetInterval = EnumHelper.GetEnumValueFromString<CurrencyResetRateEnum>((string)this.AutomaticResetComboBox.SelectedItem);

                this.currency.SpecialIdentifier = SpecialIdentifierStringBuilder.ConvertToSpecialIdentifier(this.currency.Name);

                if (this.IsRankToggleButton.IsChecked.GetValueOrDefault())
                {
                    this.currency.Ranks = ranks.ToList();
                    this.currency.RankChangedCommand = this.rankChangedCommand;
                }
                else
                {
                    this.currency.Ranks = new List<UserRankViewModel>();
                    this.currency.RankChangedCommand = null;
                }

                await ChannelSession.SaveSettings();

                if (isNew)
                {
                    List<NewCurrencyRankCommand> commandsToAdd = new List<NewCurrencyRankCommand>();

                    ChatCommand statusCommand = new ChatCommand("User " + this.currency.Name, this.currency.SpecialIdentifier, new RequirementViewModel(MixerRoleEnum.User, 5));
                    string statusChatText = string.Empty;
                    if (this.currency.IsRank)
                    {
                        statusChatText = string.Format("@$username is a ${0} with ${1} {2}!", this.currency.UserRankNameSpecialIdentifier, this.currency.UserAmountSpecialIdentifier, this.currency.Name);
                    }
                    else
                    {
                        statusChatText = string.Format("@$username has ${0} {1}!", this.currency.UserAmountSpecialIdentifier, this.currency.Name);
                    }
                    statusCommand.Actions.Add(new ChatAction(statusChatText));
                    commandsToAdd.Add(new NewCurrencyRankCommand(string.Format("!{0} - {1}", statusCommand.Commands.First(), "Shows User's Amount"), statusCommand));

                    ChatCommand addCommand = new ChatCommand("Add " + this.currency.Name, "add" + this.currency.SpecialIdentifier, new RequirementViewModel(MixerRoleEnum.User, 5));
                    addCommand.Actions.Add(new CurrencyAction(this.currency, CurrencyActionTypeEnum.GiveToSpecificUser, "$arg2text", "$targetusername"));
                    addCommand.Actions.Add(new ChatAction(string.Format("@$targetusername received $arg2text {0}!", this.currency.Name)));
                    commandsToAdd.Add(new NewCurrencyRankCommand(string.Format("!{0} - {1}", addCommand.Commands.First(), "Adds Amount To Specified User"), addCommand));

                    ChatCommand addAllCommand = new ChatCommand("Add All " + this.currency.Name, "addall" + this.currency.SpecialIdentifier, new RequirementViewModel(MixerRoleEnum.User, 5));
                    addAllCommand.Actions.Add(new CurrencyAction(this.currency, CurrencyActionTypeEnum.GiveToAllChatUsers, "$arg1text"));
                    addAllCommand.Actions.Add(new ChatAction(string.Format("Everyone got $arg1text {0}!", this.currency.Name)));
                    commandsToAdd.Add(new NewCurrencyRankCommand(string.Format("!{0} - {1}", addAllCommand.Commands.First(), "Adds Amount To All Chat Users"), addAllCommand));

                    if (!this.currency.IsRank)
                    {
                        ChatCommand giveCommand = new ChatCommand("Give " + this.currency.Name, "give" + this.currency.SpecialIdentifier, new RequirementViewModel(MixerRoleEnum.User, 5));
                        giveCommand.Actions.Add(new CurrencyAction(this.currency, CurrencyActionTypeEnum.GiveToSpecificUser, "$arg2text", "$targetusername", deductFromUser: true));
                        giveCommand.Actions.Add(new ChatAction(string.Format("@$username gave @$targetusername $arg2text {0}!", this.currency.Name)));
                        commandsToAdd.Add(new NewCurrencyRankCommand(string.Format("!{0} - {1}", giveCommand.Commands.First(), "Gives Amount To Specified User"), giveCommand));
                    }

                    NewCurrencyRankCommandsDialogControl dControl = new NewCurrencyRankCommandsDialogControl(this.currency, commandsToAdd);
                    string result = await MessageBoxHelper.ShowCustomDialog(dControl);
                    if (!string.IsNullOrEmpty(result) && result.Equals("True"))
                    {
                        foreach (NewCurrencyRankCommand command in dControl.commands)
                        {
                            if (command.AddCommand)
                            {
                                ChannelSession.Settings.ChatCommands.Add(command.Command);
                            }
                        }
                    }
                }

                this.Close();
            });
        }

        private void Window_CommandSaveSuccessfully(object sender, CommandBase e)
        {
            this.rankChangedCommand = (CustomCommand)e;
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            this.UpdateRankChangedCommand();
        }

        private void UpdateRankChangedCommand()
        {
            if (this.rankChangedCommand != null)
            {
                this.NewCommandButton.Visibility = Visibility.Collapsed;
                this.CommandButtons.Visibility = Visibility.Visible;
                this.CommandButtons.DataContext = this.rankChangedCommand;
            }
            else
            {
                this.NewCommandButton.Visibility = Visibility.Visible;
                this.CommandButtons.Visibility = Visibility.Collapsed;
            }
        }
    }
}
