using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.WPF.Windows.Command;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Command
{
    /// <summary>
    /// Interaction logic for CommandButtonsControl.xaml
    /// </summary>
    public partial class CommandButtonsControl : UserControl
    {
        private CommandBase command;

        public CommandButtonsControl()
        {
            this.Loaded += CommandButtonsControl_Loaded;
            InitializeComponent();
        }

        public void Initialize(CommandBase command) { this.DataContext = this.command = command; }

        private void CommandButtonsControl_Loaded(object sender, RoutedEventArgs e) { this.Initialize((CommandBase)this.DataContext); }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.command != null)
            {
                this.PlayButton.Visibility = Visibility.Collapsed;
                this.StopButton.Visibility = Visibility.Visible;

                this.EditButton.IsEnabled = false;
                this.DeleteButton.IsEnabled = false;
                this.EnableDisableToggleSwitch.IsEnabled = false;

                await this.command.PerformAndWait(ChannelSession.GetCurrentUser(), new List<string>() { "@" + ChannelSession.GetCurrentUser().UserName });

                this.StopCommand();
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.command != null)
            {
                this.command.StopCurrentRun();
                this.StopCommand();
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.command != null)
            {
                CommandDetailsControlBase commandDetails = null;
                if (this.command is ChatCommand) { commandDetails = new ChatCommandDetailsControl((ChatCommand)command); }
                if (this.command is InteractiveCommand) { commandDetails = new InteractiveCommandDetailsControl((InteractiveCommand)command); }
                if (this.command is EventCommand) { commandDetails = new EventCommandDetailsControl((EventCommand)command); }
                if (this.command is TimerCommand) { commandDetails = new TimerCommandDetailsControl((TimerCommand)command); }

                CommandWindow window = new CommandWindow(commandDetails);
                window.Show();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.command != null)
            {
                if (this.command is ChatCommand) { ChannelSession.Settings.ChatCommands.Remove((ChatCommand)command); }
                if (this.command is InteractiveCommand) { ChannelSession.Settings.InteractiveCommands.Remove((InteractiveCommand)command); }
                if (this.command is EventCommand) { ChannelSession.Settings.EventCommands.Remove((EventCommand)command); }
                if (this.command is TimerCommand) { ChannelSession.Settings.TimerCommands.Remove((TimerCommand)command); }

                GlobalEvents.CommandUpdated(command);
            }
        }

        private void EnableDisableToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            if (this.command != null)
            {
                this.command.IsEnabled = this.EnableDisableToggleSwitch.IsChecked.GetValueOrDefault();
            }
        }

        private void StopCommand()
        {
            this.PlayButton.Visibility = Visibility.Visible;
            this.StopButton.Visibility = Visibility.Collapsed;

            this.EditButton.IsEnabled = true;
            this.DeleteButton.IsEnabled = true;
            this.EnableDisableToggleSwitch.IsEnabled = true;
        }
    }
}
