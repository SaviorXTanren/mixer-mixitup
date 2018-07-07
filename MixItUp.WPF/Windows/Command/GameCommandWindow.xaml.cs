using MixItUp.Base.Commands;
using MixItUp.WPF.Controls.Games;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Windows.Command
{
    public class GameTypeListing
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public GameEditorControlBase EditorControl { get; set; }

        public GameTypeListing(string name, string description, GameEditorControlBase editorControl)
        {
            this.Name = name;
            this.Description = description;
            this.EditorControl = editorControl;
        }
    }

    /// <summary>
    /// Interaction logic for GameCommandWindow.xaml
    /// </summary>
    public partial class GameCommandWindow : LoadingWindowBase
    {
        private GameCommandBase command;

        private ObservableCollection<GameTypeListing> gameListings = new ObservableCollection<GameTypeListing>();

        private GameEditorControlBase gameEditor = null;

        public GameCommandWindow(GameCommandBase command) : this() { this.command = command; }

        public GameCommandWindow()
        {
            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        protected override Task OnLoaded()
        {
            if (this.command != null)
            {
                if (this.command is SpinGameCommand) { this.SetGameEditorControl(new SpinGameEditorControl((SpinGameCommand)this.command)); }
                if (this.command is VendingMachineGameCommand) { this.SetGameEditorControl(new VendingMachineGameEditorControl((VendingMachineGameCommand)this.command)); }
                if (this.command is StealGameCommand) { this.SetGameEditorControl(new StealGameEditorControl((StealGameCommand)this.command)); }
                if (this.command is PickpocketGameCommand) { this.SetGameEditorControl(new PickpocketGameEditorControl((PickpocketGameCommand)this.command)); }
                if (this.command is DuelGameCommand) { this.SetGameEditorControl(new DuelGameEditorControl((DuelGameCommand)this.command)); }
                if (this.command is HeistGameCommand) { this.SetGameEditorControl(new HeistGameEditorControl((HeistGameCommand)this.command)); }
                if (this.command is RussianRouletteGameCommand) { this.SetGameEditorControl(new RussianRouletteGameEditorControl((RussianRouletteGameCommand)this.command)); }
                if (this.command is BidGameCommand) { this.SetGameEditorControl(new BidGameEditorControl((BidGameCommand)this.command)); }
            }
            else
            {
                this.GameTypeSelectionGrid.Visibility = Visibility.Visible;

                this.GameTypeComboBox.ItemsSource = this.gameListings;

                this.gameListings.Add(new GameTypeListing("Spin", "The Spin game picks a random number and selects an outcome based on that number. Besides selecting a payout for each outcome, you can also run a customized command for each outcome."
                    + Environment.NewLine + Environment.NewLine + "\tEX: !spin 100", new SpinGameEditorControl()));

                this.gameListings.Add(new GameTypeListing("Vending Machine", "The Vending Machine game picks a random number and selects an outcome based on that number. Unlike the Spin game, the Vending Machine game doesn't have a payout for each outcome and instead is more focused on an \"action\" occurring with each outcome, such as a sound effect, image, or a specialized effect."
                    + Environment.NewLine + Environment.NewLine + "\tEX: !vend" + Environment.NewLine + Environment.NewLine + "Game Designed By: https://mixer.com/InsertCoinTheater", new VendingMachineGameEditorControl()));

                this.gameListings.Add(new GameTypeListing("Steal", "The Steal game picks a random user in chat and attempts to steal currency from them. If successful, the user steals the bet amount from the random user. If failed, they lose the bet amount."
                    + Environment.NewLine + Environment.NewLine + "\tEX: !steal 100", new StealGameEditorControl()));

                this.gameListings.Add(new GameTypeListing("Pickpocket", "The Pickpocket game attempts to steal currency from a specified user. If successful, the user steals the bet amount from the specified user. If failed, they lose the bet amount."
                    + Environment.NewLine + Environment.NewLine + "\tEX: !pickpocket <USERNAME> 100", new PickpocketGameEditorControl()));

                this.gameListings.Add(new GameTypeListing("Duel", "The Duel game challenges the specified user to a winner-takes-all for the bet amount. If successful, the user takes the bet amount from the specified user. If failed, the specified user takes the bet amount from the user."
                    + Environment.NewLine + Environment.NewLine + "\tEX: !duel <USERNAME> 100", new DuelGameEditorControl()));

                this.gameListings.Add(new GameTypeListing("Heist", "The Heist game allows a user to start a group activity for users to individually bet when they participate. Each user has their own individual chance to succeed and win back more or fail and lose their bet."
                    + Environment.NewLine + Environment.NewLine + "\tEX: !heist 100", new HeistGameEditorControl()));

                this.gameListings.Add(new GameTypeListing("Russian Roulette", "The Russian Roulette game allows a user to start a winner-takes-all bet amongst all entered users. By default, the user that starts the game specifies how much the bet is and all subsequent users must bet that amount to join, with all winners of the game splitting the total payout equally."
                    + Environment.NewLine + Environment.NewLine + "\tEX: !rr 100\t\tAfter Start: !rr", new RussianRouletteGameEditorControl()));

                this.gameListings.Add(new GameTypeListing("Bid", "The Bid game allows a user to start a bidding competition amongst all users to win a prize or special privilege. A user must bid at least 1 currency amount higher than the highest bid to become the leading bidder. When a user is outbid, they receive their bet currency back and the highest bidder when time runs out wins. Additionally, you can specify what user roles can start this game to ensure it isn't abused, such as Moderators or higher only."
                    + Environment.NewLine + Environment.NewLine + "\tEX: !bid 100", new BidGameEditorControl()));
            }

            return Task.FromResult(0);
        }

        private void GameTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.GameTypeComboBox.SelectedIndex >= 0)
            {
                GameTypeListing selectedType = (GameTypeListing)this.GameTypeComboBox.SelectedItem;
                this.GameDescriptionTextBlock.Text = selectedType.Description;
                this.NextButton.IsEnabled = true;
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.GameTypeComboBox.SelectedIndex >= 0)
            {
                GameTypeListing selectedType = (GameTypeListing)this.GameTypeComboBox.SelectedItem;
                this.SetGameEditorControl(selectedType.EditorControl);
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                if (await gameEditor.Validate())
                {
                    gameEditor.SaveGameCommand();
                    this.Close();
                }
            });
        }

        private void SetGameEditorControl(GameEditorControlBase gameEditorControl)
        {
            this.MainContentControl.Content = this.gameEditor = gameEditorControl;

            this.GameTypeSelectionGrid.Visibility = Visibility.Collapsed;
            this.GameEditorGrid.Visibility = Visibility.Visible;
        }
    }
}
