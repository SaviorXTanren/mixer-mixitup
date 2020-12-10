using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Command;
using MixItUp.WPF.Controls.Games;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Windows.Command
{
    /// <summary>
    /// Interaction logic for GameCommandWindow.xaml
    /// </summary>
    public partial class GameCommandWindow : LoadingWindowBase
    {
        private GameCommandWindowViewModel viewModel;

        private Dictionary<string, GameEditorControlBase> gameEditors = new Dictionary<string, GameEditorControlBase>();
        private GameEditorControlBase gameEditor;

        public GameCommandWindow(GameCommandBase command)
            : this()
        {
            this.viewModel = new GameCommandWindowViewModel(command);
        }

        public GameCommandWindow()
        {
            InitializeComponent();

            this.viewModel = new GameCommandWindowViewModel();

            this.Initialize(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            this.DataContext = this.viewModel;
            this.viewModel.GameTypeSelected += ViewModel_GameTypeSelected;
            await this.viewModel.OnLoaded();
        }

        private void ViewModel_GameTypeSelected(object sender, GameTypeListing e)
        {
            this.SetGameEditorControl(this.gameEditors[this.viewModel.SelectedGameType.Name]);
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

        private void SetGameEditorControl(GameEditorControlBase gameEditor)
        {
            this.MainContentControl.Content = this.gameEditor = gameEditor;
        }
    }
}
