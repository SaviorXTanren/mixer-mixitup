using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.Window.Commands;
using MixItUp.WPF.Controls.Commands;
using System.Threading.Tasks;

namespace MixItUp.WPF.Windows.Commands
{
    /// <summary>
    /// Interaction logic for CommandEditorWindow.xaml
    /// </summary>
    public partial class CommandEditorWindow : LoadingWindowBase
    {
        public CommandEditorDetailsControlBase editorDetailsControl { get; private set; }

        public CommandEditorWindowViewModelBase viewModel { get; private set; }

        public CommandEditorWindow(CommandModelBase existingCommand)
            : this()
        {
            switch (existingCommand.Type)
            {
                case CommandTypeEnum.Chat:
                    this.editorDetailsControl = new ChatCommandEditorDetailsControl();
                    this.viewModel = new ChatCommandEditorWindowViewModel((ChatCommandModel)existingCommand);
                    break;
                case CommandTypeEnum.Event:
                    this.editorDetailsControl = new EventCommandEditorDetailsControl();
                    this.viewModel = new EventCommandEditorWindowViewModel((EventCommandModel)existingCommand);
                    break;
                case CommandTypeEnum.Timer:
                    this.editorDetailsControl = new TimerCommandEditorDetailsControl();
                    this.viewModel = new TimerCommandEditorWindowViewModel((TimerCommandModel)existingCommand);
                    break;
                case CommandTypeEnum.TwitchChannelPoints:
                    this.editorDetailsControl = new TwitchChannelPointsCommandEditorDetailsControl();
                    this.viewModel = new TwitchChannelPointsCommandEditorWindowViewModel((TwitchChannelPointsCommandModel)existingCommand);
                    break;
                case CommandTypeEnum.ActionGroup:
                    this.editorDetailsControl = new ActionGroupCommandEditorDetailsControl();
                    this.viewModel = new ActionGroupCommandEditorWindowViewModel((ActionGroupCommandModel)existingCommand);
                    break;
                case CommandTypeEnum.Game:
                    break;
            }
            this.DataContext = this.ViewModel = this.viewModel;

            this.ViewModel.StartLoadingOperationOccurred += (sender, eventArgs) => { this.StartLoadingOperation(); };
            this.ViewModel.EndLoadingOperationOccurred += (sender, eventArgs) => { this.EndLoadingOperation(); };
        }

        public CommandEditorWindow(CommandTypeEnum commandType)
            : this()
        {
            switch (commandType)
            {
                case CommandTypeEnum.Chat:
                    this.editorDetailsControl = new ChatCommandEditorDetailsControl();
                    this.viewModel = new ChatCommandEditorWindowViewModel();
                    break;
                case CommandTypeEnum.Timer:
                    this.editorDetailsControl = new TimerCommandEditorDetailsControl();
                    this.viewModel = new TimerCommandEditorWindowViewModel();
                    break;
                case CommandTypeEnum.TwitchChannelPoints:
                    this.editorDetailsControl = new TwitchChannelPointsCommandEditorDetailsControl();
                    this.viewModel = new TwitchChannelPointsCommandEditorWindowViewModel();
                    break;
                case CommandTypeEnum.ActionGroup:
                    this.editorDetailsControl = new ActionGroupCommandEditorDetailsControl();
                    this.viewModel = new ActionGroupCommandEditorWindowViewModel();
                    break;
                case CommandTypeEnum.Game:
                    break;
            }
            this.DataContext = this.ViewModel = this.viewModel;

            this.ViewModel.StartLoadingOperationOccurred += (sender, eventArgs) => { this.StartLoadingOperation(); };
            this.ViewModel.EndLoadingOperationOccurred += (sender, eventArgs) => { this.EndLoadingOperation(); };
        }

        public CommandEditorWindow(EventTypeEnum eventType)
            : this()
        {
            this.editorDetailsControl = new EventCommandEditorDetailsControl();
            this.DataContext = this.ViewModel = this.viewModel = new EventCommandEditorWindowViewModel(eventType);

            this.ViewModel.StartLoadingOperationOccurred += (sender, eventArgs) => { this.StartLoadingOperation(); };
            this.ViewModel.EndLoadingOperationOccurred += (sender, eventArgs) => { this.EndLoadingOperation(); };
        }

        private CommandEditorWindow()
            : base()
        {
            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            this.viewModel.CloseEditor += ViewModel_CloseEditor;
            await this.viewModel.OnLoaded();

            this.DetailsContentControl.Content = this.editorDetailsControl;

            await base.OnLoaded();
        }

        private void ViewModel_CloseEditor(object sender, System.EventArgs e)
        {
            this.Close();
        }
    }
}
