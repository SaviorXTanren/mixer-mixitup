using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.WPF.Controls.Chat;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Windows.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for ChatCommandsControl.xaml
    /// </summary>
    public partial class ChatCommandsControl : MainCommandControlBase
    {
        private ObservableCollection<ChatCommand> customChatCommands = new ObservableCollection<ChatCommand>();

        public ChatCommandsControl()
        {
            InitializeComponent();
        }

        protected override Task InitializeInternal()
        {
            foreach (PreMadeChatCommand command in ChannelSession.PreMadeChatCommands.OrderBy(c => c.Name))
            {
                PreMadeChatCommandControl control = new PreMadeChatCommandControl();
                control.Margin = new Thickness(0, 5, 0, 5);
                control.Initialize(this.Window, command);
                this.PreMadeChatCommandsStackPanel.Children.Add(control);
            }

            this.CustomCommandsListView.ItemsSource = this.customChatCommands;

            this.RefreshList();

            if (this.customChatCommands.Count > 0)
            {
                this.PreMadeCommandsButton.IsEnabled = true;
                this.CustomCommandsGrid.Visibility = Visibility.Visible;
            }
            else
            {
                this.CustomCommandsButton.IsEnabled = true;
                this.PreMadeCommandsGrid.Visibility = Visibility.Visible;
            }

            return base.InitializeInternal();
        }

        private void RefreshList()
        {
            this.CustomCommandsListView.SelectedIndex = -1;

            this.customChatCommands.Clear();
            foreach (ChatCommand command in ChannelSession.Settings.ChatCommands)
            {
                this.customChatCommands.Add(command);
            }
        }

        private void PreMadeCommandsButton_Click(object sender, RoutedEventArgs e)
        {
            this.PreMadeCommandsButton.IsEnabled = false;
            this.CustomCommandsButton.IsEnabled = true;
            this.PreMadeCommandsGrid.Visibility = Visibility.Visible;
            this.CustomCommandsGrid.Visibility = Visibility.Collapsed;
        }

        private void CustomCommandsButton_Click(object sender, RoutedEventArgs e)
        {
            this.PreMadeCommandsButton.IsEnabled = true;
            this.CustomCommandsButton.IsEnabled = false;
            this.PreMadeCommandsGrid.Visibility = Visibility.Collapsed;
            this.CustomCommandsGrid.Visibility = Visibility.Visible;
        }

        private async void CommandButtons_PlayClicked(object sender, RoutedEventArgs e)
        {
            await this.HandleCommandPlay(sender);
        }

        private void CommandButtons_StopClicked(object sender, RoutedEventArgs e)
        {
            this.HandleCommandStop(sender);
        }

        private void CommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            ChatCommand command = this.GetCommandFromCommandButtons<ChatCommand>(sender);
            if (command != null)
            {
                CommandWindow window = new CommandWindow(new ChatCommandDetailsControl(command));
                window.Closed += Window_Closed;
                window.Show();
            }
        }

        private async void CommandButtons_DeleteClicked(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                ChatCommand command = this.GetCommandFromCommandButtons<ChatCommand>(sender);
                if (command != null)
                {
                    ChannelSession.Settings.ChatCommands.Remove(command);
                    await ChannelSession.SaveSettings();
                    this.RefreshList();
                }
            });
        }

        private void CommandButtons_EnableDisableToggled(object sender, RoutedEventArgs e)
        {
            this.HandleCommandEnableDisable(sender);
        }

        private void AddCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommandWindow window = new CommandWindow(new ChatCommandDetailsControl());
            window.Closed += Window_Closed;
            window.Show();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.RefreshList();
        }
    }
}
