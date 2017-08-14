using Mixer.Base.Interactive;
using Mixer.Base.Model.Interactive;
using MixItUp.Base;
using MixItUp.WPF.Util;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Interactive
{
    public class GameSceneTabItem : NotifyPropertyChangedBase
    {
        public InteractiveConnectedSceneGroupModel Scene { get; set; }

        public GameSceneTabItem(InteractiveConnectedSceneGroupModel scene)
        {
            this.Scene = scene;
        }

        public string Header { get { return this.Scene.sceneID; } }

        public string Content { get { return ""; } }
    }

    /// <summary>
    /// Interaction logic for InteractiveControl.xaml
    /// </summary>
    public partial class InteractiveControl : MainControlBase
    {
        private ObservableCollection<InteractiveGameListingModel> interactiveGames = new ObservableCollection<InteractiveGameListingModel>();

        private InteractiveGameListingModel SelectedGame;
        private ObservableCollection<GameSceneTabItem> GameScenes = new ObservableCollection<GameSceneTabItem>();

        public InteractiveControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.InteractiveGamesComboBox.ItemsSource = this.interactiveGames;
            this.GameScenesTabControl.ItemsSource = this.GameScenes;

            this.interactiveGames.Clear();
            foreach (InteractiveGameListingModel game in await MixerAPIHandler.MixerConnection.Interactive.GetOwnedInteractiveGames(this.Window.Channel))
            {
                this.interactiveGames.Add(game);
            }
        }

        private async Task Connect()
        {
            bool result = await this.Window.RunAsyncOperation(async () =>
            {
                return await MixerAPIHandler.InitializeInteractiveClient(this.Window.Channel, this.SelectedGame);
            });

            if (!result)
            {
                this.SelectedGame = null;
                MessageBoxHelper.ShowError("Unable to connect to interactive with selected game. Please try again.");
                return;
            }
        }

        private async Task RefreshSelectedInteractiveGame()
        {
            this.GameScenes.Clear();

            this.GameNameTextBox.Text = this.SelectedGame.name;
            this.GameDescriptionTextBox.Text = this.SelectedGame.description;

            InteractiveConnectedSceneGroupCollectionModel sceneCollection = await this.Window.RunAsyncOperation(async () =>
            {
                return await MixerAPIHandler.InteractiveClient.GetScenes();
            });

            if (sceneCollection != null)
            {
                foreach (InteractiveConnectedSceneGroupModel scene in sceneCollection.scenes)
                {
                    this.GameScenes.Add(new GameSceneTabItem(scene));
                }
            }

            this.GameScenesTabControl.IsEnabled = true;

            this.SaveChangedButton.IsEnabled = true;
            this.GameDetailsGrid.IsEnabled = true;
        }

        private async void InteractiveGamesComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.InteractiveGamesComboBox.SelectedIndex >= 0)
            {
                this.SelectedGame = (InteractiveGameListingModel)this.InteractiveGamesComboBox.SelectedItem;
                await this.RefreshSelectedInteractiveGame();
            }
        }

        private void NewGameButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.SelectedGame = null;
            this.GameScenes.Clear();

            this.GameNameTextBox.Clear();
            this.GameDescriptionTextBox.Clear();

            InteractiveSceneModel scene = InteractiveGameHelper.CreateDefaultScene();
            this.GameScenes.Add(new GameSceneTabItem(scene));
            scene.controls.Add(InteractiveGameHelper.CreateButton("Test Button", "Test Button"));

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

            foreach (GameSceneTabItem scene in this.GameScenes)
            {
                if (scene.Scene.controls.Count == 0)
                {
                    MessageBoxHelper.ShowError("The following scene does not contain any controls: " + scene.Scene.sceneID);
                    return;
                }
            }

            if (this.SelectedGame == null)
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

                this.SelectedGame = gameListing;
                await this.RefreshSelectedInteractiveGame();
            }
            else
            {

            }
        }
    }
}
