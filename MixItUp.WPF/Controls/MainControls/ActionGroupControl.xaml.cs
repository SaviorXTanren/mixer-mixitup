using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Windows.Command;
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
    /// Interaction logic for ActionGroupControl.xaml
    /// </summary>
    public partial class ActionGroupControl : MainControlBase
    {
        private ObservableCollection<ActionGroupCommand> actionGroupCommands = new ObservableCollection<ActionGroupCommand>();

        private DataGridColumn lastSortedColumn = null;

        public ActionGroupControl()
        {
            InitializeComponent();
        }

        protected override Task InitializeInternal()
        {
            this.ActionGroupCommandsListView.ItemsSource = this.actionGroupCommands;

            this.RefreshList();

            return base.InitializeInternal();
        }

        private void RefreshList(DataGridColumn sortColumn = null)
        {
            string filter = this.ActionGroupNameFilterTextBox.Text;
            if (!string.IsNullOrEmpty(filter))
            {
                filter = filter.ToLower();
            }

            this.ActionGroupCommandsListView.SelectedIndex = -1;

            this.actionGroupCommands.Clear();

            IEnumerable<ActionGroupCommand> data = ChannelSession.Settings.ActionGroupCommands.ToList();
            if (sortColumn != null)
            {
                int columnIndex = this.ActionGroupCommandsListView.Columns.IndexOf(sortColumn);
                if (columnIndex == 0) { data = data.OrderBy(u => u.Name); }

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
                    this.actionGroupCommands.Add(commandData);
                }
            }
        }

        private void ActionGroupNameFilterTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            this.RefreshList();
        }

        private void CommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
            ActionGroupCommand command = commandButtonsControl.GetCommandFromCommandButtons<ActionGroupCommand>(sender);
            if (command != null)
            {
                CommandWindow window = new CommandWindow(new ActionGroupCommandDetailsControl(command));
                window.Closed += Window_Closed;
                window.Show();
            }
        }

        private async void CommandButtons_DeleteClicked(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
                ActionGroupCommand command = commandButtonsControl.GetCommandFromCommandButtons<ActionGroupCommand>(sender);
                if (command != null)
                {
                    ChannelSession.Settings.ActionGroupCommands.Remove(command);
                    await ChannelSession.SaveSettings();
                    this.RefreshList();
                }
            });
        }

        private void AddCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommandWindow window = new CommandWindow(new ActionGroupCommandDetailsControl());
            window.Closed += Window_Closed;
            window.Show();
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            this.RefreshList();
        }
    }
}
