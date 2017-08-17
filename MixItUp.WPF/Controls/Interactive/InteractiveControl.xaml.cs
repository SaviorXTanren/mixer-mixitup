using Mixer.Base.Interactive;
using Mixer.Base.Model.Interactive;
using MixItUp.Base;
using MixItUp.WPF.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Interactive
{
    /// <summary>
    /// Interaction logic for InteractiveControl.xaml
    /// </summary>
    public partial class InteractiveControl : MainControlBase
    {
        private ObservableCollection<InteractiveGameListingModel> interactiveGames = new ObservableCollection<InteractiveGameListingModel>();

        private InteractiveGameListingModel selectedGame;
        private ObservableCollection<InteractiveSceneModel> gameScenes = new ObservableCollection<InteractiveSceneModel>();

        public InteractiveControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.InteractiveGamesComboBox.ItemsSource = this.interactiveGames;
            this.GameScenesTabControl.ItemsSource = this.gameScenes;

            await this.RefreshInteractiveGames();
        }

        private async Task Connect()
        {
            bool result = await this.Window.RunAsyncOperation(async () =>
            {
                return await MixerAPIHandler.InitializeInteractiveClient(this.Window.Channel, this.selectedGame);
            });

            if (!result)
            {
                this.selectedGame = null;
                MessageBoxHelper.ShowError("Unable to connect to interactive with selected game. Please try again.");
                return;
            }

            InteractiveConnectedSceneGroupCollectionModel sceneCollection = await this.Window.RunAsyncOperation(async () =>
            {
                return await MixerAPIHandler.InteractiveClient.GetScenes();
            });
        }

        private async Task RefreshInteractiveGames()
        {
            IEnumerable<InteractiveGameListingModel> gameListings = await this.Window.RunAsyncOperation(async () =>
            {
                return await MixerAPIHandler.MixerConnection.Interactive.GetOwnedInteractiveGames(this.Window.Channel);
            });

            this.interactiveGames.Clear();
            foreach (InteractiveGameListingModel game in gameListings)
            {
                this.interactiveGames.Add(game);
            }
        }

        private async Task RefreshSelectedInteractiveGame()
        {
            this.gameScenes.Clear();

            InteractiveVersionModel version = await this.Window.RunAsyncOperation(async () =>
            {
                await this.RefreshInteractiveGames();
                this.selectedGame = this.interactiveGames.First(g => g.id.Equals(this.selectedGame.id));

                return await MixerAPIHandler.MixerConnection.Interactive.GetInteractiveVersionInfo(this.selectedGame.versions.First());
            });

            this.GameNameTextBox.Text = this.selectedGame.name;
            this.GameDescriptionTextBox.Text = this.selectedGame.description;

            foreach (InteractiveSceneModel scene in version.controls.scenes)
            {
                this.gameScenes.Add(scene);
            }
            this.GameScenesTabControl.SelectedIndex = 0;

            this.GameScenesTabControl.IsEnabled = true;

            this.SaveChangedButton.IsEnabled = true;
            this.GameDetailsGrid.IsEnabled = true;
        }

        private async void InteractiveGamesComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.InteractiveGamesComboBox.SelectedIndex >= 0)
            {
                this.selectedGame = (InteractiveGameListingModel)this.InteractiveGamesComboBox.SelectedItem;
                await this.RefreshSelectedInteractiveGame();
            }
        }

        private void NewGameButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.selectedGame = null;
            this.gameScenes.Clear();

            this.GameNameTextBox.Clear();
            this.GameDescriptionTextBox.Clear();

            InteractiveSceneModel scene = InteractiveGameHelper.CreateDefaultScene();
            this.gameScenes.Add(scene);
            this.GameScenesTabControl.SelectedIndex = 0;

            this.GameScenesTabControl.IsEnabled = true;

            this.SaveChangedButton.IsEnabled = true;
            this.GameDetailsGrid.IsEnabled = true;
        }

        private async void SaveChangedButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.GameNameTextBox.Text))
            {
                MessageBoxHelper.ShowError("A name must be specified for the game");
                return;
            }

            foreach (InteractiveSceneModel scene in this.gameScenes)
            {
                if (scene.buttons.Count == 0 && scene.joysticks.Count == 0)
                {
                    MessageBoxHelper.ShowError("The following scene does not contain any controls: " + scene.sceneID);
                    return;
                }
            }

            if (this.selectedGame == null)
            {
                InteractiveGameListingModel gameListing = await this.Window.RunAsyncOperation(async () =>
                {
                     return await InteractiveGameHelper.CreateInteractive2Game(MixerAPIHandler.MixerConnection, this.Window.Channel, this.Window.User, this.GameNameTextBox.Text, null);
                });    
                
                if (gameListing == null)
                {
                    MessageBoxHelper.ShowError("Failed to create game, please try again");
                    return;
                }

                this.selectedGame = gameListing;
                await this.RefreshSelectedInteractiveGame();
            }
            else
            {

            }
        }
    }
}
