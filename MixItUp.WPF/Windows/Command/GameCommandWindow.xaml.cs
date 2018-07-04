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
            }
            else
            {
                this.GameTypeSelectionGrid.Visibility = Visibility.Visible;

                this.GameTypeComboBox.ItemsSource = this.gameListings;

                this.gameListings.Add(new GameTypeListing("Spin", "The Spin game picks a random number and selects an outcome based on that number. Besides selecting a payout for each outcome, you can also run a customized command for each outcome."
                    + Environment.NewLine + Environment.NewLine + "\tEX: !spin 100", new SpinGameEditorControl()));

                this.gameListings.Add(new GameTypeListing("Vending Machine", "The Vending Machine game picks a random number and selects an outcome based on that number. Unlike the Spin game, the Vending Machine game doesn't have a payout for each outcome and instead is more focused on an \"action\" occurring with each outcome, such as a sound effect, image, or a specialized effect."
                    + Environment.NewLine + Environment.NewLine + "\tEX: !vend", new VendingMachineGameEditorControl()));

                this.gameListings.Add(new GameTypeListing("Steal", "The Steal game picks a random user in chat and attempts to steal currency from them. If successful, the user steals the bet amount from the random user. If failed, they lose the bet amount."
                    + Environment.NewLine + Environment.NewLine + "\tEX: !steal 100", new StealGameEditorControl()));

                this.gameListings.Add(new GameTypeListing("Pickpocket", "The Pickpocket game attempts to steal currency from a specified user. If successful, the user steals the bet amount from the specified user. If failed, they lose the bet amount."
                    + Environment.NewLine + Environment.NewLine + "\tEX: !pickpocket <USERNAME> 100", new PickpocketGameEditorControl()));

                this.gameListings.Add(new GameTypeListing("Duel", "The Duel game challenges the specified user to a winner-takes-all for the bet amount. If successful, the user takes the bet amount from the specified user. If failed, the specified user takes the bet amount from the user."
                    + Environment.NewLine + Environment.NewLine + "\tEX: !duel <USERNAME> 100", new DuelGameEditorControl()));
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
