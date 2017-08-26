using MixItUp.Base;
using MixItUp.Base.Commands;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using Mixer.Base.Model.Interactive;
using MixItUp.WPF.Util;
using System.Collections.Generic;
using System.Linq;
using Mixer.Base.ViewModel;
using MixItUp.WPF.Windows.Chat;

namespace MixItUp.WPF.Controls.Commands
{
    /// <summary>
    /// Interaction logic for CommandsControl.xaml
    /// </summary>
    public partial class CommandsControl : MainControlBase
    {
        private ObservableCollection<ChatCommand> chatCommands = new ObservableCollection<ChatCommand>();

        public CommandsControl()
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
                await command.Perform();
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

        private void CommandDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            ChatCommand command = (ChatCommand)button.DataContext;
            ChannelSession.Settings.ChatCommands.Remove(command);

            this.CommandsListView.SelectedIndex = -1;

            this.RefreshList();
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
            await ChannelSession.Settings.SaveSettings();
        }
    }
}
