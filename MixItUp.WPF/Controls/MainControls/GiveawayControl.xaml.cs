using Mixer.Base.Model.User;
using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Util;
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
    public partial class GiveawayControl : MainControlBase
    {
        public bool giveawayEnabled;
        public string giveawayItem;

        private ObservableCollection<GiveawayUser> enteredUsers = new ObservableCollection<GiveawayUser>();
        private LockedDictionary<uint, GiveawayUser> enteredUsersDictionary = new LockedDictionary<uint, GiveawayUser>();

        private UserViewModel selectedWinner = null;

        private int timeLeft = 120;
        private CancellationTokenSource backgroundThreadCancellationTokenSource = new CancellationTokenSource();

        public GiveawayControl()
        {
            InitializeComponent();

            GlobalEvents.OnChatCommandMessageReceived += GlobalEvents_OnChatCommandMessageReceived;
            GlobalEvents.OnDonationOccurred += GlobalEvents_OnDonationOccurred;
        }

        protected override Task InitializeInternal()
        {
            this.EntryMethodTypeComboBox.ItemsSource = EnumHelper.GetEnumNames<GiveawayEntryTypeEnum>();
            this.DonationEntryQualifierComboBox.ItemsSource = EnumHelper.GetEnumNames<GiveawayDonationEntryQualificationTypeEnum>();

            this.EnteredUsersListView.ItemsSource = this.enteredUsers;

            this.TimerTextBox.Text = ChannelSession.Settings.GiveawayTimer.ToString();

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
            await ChannelSession.SaveSettings();

            this.giveawayEnabled = true;
            this.giveawayItem = this.ItemTextBox.Text;
            this.TimeLeftTextBlock.Text = this.timeLeft.ToString();

            this.enteredUsersDictionary.Clear();
            this.enteredUsers.Clear();

            this.WinnerTextBlock.Text = "";
            this.EnableGiveawayButton.Visibility = Visibility.Collapsed;
            this.DisableGiveawayButton.Visibility = Visibility.Visible;

            this.GiveawayBasicsGrid.IsEnabled = this.EntryMethodTypeComboBox.IsEnabled = this.CommandEntryGrid.IsEnabled = this.DonationEntryGrid.IsEnabled = false;

            await ChannelSession.Chat.SendMessage(string.Format("A giveaway for {0} has started! {1} in the next {2} seconds", this.giveawayItem, this.GetEntryInstructions(), ChannelSession.Settings.GiveawayTimer));

            this.backgroundThreadCancellationTokenSource = new CancellationTokenSource();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () => { await this.GiveawayTimerBackground(); }, this.backgroundThreadCancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private async void DisableGiveawayButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.EndGiveaway();
        }

        private async Task EndGiveaway()
        {
            this.backgroundThreadCancellationTokenSource.Cancel();

            this.giveawayEnabled = false;
            await this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.selectedWinner = null;
                this.TimeLeftTextBlock.Text = "";

                this.GiveawayBasicsGrid.IsEnabled = this.EntryMethodTypeComboBox.IsEnabled = this.CommandEntryGrid.IsEnabled = this.DonationEntryGrid.IsEnabled = true;

                this.DisableGiveawayButton.Visibility = Visibility.Collapsed;
                this.EnableGiveawayButton.Visibility = Visibility.Visible;

                this.enteredUsersDictionary.Clear();
                this.enteredUsers.Clear();
            }));
        }

        private async Task GiveawayTimerBackground()
        {
            while (this.timeLeft > 0)
            {
                await Task.Delay(1000);
                this.timeLeft--;
                await this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    this.TimeLeftTextBlock.Text = this.timeLeft.ToString();
                }));

                string timeLeftText = null;
                if (this.timeLeft > 60 && this.timeLeft % 300 == 0)
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

            Dictionary<UserViewModel, int> giveawayUsers = new Dictionary<UserViewModel, int>();
            if (this.enteredUsersDictionary.Count > 0)
            {
                foreach (GiveawayUser gUser in this.enteredUsersDictionary.Values)
                {
                    if (gUser.Entries > 0)
                    {
                        giveawayUsers[gUser.User] = gUser.Entries;
                    }
                }
            }

            while (true)
            {
                if (giveawayUsers.Count > 0)
                {
                    Random random = new Random();
                    int entryNumber = random.Next(giveawayUsers.Select(gu => gu.Value).Sum(e => e));
                    int totalEntries = 0;
                    foreach (var kvp in giveawayUsers)
                    {
                        totalEntries += kvp.Value;
                        if (entryNumber < totalEntries)
                        {
                            this.selectedWinner = kvp.Key;
                            break;
                        }
                    }

                    giveawayUsers.Remove(this.selectedWinner);

                    await this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        this.WinnerTextBlock.Text = this.selectedWinner.UserName;
                    }));

                    await ChannelSession.Chat.SendMessage(string.Format("Congratulations @{0}, you won {1}! Type \"!claim\" in chat in the next 60 seconds to claim your prize!", this.selectedWinner.UserName, this.giveawayItem));

                    this.timeLeft = 60;
                    while (this.timeLeft > 0)
                    {
                        await Task.Delay(1000);
                        this.timeLeft--;
                        await this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            this.TimeLeftTextBlock.Text = this.timeLeft.ToString();
                        }));

                        if (this.backgroundThreadCancellationTokenSource.Token.IsCancellationRequested)
                        {
                            await this.EndGiveaway();
                            return;
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

        private async void GlobalEvents_OnChatCommandMessageReceived(object sender, ChatMessageViewModel e)
        {
            if (this.giveawayEnabled)
            {
                if (this.selectedWinner == null && !string.IsNullOrEmpty(ChannelSession.Settings.GiveawayCommand) && e.CommandName.Equals("!" + ChannelSession.Settings.GiveawayCommand))
                {
                    if (this.enteredUsersDictionary.ContainsKey(e.User.ID))
                    {
                        await ChannelSession.Chat.Whisper(e.User.UserName, "You have already entered into this giveaway, stay tuned to see who wins!");
                        return;
                    }

                    if (await ChannelSession.Settings.GiveawayRequirements.DoesMeetUserRoleRequirement(e.User))
                    {
                        if (ChannelSession.Settings.GiveawayRequirements.Rank != null && ChannelSession.Settings.GiveawayRequirements.Rank.GetCurrency() != null)
                        {
                            if (!ChannelSession.Settings.GiveawayRequirements.DoesMeetRankRequirement(e.User))
                            {
                                await ChannelSession.Settings.GiveawayRequirements.Rank.SendRankNotMetWhisper(e.User);
                                return;
                            }
                        }

                        if (ChannelSession.Settings.GiveawayRequirements.Currency != null && ChannelSession.Settings.GiveawayRequirements.Currency.GetCurrency() != null)
                        {
                            if (!ChannelSession.Settings.GiveawayRequirements.TrySubtractCurrencyAmount(e.User))
                            {
                                await ChannelSession.Settings.GiveawayRequirements.Currency.SendCurrencyNotMetWhisper(e.User);
                                return;
                            }
                        }

                        await ChannelSession.Chat.Whisper(e.User.UserName, "You have been entered into the giveaway, stay tuned to see who wins!");

                        enteredUsersDictionary[e.User.ID] = new GiveawayUser() { User = e.User, Entries = 1 };

                        await this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            this.enteredUsers.Add(enteredUsersDictionary[e.User.ID]);
                        }));
                    }
                    else
                    {
                        await ChannelSession.Chat.Whisper(e.User.UserName, string.Format("You are not able to enter this giveaway as it is only for {0}s", ChannelSession.Settings.GiveawayRequirements.Role.RoleNameString));
                    }
                }
                else if (this.selectedWinner != null && e.CommandName.Equals("!claim") && this.selectedWinner.Equals(e.User))
                {
                    await ChannelSession.Chat.SendMessage(string.Format("@{0} has claimed their prize! Listen closely to the streamer for instructions on getting your prize.", e.User.UserName));
                    await this.EndGiveaway();
                }
            }
        }

        private async void GlobalEvents_OnDonationOccurred(object sender, UserDonationModel e)
        {
            if (this.giveawayEnabled && this.selectedWinner == null)
            {
                if  ((ChannelSession.Settings.GiveawayGawkBoxTrigger && e.Source == UserDonationSourceEnum.GawkBox) ||
                    (ChannelSession.Settings.GiveawayStreamlabsTrigger && e.Source == UserDonationSourceEnum.Streamlabs) ||
                    (ChannelSession.Settings.GiveawayTiltifyTrigger && e.Source == UserDonationSourceEnum.Tiltify))
                {
                    UserModel userModel = await ChannelSession.Connection.GetUser(e.UserName);
                    if (userModel != null)
                    {
                        UserViewModel user = new UserViewModel(userModel);

                        if (!this.enteredUsersDictionary.ContainsKey(user.ID))
                        {
                            this.enteredUsersDictionary[user.ID] = new GiveawayUser() { User = user, Entries = 0 };
                        }
                        this.enteredUsersDictionary[user.ID].DonationAmount += e.Amount;

                        int newEntryAmount = 0;
                        if (ChannelSession.Settings.GiveawayDonationAmount > 0.0)
                        {
                            if (ChannelSession.Settings.GiveawayDonationRequiredAmount && this.enteredUsersDictionary[user.ID].DonationAmount >= ChannelSession.Settings.GiveawayDonationAmount)
                            {
                                newEntryAmount = 1;
                            }
                            else
                            {
                                newEntryAmount = (int)(this.enteredUsersDictionary[user.ID].DonationAmount / ChannelSession.Settings.GiveawayDonationAmount);
                            }
                        }
                        else
                        {
                            newEntryAmount = 1;
                        }

                        if (newEntryAmount > this.enteredUsersDictionary[user.ID].Entries)
                        {
                            await ChannelSession.Chat.Whisper(user.UserName, "You've gotten an entry into the giveaway, stay tuned to see who wins!");
                        }
                        this.enteredUsersDictionary[user.ID].Entries = newEntryAmount;

                        await this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            this.enteredUsers.Clear();
                            foreach (GiveawayUser gUser in this.enteredUsersDictionary.Values)
                            {
                                this.enteredUsers.Add(gUser);
                            }
                        }));
                    }
                }
            }
        }

        private string GetEntryInstructions()
        {
            string entryInstructions = string.Empty;
            if (!string.IsNullOrEmpty(ChannelSession.Settings.GiveawayCommand))
            {
                entryInstructions = string.Format("Type \"!{0}\" in chat to enter", ChannelSession.Settings.GiveawayCommand);
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

                string requiredAmount = string.Empty;
                if (ChannelSession.Settings.GiveawayDonationAmount > 0.0)
                {
                    if (ChannelSession.Settings.GiveawayDonationRequiredAmount)
                    {
                        entryInstructions = string.Format("All tips over ${0} through {1} get an entry to win", ChannelSession.Settings.GiveawayDonationRequiredAmount, service);
                    }
                    else
                    {
                        entryInstructions = string.Format("Every ${0} in tip(s) through {1} get an entry to win", ChannelSession.Settings.GiveawayDonationRequiredAmount, service);
                    }
                }
                else
                {
                    entryInstructions = string.Format("Any tips through {0} get an entry to win", service);
                }
            }
            return entryInstructions;
        }
    }
}