using MixItUp.Base;
using MixItUp.Base.Commands;
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
        public static readonly DependencyProperty EditingButtonsEnabledProperty = DependencyProperty.Register("EditingButtonsEnabled", typeof(bool), typeof(CommandButtonsControl), new PropertyMetadata(true));

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
        }

        public T GetCommandFromCommandButtons<T>(object sender) where T : CommandBase
        {
            CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
            if (commandButtonsControl.DataContext != null && commandButtonsControl.DataContext is CommandBase)
            {
                return (T)commandButtonsControl.DataContext;
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

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            this.PlayButton.Visibility = Visibility.Collapsed;
            this.StopButton.Visibility = Visibility.Visible;

            this.EditButton.IsEnabled = false;
            this.DeleteButton.IsEnabled = false;
            this.EnableDisableToggleSwitch.IsEnabled = false;

            if (this.DataContext != null && this.DataContext is CommandBase)
            {
                CommandBase command = (CommandBase)this.DataContext;
                await command.PerformAndWait(ChannelSession.GetCurrentUser(), new List<string>() { "@" + ChannelSession.GetCurrentUser().UserName });
                this.SwitchToPlay();
            }

            this.RaiseEvent(new RoutedEventArgs(CommandButtonsControl.PlayClickedEvent, this));
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            this.SwitchToPlay();

            if (this.DataContext != null && this.DataContext is CommandBase)
            {
                CommandBase command = (CommandBase)this.DataContext;
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
            if (this.DataContext != null && this.DataContext is CommandBase)
            {
                CommandBase command = (CommandBase)this.DataContext;
                command.IsEnabled = this.EnableDisableToggleSwitch.IsChecked.GetValueOrDefault();
            }

            this.RaiseEvent(new RoutedEventArgs(CommandButtonsControl.EnableDisableToggledEvent, this));
        }
    }
}
