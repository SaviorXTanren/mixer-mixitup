using Mixer.Base.Model.User;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Controls.Dialogs;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Command;
using StreamingClient.Base.Util;
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
        [Name("1PerMinute")]
        Minutes,
        [Name("1PerHour")]
        Hours,
        [Name("1PerSpark")]
        Sparks,
        [Name("1PerEmber")]
        Embers,
        [Name("FanProgression")]
        FanProgression,
        [Name("1PerBit")]
        Bits,
        Custom,
        Disabled,
    }

    /// <summary>
    /// Interaction logic for CurrencyWindow.xaml
    /// </summary>
    public partial class CurrencyWindow : LoadingWindowBase
    {
        private UserCurrencyModel currency;
        private CustomCommand rankChangedCommand;

        private Dictionary<Guid, int> userImportData = new Dictionary<Guid, int>();

        private ObservableCollection<UserRankViewModel> ranks = new ObservableCollection<UserRankViewModel>();

        private string CurrencyRankIdentifierString { get { return (this.IsRankToggleButton.IsChecked.GetValueOrDefault()) ? MixItUp.Base.Resources.Rank.ToLower() : MixItUp.Base.Resources.Currency.ToLower(); } }

        public CurrencyWindow()
        {
            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        public CurrencyWindow(UserCurrencyModel currency)
        {
            this.currency = currency;
            this.rankChangedCommand = this.currency.RankChangedCommand;

            InitializeComponent();

            this.RetroactivelyGivePointsButton.IsEnabled = true;
            this.ExportToFileButton.IsEnabled = true;
            this.ImportFromFileButton.IsEnabled = true;

            this.Initialize(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            this.RanksListView.ItemsSource = this.ranks;

            this.OnlineRateComboBox.ItemsSource = Enum.GetValues(typeof(CurrencyAcquireRateTypeEnum));
            this.OfflineRateComboBox.ItemsSource = new List<CurrencyAcquireRateTypeEnum>() { CurrencyAcquireRateTypeEnum.Minutes, CurrencyAcquireRateTypeEnum.Hours, CurrencyAcquireRateTypeEnum.Custom, CurrencyAcquireRateTypeEnum.Disabled };

            this.AutomaticResetComboBox.ItemsSource = Enum.GetValues(typeof(CurrencyResetRateEnum));

            this.IsPrimaryToggleButton.IsChecked = true;
            if (ChannelSession.Settings.Currencies.Values.Any(c => !c.IsRank && c.IsPrimary) && ChannelSession.Settings.Currencies.Values.Any(c => c.IsRank && c.IsPrimary))
            {
                this.IsPrimaryToggleButton.IsChecked = false;
            }

            if (this.currency != null)
            {
                this.NameTextBox.Text = this.currency.Name;
                this.IsPrimaryToggleButton.IsChecked = this.currency.IsPrimary;

                if (this.currency.MaxAmount != int.MaxValue)
                {
                    this.MaxAmountTextBox.Text = this.currency.MaxAmount.ToString();
                }

                if (this.currency.IsTrackingSparks)
                {
                    this.OnlineRateComboBox.SelectedItem = CurrencyAcquireRateTypeEnum.Sparks;
                }
                else if (this.currency.IsTrackingEmbers)
                {
                    this.OnlineRateComboBox.SelectedItem = CurrencyAcquireRateTypeEnum.Embers;
                }
                else if (this.currency.IsTrackingFanProgression)
                {
                    this.OnlineRateComboBox.SelectedItem = CurrencyAcquireRateTypeEnum.FanProgression;
                }
                else if (this.currency.IsTrackingBits)
                {
                    this.OnlineRateComboBox.SelectedItem = EnumHelper.GetEnumName(CurrencyAcquireRateTypeEnum.Bits);
                }
                else if (this.currency.IsOnlineIntervalMinutes)
                {
                    this.OnlineRateComboBox.SelectedItem = CurrencyAcquireRateTypeEnum.Minutes;
                }
                else if (this.currency.IsOnlineIntervalHours)
                {
                    this.OnlineRateComboBox.SelectedItem = CurrencyAcquireRateTypeEnum.Hours;
                }
                else if (this.currency.IsOnlineIntervalDisabled)
                {
                    this.OnlineRateComboBox.SelectedItem = CurrencyAcquireRateTypeEnum.Disabled;
                }
                else
                {
                    this.OnlineRateComboBox.SelectedItem = CurrencyAcquireRateTypeEnum.Custom;
                }
                this.OnlineAmountRateTextBox.Text = this.currency.AcquireAmount.ToString();
                this.OnlineTimeRateTextBox.Text = this.currency.AcquireInterval.ToString();

                if (this.currency.IsOfflineIntervalMinutes)
                {
                    this.OfflineRateComboBox.SelectedItem = CurrencyAcquireRateTypeEnum.Minutes;
                }
                else if (this.currency.IsOfflineIntervalHours)
                {
                    this.OfflineRateComboBox.SelectedItem = CurrencyAcquireRateTypeEnum.Hours;
                }
                else if (this.currency.IsOfflineIntervalDisabled)
                {
                    this.OfflineRateComboBox.SelectedItem = CurrencyAcquireRateTypeEnum.Disabled;
                }
                else
                {
                    this.OfflineRateComboBox.SelectedItem = CurrencyAcquireRateTypeEnum.Custom;
                }
                this.OfflineAmountRateTextBox.Text = this.currency.OfflineAcquireAmount.ToString();
                this.OfflineTimeRateTextBox.Text = this.currency.OfflineAcquireInterval.ToString();

                this.SubscriberBonusTextBox.Text = this.currency.SubscriberBonus.ToString();
                this.ModeratorBonusTextBox.Text = this.currency.ModeratorBonus.ToString();

                this.OnFollowBonusTextBox.Text = this.currency.OnFollowBonus.ToString();
                this.OnHostBonusTextBox.Text = this.currency.OnHostBonus.ToString();
                this.OnSubscribeBonusTextBox.Text = this.currency.OnSubscribeBonus.ToString();

                this.MinimumActivityRateTextBox.Text = this.currency.MinimumActiveRate.ToString();
                this.AutomaticResetComboBox.SelectedItem = this.currency.ResetInterval;

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
                this.OnlineRateComboBox.SelectedItem = CurrencyAcquireRateTypeEnum.Minutes;
                this.OfflineRateComboBox.SelectedItem = CurrencyAcquireRateTypeEnum.Disabled;

                this.SubscriberBonusTextBox.Text = "0";
                this.ModeratorBonusTextBox.Text = "0";
                this.OnFollowBonusTextBox.Text = "0";
                this.OnHostBonusTextBox.Text = "0";
                this.OnSubscribeBonusTextBox.Text = "0";

                this.MinimumActivityRateTextBox.Text = "0";
                this.AutomaticResetComboBox.SelectedItem = CurrencyResetRateEnum.Never;
            }

            await base.OnLoaded();
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessHelper.LaunchLink("https://github.com/SaviorXTanren/mixer-mixitup/wiki/Currency-&-Rank");
        }

        private void IsRankToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            this.RankListGrid.Visibility = (this.IsRankToggleButton.IsChecked.GetValueOrDefault()) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OnlineRateComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.OnlineRateComboBox.SelectedIndex >= 0)
            {
                CurrencyAcquireRateTypeEnum acquireRate = (CurrencyAcquireRateTypeEnum)this.OnlineRateComboBox.SelectedItem;
                this.OnlineAmountRateTextBox.IsEnabled = (acquireRate == CurrencyAcquireRateTypeEnum.Custom);
                this.OnlineTimeRateTextBox.IsEnabled = (acquireRate == CurrencyAcquireRateTypeEnum.Custom);

                this.OfflineRateGroupBox.IsEnabled = true;
                this.BonusesGrid.IsEnabled = true;
                this.MinimumActivityRateTextBox.IsEnabled = true;

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
                else if (acquireRate == CurrencyAcquireRateTypeEnum.Sparks || acquireRate == CurrencyAcquireRateTypeEnum.Embers ||
                    acquireRate == CurrencyAcquireRateTypeEnum.FanProgression || acquireRate == CurrencyAcquireRateTypeEnum.Bits)
                {
                    this.OnlineAmountRateTextBox.Text = "1";
                    this.OnlineTimeRateTextBox.Text = "1";

                    this.OfflineRateComboBox.SelectedItem = CurrencyAcquireRateTypeEnum.Disabled;
                    this.OfflineRateGroupBox.IsEnabled = false;

                    this.BonusesGrid.IsEnabled = false;
                    this.SubscriberBonusTextBox.Text = "0";
                    this.ModeratorBonusTextBox.Text = "0";
                    this.OnFollowBonusTextBox.Text = "0";
                    this.OnHostBonusTextBox.Text = "0";
                    this.OnSubscribeBonusTextBox.Text = "0";

                    this.MinimumActivityRateTextBox.IsEnabled = false;
                    this.MinimumActivityRateTextBox.Text = "0";
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
                CurrencyAcquireRateTypeEnum acquireRate = (CurrencyAcquireRateTypeEnum)this.OfflineRateComboBox.SelectedItem;
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
                await DialogHelper.ShowMessage("A rank name must be specified");
                return;
            }

            int rankAmount = 0;
            if (string.IsNullOrEmpty(this.RankAmountTextBox.Text) || !int.TryParse(this.RankAmountTextBox.Text, out rankAmount) || rankAmount < 0)
            {
                await DialogHelper.ShowMessage("A minimum amount must be specified");
                return;
            }

            if (this.ranks.Any(r => r.Name.Equals(this.RankNameTextBox.Text) || r.MinimumPoints == rankAmount))
            {
                await DialogHelper.ShowMessage("Every rank must have a unique name and minimum amount");
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
            CommandWindow window = new CommandWindow(new CustomCommandDetailsControl(new CustomCommand(MixItUp.Base.Resources.UserRankChanged)));
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
                if (await DialogHelper.ShowConfirmation(string.Format("Do you want to reset all {0} points?", this.CurrencyRankIdentifierString)))
                {
                    if (this.currency != null)
                    {
                        await this.currency.Reset();
                    }
                }
            });
        }

        private async void RetroactivelyGivePointsButton_Click(object sender, RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                if (await DialogHelper.ShowConfirmation(string.Format("This option will reset all {0} points for this {0} & assign an amount to each user that directly equals the SAVED online rate, not the currently edited online rate. Before using this option, please save all edits to this {0}, re-edit it, then select this option." +
                    Environment.NewLine + Environment.NewLine + "EX: If the Online Rate is \"1 Per Hour\" and a user has 16 viewing hours, then that user's {0} points will be set to 16." +
                    Environment.NewLine + Environment.NewLine + "This process may take some time; are you sure you wish to do this?", this.CurrencyRankIdentifierString)))
                {
                    if (this.currency != null && this.currency.AcquireInterval > 0)
                    {
                        if (this.currency.IsTrackingSparks || this.currency.IsTrackingEmbers || this.currency.IsTrackingFanProgression || this.currency.IsTrackingBits)
                        {
                            await DialogHelper.ShowMessage("The rate type for this currency does not support retroactively giving points.");
                            return;
                        }

                        await this.currency.Reset();

                        HashSet<uint> subscriberIDs = new HashSet<uint>();
                        foreach (UserWithGroupsModel user in await ChannelSession.MixerUserConnection.GetUsersWithRoles(ChannelSession.MixerChannel, UserRoleEnum.Subscriber))
                        {
                            subscriberIDs.Add(user.id);
                        }

                        HashSet<uint> modIDs = new HashSet<uint>();
                        foreach (UserWithGroupsModel user in await ChannelSession.MixerUserConnection.GetUsersWithRoles(ChannelSession.MixerChannel, UserRoleEnum.Mod))
                        {
                            modIDs.Add(user.id);
                        }
                        foreach (UserWithGroupsModel user in await ChannelSession.MixerUserConnection.GetUsersWithRoles(ChannelSession.MixerChannel, UserRoleEnum.ChannelEditor))
                        {
                            modIDs.Add(user.id);
                        }

                        foreach (UserDataModel userData in ChannelSession.Settings.UserData.Values)
                        {
                            int intervalsToGive = userData.ViewingMinutes / this.currency.AcquireInterval;
                            this.currency.AddAmount(userData, this.currency.AcquireAmount * intervalsToGive);
                            if (modIDs.Contains(userData.MixerID))
                            {
                                this.currency.AddAmount(userData, this.currency.ModeratorBonus * intervalsToGive);
                            }
                            else if (subscriberIDs.Contains(userData.MixerID))
                            {
                                this.currency.AddAmount(userData, this.currency.SubscriberBonus * intervalsToGive);
                            }
                            ChannelSession.Settings.UserData.ManualValueChanged(userData.ID);
                        }
                    }
                }
            });
        }

        private async void ImportFromFileButton_Click(object sender, RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                this.userImportData.Clear();

                if (await DialogHelper.ShowConfirmation(string.Format("This will allow you to import the total amounts that each user had, assign them to this {0}, and will overwrite any amounts that each user has." +
                    Environment.NewLine + Environment.NewLine + "This process may take some time; are you sure you wish to do this?", this.CurrencyRankIdentifierString)))
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
                                    UserModel mixerUser = null;
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
                                            mixerUser = await ChannelSession.MixerUserConnection.GetUser(id);
                                        }
                                        else if (!string.IsNullOrEmpty(username))
                                        {
                                            mixerUser = await ChannelSession.MixerUserConnection.GetUser(username);
                                        }
                                    }

                                    if (mixerUser != null)
                                    {
                                        UserViewModel user = new UserViewModel(mixerUser);
                                        if (!this.userImportData.ContainsKey(user.ID))
                                        {
                                            this.userImportData[user.ID] = amount;
                                        }
                                        this.userImportData[user.ID] = Math.Max(this.userImportData[user.ID], amount);
                                        this.ImportFromFileButton.Content = string.Format("{0} Imported...", this.userImportData.Count());
                                    }
                                }

                                foreach (var kvp in this.userImportData)
                                {
                                    this.currency.SetAmount(kvp.Key, kvp.Value);
                                }

                                this.ImportFromFileButton.Content = "Import From File";
                                return;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                    }

                    await DialogHelper.ShowMessage("We were unable to import the data. Please ensure your file is in one of the following formats:" +
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
                    foreach (UserDataModel userData in ChannelSession.Settings.UserData.Values.ToList())
                    {
                        fileContents.AppendLine(string.Format("{0} {1} {2}", userData.MixerID, userData.Username, this.currency.GetAmount(userData)));
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
                    await DialogHelper.ShowMessage(string.Format("A {0} name must be specified", this.CurrencyRankIdentifierString));
                    return;
                }

                if (this.NameTextBox.Text.Any(c => char.IsDigit(c)))
                {
                    await DialogHelper.ShowMessage("The name can not contain any number digits in it");
                    return;
                }

                UserCurrencyModel dupeCurrency = ChannelSession.Settings.Currencies.Values.FirstOrDefault(c => c.Name.Equals(this.NameTextBox.Text));
                if (dupeCurrency != null && (this.currency == null || !this.currency.ID.Equals(dupeCurrency.ID)))
                {
                    await DialogHelper.ShowMessage("There already exists a currency or rank system with this name");
                    return;
                }

                UserInventoryModel dupeInventory = ChannelSession.Settings.Inventories.Values.FirstOrDefault(c => c.Name.Equals(this.NameTextBox.Text));
                if (dupeInventory != null)
                {
                    await DialogHelper.ShowMessage("There already exists an inventory with this name");
                    return;
                }

                string siName = SpecialIdentifierStringBuilder.ConvertToSpecialIdentifier(this.NameTextBox.Text);
                if (siName.Equals("time") || siName.Equals("hours") || siName.Equals("mins") || siName.Equals("sparks") || siName.Equals("embers") || siName.Equals("fanprogression") || siName.Equals("bits"))
                {
                    await DialogHelper.ShowMessage("The following names are reserved and can not be used: time, hours, mins, sparks, embers, fanprogression, bits");
                    return;
                }

                if (string.IsNullOrEmpty(siName))
                {
                    await DialogHelper.ShowMessage("The name must have at least 1 letter in it");
                    return;
                }

                int maxAmount = int.MaxValue;
                if (!string.IsNullOrEmpty(this.MaxAmountTextBox.Text))
                {
                    if (!int.TryParse(this.MaxAmountTextBox.Text, out maxAmount) || maxAmount <= 0)
                    {
                        await DialogHelper.ShowMessage("The max amount must be greater than 0 or can be left empty for no max amount");
                        return;
                    }
                }

                if (string.IsNullOrEmpty(this.OnlineAmountRateTextBox.Text) || !int.TryParse(this.OnlineAmountRateTextBox.Text, out int onlineAmount) || onlineAmount < 0)
                {
                    await DialogHelper.ShowMessage("The online amount must be 0 or greater");
                    return;
                }

                if (string.IsNullOrEmpty(this.OnlineTimeRateTextBox.Text) || !int.TryParse(this.OnlineTimeRateTextBox.Text, out int onlineTime) || onlineTime < 0)
                {
                    await DialogHelper.ShowMessage("The online minutes must be 0 or greater");
                    return;
                }

                if (string.IsNullOrEmpty(this.OfflineAmountRateTextBox.Text) || !int.TryParse(this.OfflineAmountRateTextBox.Text, out int offlineAmount) || offlineAmount < 0)
                {
                    await DialogHelper.ShowMessage("The offline amount must be 0 or greater");
                    return;
                }

                if (string.IsNullOrEmpty(this.OfflineTimeRateTextBox.Text) || !int.TryParse(this.OfflineTimeRateTextBox.Text, out int offlineTime) || offlineTime < 0)
                {
                    await DialogHelper.ShowMessage("The offline minutes must be 0 or greater");
                    return;
                }

                if (onlineAmount > 0 && onlineTime == 0)
                {
                    await DialogHelper.ShowMessage("The online time can not be 0 if the online amount is greater than 0");
                    return;
                }

                if (offlineAmount > 0 && offlineTime == 0)
                {
                    await DialogHelper.ShowMessage("The offline time can not be 0 if the offline amount is greater than 0");
                    return;
                }

                int subscriberBonus = 0;
                if (string.IsNullOrEmpty(this.SubscriberBonusTextBox.Text) || !int.TryParse(this.SubscriberBonusTextBox.Text, out subscriberBonus) || subscriberBonus < 0)
                {
                    await DialogHelper.ShowMessage("The Subscriber bonus must be 0 or greater");
                    return;
                }

                int modBonus = 0;
                if (string.IsNullOrEmpty(this.ModeratorBonusTextBox.Text) || !int.TryParse(this.ModeratorBonusTextBox.Text, out modBonus) || modBonus < 0)
                {
                    await DialogHelper.ShowMessage("The Moderator bonus must be 0 or greater");
                    return;
                }

                int onFollowBonus = 0;
                if (string.IsNullOrEmpty(this.OnFollowBonusTextBox.Text) || !int.TryParse(this.OnFollowBonusTextBox.Text, out onFollowBonus) || onFollowBonus < 0)
                {
                    await DialogHelper.ShowMessage("The On Follow bonus must be 0 or greater");
                    return;
                }

                int onHostBonus = 0;
                if (string.IsNullOrEmpty(this.OnHostBonusTextBox.Text) || !int.TryParse(this.OnHostBonusTextBox.Text, out onHostBonus) || onHostBonus < 0)
                {
                    await DialogHelper.ShowMessage("The On Host bonus must be 0 or greater");
                    return;
                }

                int onSubscribeBonus = 0;
                if (string.IsNullOrEmpty(this.OnSubscribeBonusTextBox.Text) || !int.TryParse(this.OnSubscribeBonusTextBox.Text, out onSubscribeBonus) || onSubscribeBonus < 0)
                {
                    await DialogHelper.ShowMessage("The On Subscribe bonus must be 0 or greater");
                    return;
                }

                if (this.IsRankToggleButton.IsChecked.GetValueOrDefault())
                {
                    if (this.ranks.Count() < 1)
                    {
                        await DialogHelper.ShowMessage("At least one rank must be created");
                        return;
                    }
                }

                int minActivityRate = 0;
                if (string.IsNullOrEmpty(this.MinimumActivityRateTextBox.Text) || !int.TryParse(this.MinimumActivityRateTextBox.Text, out minActivityRate) || minActivityRate < 0)
                {
                    await DialogHelper.ShowMessage("The Minimum Activity Rate must be 0 or greater");
                    return;
                }

                bool isNew = false;
                if (this.currency == null)
                {
                    isNew = true;
                    this.currency = new UserCurrencyModel();
                    ChannelSession.Settings.Currencies[this.currency.ID] = this.currency;
                }

                CurrencyAcquireRateTypeEnum acquireRate = CurrencyAcquireRateTypeEnum.Custom;
                if (this.OnlineRateComboBox.SelectedIndex >= 0)
                {
                    acquireRate = (CurrencyAcquireRateTypeEnum)this.OnlineRateComboBox.SelectedItem;
                }

                this.currency.IsTrackingSparks = false;
                this.currency.IsTrackingEmbers = false;
                this.currency.IsTrackingFanProgression = false;
                this.currency.IsTrackingBits = false;
                if (acquireRate == CurrencyAcquireRateTypeEnum.Sparks)
                {
                    this.currency.IsTrackingSparks = true;
                }
                else if (acquireRate == CurrencyAcquireRateTypeEnum.Embers)
                {
                    this.currency.IsTrackingEmbers = true;
                }
                else if (acquireRate == CurrencyAcquireRateTypeEnum.FanProgression)
                {
                    this.currency.IsTrackingFanProgression = true;
                }
                else if (acquireRate == CurrencyAcquireRateTypeEnum.Bits)
                {
                    this.currency.IsTrackingBits = true;
                }

                this.currency.Name = this.NameTextBox.Text;
                this.currency.MaxAmount = maxAmount;

                this.currency.AcquireAmount = onlineAmount;
                this.currency.AcquireInterval = onlineTime;
                this.currency.OfflineAcquireAmount = offlineAmount;
                this.currency.OfflineAcquireInterval = offlineTime;

                this.currency.SubscriberBonus = subscriberBonus;
                this.currency.ModeratorBonus = modBonus;
                this.currency.OnFollowBonus = onFollowBonus;
                this.currency.OnHostBonus = onHostBonus;
                this.currency.OnSubscribeBonus = onSubscribeBonus;

                this.currency.MinimumActiveRate = minActivityRate;
                this.currency.ResetInterval = (CurrencyResetRateEnum)this.AutomaticResetComboBox.SelectedItem;

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

                foreach (var otherCurrencies in ChannelSession.Settings.Currencies)
                {
                    if (otherCurrencies.Value.IsRank == this.currency.IsRank)
                    {
                        // Turn off primary for all other currencies/ranks of the same kind
                        otherCurrencies.Value.IsPrimary = false;
                    }
                }
                this.currency.IsPrimary = this.IsPrimaryToggleButton.IsChecked.GetValueOrDefault();

                await ChannelSession.SaveSettings();

                if (isNew)
                {
                    List<NewCurrencyRankCommand> commandsToAdd = new List<NewCurrencyRankCommand>();

                    ChatCommand statusCommand = new ChatCommand("User " + this.currency.Name, this.currency.SpecialIdentifier, new RequirementViewModel(UserRoleEnum.User, 5));
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

                    if (!this.currency.IsTrackingSparks && !this.currency.IsTrackingEmbers && !this.currency.IsTrackingFanProgression && !this.currency.IsTrackingBits)
                    {
                        ChatCommand addCommand = new ChatCommand("Add " + this.currency.Name, "add" + this.currency.SpecialIdentifier, new RequirementViewModel(UserRoleEnum.Mod, 5));
                        addCommand.Actions.Add(new CurrencyAction(this.currency, CurrencyActionTypeEnum.AddToSpecificUser, "$arg2text", username: "$targetusername"));
                        addCommand.Actions.Add(new ChatAction(string.Format("@$targetusername received $arg2text {0}!", this.currency.Name)));
                        commandsToAdd.Add(new NewCurrencyRankCommand(string.Format("!{0} - {1}", addCommand.Commands.First(), "Adds Amount To Specified User"), addCommand));

                        ChatCommand addAllCommand = new ChatCommand("Add All " + this.currency.Name, "addall" + this.currency.SpecialIdentifier, new RequirementViewModel(UserRoleEnum.Mod, 5));
                        addAllCommand.Actions.Add(new CurrencyAction(this.currency, CurrencyActionTypeEnum.AddToAllChatUsers, "$arg1text"));
                        addAllCommand.Actions.Add(new ChatAction(string.Format("Everyone got $arg1text {0}!", this.currency.Name)));
                        commandsToAdd.Add(new NewCurrencyRankCommand(string.Format("!{0} - {1}", addAllCommand.Commands.First(), "Adds Amount To All Chat Users"), addAllCommand));

                        if (!this.currency.IsRank)
                        {
                            ChatCommand giveCommand = new ChatCommand("Give " + this.currency.Name, "give" + this.currency.SpecialIdentifier, new RequirementViewModel(UserRoleEnum.User, 5));
                            giveCommand.Actions.Add(new CurrencyAction(this.currency, CurrencyActionTypeEnum.AddToSpecificUser, "$arg2text", username: "$targetusername", deductFromUser: true));
                            giveCommand.Actions.Add(new ChatAction(string.Format("@$username gave @$targetusername $arg2text {0}!", this.currency.Name)));
                            commandsToAdd.Add(new NewCurrencyRankCommand(string.Format("!{0} - {1}", giveCommand.Commands.First(), "Gives Amount To Specified User"), giveCommand));
                        }
                    }

                    NewCurrencyRankCommandsDialogControl customDialogControl = new NewCurrencyRankCommandsDialogControl(this.currency, commandsToAdd);
                    if (bool.Equals(await DialogHelper.ShowCustom(customDialogControl), true))
                    {
                        foreach (NewCurrencyRankCommand command in customDialogControl.commands)
                        {
                            if (command.AddCommand)
                            {
                                ChannelSession.Settings.ChatCommands.Add(command.Command);
                            }
                        }
                        ChannelSession.Services.Chat.RebuildCommandTriggers();
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
