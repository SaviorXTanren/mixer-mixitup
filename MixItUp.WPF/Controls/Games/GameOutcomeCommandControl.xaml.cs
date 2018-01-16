using MixItUp.Base.Commands;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Controls.MainControls;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Command;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Games
{
    /// <summary>
    /// Interaction logic for GameOutcomeCommandControl.xaml
    /// </summary>
    public partial class GameOutcomeCommandControl : MainControlBase
    {
        private GameCommandWindow gameWindow;
        private CustomCommand command;

        public GameOutcomeCommandControl(GameOutcome outcome)
            : this()
        {
            this.command = outcome.ResultCommand;
            this.OutcomeNameTextBox.Text = outcome.Name;
            this.OutcomeNameTextBox.IsEnabled = false;
        }

        public GameOutcomeCommandControl()
        {
            InitializeComponent();
        }

        public async Task Initialize(GameCommandWindow window)
        {
            this.gameWindow = window;
            await base.Initialize(window);

            this.RefreshControl();
        }

        public async Task<GameOutcome> GetOutcome()
        {
            if (string.IsNullOrEmpty(this.OutcomeNameTextBox.Text))
            {
                await MessageBoxHelper.ShowMessageDialog("An outcome is missing a name");
                return null;
            }
            return new GameOutcome(this.OutcomeNameTextBox.Text, this.command);
        }

        private void NewCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommandWindow window = new CommandWindow(new CustomCommandDetailsControl(new CustomCommand("Outcome Game Command")));
            window.CommandSaveSuccessfully += Window_CommandSaveSuccessfully;
            window.Show();
        }

        private void CommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandButtonsControl commandButtonControl = (CommandButtonsControl)sender;
            CustomCommand command = commandButtonControl.GetCommandFromCommandButtons<CustomCommand>(sender);
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
