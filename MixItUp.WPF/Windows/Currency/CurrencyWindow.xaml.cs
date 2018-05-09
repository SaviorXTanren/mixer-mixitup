using Mixer.Base.Model.User;
using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Windows.Currency
{
    public enum CurrencyAcquireRateTypeEnum
    {
        Minutes,
        Hours,
        Custom,
    }

    /// <summary>
    /// Interaction logic for CurrencyWindow.xaml
    /// </summary>
    public partial class CurrencyWindow : LoadingWindowBase
    {
        private const string RankChangedCommandName = "User Rank Changed";

        private bool isRank = false;
        private UserCurrencyViewModel currency;
        private CustomCommand rankChangedCommand;

        private string specialIdentifier = null;

        private Dictionary<UserDataViewModel, int> userImportData = new Dictionary<UserDataViewModel, int>();

        private ObservableCollection<UserRankViewModel> ranks = new ObservableCollection<UserRankViewModel>();

        public CurrencyWindow(bool isRank)
        {
            this.isRank = isRank;

            InitializeComponent();

            this.Initialize(this.StatusBar);

            this.ExportUserCurrencyToFileButton.IsEnabled = false;
        }

        public CurrencyWindow(UserCurrencyViewModel currency)
        {
            this.currency = currency;
            this.isRank = this.currency.IsRank;
            this.rankChangedCommand = this.currency.RankChangedCommand;

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
            }
            else
            {
                this.Title += "Currency";
            }

            this.CurrencyAcquireRateComboBox.ItemsSource = EnumHelper.GetEnumNames<CurrencyAcquireRateTypeEnum>();

            this.ResetCurrencyComboBox.ItemsSource = EnumHelper.GetEnumNames<CurrencyResetRateEnum>();

            if (this.currency != null)
            {
                this.CurrencyNameTextBox.Text = this.currency.Name;

                if (this.currency.IsMinutesInterval)
                {
                    this.CurrencyAcquireRateComboBox.SelectedItem = EnumHelper.GetEnumName(CurrencyAcquireRateTypeEnum.Minutes);
                }
                else if (this.currency.IsHoursInterval)
                {
                    this.CurrencyAcquireRateComboBox.SelectedItem = EnumHelper.GetEnumName(CurrencyAcquireRateTypeEnum.Hours);
                }
                else
                {
                    this.CurrencyAcquireRateComboBox.SelectedItem = EnumHelper.GetEnumName(CurrencyAcquireRateTypeEnum.Custom);
                }
                this.CurrencyAmountTextBox.Text = this.currency.AcquireAmount.ToString();
                this.CurrencyTimeTextBox.Text = this.currency.AcquireInterval.ToString();

                if (this.currency.MaxAmount != int.MaxValue)
                {
                    this.CurrencyMaxAmountTextBox.Text = this.currency.MaxAmount.ToString();
                }

                this.CurrencySubscriberBonusTextBox.Text = this.currency.SubscriberBonus.ToString();
                this.CurrencyOnFollowBonusTextBox.Text = this.currency.OnFollowBonus.ToString();
                this.CurrencyOnHostBonusTextBox.Text = this.currency.OnHostBonus.ToString();
                this.CurrencyOnSubscribeBonusTextBox.Text = this.currency.OnSubscribeBonus.ToString();

                this.ResetCurrencyComboBox.SelectedItem = EnumHelper.GetEnumName(this.currency.ResetInterval);

                if (this.currency.IsRank)
                {
                    foreach (UserRankViewModel rank in this.currency.Ranks.OrderBy(r => r.MinimumPoints))
                    {
                        this.ranks.Add(rank);
                    }
                    this.UpdateRankChangedCommand();
                }
            }
            else
            {
                this.CurrencySubscriberBonusTextBox.Text = "0";
                this.CurrencyOnFollowBonusTextBox.Text = "0";
                this.CurrencyOnHostBonusTextBox.Text = "0";
                this.CurrencyOnSubscribeBonusTextBox.Text = "0";

                this.ResetCurrencyComboBox.SelectedItem = EnumHelper.GetEnumName(CurrencyResetRateEnum.Never);
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

        private async void ImportUserCurrencyFromFileButton_Click(object sender, RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                this.userImportData.Clear();

                string message = "This will allow you to import the total amounts that each user had and assign them to this ";
                message += (this.isRank) ? "rank" : "currency";
                message += " and will overwrite any amounts that each user has.";
                message += Environment.NewLine + Environment.NewLine + "This process may take some time; are you sure you wish to do this?";

                if (await MessageBoxHelper.ShowConfirmationDialog(message))
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
                                        this.ImportUserCurrencyFromFileButton.Content = string.Format("{0} Imported...", this.userImportData.Count());
                                    }
                                }
                                this.ImportUserCurrencyFromFileButton.Content = "Import From File";
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

                    this.ImportUserCurrencyFromFileButton.Content = "Import From File";
                }
            });
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

                UserCurrencyViewModel dupeCurrency = ChannelSession.Settings.Currencies.Values.FirstOrDefault(c => c.Name.Equals(this.CurrencyNameTextBox.Text));
                if (dupeCurrency != null && (this.currency == null || !this.currency.ID.Equals(dupeCurrency.ID)))
                {
                    await MessageBoxHelper.ShowMessageDialog("There already exists a currency or rank system with this name");
                    return;
                }

                if (this.CurrencyAcquireRateComboBox.SelectedIndex < 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("The currency rate must be selected");
                    return;
                }
                CurrencyAcquireRateTypeEnum acquireRate = EnumHelper.GetEnumValueFromString<CurrencyAcquireRateTypeEnum>((string)this.CurrencyAcquireRateComboBox.SelectedItem);

                int currencyAmount = 1;
                int currencyTime = 1;
                if (acquireRate == CurrencyAcquireRateTypeEnum.Hours)
                {
                    currencyTime = 60;
                }
                else if (acquireRate == CurrencyAcquireRateTypeEnum.Custom)
                {
                    if (string.IsNullOrEmpty(this.CurrencyAmountTextBox.Text) || !int.TryParse(this.CurrencyAmountTextBox.Text, out currencyAmount) || currencyAmount < 0)
                    {
                        await MessageBoxHelper.ShowMessageDialog("The currency rate must be 0 or greater");
                        return;
                    }

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
                }

                int maxAmount = int.MaxValue;
                if (!string.IsNullOrEmpty(this.CurrencyMaxAmountTextBox.Text) && (!int.TryParse(this.CurrencyMaxAmountTextBox.Text, out maxAmount) || maxAmount <= 0))
                {
                    await MessageBoxHelper.ShowMessageDialog("The max amount must be greater than 0 or can be left empty for no max amount");
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

                bool newCurrencyRank = false;
                if (this.currency == null)
                {
                    newCurrencyRank = true;
                    this.currency = new UserCurrencyViewModel();
                }

                this.currency.Name = this.CurrencyNameTextBox.Text;
                this.currency.AcquireAmount = currencyAmount;
                this.currency.AcquireInterval = currencyTime;
                this.currency.MaxAmount = maxAmount;

                this.currency.SubscriberBonus = subscriberBonus;
                this.currency.OnFollowBonus = onFollowBonus;
                this.currency.OnHostBonus = onHostBonus;
                this.currency.OnSubscribeBonus = onSubscribeBonus;

                this.currency.ResetInterval = EnumHelper.GetEnumValueFromString<CurrencyResetRateEnum>((string)this.ResetCurrencyComboBox.SelectedItem);

                this.currency.SpecialIdentifier = this.specialIdentifier;

                if (this.isRank)
                {
                    this.currency.Ranks = ranks.ToList();
                    this.currency.RankChangedCommand = this.rankChangedCommand;
                }

                if (!ChannelSession.Settings.Currencies.ContainsKey(this.currency.ID))
                {
                    ChannelSession.Settings.Currencies[this.currency.ID] = this.currency;
                }

                foreach (var kvp in this.userImportData)
                {
                    kvp.Key.SetCurrencyAmount(this.currency, kvp.Value);
                }

                await ChannelSession.SaveSettings();

                if (newCurrencyRank)
                {
                    string type = (this.currency.IsRank) ? "rank" : "currency";
                    if (await MessageBoxHelper.ShowConfirmationDialog("Since you just created a new " + type + ", would you like to create a chat command to show a user's " + type + "?"))
                    {
                        ChatCommand currencyRankCommand = new ChatCommand(this.currency.Name, this.currency.SpecialIdentifier, new RequirementViewModel(MixerRoleEnum.User, 5));
                        string chatText = string.Empty;
                        if (this.currency.IsRank)
                        {
                            chatText = string.Format("@$username is a ${0} with ${1} {2}!", this.currency.UserRankNameSpecialIdentifier, this.currency.UserAmountSpecialIdentifier, this.currency.Name);
                        }
                        else
                        {
                            chatText = string.Format("@$username has ${0} {1}!", this.currency.UserAmountSpecialIdentifier, this.currency.Name);
                        }
                        ChatAction chatAction = new ChatAction(chatText);
                        currencyRankCommand.Actions.Add(chatAction);

                        CommandWindow window = new CommandWindow(new ChatCommandDetailsControl(currencyRankCommand));
                        window.Closed += Window_Closed;
                        window.Show();
                        window.Focus();
                    }
                }

                this.Close();
            });
        }

        private async void ExportUserCurrencyToFileButton_Click(object sender, RoutedEventArgs e)
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

        private void CurrencyAcquireRateComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.CurrencyAcquireRateComboBox.SelectedIndex >= 0)
            {
                CurrencyAcquireRateTypeEnum acquireRate = EnumHelper.GetEnumValueFromString<CurrencyAcquireRateTypeEnum>((string)this.CurrencyAcquireRateComboBox.SelectedItem);
                this.CustomRateGrid.Visibility = (acquireRate == CurrencyAcquireRateTypeEnum.Custom) ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}
