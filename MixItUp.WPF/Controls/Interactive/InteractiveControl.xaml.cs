using Mixer.Base.Interactive;
using Mixer.Base.Model.Interactive;
using MixItUp.Base;
using MixItUp.WPF.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MixItUp.WPF.Controls.Interactive
{
    public class InteractiveBoardSize
    {
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    /// <summary>
    /// Interaction logic for InteractiveControl.xaml
    /// </summary>
    public partial class InteractiveControl : MainControlBase
    {
        private const int LargeWidth = 80;
        private const int LargeHeight = 22;

        private const int MediumWidth = 40;
        private const int MediumHeight = 25;

        private const int SmallWidth = 30;
        private const int SmallHeight = 40;

        public ObservableCollection<InteractiveBoardSize> boardSizes = new ObservableCollection<InteractiveBoardSize>();

        private ObservableCollection<InteractiveGameListingModel> interactiveGames = new ObservableCollection<InteractiveGameListingModel>();

        private InteractiveGameListingModel selectedGame;
        private ObservableCollection<InteractiveSceneModel> gameScenes = new ObservableCollection<InteractiveSceneModel>();
        private InteractiveSceneModel selectedScene;
        private InteractiveBoardSize selectedBoardSize;

        private bool[,] boardBlocks;
        private int blockWidthHeight;

        public InteractiveControl()
        {
            InitializeComponent();

            this.boardSizes.Add(new InteractiveBoardSize() { Name = "Large", Width = LargeWidth, Height = LargeHeight });
            this.boardSizes.Add(new InteractiveBoardSize() { Name = "Medium", Width = MediumWidth, Height = MediumHeight });
            this.boardSizes.Add(new InteractiveBoardSize() { Name = "Small", Width = SmallWidth, Height = SmallHeight });
            this.selectedBoardSize = this.boardSizes.First();

            this.SizeChanged += InteractiveControl_SizeChanged;
        }

        protected override async Task InitializeInternal()
        {
            this.InteractiveGamesComboBox.ItemsSource = this.interactiveGames;
            this.SceneComboBox.ItemsSource = this.gameScenes;
            this.BoardSizeComboBox.ItemsSource = this.boardSizes;

            await this.RefreshInteractiveGames();
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

                return await MixerAPIHandler.MixerConnection.Interactive.GetInteractiveGameVersion(this.selectedGame.versions.First());
            });

            this.GameNameTextBox.Text = this.selectedGame.name;
            this.GameDescriptionTextBox.Text = this.selectedGame.description;

            foreach (InteractiveSceneModel scene in version.controls.scenes)
            {
                this.gameScenes.Add(scene);
            }

            this.selectedScene = this.gameScenes.First();
            this.SceneComboBox.SelectedIndex = 0;
            this.BoardSizeComboBox.SelectedIndex = 0;

            this.RefreshScene();

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

            this.selectedScene = InteractiveGameHelper.CreateDefaultScene();
            this.gameScenes.Add(this.selectedScene);
            this.SceneComboBox.SelectedIndex = 0;
            this.BoardSizeComboBox.SelectedIndex = 0;

            this.RefreshScene();

            this.SaveChangedButton.IsEnabled = true;
            this.GameDetailsGrid.IsEnabled = true;
        }

        private void SceneComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.selectedScene != null && this.SceneComboBox.SelectedIndex >= 0)
            {
                this.selectedScene = (InteractiveSceneModel)this.SceneComboBox.SelectedItem;
                this.RefreshScene();
            }
        }

        private void BoardSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.selectedScene != null && this.BoardSizeComboBox.SelectedIndex >= 0)
            {
                this.selectedBoardSize = (InteractiveBoardSize)this.BoardSizeComboBox.SelectedItem;
                this.RefreshScene();
            }
        }

        private void InteractiveControl_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            if (this.selectedScene != null)
            {
                this.RefreshScene();
            }
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

        public void RefreshScene()
        {
            this.InteractiveBoardGrid.IsEnabled = true;

            this.InteractiveBoardCanvas.Children.Clear();
            this.boardBlocks = new bool[this.selectedBoardSize.Width, this.selectedBoardSize.Height];

            int perBlockWidth = (int)this.InteractiveBoardCanvas.ActualWidth / (this.selectedBoardSize.Width);
            int perBlockHeight = (int)this.InteractiveBoardCanvas.ActualHeight / (this.selectedBoardSize.Height);
            this.blockWidthHeight = Math.Min(perBlockWidth, perBlockHeight);

            foreach (InteractiveControlModel control in this.selectedScene.allControls)
            {
                this.BlockOutControlArea(control);
            }

            for (int w = 0; w < this.selectedBoardSize.Width; w++)
            {
                for (int h = 0; h < this.selectedBoardSize.Height; h++)
                {
                    if (!this.boardBlocks[w, h])
                    {
                        this.RenderRectangle(w, h, 1, 1, Brushes.Blue);
                    }
                }
            }
        }

        private void BlockOutControlArea(InteractiveControlModel control)
        {
            InteractiveControlPositionModel position = control.position.FirstOrDefault(p => p.size.Equals(this.selectedBoardSize.Name.ToLower()));
            for (int w = 0; w < position.width; w++)
            {
                for (int h = 0; h < position.height; h++)
                {
                    this.boardBlocks[position.x + w, position.y + h] = true;
                }
            }
            this.RenderRectangle(position.x, position.y, position.width, position.height, Brushes.Black);

            TextBlock textBlock = new TextBlock();
            textBlock.Text = control.controlID;
            textBlock.Foreground = Brushes.Black;
            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.TextAlignment = TextAlignment.Center;
            this.AddElementToCanvas(textBlock, position.x + 1, position.y + 1, position.width - 2, position.height - 2);
        }

        private void RenderRectangle(int x, int y, int width, int height, Brush color)
        {
            Rectangle rect = new Rectangle();
            rect.Stroke = color;
            rect.StrokeThickness = 1;
            this.AddElementToCanvas(rect, x, y, width, height);
        }

        private void AddElementToCanvas(FrameworkElement element, int x, int y, int width, int height)
        {
            element.Width = width * this.blockWidthHeight;
            element.Height = height * this.blockWidthHeight;
            Canvas.SetLeft(element, x * this.blockWidthHeight);
            Canvas.SetTop(element, y * this.blockWidthHeight);
            this.InteractiveBoardCanvas.Children.Add(element);
        }
    }
}
