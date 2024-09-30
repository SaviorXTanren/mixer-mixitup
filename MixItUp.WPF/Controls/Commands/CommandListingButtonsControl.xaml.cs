using MixItUp.Base;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Commands;
using MixItUp.Base.ViewModel.Currency;
using MixItUp.Base.ViewModel.Games;
using MixItUp.Base.ViewModel.MainControls;
using MixItUp.Base.ViewModel.Overlay;
using System.Windows;

namespace MixItUp.WPF.Controls.Commands
{
    /// <summary>
    /// Interaction logic for CommandListingButtonsControl.xaml
    /// </summary>
    public partial class CommandListingButtonsControl : NotifyPropertyChangedUserControl
    {
        public static readonly DependencyProperty HideEditingButtonProperty = DependencyProperty.Register("HideEditingButton", typeof(bool), typeof(CommandListingButtonsControl), new PropertyMetadata(false));
        public static readonly DependencyProperty HideDeleteButtonProperty = DependencyProperty.Register("HideDeleteButton", typeof(bool), typeof(CommandListingButtonsControl), new PropertyMetadata(false));
        public static readonly DependencyProperty HideEnableDisableToggleProperty = DependencyProperty.Register("HideEnableDisableToggle", typeof(bool), typeof(CommandListingButtonsControl), new PropertyMetadata(false));

        public static readonly RoutedEvent PlayClickedEvent = EventManager.RegisterRoutedEvent("PlayClicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CommandListingButtonsControl));
        public static readonly RoutedEvent StopClickedEvent = EventManager.RegisterRoutedEvent("StopClicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CommandListingButtonsControl));
        public static readonly RoutedEvent EditClickedEvent = EventManager.RegisterRoutedEvent("EditClicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CommandListingButtonsControl));
        public static readonly RoutedEvent DeleteClickedEvent = EventManager.RegisterRoutedEvent("DeleteClicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CommandListingButtonsControl));
        public static readonly RoutedEvent EnableDisableToggledEvent = EventManager.RegisterRoutedEvent("EnableDisableToggled", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CommandListingButtonsControl));

        public event RoutedEventHandler PlayClicked { add { this.AddHandler(PlayClickedEvent, value); } remove { this.RemoveHandler(PlayClickedEvent, value); } }
        public event RoutedEventHandler StopClicked { add { this.AddHandler(StopClickedEvent, value); } remove { this.RemoveHandler(StopClickedEvent, value); } }
        public event RoutedEventHandler EditClicked { add { this.AddHandler(EditClickedEvent, value); } remove { this.RemoveHandler(EditClickedEvent, value); } }
        public event RoutedEventHandler DeleteClicked { add { this.AddHandler(DeleteClickedEvent, value); } remove { this.RemoveHandler(DeleteClickedEvent, value); } }
        public event RoutedEventHandler EnableDisableToggled { add { this.AddHandler(EnableDisableToggledEvent, value); } remove { this.RemoveHandler(EnableDisableToggledEvent, value); } }

        public static T GetCommandFromCommandButtons<T>(object sender) where T : CommandModelBase
        {
            CommandListingButtonsControl commandListingButtonsControl = (CommandListingButtonsControl)sender;
            if (commandListingButtonsControl != null && commandListingButtonsControl.DataContext != null)
            {
                if (commandListingButtonsControl.DataContext is CommandModelBase)
                {
                    return (T)commandListingButtonsControl.DataContext;
                }
                else if (commandListingButtonsControl.DataContext is EventCommandItemViewModel)
                {
                    EventCommandItemViewModel commandItem = (EventCommandItemViewModel)commandListingButtonsControl.DataContext;
                    return (T)(CommandModelBase)commandItem.Command;
                }
                else if (commandListingButtonsControl.DataContext is StreamPassCustomLevelUpCommandViewModel)
                {
                    StreamPassCustomLevelUpCommandViewModel commandItem = (StreamPassCustomLevelUpCommandViewModel)commandListingButtonsControl.DataContext;
                    return (T)commandItem.Command;
                }
                else if (commandListingButtonsControl.DataContext is RedemptionStoreProductViewModel)
                {
                    RedemptionStoreProductViewModel commandItem = (RedemptionStoreProductViewModel)commandListingButtonsControl.DataContext;
                    return (T)commandItem.Command;
                }
                else if (commandListingButtonsControl.DataContext is GameOutcomeViewModel)
                {
                    GameOutcomeViewModel commandItem = (GameOutcomeViewModel)commandListingButtonsControl.DataContext;
                    return (T)(CommandModelBase)commandItem.Command;
                }
                else if (commandListingButtonsControl.DataContext is OverlayWheelOutcomeV3ViewModel)
                {
                    OverlayWheelOutcomeV3ViewModel commandItem = (OverlayWheelOutcomeV3ViewModel)commandListingButtonsControl.DataContext;
                    return (T)(CommandModelBase)commandItem.Command;
                }
                else if (commandListingButtonsControl.DataContext is WebhookCommandItemViewModel)
                {
                    WebhookCommandItemViewModel commandItem = (WebhookCommandItemViewModel)commandListingButtonsControl.DataContext;
                    return (T)(CommandModelBase)commandItem.Command;
                }
            }
            return null;
        }

        public CommandListingButtonsControl()
        {
            InitializeComponent();

            this.Loaded += CommandListingButtonsControl_Loaded;
            this.DataContextChanged += CommandListingButtonsControl_DataContextChanged;
        }

        public bool HideEditingButton
        {
            get { return (bool)GetValue(HideEditingButtonProperty); }
            set { SetValue(HideEditingButtonProperty, value); }
        }

        public bool HideDeleteButton
        {
            get { return (bool)GetValue(HideDeleteButtonProperty); }
            set { SetValue(HideDeleteButtonProperty, value); }
        }

        public bool HideEnableDisableToggle
        {
            get { return (bool)GetValue(HideEnableDisableToggleProperty); }
            set { SetValue(HideEnableDisableToggleProperty, value); }
        }

        public CommandModelBase GetCommandFromCommandButtons() { return CommandListingButtonsControl.GetCommandFromCommandButtons<CommandModelBase>(this); }

        public T GetCommandFromCommandButtons<T>() where T : CommandModelBase { return CommandListingButtonsControl.GetCommandFromCommandButtons<T>(this); }

        public void RefreshUI()
        {
            if (this.EditButton != null && this.HideEditingButton)
            {
                this.EditButton.Visibility = Visibility.Collapsed;
            }

            if (this.DeleteButton != null && this.HideDeleteButton)
            {
                this.DeleteButton.Visibility = Visibility.Collapsed;
            }

            if (this.EnableDisableToggleSwitch != null && this.HideEnableDisableToggle)
            {
                this.EnableDisableToggleSwitch.Visibility = Visibility.Collapsed;
            }

            CommandModelBase command = this.GetCommandFromCommandButtons();
            if (command != null)
            {
                if (this.EnableDisableToggleSwitch != null)
                {
                    this.EnableDisableToggleSwitch.IsChecked = command.IsEnabled;
                }
            }
            else
            {
                if (this.EnableDisableToggleSwitch != null)
                {
                    this.EnableDisableToggleSwitch.IsChecked = true;
                }
            }
        }

        private void CommandListingButtonsControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.RefreshUI();
        }

        private void CommandListingButtonsControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.RefreshUI();
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(CommandListingButtonsControl.PlayClickedEvent, this));

            CommandModelBase command = this.GetCommandFromCommandButtons();
            if (command != null)
            {
                await CommandEditorWindowViewModelBase.TestCommandWithTestCommandParameters(command);
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(CommandListingButtonsControl.StopClickedEvent, this));
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(CommandListingButtonsControl.EditClickedEvent, this));
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (await DialogHelper.ShowConfirmation(MixItUp.Base.Resources.ConfirmDeleteCommand))
            {
                this.RaiseEvent(new RoutedEventArgs(CommandListingButtonsControl.DeleteClickedEvent, this));
            }
        }

        private void EnableDisableToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(CommandListingButtonsControl.EnableDisableToggledEvent, this));

            CommandModelBase command = this.GetCommandFromCommandButtons();
            if (command != null)
            {
                command.IsEnabled = this.EnableDisableToggleSwitch.IsChecked.GetValueOrDefault();
                ChannelSession.Settings.Commands.ManualValueChanged(command.ID);

                if (command is ChatCommandModel)
                {
                    ServiceManager.Get<ChatService>().RebuildCommandTriggers();
                }
            }
        }
    }
}
