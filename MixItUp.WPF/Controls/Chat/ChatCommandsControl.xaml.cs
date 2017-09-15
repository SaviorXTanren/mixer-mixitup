using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.WPF.Windows.Chat;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Chat
{
    /// <summary>
    /// Interaction logic for ChatCommandsControl.xaml
    /// </summary>
    public partial class ChatCommandsControl : MainControlBase
    {
        private ObservableCollection<ChatCommand> chatCommands = new ObservableCollection<ChatCommand>();

        public ChatCommandsControl()
        {
            InitializeComponent();
        }

        protected override Task InitializeInternal()
        {
            this.CommandsListView.ItemsSource = this.chatCommands;

            this.RefreshList();

            return base.InitializeInternal();
        }

        private void RefreshList()
        {
            this.chatCommands.Clear();
            foreach (ChatCommand command in ChannelSession.Settings.ChatCommands)
            {
                this.chatCommands.Add(command);
            }
        }

        private async void CommandTestButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            ChatCommand command = (ChatCommand)button.DataContext;

            await this.Window.RunAsyncOperation(async () =>
            {
                await command.Perform(ChannelSession.GetCurrentUser(), new List<string>() { "@" + ChannelSession.GetCurrentUser().UserName });
            });
        }

        private void CommandEditButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            ChatCommand command = (ChatCommand)button.DataContext;

            ChatCommandWindow window = new ChatCommandWindow(command);
            window.Closed += Window_Closed;
            window.Show();
        }

        private async void CommandDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            ChatCommand command = (ChatCommand)button.DataContext;
            ChannelSession.Settings.ChatCommands.Remove(command);

            await this.Window.RunAsyncOperation(async () => { await ChannelSession.SaveSettings(); });

            this.CommandsListView.SelectedIndex = -1;

            this.RefreshList();
        }

        private void CommandEnableDisableButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            ChatCommand command = (ChatCommand)button.DataContext;
            command.IsEnabled = !command.IsEnabled;
        }

        private void AddCommandButton_Click(object sender, RoutedEventArgs e)
        {
            ChatCommandWindow window = new ChatCommandWindow();
            window.Closed += Window_Closed;
            window.Show();
        }

        private async void Window_Closed(object sender, System.EventArgs e)
        {
            this.RefreshList();
            await ChannelSession.Settings.Save();
        }
    }
}
