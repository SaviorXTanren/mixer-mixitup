using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Controls.Commands;
using MixItUp.Base.ViewModel.Controls.MainControls;
using MixItUp.Base.ViewModel.Window;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Windows.Command;
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
        private ActionGroupMainControlViewModel viewModel;

        public ActionGroupControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.DataContext = this.viewModel = new ActionGroupMainControlViewModel((MainWindowViewModel)this.Window.ViewModel);
            await this.viewModel.OnLoaded();

            await base.InitializeInternal();
        }

        private void NameFilterTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            this.viewModel.NameFilter = this.NameFilterTextBox.Text;
        }

        private void CommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
            ActionGroupCommand command = commandButtonsControl.GetCommandFromCommandButtons<ActionGroupCommand>(sender);
            if (command != null)
            {
                CommandWindow window = new CommandWindow(new ActionGroupCommandDetailsControl(command));
                window.CommandSaveSuccessfully += Window_CommandSaveSuccessfully;
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
                    this.viewModel.RemoveCommand(command);
                }
            });
        }

        private void AddCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommandWindow window = new CommandWindow(new ActionGroupCommandDetailsControl());
            window.CommandSaveSuccessfully += Window_CommandSaveSuccessfully;
            window.Show();
        }

        private void Window_CommandSaveSuccessfully(object sender, CommandBase e)
        {
            this.viewModel.RemoveCommand((ActionGroupCommand)e);
            this.viewModel.AddCommand((ActionGroupCommand)e);
        }

        private void AccordianGroupBoxControl_Minimized(object sender, RoutedEventArgs e)
        {
            AccordianGroupBoxControl control = (AccordianGroupBoxControl)sender;
            if (control.Content != null)
            {
                FrameworkElement content = (FrameworkElement)control.Content;
                if (content != null)
                {
                    CommandGroupControlViewModel group = (CommandGroupControlViewModel)content.DataContext;
                    if (group != null)
                    {
                        group.IsMinimized = true;
                    }
                }
            }
        }

        private void AccordianGroupBoxControl_Maximized(object sender, RoutedEventArgs e)
        {
            AccordianGroupBoxControl control = (AccordianGroupBoxControl)sender;
            if (control.Content != null)
            {
                FrameworkElement content = (FrameworkElement)control.Content;
                if (content != null)
                {
                    CommandGroupControlViewModel group = (CommandGroupControlViewModel)content.DataContext;
                    if (group != null)
                    {
                        group.IsMinimized = false;
                    }
                }
            }
        }
    }
}
