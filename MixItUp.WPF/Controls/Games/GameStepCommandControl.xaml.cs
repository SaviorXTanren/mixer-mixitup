using MixItUp.Base.Commands;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Controls.MainControls;
using MixItUp.WPF.Windows.Command;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Games
{
    /// <summary>
    /// Interaction logic for GameStepCommandControl.xaml
    /// </summary>
    public partial class GameStepCommandControl : MainCommandControlBase
    {
        private GameCommandWindow window;
        private CustomCommand command;

        public GameStepCommandControl() { InitializeComponent(); }

        public async Task Initialize(GameCommandWindow window, CustomCommand command)
        {
            this.window = window;
            this.command = command;

            await base.Initialize(window);

            this.RefreshControl();
        }

        public CustomCommand GetCommand() { return this.command; }

        private void NewCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommandWindow window = new CommandWindow(new CustomCommandDetailsControl(new CustomCommand("Custom Game Command")));
            window.CommandSaveSuccessfully += Window_CommandSaveSuccessfully;
            window.Show();
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
            CustomCommand command = this.GetCommandFromCommandButtons<CustomCommand>(sender);
            if (command != null)
            {
                CommandWindow window = new CommandWindow(new CustomCommandDetailsControl(command));
                window.CommandSaveSuccessfully += Window_CommandSaveSuccessfully;
                window.Show();
            }
        }

        private void CommandButtons_DeleteClicked(object sender, RoutedEventArgs e)
        {
            this.command = null;
            this.RefreshControl();
        }

        private void CommandButtons_EnableDisableToggled(object sender, RoutedEventArgs e)
        {
            this.HandleCommandEnableDisable(sender);
        }

        private void Window_CommandSaveSuccessfully(object sender, CommandBase e)
        {
            this.CommandButtons.DataContext = this.command = (CustomCommand)e;
            this.RefreshControl();
        }

        private void RefreshControl()
        {
            this.CommandButtons.DataContext = this.command;

            this.NewCommandButton.Visibility = (this.command == null) ? Visibility.Visible : Visibility.Collapsed;
            this.CommandButtons.Visibility = (this.command != null) ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
