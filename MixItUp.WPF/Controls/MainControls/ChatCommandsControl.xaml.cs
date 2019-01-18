using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Controls.MainControls;
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
    public partial class ChatCommandsControl : MainControlBase
    {
        private ObservableCollection<CommandGroupControlViewModel> customChatCommands = new ObservableCollection<CommandGroupControlViewModel>();

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

            this.CustomCommandsItemsControl.ItemsSource = this.customChatCommands;

            this.RefreshList();

            if (this.customChatCommands.Count > 0)
            {
                this.PreMadeCommandsButton.IsEnabled = true;
                this.CustomCommandsGrid.Visibility = Visibility.Visible;
                this.CommandNameFilterGrid.Visibility = Visibility.Visible;
            }
            else
            {
                this.CustomCommandsButton.IsEnabled = true;
                this.PreMadeCommandsGrid.Visibility = Visibility.Visible;
                this.CommandNameFilterGrid.Visibility = Visibility.Collapsed;
            }

            return base.InitializeInternal();
        }

        protected override Task OnVisibilityChanged()
        {
            this.RefreshList();
            return Task.FromResult(0);
        }

        private void RefreshList()
        {
            string filter = this.CommandNameFilterTextBox.Text;
            if (!string.IsNullOrEmpty(filter))
            {
                filter = filter.ToLower();
            }

            this.customChatCommands.Clear();

            IEnumerable<ChatCommand> commands = ChannelSession.Settings.ChatCommands.ToList();
            foreach (var group in commands.Where(c => string.IsNullOrEmpty(filter) || c.Name.ToLower().Contains(filter)).GroupBy(c => c.GroupName))
            {
                this.customChatCommands.Add(new CommandGroupControlViewModel(group.OrderBy(c => c.Name)));
            }
        }

        private void CommandNameFilterTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            this.RefreshList();
        }

        private void PreMadeCommandsButton_Click(object sender, RoutedEventArgs e)
        {
            this.PreMadeCommandsButton.IsEnabled = false;
            this.CustomCommandsButton.IsEnabled = true;
            this.PreMadeCommandsGrid.Visibility = Visibility.Visible;
            this.CustomCommandsGrid.Visibility = Visibility.Collapsed;
            this.CommandNameFilterGrid.Visibility = Visibility.Collapsed;
        }

        private void CustomCommandsButton_Click(object sender, RoutedEventArgs e)
        {
            this.PreMadeCommandsButton.IsEnabled = true;
            this.CustomCommandsButton.IsEnabled = false;
            this.PreMadeCommandsGrid.Visibility = Visibility.Collapsed;
            this.CustomCommandsGrid.Visibility = Visibility.Visible;
            this.CommandNameFilterGrid.Visibility = Visibility.Visible;
        }

        private void CommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
            ChatCommand command = commandButtonsControl.GetCommandFromCommandButtons<ChatCommand>(sender);
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
                CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
                ChatCommand command = commandButtonsControl.GetCommandFromCommandButtons<ChatCommand>(sender);
                if (command != null)
                {
                    ChannelSession.Settings.ChatCommands.Remove(command);
                    await ChannelSession.SaveSettings();
                    this.RefreshList();
                }
            });
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

        private void GroupCommandsToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            this.RefreshList();
        }
    }
}
