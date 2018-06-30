using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Command;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for GamesControl.xaml
    /// </summary>
    public partial class GamesControl : MainControlBase
    {
        private ObservableCollection<GameCommandBase> gameCommands = new ObservableCollection<GameCommandBase>();

        public GamesControl()
        {
            InitializeComponent();
        }

        protected override Task InitializeInternal()
        {
            this.NoCurrenciesGrid.Visibility = Visibility.Collapsed;
            this.GamesGrid.Visibility = Visibility.Collapsed;

            if (ChannelSession.Settings.Currencies.Count > 0)
            {
                this.GamesGrid.Visibility = Visibility.Visible;

                this.GameCommandsListView.ItemsSource = this.gameCommands;

                this.RefreshList();
            }
            else
            {
                this.NoCurrenciesGrid.Visibility = Visibility.Visible;
            }
            return base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged() { await this.InitializeInternal(); }

        private void RefreshList()
        {
            this.GameCommandsListView.SelectedIndex = -1;

            this.gameCommands.Clear();
            foreach (GameCommandBase command in ChannelSession.Settings.GameCommands.OrderBy(c => c.Name))
            {
                this.gameCommands.Add(command);
            }
        }

        private void CommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandButtonsControl commandButtonControl = (CommandButtonsControl)sender;
            GameCommandBase command = commandButtonControl.GetCommandFromCommandButtons<GameCommandBase>(sender);
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
                CommandButtonsControl commandButtonControl = (CommandButtonsControl)sender;
                GameCommandBase command = commandButtonControl.GetCommandFromCommandButtons<GameCommandBase>(sender);
                if (command != null)
                {
                    ChannelSession.Settings.GameCommands.Remove(command);
                    await ChannelSession.SaveSettings();
                    this.RefreshList();
                }
            });
        }

        private void AddGameButton_Click(object sender, RoutedEventArgs e)
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
