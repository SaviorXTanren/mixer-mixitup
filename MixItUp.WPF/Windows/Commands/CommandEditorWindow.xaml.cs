using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.Commands;
using MixItUp.WPF.Controls.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.WPF.Windows.Commands
{
    /// <summary>
    /// Interaction logic for CommandEditorWindow.xaml
    /// </summary>
    public partial class CommandEditorWindow : LoadingWindowBase
    {
        private Guid commandId = Guid.Empty;

        public CommandEditorDetailsControlBase editorDetailsControl { get; private set; }

        public CommandEditorWindowViewModelBase viewModel { get; private set; }

        public event EventHandler<CommandModelBase> CommandSaved = delegate { };

        private static Dictionary<Guid, CommandEditorWindow> OpenWindows = new Dictionary<Guid, CommandEditorWindow>();
        public static CommandEditorWindow GetCommandEditorWindow(CommandModelBase existingCommand)
        {
            if (OpenWindows.TryGetValue(existingCommand.ID, out var window))
            {
                return window;
            }

            window = new CommandEditorWindow(existingCommand);
            OpenWindows.Add(existingCommand.ID, window);
            return window;
        }

        private CommandEditorWindow(CommandModelBase existingCommand)
            : this()
        {
            commandId = existingCommand.ID;

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
                case CommandTypeEnum.Custom:
                    this.editorDetailsControl = new CustomCommandEditorDetailsControl();
                    this.viewModel = new CustomCommandEditorWindowViewModel((CustomCommandModel)existingCommand);
                    break;
                case CommandTypeEnum.UserOnlyChat:
                    this.editorDetailsControl = new ChatCommandEditorDetailsControl();
                    this.viewModel = new UserOnlyChatCommandEditorWindowViewModel((UserOnlyChatCommandModel)existingCommand);
                    break;
            }
            this.DataContext = this.ViewModel = this.viewModel;

            this.ViewModel.StartLoadingOperationOccurred += (sender, eventArgs) => { this.StartLoadingOperation(); };
            this.ViewModel.EndLoadingOperationOccurred += (sender, eventArgs) => { this.EndLoadingOperation(); };
        }

        public CommandEditorWindow(CommandTypeEnum commandType, string name = null)
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
                case CommandTypeEnum.Custom:
                    this.editorDetailsControl = new CustomCommandEditorDetailsControl();
                    this.viewModel = (!string.IsNullOrEmpty(name)) ? new CustomCommandEditorWindowViewModel(name) : new CustomCommandEditorWindowViewModel();
                    break;
            }
            this.DataContext = this.ViewModel = this.viewModel;

            this.ViewModel.StartLoadingOperationOccurred += (sender, eventArgs) => { this.StartLoadingOperation(); };
            this.ViewModel.EndLoadingOperationOccurred += (sender, eventArgs) => { this.EndLoadingOperation(); };
        }

        public CommandEditorWindow(CommandTypeEnum commandType, Guid id)
            : this()
        {
            switch (commandType)
            {
                case CommandTypeEnum.UserOnlyChat:
                    this.editorDetailsControl = new ChatCommandEditorDetailsControl();
                    this.viewModel = new UserOnlyChatCommandEditorWindowViewModel(id);
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
            this.viewModel.CommandSaved += ViewModel_CommandSaved;
            await this.viewModel.OnLoaded();

            this.DetailsContentControl.Content = this.editorDetailsControl;

            await base.OnLoaded();
        }

        protected override void OnClosed(EventArgs e)
        {
            OpenWindows.Remove(this.commandId);
            base.OnClosed(e);
        }

        private void ViewModel_CommandSaved(object sender, CommandModelBase command)
        {
            this.CommandSaved(this, command);
            this.Close();
        }

        public void ForceShow()
        {
            if (!this.IsVisible)
            {
                this.Show();
            }

            if (this.WindowState == System.Windows.WindowState.Minimized)
            {
                this.WindowState = System.Windows.WindowState.Normal;
            }

            this.Activate();
            this.Topmost = true;  // important
            this.Topmost = false; // important
            this.Focus();         // important
        }
    }
}
