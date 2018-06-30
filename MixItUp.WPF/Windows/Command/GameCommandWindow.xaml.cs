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
            }
            else
            {
                this.GameTypeSelectionGrid.Visibility = Visibility.Visible;

                this.GameTypeComboBox.ItemsSource = this.gameListings;

                this.gameListings.Add(new GameTypeListing("Spin", "The Spin game picks a random number and selects an outcome based on that number. Besides selecting a payout for each outcome, you can also run a customized command for each outcome."
                    + Environment.NewLine + Environment.NewLine + "\tEX: !spin 100", new SpinGameEditorControl()));
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
