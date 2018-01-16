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

                List<string> preMadeGameNames = new List<string>() { "Spin Wheel", "Heist", "Russian Roulette", "Charity", "Give" };
                this.PreMadeGamesComboBox.ItemsSource = preMadeGameNames.OrderBy(s => s);

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

        private async void AddPreMadeGameButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.PreMadeGamesComboBox.SelectedIndex >= 0)
            {
                await this.Window.RunAsyncOperation(async () =>
                {
                    string gameName = (string)this.PreMadeGamesComboBox.SelectedItem;
                    if (ChannelSession.Settings.GameCommands.Any(c => c.Name.Equals(gameName)))
                    {
                        await MessageBoxHelper.ShowMessageDialog("This game already exist in your game list");
                        return;
                    }

                    UserCurrencyViewModel currency = ChannelSession.Settings.Currencies.Values.First();
                    GameCommandBase game = null;
                    switch (gameName)
                    {
                        case "Spin Wheel": game = new SpinWheelGameCommand(currency); break;
                        case "Heist": game = new HeistGameCommand(currency); break;
                        case "Russian Roulette": game = new RussianRouletteGameCommand(currency); break;
                        case "Charity": game = new CharityGameCommand(currency); break;
                        case "Give": game = new GiveGameCommand(currency); break;
                    }

                    ChannelSession.Settings.GameCommands.Add(game);
                    await ChannelSession.SaveSettings();

                    this.PreMadeGamesComboBox.SelectedIndex = -1;

                    this.RefreshList();
                });
            }
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
