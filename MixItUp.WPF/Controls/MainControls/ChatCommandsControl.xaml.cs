using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.WPF.Controls.Chat;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Windows.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for ChatCommandsControl.xaml
    /// </summary>
    public partial class ChatCommandsControl : MainControlBase
    {
        private ObservableCollection<ChatCommand> customChatCommands = new ObservableCollection<ChatCommand>();

        private DataGridColumn lastSortedColumn = null;

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
            this.CustomCommandsListView.Sorted += CustomCommandsListView_Sorted;

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

        private void CustomCommandsListView_Sorted(object sender, DataGridColumn column)
        {
            this.RefreshList(column);
        }

        protected override Task OnVisibilityChanged()
        {
            this.RefreshList();
            return Task.FromResult(0);
        }

        private void RefreshList(DataGridColumn sortColumn = null)
        {
            string filter = this.CommandNameFilterTextBox.Text;
            if (!string.IsNullOrEmpty(filter))
            {
                filter = filter.ToLower();
            }

            this.CustomCommandsListView.SelectedIndex = -1;

            this.customChatCommands.Clear();

            IEnumerable<ChatCommand> data = ChannelSession.Settings.ChatCommands.ToList();
            data = data.OrderBy(c => c.Name);
            if (sortColumn != null)
            {
                int columnIndex = this.CustomCommandsListView.Columns.IndexOf(sortColumn);
                if (columnIndex == 0) { data = data.OrderBy(u => u.Name); }
                if (columnIndex == 1) { data = data.OrderBy(u => u.CommandsString); }
                if (columnIndex == 2) { data = data.OrderBy(u => u.UserRoleRequirementString); }
                if (columnIndex == 3) { data = data.OrderBy(u => u.Requirements.Cooldown.CooldownAmount); }

                if (sortColumn.SortDirection.GetValueOrDefault() == ListSortDirection.Descending)
                {
                    data = data.Reverse();
                }
                lastSortedColumn = sortColumn;
            }

            foreach (var commandData in data)
            {
                if (string.IsNullOrEmpty(filter) || commandData.Name.ToLower().Contains(filter))
                {
                    this.customChatCommands.Add(commandData);
                }
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
    }
}
