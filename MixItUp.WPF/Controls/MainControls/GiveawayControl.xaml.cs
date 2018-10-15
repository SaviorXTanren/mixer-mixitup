using Mixer.Base.Model.User;
using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.MainControls
{
    public enum GiveawayEntryTypeEnum
    {
        Command,
        [Name("GawkBox Tip")]
        GawkBox,
        [Name("Streamlabs Tip")]
        Streamlabs,
        [Name("Tiltify Donations")]
        Tiltify,
    }

    public enum GiveawayDonationEntryQualificationTypeEnum
    {
        [Name("One Entry Per User")]
        OneEntryPerUser,
        [Name("One Entry Per Amount")]
        OneEntryPerAmount,
        [Name("Minimum Amount Required")]
        MinimumAmountRequired,
    }

    public class GiveawayUser
    {
        public UserViewModel User { get; set; }
        public int Entries { get; set; }
        public double DonationAmount { get; set; }
    }

    /// <summary>
    /// Interaction logic for GiveawayControl.xaml
    /// </summary>
    public partial class GiveawayControl : MainControlBase, IDisposable
    {
        private string giveawayItem;

        private ObservableCollection<GiveawayUser> enteredUsersUICollection = new ObservableCollection<GiveawayUser>();
        private LockedDictionary<uint, GiveawayUser> enteredUsers = new LockedDictionary<uint, GiveawayUser>();

        private ChatCommand giveawayCommand = null;

        private int timeLeft = 0;
        private int reminder = 0;

        private UserViewModel selectedWinner = null;

        private CancellationTokenSource backgroundThreadCancellationTokenSource = new CancellationTokenSource();

        public GiveawayControl()
        {
            InitializeComponent();
        }

        protected override Task InitializeInternal()
        {
            this.EntryMethodTypeComboBox.ItemsSource = EnumHelper.GetEnumNames<GiveawayEntryTypeEnum>();
            this.DonationEntryQualifierComboBox.ItemsSource = EnumHelper.GetEnumNames<GiveawayDonationEntryQualificationTypeEnum>();

            this.EnteredUsersListView.ItemsSource = this.enteredUsersUICollection;

            this.MaximumEntriesTextBox.Text = ChannelSession.Settings.GiveawayMaximumEntries.ToString();
            this.TimerTextBox.Text = ChannelSession.Settings.GiveawayTimer.ToString();
            this.ReminderTextBox.Text = ChannelSession.Settings.GiveawayReminderInterval.ToString();

            this.RequireClaimCheckBox.IsChecked = ChannelSession.Settings.GiveawayRequireClaim;

            if (!string.IsNullOrEmpty(ChannelSession.Settings.GiveawayCommand))
            {
                this.EntryMethodTypeComboBox.SelectedItem = EnumHelper.GetEnumName(GiveawayEntryTypeEnum.Command);
                this.CommandTextBox.Text = ChannelSession.Settings.GiveawayCommand;
                this.Requirements.SetRequirements(ChannelSession.Settings.GiveawayRequirements);
            }
            else if (ChannelSession.Settings.GiveawayGawkBoxTrigger || ChannelSession.Settings.GiveawayStreamlabsTrigger)
            {
                if (ChannelSession.Settings.GiveawayGawkBoxTrigger)
                {
                    this.EntryMethodTypeComboBox.SelectedItem = EnumHelper.GetEnumName(GiveawayEntryTypeEnum.GawkBox);
                }
                else if (ChannelSession.Settings.GiveawayStreamlabsTrigger)
                {
                    this.EntryMethodTypeComboBox.SelectedItem = EnumHelper.GetEnumName(GiveawayEntryTypeEnum.Streamlabs);
                }
                else if (ChannelSession.Settings.GiveawayTiltifyTrigger)
                {
                    this.EntryMethodTypeComboBox.SelectedItem = EnumHelper.GetEnumName(GiveawayEntryTypeEnum.Tiltify);
                }

                if (ChannelSession.Settings.GiveawayDonationAmount > 0.0)
                {
                    this.DonationEntryQualifierAmountTextBox.Text = ChannelSession.Settings.GiveawayDonationAmount.ToString();
                    if (ChannelSession.Settings.GiveawayDonationRequiredAmount)
                    {
                        this.DonationEntryQualifierComboBox.SelectedItem = EnumHelper.GetEnumName(GiveawayDonationEntryQualificationTypeEnum.MinimumAmountRequired);
                    }
                    else
                    {
                        this.DonationEntryQualifierComboBox.SelectedItem = EnumHelper.GetEnumName(GiveawayDonationEntryQualificationTypeEnum.OneEntryPerAmount);
                    }
                }
                else
                {
                    this.DonationEntryQualifierComboBox.SelectedItem = EnumHelper.GetEnumName(GiveawayDonationEntryQualificationTypeEnum.OneEntryPerUser);
                }
            }

            this.GiveawayUserJoinedCommand.DataContext = ChannelSession.Settings.GiveawayUserJoinedCommand;
            this.GiveawayWinnerSelectedCommand.DataContext = ChannelSession.Settings.GiveawayWinnerSelectedCommand;

            return base.InitializeInternal();
        }

        private void EntryMethodTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            this.CommandEntryGrid.Visibility = Visibility.Collapsed;
            this.DonationEntryGrid.Visibility = Visibility.Collapsed;

            if (this.EntryMethodTypeComboBox.SelectedIndex >= 0)
            {
                GiveawayEntryTypeEnum entryType = EnumHelper.GetEnumValueFromString<GiveawayEntryTypeEnum>((string)this.EntryMethodTypeComboBox.SelectedItem);
                if (entryType == GiveawayEntryTypeEnum.Command)
                {
                    this.CommandEntryGrid.Visibility = Visibility.Visible;
                }
                else
                {
                    this.DonationEntryGrid.Visibility = Visibility.Visible;
                }
            }
        }

        private void DonationEntryQualifierComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            this.DonationEntryQualifierAmountTextBox.IsEnabled = false;
            if (this.DonationEntryQualifierComboBox.SelectedIndex >= 0)
            {
                GiveawayDonationEntryQualificationTypeEnum qualificationType = EnumHelper.GetEnumValueFromString<GiveawayDonationEntryQualificationTypeEnum>((string)this.DonationEntryQualifierComboBox.SelectedItem);
                if (qualificationType == GiveawayDonationEntryQualificationTypeEnum.MinimumAmountRequired || qualificationType == GiveawayDonationEntryQualificationTypeEnum.OneEntryPerAmount)
                {
                    this.DonationEntryQualifierAmountTextBox.IsEnabled = true;
                }
            }
        }

        private async void EnableGiveawayButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.ItemTextBox.Text))
            {
                await MessageBoxHelper.ShowMessageDialog("An item to give away must be specified");
                return;
            }

            this.timeLeft = 0;
            if (string.IsNullOrEmpty(this.TimerTextBox.Text) || !int.TryParse(this.TimerTextBox.Text, out this.timeLeft) || this.timeLeft <= 0)
            {
                await MessageBoxHelper.ShowMessageDialog("Timer must be greater than 0");
                return;
            }

            this.reminder = 0;
            if (string.IsNullOrEmpty(this.ReminderTextBox.Text) || !int.TryParse(this.ReminderTextBox.Text, out this.reminder) || this.reminder <= 0)
            {
                await MessageBoxHelper.ShowMessageDialog("Reminder must be greater than 0");
                return;
            }

            if (string.IsNullOrEmpty(this.MaximumEntriesTextBox.Text) || !int.TryParse(this.MaximumEntriesTextBox.Text, out int maxEntries) || maxEntries <= 0)
            {
                await MessageBoxHelper.ShowMessageDialog("Maximum Entries must be greater than 0");
                return;
            }

            if (this.EntryMethodTypeComboBox.SelectedIndex < 0)
            {
                await MessageBoxHelper.ShowMessageDialog("An entry method must be selected");
                return;
            }

            GiveawayEntryTypeEnum entryType = EnumHelper.GetEnumValueFromString<GiveawayEntryTypeEnum>((string)this.EntryMethodTypeComboBox.SelectedItem);
            if (entryType == GiveawayEntryTypeEnum.Command)
            {
                if (string.IsNullOrEmpty(this.CommandTextBox.Text))
                {
                    await MessageBoxHelper.ShowMessageDialog("Giveaway command must be specified");
                    return;
                }

                if (this.CommandTextBox.Text.Any(c => !Char.IsLetterOrDigit(c)))
                {
                    await MessageBoxHelper.ShowMessageDialog("Giveaway Command can only contain letters and numbers");
                    return;
                }

                if (!await this.Requirements.Validate())
                {
                    return;
                }

                ChannelSession.Settings.GiveawayCommand = this.CommandTextBox.Text.ToLower();
                ChannelSession.Settings.GiveawayRequirements = this.Requirements.GetRequirements();
                ChannelSession.Settings.GiveawayGawkBoxTrigger = false;
                ChannelSession.Settings.GiveawayStreamlabsTrigger = false;
                ChannelSession.Settings.GiveawayDonationRequiredAmount = false;
                ChannelSession.Settings.GiveawayDonationAmount = 0.0;
            }
            else
            {
                if (this.DonationEntryQualifierComboBox.SelectedIndex < 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("An entry method must be selected");
                    return;
                }

                double donationAmount = 0.0;
                GiveawayDonationEntryQualificationTypeEnum qualificationType = EnumHelper.GetEnumValueFromString<GiveawayDonationEntryQualificationTypeEnum>((string)this.DonationEntryQualifierComboBox.SelectedItem);
                if (qualificationType == GiveawayDonationEntryQualificationTypeEnum.MinimumAmountRequired || qualificationType == GiveawayDonationEntryQualificationTypeEnum.OneEntryPerAmount)
                {
                    if (string.IsNullOrEmpty(this.DonationEntryQualifierAmountTextBox.Text) || !double.TryParse(this.DonationEntryQualifierAmountTextBox.Text, out donationAmount) || donationAmount <= 0.0)
                    {
                        await MessageBoxHelper.ShowMessageDialog("A validation tip amount must be entered");
                        return;
                    }

                    donationAmount = Math.Round(donationAmount, 2);
                }

                ChannelSession.Settings.GiveawayCommand = null;
                ChannelSession.Settings.GiveawayGawkBoxTrigger = (entryType == GiveawayEntryTypeEnum.GawkBox);
                ChannelSession.Settings.GiveawayStreamlabsTrigger = (entryType == GiveawayEntryTypeEnum.Streamlabs);
                ChannelSession.Settings.GiveawayTiltifyTrigger = (entryType == GiveawayEntryTypeEnum.Tiltify);
                ChannelSession.Settings.GiveawayDonationRequiredAmount = (qualificationType == GiveawayDonationEntryQualificationTypeEnum.MinimumAmountRequired);
                ChannelSession.Settings.GiveawayDonationAmount = donationAmount;
            }

            ChannelSession.Settings.GiveawayTimer = this.timeLeft;
            ChannelSession.Settings.GiveawayReminderInterval = this.reminder;
            ChannelSession.Settings.GiveawayMaximumEntries = maxEntries;
            ChannelSession.Settings.GiveawayRequireClaim = this.RequireClaimCheckBox.IsChecked.GetValueOrDefault();
            await ChannelSession.SaveSettings();

            this.giveawayCommand = new ChatCommand("Giveaway Command", ChannelSession.Settings.GiveawayCommand, new RequirementViewModel());

            this.timeLeft = this.timeLeft * 60;
            this.reminder = this.reminder * 60;

            this.giveawayItem = this.ItemTextBox.Text;

            this.enteredUsers.Clear();
            this.enteredUsersUICollection.Clear();

            this.WinnerTextBlock.Text = "";
            this.EnableGiveawayButton.Visibility = Visibility.Collapsed;
            this.DisableGiveawayButton.Visibility = Visibility.Visible;

            this.GiveawayBasicsGrid.IsEnabled = this.GiveawayTimersGrid.IsEnabled = this.GiveawayCommandsGrid.IsEnabled = this.CommandEntryGrid.IsEnabled = this.DonationEntryGrid.IsEnabled = false;

            await ChannelSession.Chat.SendMessage(string.Format("A giveaway for {0} has started! {1} in the next {2} minute(s)", this.giveawayItem, this.GetEntryInstructions(), ChannelSession.Settings.GiveawayTimer));

            this.backgroundThreadCancellationTokenSource = new CancellationTokenSource();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () => { await this.GiveawayTimerBackground(); }, this.backgroundThreadCancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private async void DisableGiveawayButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.EndGiveaway();
        }

        private void GiveawayCommand_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
            CustomCommand command = commandButtonsControl.GetCommandFromCommandButtons<CustomCommand>(sender);
            if (command != null)
            {
                CommandWindow window = new CommandWindow(new CustomCommandDetailsControl(command));
                window.Show();
            }
        }

        private async Task EndGiveaway()
        {
            this.backgroundThreadCancellationTokenSource.Cancel();

            this.timeLeft = 0;
            this.selectedWinner = null;
            this.giveawayCommand = null;

            GlobalEvents.OnChatCommandMessageReceived -= GlobalEvents_OnChatCommandMessageReceived;
            GlobalEvents.OnDonationOccurred -= GlobalEvents_OnDonationOccurred;

            await this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.TimeLeftTextBlock.Text = "";

                this.GiveawayBasicsGrid.IsEnabled = this.GiveawayTimersGrid.IsEnabled = this.GiveawayCommandsGrid.IsEnabled = this.CommandEntryGrid.IsEnabled = this.DonationEntryGrid.IsEnabled = true;

                this.DisableGiveawayButton.Visibility = Visibility.Collapsed;
                this.EnableGiveawayButton.Visibility = Visibility.Visible;

                this.enteredUsers.Clear();
                this.enteredUsersUICollection.Clear();
            }));
        }

        private async Task GiveawayTimerBackground()
        {
            GlobalEvents.OnChatCommandMessageReceived += GlobalEvents_OnChatCommandMessageReceived;
            GlobalEvents.OnDonationOccurred += GlobalEvents_OnDonationOccurred;

            try
            {
                while (this.timeLeft > 0)
                {
                    await Task.Delay(1000);
                    this.timeLeft--;

                    string timeLeftUIText = (this.timeLeft % 60).ToString() + " Seconds";
                    if (this.timeLeft > 60)
                    {
                        timeLeftUIText = (this.timeLeft / 60).ToString() + " Minutes " + timeLeftUIText;
                    }

                    await this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        this.TimeLeftTextBlock.Text = timeLeftUIText;
                    }));

                    string timeLeftText = null;
                    if (this.timeLeft > 60 && (this.timeLeft % this.reminder) == 0)
                    {
                        int minutesLeft = this.timeLeft / 60;
                        timeLeftText = minutesLeft + " minutes";
                    }
                    else if (this.timeLeft == 60 || this.timeLeft == 30 || this.timeLeft == 10)
                    {
                        timeLeftText = this.timeLeft + " seconds";
                    }

                    if (!string.IsNullOrEmpty(timeLeftText))
                    {
                        await ChannelSession.Chat.SendMessage(string.Format("The giveaway will end in {0}. {1}!", timeLeftText, this.GetEntryInstructions()));
                    }

                    if (this.backgroundThreadCancellationTokenSource.Token.IsCancellationRequested)
                    {
                        await this.EndGiveaway();
                        return;
                    }
                }

                while (true)
                {
                    this.selectedWinner = this.SelectWinner();
                    if (this.selectedWinner != null)
                    {
                        await this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            this.WinnerTextBlock.Text = this.selectedWinner.UserName;
                        }));

                        await ChannelSession.Settings.GiveawayWinnerSelectedCommand.Perform(this.selectedWinner);

                        if (!ChannelSession.Settings.GiveawayRequireClaim)
                        {
                            await this.EndGiveaway();
                            return;
                        }
                        else
                        {
                            int claimTime = 60;
                            while (claimTime > 0)
                            {
                                await Task.Delay(1000);
                                claimTime--;

                                await this.Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    this.TimeLeftTextBlock.Text = claimTime.ToString();
                                }));

                                if (this.backgroundThreadCancellationTokenSource.Token.IsCancellationRequested)
                                {
                                    await this.EndGiveaway();
                                    return;
                                }
                            }
                        }
                    }
                    else
                    {
                        await ChannelSession.Chat.SendMessage("There are no users that entered/left in the giveaway");
                        await this.EndGiveaway();
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MixItUp.Base.Util.Logger.Log(ex);
            }
        }

        private async void GlobalEvents_OnChatCommandMessageReceived(object sender, ChatMessageViewModel message)
        {
            if (this.timeLeft > 0 && this.selectedWinner == null && this.giveawayCommand.MatchesOrContainsCommand(message.Message))
            {
                int entries = 1;

                IEnumerable<string> arguments = this.giveawayCommand.GetArgumentsFromText(message.Message);
                if (arguments.Count() > 0)
                {
                    int.TryParse(arguments.ElementAt(0), out entries);
                }

                int currentEntries = 0;
                if (this.enteredUsers.ContainsKey(message.User.ID))
                {
                    currentEntries = this.enteredUsers[message.User.ID].Entries;
                }

                if ((entries + currentEntries) > ChannelSession.Settings.GiveawayMaximumEntries)
                {
                    await ChannelSession.Chat.Whisper(message.User.UserName, string.Format("You may only enter {0} time(s), you currently have entered {1} time(s)", ChannelSession.Settings.GiveawayMaximumEntries, currentEntries));
                    return;
                }

                if (await ChannelSession.Settings.GiveawayRequirements.DoesMeetUserRoleRequirement(message.User))
                {
                    if (ChannelSession.Settings.GiveawayRequirements.Rank != null && ChannelSession.Settings.GiveawayRequirements.Rank.GetCurrency() != null)
                    {
                        if (!ChannelSession.Settings.GiveawayRequirements.DoesMeetRankRequirement(message.User))
                        {
                            await ChannelSession.Settings.GiveawayRequirements.Rank.SendRankNotMetWhisper(message.User);
                            return;
                        }
                    }

                    if (ChannelSession.Settings.GiveawayRequirements.Currency != null && ChannelSession.Settings.GiveawayRequirements.Currency.GetCurrency() != null)
                    {
                        int totalAmount = ChannelSession.Settings.GiveawayRequirements.Currency.RequiredAmount * entries;
                        if (!ChannelSession.Settings.GiveawayRequirements.TrySubtractCurrencyAmount(message.User, totalAmount))
                        {
                            await ChannelSession.Chat.Whisper(message.User.UserName, string.Format("You do not have the required {0} {1} to do this", totalAmount, ChannelSession.Settings.GiveawayRequirements.Currency.GetCurrency().Name));
                            return;
                        }
                    }

                    if (!this.enteredUsers.ContainsKey(message.User.ID))
                    {
                        this.enteredUsers[message.User.ID] = new GiveawayUser() { User = message.User, Entries = 0 };
                    }
                    GiveawayUser giveawayUser = this.enteredUsers[message.User.ID];

                    giveawayUser.Entries += entries;

                    await this.RefreshUserList();

                    await ChannelSession.Settings.GiveawayUserJoinedCommand.Perform(message.User);
                }
                else
                {
                    await ChannelSession.Chat.Whisper(message.User.UserName, string.Format("You are not able to enter this giveaway as it is only for {0}s", ChannelSession.Settings.GiveawayRequirements.Role.RoleNameString));
                }
            }
            else if (this.selectedWinner != null && message.Message.Equals("!claim", StringComparison.InvariantCultureIgnoreCase) && this.selectedWinner.Equals(message.User))
            {
                await ChannelSession.Chat.SendMessage(string.Format("@{0} has claimed their prize! Listen closely to the streamer for instructions on getting your prize.", message.User.UserName));
                await this.EndGiveaway();
            }
        }

        private async void GlobalEvents_OnDonationOccurred(object sender, UserDonationModel e)
        {
            if (this.timeLeft > 0 && this.selectedWinner == null)
            {
                if  ((ChannelSession.Settings.GiveawayGawkBoxTrigger && e.Source == UserDonationSourceEnum.GawkBox) ||
                    (ChannelSession.Settings.GiveawayStreamlabsTrigger && e.Source == UserDonationSourceEnum.Streamlabs) ||
                    (ChannelSession.Settings.GiveawayTiltifyTrigger && e.Source == UserDonationSourceEnum.Tiltify))
                {
                    UserModel userModel = await ChannelSession.Connection.GetUser(e.UserName);
                    if (userModel != null)
                    {
                        UserViewModel user = new UserViewModel(userModel);

                        if (!this.enteredUsers.ContainsKey(user.ID))
                        {
                            this.enteredUsers[user.ID] = new GiveawayUser() { User = user, Entries = 0 };
                        }
                        GiveawayUser giveawayUser = this.enteredUsers[user.ID];

                        giveawayUser.DonationAmount += e.Amount;

                        int newEntryAmount = 0;
                        if (ChannelSession.Settings.GiveawayDonationAmount > 0.0)
                        {
                            if (ChannelSession.Settings.GiveawayDonationRequiredAmount && giveawayUser.DonationAmount >= ChannelSession.Settings.GiveawayDonationAmount)
                            {
                                newEntryAmount = 1;
                            }
                            else
                            {
                                newEntryAmount = (int)(giveawayUser.DonationAmount / ChannelSession.Settings.GiveawayDonationAmount);
                            }
                        }
                        else
                        {
                            newEntryAmount = 1;
                        }

                        newEntryAmount = Math.Min(newEntryAmount, ChannelSession.Settings.GiveawayMaximumEntries);

                        if (newEntryAmount > giveawayUser.Entries)
                        {
                            await ChannelSession.Chat.Whisper(user.UserName, "You've gotten an entry into the giveaway, stay tuned to see who wins!");
                        }
                        giveawayUser.Entries = newEntryAmount;

                        await this.RefreshUserList();
                    }
                }
            }
        }

        private async Task RefreshUserList()
        {
            await this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.enteredUsersUICollection.Clear();
                foreach (GiveawayUser gUser in this.enteredUsers.Values)
                {
                    this.enteredUsersUICollection.Add(gUser);
                }
            }));
        }

        private string GetEntryInstructions()
        {
            string entryInstructions = string.Empty;
            if (!string.IsNullOrEmpty(ChannelSession.Settings.GiveawayCommand))
            {
                string bonusEntriesText = string.Empty;
                if (ChannelSession.Settings.GiveawayMaximumEntries > 1)
                {
                    bonusEntriesText += string.Format("or \"!{0} <AMOUNT>\" ", ChannelSession.Settings.GiveawayCommand);
                }
                entryInstructions = string.Format("Type \"!{0}\" {1}in chat to enter", ChannelSession.Settings.GiveawayCommand, bonusEntriesText);
            }
            else
            {
                string service = string.Empty;
                if (ChannelSession.Settings.GiveawayGawkBoxTrigger)
                {
                    service = "GawkBox";
                }
                else if (ChannelSession.Settings.GiveawayStreamlabsTrigger)
                {
                    service = "Streamlabs";
                }
                else if (ChannelSession.Settings.GiveawayTiltifyTrigger)
                {
                    service = "Tiltify";
                }

                string requiredAmount = string.Empty;
                if (ChannelSession.Settings.GiveawayDonationAmount > 0.0)
                {
                    if (ChannelSession.Settings.GiveawayDonationRequiredAmount)
                    {
                        entryInstructions = string.Format("All donations/tips over ${0} through {1} get an entry to win", ChannelSession.Settings.GiveawayDonationRequiredAmount, service);
                    }
                    else
                    {
                        entryInstructions = string.Format("Every ${0} in donation(s)/tip(s) through {1} get an entry to win", ChannelSession.Settings.GiveawayDonationRequiredAmount, service);
                    }
                }
                else
                {
                    entryInstructions = string.Format("Any donations/tips through {0} get an entry to win", service);
                }
            }
            return entryInstructions;
        }

        private UserViewModel SelectWinner()
        {
            if (this.enteredUsers.Count > 0)
            {
                int totalEntries = this.enteredUsers.Values.Sum(u => u.Entries);
                int entryNumber = RandomHelper.GenerateRandomNumber(totalEntries);

                int currentEntry = 0;
                foreach (var kvp in this.enteredUsers.Values)
                {
                    currentEntry += kvp.Entries;
                    if (entryNumber < currentEntry)
                    {
                        this.enteredUsers.Remove(kvp.User.ID);
                        return kvp.User;
                    }
                }
            }
            return null;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                    this.backgroundThreadCancellationTokenSource.Dispose();
                }

                // Free unmanaged resources (unmanaged objects) and override a finalizer below.
                // Set large fields to null.

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}