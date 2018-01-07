using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.User;
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
    public partial class GamesControl : MainCommandControlBase
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
                this.PreMadeGamesComboBox.ItemsSource = new List<string>() { "Charity", "Heist", "Roulette", "Russian Roulette" };

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
            GameCommandBase command = this.GetCommandFromCommandButtons<GameCommandBase>(sender);
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
                GameCommandBase command = this.GetCommandFromCommandButtons<GameCommandBase>(sender);
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
                        case "Charity": game = new CharityGameCommand(currency); break;
                        case "Heist": game = new HeistGameCommand(currency); break;
                        case "Roulette": game = new RouletteGameCommand(currency); break;
                        case "Russian Roulette": game = new RussianRouletteGameCommand(currency); break;
                    }

                    ChannelSession.Settings.GameCommands.Add(game);
                    await ChannelSession.SaveSettings();

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
