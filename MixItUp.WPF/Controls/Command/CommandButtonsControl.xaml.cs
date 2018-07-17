using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.WPF.Controls.MainControls;
using MixItUp.WPF.Util;
using System.Collections.Generic;
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
                else if (commandButtonsControl.DataContext is EventCommandItem)
                {
                    EventCommandItem commandItem = (EventCommandItem)commandButtonsControl.DataContext;
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
                this.EnableDisableToggleSwitch.IsChecked = command.IsEnabled;
                if (!command.IsEditable)
                {
                    this.EditButton.IsEnabled = false;
                }

                if (this.RemoveEditingButton)
                {
                    this.EditButton.Visibility = Visibility.Collapsed;
                }

                if (this.RemoveDeleteButton)
                {
                    this.DeleteButton.Visibility = Visibility.Collapsed;
                }

                if (this.RemoveEnableDisableToggle)
                {
                    this.EnableDisableToggleSwitch.Visibility = Visibility.Collapsed;
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
            if (command != null)
            {
                Dictionary<string, string> extraSpecialIdentifiers = new Dictionary<string, string>();
                if (command is EventCommand)
                {
                    EventCommand eventCommand = command as EventCommand;
                    switch (eventCommand.EventType)
                    {
                        case Mixer.Base.Clients.ConstellationEventTypeEnum.channel__id__hosted:
                            extraSpecialIdentifiers["hostviewercount"] = "123";
                            break;
                    }

                    switch (eventCommand.OtherEventType)
                    {
                        case OtherEventTypeEnum.GameWispSubscribed:
                        case OtherEventTypeEnum.GameWispResubscribed:
                            extraSpecialIdentifiers["subscribemonths"] = "999";
                            extraSpecialIdentifiers["subscribetier"] = "Test Tier";
                            extraSpecialIdentifiers["subscribeamount"] = "$12.34";
                            break;
                        case OtherEventTypeEnum.StreamlabsDonation:
                        case OtherEventTypeEnum.GawkBoxDonation:
                        case OtherEventTypeEnum.TiltifyDonation:
                            extraSpecialIdentifiers["donationsource"] = "Test Source";
                            extraSpecialIdentifiers["donationamount"] = "$12.34";
                            extraSpecialIdentifiers["donationmessage"] = "Test donation message.";
                            extraSpecialIdentifiers["donationimage"] = ChannelSession.GetCurrentUser().AvatarLink;
                            break;
                    }
                }

                await command.PerformAndWait(ChannelSession.GetCurrentUser(), new List<string>() { "@" + ChannelSession.GetCurrentUser().UserName }, extraSpecialIdentifiers);
                if (command is PermissionsCommandBase)
                {
                    PermissionsCommandBase permissionCommand = (PermissionsCommandBase)command;
                    permissionCommand.ResetCooldown(ChannelSession.GetCurrentUser());
                }
                this.SwitchToPlay();
            }

            this.RaiseEvent(new RoutedEventArgs(CommandButtonsControl.PlayClickedEvent, this));
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
            if (await MessageBoxHelper.ShowConfirmationDialog("Are you sure you want to delete this command?"))
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
