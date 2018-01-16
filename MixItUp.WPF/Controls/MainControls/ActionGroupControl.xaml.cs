using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Windows.Command;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for ActionGroupControl.xaml
    /// </summary>
    public partial class ActionGroupControl : MainControlBase
    {
        private ObservableCollection<ActionGroupCommand> actionGroupCommands = new ObservableCollection<ActionGroupCommand>();

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

        private void RefreshList()
        {
            this.ActionGroupCommandsListView.SelectedIndex = -1;

            this.actionGroupCommands.Clear();
            foreach (ActionGroupCommand command in ChannelSession.Settings.ActionGroupCommands)
            {
                this.actionGroupCommands.Add(command);
            }
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
