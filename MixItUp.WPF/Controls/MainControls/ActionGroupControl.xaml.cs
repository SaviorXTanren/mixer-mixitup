using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Controls.MainControls;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Windows.Command;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for ActionGroupControl.xaml
    /// </summary>
    public partial class ActionGroupControl : MainControlBase
    {
        private ObservableCollection<CommandGroupControlViewModel> actionGroupCommands = new ObservableCollection<CommandGroupControlViewModel>();

        private int nameOrder = 1;

        public ActionGroupControl()
        {
            InitializeComponent();
        }

        protected override Task InitializeInternal()
        {
            this.ActionGroupCommandsItemsControl.ItemsSource = this.actionGroupCommands;

            this.RefreshList();

            return base.InitializeInternal();
        }

        protected override Task OnVisibilityChanged()
        {
            this.RefreshList();
            return Task.FromResult(0);
        }

        private void RefreshList()
        {
            string filter = this.ActionGroupNameFilterTextBox.Text;
            if (!string.IsNullOrEmpty(filter))
            {
                filter = filter.ToLower();
            }

            this.actionGroupCommands.Clear();

            IEnumerable<ActionGroupCommand> commands = ChannelSession.Settings.ActionGroupCommands.ToList();
            foreach (var group in commands.Where(c => string.IsNullOrEmpty(filter) || c.Name.ToLower().Contains(filter)).GroupBy(c => c.GroupName))
            {
                IEnumerable<CommandBase> cmds = (nameOrder > 0) ? group.OrderBy(c => c.Name) : group.OrderByDescending(c => c.Name);
                this.actionGroupCommands.Add(new CommandGroupControlViewModel(cmds));
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

        private void GroupCommandsToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            this.RefreshList();
        }

        private void Name_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            nameOrder *= -1;
            this.NameSortingIcon.Visibility = Visibility.Collapsed;
            if (nameOrder == 1)
            {
                this.NameSortingIcon.Visibility = Visibility.Visible;
                this.NameSortingIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.ArrowDown;
            }
            else if (nameOrder == -1)
            {
                this.NameSortingIcon.Visibility = Visibility.Visible;
                this.NameSortingIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.ArrowUp;
            }
            this.RefreshList();
        }
    }
}
