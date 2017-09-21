using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Windows.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MixItUp.WPF.Controls.Chat
{
    /// <summary>
    /// Interaction logic for ChatCommandsControl.xaml
    /// </summary>
    public partial class ChatCommandsControl : MainControlBase
    {
        private ObservableCollection<ChatCommand> customChatCommands = new ObservableCollection<ChatCommand>();

        public ChatCommandsControl()
        {
            InitializeComponent();
        }

        protected override Task InitializeInternal()
        {
            this.CustomCommandsListView.ItemsSource = this.customChatCommands;

            this.FollowAgeChatCommand.Initialize(this.Window, ChannelSession.PreMadeChatCommands.First(c => c.Name.Equals("Follow Age")));
            this.GameChatCommand.Initialize(this.Window, ChannelSession.PreMadeChatCommands.First(c => c.Name.Equals("Game")));
            this.GiveawayChatCommand.Initialize(this.Window, ChannelSession.PreMadeChatCommands.First(c => c.Name.Equals("Giveaway")));
            this.MixerAgeChatCommand.Initialize(this.Window, ChannelSession.PreMadeChatCommands.First(c => c.Name.Equals("Mixer Age")));
            this.PurgeChatCommand.Initialize(this.Window, ChannelSession.PreMadeChatCommands.First(c => c.Name.Equals("Purge")));
            this.QuoteChatCommand.Initialize(this.Window, ChannelSession.PreMadeChatCommands.First(c => c.Name.Equals("Quote")));
            this.SparksChatCommand.Initialize(this.Window, ChannelSession.PreMadeChatCommands.First(c => c.Name.Equals("Sparks")));
            this.StreamerAgeChatCommand.Initialize(this.Window, ChannelSession.PreMadeChatCommands.First(c => c.Name.Equals("Streamer Age")));
            this.TimeoutChatCommand.Initialize(this.Window, ChannelSession.PreMadeChatCommands.First(c => c.Name.Equals("Timeout")));
            this.TitleChatCommand.Initialize(this.Window, ChannelSession.PreMadeChatCommands.First(c => c.Name.Equals("Title")));
            this.UptimeChatCommand.Initialize(this.Window, ChannelSession.PreMadeChatCommands.First(c => c.Name.Equals("Uptime")));

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

        private async void TestButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            ChatCommand command = (ChatCommand)button.DataContext;

            await this.Window.RunAsyncOperation(async () =>
            {
                await command.Perform(ChannelSession.GetCurrentUser(), new List<string>() { "@" + ChannelSession.GetCurrentUser().UserName });
            });
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            ChatCommand command = (ChatCommand)button.DataContext;

            CommandWindow window = new CommandWindow(new ChatCommandDetailsControl(command));
            window.Closed += Window_Closed;
            window.Show();
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            ChatCommand command = (ChatCommand)button.DataContext;
            ChannelSession.Settings.ChatCommands.Remove(command);

            await this.Window.RunAsyncOperation(async () => { await ChannelSession.SaveSettings(); });

            this.CustomCommandsListView.SelectedIndex = -1;

            this.RefreshList();
        }

        private void EnableDisableToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            ToggleButton button = (ToggleButton)sender;
            CommandBase command = (CommandBase)button.DataContext;
            command.IsEnabled = true;
        }

        private void EnableDisableToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            ToggleButton button = (ToggleButton)sender;
            CommandBase command = (CommandBase)button.DataContext;
            command.IsEnabled = false;
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
