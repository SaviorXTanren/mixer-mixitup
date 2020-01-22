using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.Chat;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Controls.MainControls;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Controls.MainControls;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Currency;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Command
{
    /// <summary>
    /// Interaction logic for CommandButtonsControl.xaml
    /// </summary>
    public partial class CommandButtonsControl : NotifyPropertyChangedUserControl
    {
        public static readonly DependencyProperty RemoveEditingButtonProperty = DependencyProperty.Register("RemoveEditingButton", typeof(bool), typeof(CommandButtonsControl), new PropertyMetadata(false));
        public static readonly DependencyProperty RemoveDeleteButtonProperty = DependencyProperty.Register("RemoveDeleteButton", typeof(bool), typeof(CommandButtonsControl), new PropertyMetadata(false));
        public static readonly DependencyProperty RemoveEnableDisableToggleProperty = DependencyProperty.Register("RemoveEnableDisableToggle", typeof(bool), typeof(CommandButtonsControl), new PropertyMetadata(false));

        public static readonly RoutedEvent PlayClickedEvent = EventManager.RegisterRoutedEvent("PlayClicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CommandButtonsControl));
        public static readonly RoutedEvent StopClickedEvent = EventManager.RegisterRoutedEvent("StopClicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CommandButtonsControl));
        public static readonly RoutedEvent EditClickedEvent = EventManager.RegisterRoutedEvent("EditClicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CommandButtonsControl));
        public static readonly RoutedEvent DeleteClickedEvent = EventManager.RegisterRoutedEvent("DeleteClicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CommandButtonsControl));
        public static readonly RoutedEvent EnableDisableToggledEvent = EventManager.RegisterRoutedEvent("EnableDisableToggled", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CommandButtonsControl));

        public event RoutedEventHandler PlayClicked { add { this.AddHandler(PlayClickedEvent, value); } remove { this.RemoveHandler(PlayClickedEvent, value); } }
        public event RoutedEventHandler StopClicked { add { this.AddHandler(StopClickedEvent, value); } remove { this.RemoveHandler(StopClickedEvent, value); } }
        public event RoutedEventHandler EditClicked { add { this.AddHandler(EditClickedEvent, value); } remove { this.RemoveHandler(EditClickedEvent, value); } }
        public event RoutedEventHandler DeleteClicked { add { this.AddHandler(DeleteClickedEvent, value); } remove { this.RemoveHandler(DeleteClickedEvent, value); } }
        public event RoutedEventHandler EnableDisableToggled { add { this.AddHandler(EnableDisableToggledEvent, value); } remove { this.RemoveHandler(EnableDisableToggledEvent, value); } }

        public CommandButtonsControl()
        {
            InitializeComponent();

            this.Loaded += CommandButtonsControl_Loaded;
            this.DataContextChanged += CommandButtonsControl_DataContextChanged;
        }

        private void CommandButtonsControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            RefreshUI();
        }

        public bool RemoveEditingButton
        {
            get { return (bool)GetValue(RemoveEditingButtonProperty); }
            set { SetValue(RemoveEditingButtonProperty, value); }
        }

        public bool RemoveDeleteButton
        {
            get { return (bool)GetValue(RemoveDeleteButtonProperty); }
            set { SetValue(RemoveDeleteButtonProperty, value); }
        }

        public bool RemoveEnableDisableToggle
        {
            get { return (bool)GetValue(RemoveEnableDisableToggleProperty); }
            set { SetValue(RemoveEnableDisableToggleProperty, value); }
        }

        public T GetCommandFromCommandButtons<T>() where T : CommandBase { return this.GetCommandFromCommandButtons<T>(this); }

        public T GetCommandFromCommandButtons<T>(object sender) where T : CommandBase
        {
            CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
            if (commandButtonsControl.DataContext != null)
            {
                if (commandButtonsControl.DataContext is CommandBase)
                {
                    return (T)commandButtonsControl.DataContext;
                }
                else if (commandButtonsControl.DataContext is EventCommandItemViewModel)
                {
                    EventCommandItemViewModel commandItem = (EventCommandItemViewModel)commandButtonsControl.DataContext;
                    return (T)((CommandBase)commandItem.Command);
                }
            }
            return null;
        }

        public void SwitchToPlay()
        {
            this.PlayButton.Visibility = Visibility.Visible;
            this.StopButton.Visibility = Visibility.Collapsed;

            this.EditButton.IsEnabled = true;
            this.DeleteButton.IsEnabled = true;
            this.EnableDisableToggleSwitch.IsEnabled = true;
        }

        private void CommandButtonsControl_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshUI();
        }

        private void RefreshUI()
        {
            CommandBase command = this.GetCommandFromCommandButtons<CommandBase>(this);
            if (command != null)
            {
                if (this.EditButton != null)
                {
                    if (!command.IsEditable)
                    {
                        this.EditButton.IsEnabled = false;
                    }

                    if (this.RemoveEditingButton)
                    {
                        this.EditButton.Visibility = Visibility.Collapsed;
                    }
                }

                if (this.DeleteButton != null)
                {
                    if (this.RemoveDeleteButton)
                    {
                        this.DeleteButton.Visibility = Visibility.Collapsed;
                    }
                }

                if (this.EnableDisableToggleSwitch != null)
                {
                    this.EnableDisableToggleSwitch.IsChecked = command.IsEnabled;

                    if (this.RemoveEnableDisableToggle)
                    {
                        this.EnableDisableToggleSwitch.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            this.PlayButton.Visibility = Visibility.Collapsed;
            this.StopButton.Visibility = Visibility.Visible;

            this.EditButton.IsEnabled = false;
            this.DeleteButton.IsEnabled = false;
            this.EnableDisableToggleSwitch.IsEnabled = false;

            CommandBase command = this.GetCommandFromCommandButtons<CommandBase>(this);
            await CommandButtonsControl.TestCommand(command);
            this.SwitchToPlay();

            this.RaiseEvent(new RoutedEventArgs(CommandButtonsControl.PlayClickedEvent, this));
        }

        public static async Task TestCommand(CommandBase command)
        {
            if (command != null)
            {
                UserViewModel currentUser = await ChannelSession.GetCurrentUser();

                Dictionary<string, string> extraSpecialIdentifiers = new Dictionary<string, string>();
                if (command is EventCommand)
                {
                    EventCommand eventCommand = command as EventCommand;
                    switch (eventCommand.EventCommandType)
                    {
                        case EventTypeEnum.MixerChannelHosted:
                            extraSpecialIdentifiers["hostviewercount"] = "123";
                            break;
                        case EventTypeEnum.MixerChannelResubscribed:
                            extraSpecialIdentifiers["usersubmonths"] = "5";
                            break;
                        case EventTypeEnum.MixerFanProgressionLevelUp:
                            extraSpecialIdentifiers["userfanprogressionnext"] = "200";
                            extraSpecialIdentifiers["userfanprogressionrank"] = "10";
                            extraSpecialIdentifiers["userfanprogressioncolor"] = "#c642ea";
                            extraSpecialIdentifiers["userfanprogressionimage"] = "https://static.mixer.com/img/design/ui/fan-progression/v1_badges/purple/large.gif";
                            extraSpecialIdentifiers["userfanprogression"] = "100";
                            break;
                        case EventTypeEnum.StreamlabsDonation:
                        case EventTypeEnum.GawkBoxDonation:
                        case EventTypeEnum.TiltifyDonation:
                        case EventTypeEnum.ExtraLifeDonation:
                        case EventTypeEnum.TipeeeStreamDonation:
                        case EventTypeEnum.TreatStreamDonation:
                        case EventTypeEnum.StreamJarDonation:
                        case EventTypeEnum.JustGivingDonation:
                            UserDonationModel donation = new UserDonationModel()
                            {
                                Amount = 12.34,
                                Message = "Test donation message",
                                ImageLink = currentUser.AvatarLink
                            };

                            switch (eventCommand.EventCommandType)
                            {
                                case EventTypeEnum.StreamlabsDonation: donation.Source = UserDonationSourceEnum.Streamlabs; break;
                                case EventTypeEnum.GawkBoxDonation: donation.Source = UserDonationSourceEnum.GawkBox; break;
                                case EventTypeEnum.TiltifyDonation: donation.Source = UserDonationSourceEnum.Tiltify; break;
                                case EventTypeEnum.ExtraLifeDonation: donation.Source = UserDonationSourceEnum.ExtraLife; break;
                                case EventTypeEnum.TipeeeStreamDonation: donation.Source = UserDonationSourceEnum.TipeeeStream; break;
                                case EventTypeEnum.TreatStreamDonation: donation.Source = UserDonationSourceEnum.TreatStream; break;
                                case EventTypeEnum.StreamJarDonation: donation.Source = UserDonationSourceEnum.StreamJar; break;
                                case EventTypeEnum.JustGivingDonation: donation.Source = UserDonationSourceEnum.JustGiving; break;
                            }

                            foreach (var kvp in donation.GetSpecialIdentifiers())
                            {
                                extraSpecialIdentifiers[kvp.Key] = kvp.Value;
                            }
                            extraSpecialIdentifiers["donationtype"] = "Pizza";
                            break;
                        case EventTypeEnum.PatreonSubscribed:
                            extraSpecialIdentifiers[SpecialIdentifierStringBuilder.PatreonTierNameSpecialIdentifier] = "Super Tier";
                            extraSpecialIdentifiers[SpecialIdentifierStringBuilder.PatreonTierAmountSpecialIdentifier] = "12.34";
                            extraSpecialIdentifiers[SpecialIdentifierStringBuilder.PatreonTierImageSpecialIdentifier] = "https://xforgeassets002.xboxlive.com/xuid-2535473787585366-public/b7a1d715-3a9e-4bdd-a030-32f9e2e0f51e/0013_lots-o-stars_256.png";
                            break;
                        case EventTypeEnum.StreamlootsCardRedeemed:
                            extraSpecialIdentifiers["streamlootscardname"] = "Test Card";
                            extraSpecialIdentifiers["streamlootscardimage"] = "https://res.cloudinary.com/streamloots/image/upload/f_auto,c_scale,w_250,q_90/static/e19c7bf6-ca3e-49a8-807e-b2e9a1a47524/en_dl_character.png";
                            extraSpecialIdentifiers["streamlootscardvideo"] = "https://cdn.streamloots.com/uploads/5c645b78666f31002f2979d1/3a6bf1dc-7d61-4f93-be0a-f5dc1d0d33b6.webm";
                            extraSpecialIdentifiers["streamlootscardsound"] = "https://static.streamloots.com/b355d1ef-d931-4c16-a48f-8bed0076401b/alerts/default.mp3";
                            extraSpecialIdentifiers["streamlootsmessage"] = "Test Message";
                            break;
                        case EventTypeEnum.StreamlootsPackPurchased:
                        case EventTypeEnum.StreamlootsPackGifted:
                            extraSpecialIdentifiers["streamlootspurchasequantity"] = "1";
                            break;
                        case EventTypeEnum.MixerSkillUsed:
                            extraSpecialIdentifiers["skillname"] = "Lots of stars";
                            extraSpecialIdentifiers["skilltype"] = EnumHelper.GetEnumName(MixerSkillTypeEnum.Sticker);
                            extraSpecialIdentifiers["skillcosttype"] = "Embers";
                            extraSpecialIdentifiers["skillcost"] = "50";
                            extraSpecialIdentifiers["skillimage"] = "https://xforgeassets002.xboxlive.com/xuid-2535473787585366-public/b7a1d715-3a9e-4bdd-a030-32f9e2e0f51e/0013_lots-o-stars_256.png";
                            extraSpecialIdentifiers["skillissparks"] = false.ToString();
                            extraSpecialIdentifiers["skillisembers"] = true.ToString();
                            extraSpecialIdentifiers["skillmessage"] = "Hello World!";
                            break;
                        case EventTypeEnum.MixerMilestoneReached:
                            extraSpecialIdentifiers["milestoneamount"] = "100";
                            extraSpecialIdentifiers["milestoneremainingamount"] = "100";
                            extraSpecialIdentifiers["milestonereward"] = "$10.00";
                            extraSpecialIdentifiers["milestonenextamount"] = "100";
                            extraSpecialIdentifiers["milestonenextremainingamount"] = "100";
                            extraSpecialIdentifiers["milestonenextreward"] = "$10.00";
                            extraSpecialIdentifiers["milestonefinalamount"] = "100";
                            extraSpecialIdentifiers["milestonefinalremainingamount"] = "100";
                            extraSpecialIdentifiers["milestonefinalreward"] = "$10.00";
                            extraSpecialIdentifiers["milestoneearnedamount"] = "100";
                            extraSpecialIdentifiers["milestoneearnedreward"] = "$10.00";
                            break;
                        case EventTypeEnum.MixerSparksUsed:
                            extraSpecialIdentifiers["sparkamount"] = "10";
                            break;
                        case EventTypeEnum.MixerEmbersUsed:
                            extraSpecialIdentifiers["emberamount"] = "10";
                            break;
                    }
                }
                else if (command is MixPlayCommand)
                {
                    MixPlayCommand iCommand = (MixPlayCommand)command;

                    extraSpecialIdentifiers["mixplaycontrolid"] = iCommand.Name;
                    extraSpecialIdentifiers["mixplaycontrolcost"] = "123";
                    extraSpecialIdentifiers["mixplaycontroltext"] = "Button Name";
                }
                else if (command is CustomCommand)
                {
                    if (command.Name.Equals(InventoryWindow.ItemsBoughtCommandName) || command.Name.Equals(InventoryWindow.ItemsSoldCommandName))
                    {
                        extraSpecialIdentifiers["itemtotal"] = "5";
                        extraSpecialIdentifiers["itemname"] = "Chocolate Bars";
                        extraSpecialIdentifiers["itemcost"] = "500";
                        extraSpecialIdentifiers["currencyname"] = "CURRENCY_NAME";
                    }
                    else if (command.Name.Contains("Moderation Strike"))
                    {
                        extraSpecialIdentifiers[ModerationHelper.ModerationReasonSpecialIdentifier] = "Bad Stuff";
                    }
                    else
                    {
                        extraSpecialIdentifiers["queueposition"] = "1";
                    }
                }

                await command.PerformAndWait(currentUser, new List<string>() { "@" + currentUser.Username }, extraSpecialIdentifiers);
                if (command is PermissionsCommandBase)
                {
                    PermissionsCommandBase permissionCommand = (PermissionsCommandBase)command;
                    permissionCommand.ResetCooldown(await ChannelSession.GetCurrentUser());
                }
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            this.SwitchToPlay();

            CommandBase command = this.GetCommandFromCommandButtons<CommandBase>(this);
            if (command != null)
            {
                command.StopCurrentRun();
            }

            this.RaiseEvent(new RoutedEventArgs(CommandButtonsControl.StopClickedEvent, this));
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(CommandButtonsControl.EditClickedEvent, this));
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (await DialogHelper.ShowConfirmation("Are you sure you want to delete this command?"))
            {
                this.RaiseEvent(new RoutedEventArgs(CommandButtonsControl.DeleteClickedEvent, this));
            }
        }

        private void EnableDisableToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            CommandBase command = this.GetCommandFromCommandButtons<CommandBase>(this);
            if (command != null)
            {
                command.IsEnabled = this.EnableDisableToggleSwitch.IsChecked.GetValueOrDefault();
            }

            this.RaiseEvent(new RoutedEventArgs(CommandButtonsControl.EnableDisableToggledEvent, this));
        }
    }
}
