using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.WPF.Windows.Command;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for GamesControl.xaml
    /// </summary>
    public partial class GamesControl : MainCommandControlBase
    {
        private ObservableCollection<GameCommand> gameCommands = new ObservableCollection<GameCommand>();

        public GamesControl()
        {
            InitializeComponent();
        }

        protected override Task InitializeInternal()
        {
            this.GameCommandsListView.ItemsSource = this.gameCommands;

            this.RefreshList();

            return base.InitializeInternal();
        }

        private void RefreshList()
        {
            this.GameCommandsListView.SelectedIndex = -1;

            this.gameCommands.Clear();
            foreach (GameCommand command in ChannelSession.Settings.GameCommands)
            {
                this.gameCommands.Add(command);
            }
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
            GameCommand command = this.GetCommandFromCommandButtons<GameCommand>(sender);
            if (command != null)
            {
                GameCommandWindow window = new GameCommandWindow(command);
                window.Closed += Window_Closed;
                window.Show();
            }
        }

        private async void CommandButtons_DeleteClicked(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                GameCommand command = this.GetCommandFromCommandButtons<GameCommand>(sender);
                if (command != null)
                {
                    ChannelSession.Settings.GameCommands.Remove(command);
                    await ChannelSession.SaveSettings();
                    this.RefreshList();
                }
            });
        }

        private void CommandButtons_EnableDisableToggled(object sender, RoutedEventArgs e)
        {
            this.HandleCommandEnableDisable(sender);
        }

        private void AddCommandButton_Click(object sender, RoutedEventArgs e)
        {
            GameCommandWindow window = new GameCommandWindow();
            window.Closed += Window_Closed;
            window.Show();
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            this.RefreshList();
        }
    }
}
