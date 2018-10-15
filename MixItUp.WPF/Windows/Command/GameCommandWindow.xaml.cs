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
                if (this.command is SlotMachineGameCommand) { this.SetGameEditorControl(new SlotMachineGameEditorControl((SlotMachineGameCommand)this.command)); }
                if (this.command is VendingMachineGameCommand) { this.SetGameEditorControl(new VendingMachineGameEditorControl((VendingMachineGameCommand)this.command)); }
                if (this.command is StealGameCommand) { this.SetGameEditorControl(new StealGameEditorControl((StealGameCommand)this.command)); }
                if (this.command is PickpocketGameCommand) { this.SetGameEditorControl(new PickpocketGameEditorControl((PickpocketGameCommand)this.command)); }
                if (this.command is DuelGameCommand) { this.SetGameEditorControl(new DuelGameEditorControl((DuelGameCommand)this.command)); }
                if (this.command is HeistGameCommand) { this.SetGameEditorControl(new HeistGameEditorControl((HeistGameCommand)this.command)); }
                if (this.command is RussianRouletteGameCommand) { this.SetGameEditorControl(new RussianRouletteGameEditorControl((RussianRouletteGameCommand)this.command)); }
                if (this.command is BidGameCommand) { this.SetGameEditorControl(new BidGameEditorControl((BidGameCommand)this.command)); }
                if (this.command is RouletteGameCommand) { this.SetGameEditorControl(new RouletteGameEditorControl((RouletteGameCommand)this.command)); }
                if (this.command is HitmanGameCommand) { this.SetGameEditorControl(new HitmanGameEditorControl((HitmanGameCommand)this.command)); }
                if (this.command is CoinPusherGameCommand) { this.SetGameEditorControl(new CoinPusherGameEditorControl((CoinPusherGameCommand)this.command)); }
                if (this.command is VolcanoGameCommand) { this.SetGameEditorControl(new VolcanoGameEditorControl((VolcanoGameCommand)this.command)); }
                if (this.command is LockBoxGameCommand) { this.SetGameEditorControl(new LockBoxGameEditorControl((LockBoxGameCommand)this.command)); }
            }
            else
            {
                this.GameTypeSelectionGrid.Visibility = Visibility.Visible;

                this.GameTypeComboBox.ItemsSource = this.gameListings;

                this.gameListings.Add(new GameTypeListing("Spin", "The Spin game picks a random number and selects an outcome based on that number. Besides selecting a payout for each outcome, you can also run a customized command for each outcome."
                    + Environment.NewLine + Environment.NewLine + "\tEX: !spin 100", new SpinGameEditorControl()));

                this.gameListings.Add(new GameTypeListing("Slot Machine", "The Slot Machine game picks a random set of 3 symbols from a pre-defined list and selects any outcome that matches those symbols. Besides selecting a payout for each outcome, you can also run a customized command for each outcome."
                    + Environment.NewLine + Environment.NewLine + "\tEX: !slots 100", new SlotMachineGameEditorControl()));

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

                this.gameListings.Add(new GameTypeListing("Roulette", "The Roulette game allows a user to start a group betting game amongst all users to bet on a specific number or name. If the game is based on a number range, then a user can select any number in that range. If the game is based on a series of names, a user can select any one of the names. All users who select the winning bet type get the payout for their bet, everyone else losses their bet."
                    + Environment.NewLine + Environment.NewLine + "\tEX: !roulette <NUMBER> 100", new RouletteGameEditorControl()));

                this.gameListings.Add(new GameTypeListing("Bid", "The Bid game allows a user to start a bidding competition amongst all users to win a prize or special privilege. A user must bid at least 1 currency amount higher than the highest bid to become the leading bidder. When a user is outbid, they receive their bet currency back and the highest bidder when time runs out wins. Additionally, you can specify what user roles can start this game to ensure it isn't abused, such as Moderators or higher only."
                    + Environment.NewLine + Environment.NewLine + "\tEX: !bid 100", new BidGameEditorControl()));

                this.gameListings.Add(new GameTypeListing("Hitman", "The Hitman game allows a user to start a winner-takes-all bet amongst all entered users. After the initial time limit, a hitman with a specific name will appear in chat. If a user types the hitman's name in chat within the time limit, they win the entire pot. Otherwise, everyone loses their money."
                    + Environment.NewLine + Environment.NewLine + "\tEX: !hitman 100", new HitmanGameEditorControl()));

                this.gameListings.Add(new GameTypeListing("Coin Pusher", "The Coin Pusher game allows a user to deposit a specific amount of currency into a machine with the chance for a large payout. A minimum amount of currency must be inserted into the machine to allow for a payout and a minimum & maximum payout percentage can be specified when a payout occurs."
                    + Environment.NewLine + Environment.NewLine + "\tEX: !pusher 100", new CoinPusherGameEditorControl()));

                this.gameListings.Add(new GameTypeListing("Volcano", "The Volcano game allows a user to deposit a specific amount of currency into a volcano with the chance for a personal payout and a payout for all users in chat. The volcano goes through 3 stages as more and more currency is deposited into it and a different set of Deposit & Status commands are used depending on what stage the Volcano is at." + Environment.NewLine + Environment.NewLine +
                    "Once the volcano reaches stage 3, each subsequent deposit has a chance to trigger an eruption. When an eruption occurs, the user who triggered it gets a specialized payout for them. After the eruption, all users have a chance to collect erupted currency during the collection time limit. After the collection is done, the volcano contents resets back to 0."
                    + Environment.NewLine + Environment.NewLine + "\tEX: !volcano 100" + Environment.NewLine + Environment.NewLine + "Game Designed By: https://mixer.com/InsertCoinTheater", new VolcanoGameEditorControl()));

                this.gameListings.Add(new GameTypeListing("Lock Box", "The Lock Box game allows you to guess the combination of a locked box using the numbers 0 - 9 that contains a large amount of currency. Every failed guess puts the required currency amount into the box and gives a hint to the combination, while a correct guess gets all the currency inside." + Environment.NewLine + Environment.NewLine +
                    "Users have the ability to see the status of the lock box to see how much is in it and inspect the lock box for a hint as to the combination."
                    + Environment.NewLine + Environment.NewLine + "\tEX: !lockbox 100", new LockBoxGameEditorControl()));
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
