using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Commands.Games;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.ViewModel.Games;
using MixItUp.WPF.Controls.Commands;
using MixItUp.WPF.Controls.Commands.Games;
using System;
using System.Threading.Tasks;

namespace MixItUp.WPF.Windows.Commands
{
    /// <summary>
    /// Interaction logic for GameCommandEditorWindow.xaml
    /// </summary>
    public partial class GameCommandEditorWindow : LoadingWindowBase
    {
        public CommandEditorDetailsControlBase editorDetailsControl { get; private set; }

        public GameCommandEditorWindowViewModelBase viewModel { get; private set; }

        public event EventHandler<CommandModelBase> CommandSaved = delegate { };

        public GameCommandEditorWindow(GameCommandModelBase existingCommand)
            : this()
        {
            switch (existingCommand.GameType)
            {
                case GameCommandTypeEnum.Bet:
                    this.editorDetailsControl = new BetGameCommandEditorDetailsControl();
                    this.viewModel = new BetGameCommandEditorWindowViewModel((BetGameCommandModel)existingCommand);
                    break;
                case GameCommandTypeEnum.Bid:
                    this.editorDetailsControl = new BidGameCommandEditorDetailsControl();
                    this.viewModel = new BidGameCommandEditorWindowViewModel((BidGameCommandModel)existingCommand);
                    break;
                case GameCommandTypeEnum.CoinPusher:
                    this.editorDetailsControl = new CoinPusherGameCommandEditorDetailsControl();
                    this.viewModel = new CoinPusherGameCommandEditorWindowViewModel((CoinPusherGameCommandModel)existingCommand);
                    break;
                case GameCommandTypeEnum.Duel:
                    this.editorDetailsControl = new DuelGameCommandEditorDetailsControl();
                    this.viewModel = new DuelGameCommandEditorWindowViewModel((DuelGameCommandModel)existingCommand);
                    break;
                case GameCommandTypeEnum.Hangman:
                    this.editorDetailsControl = new HangmanGameCommandEditorDetailsControl();
                    this.viewModel = new HangmanGameCommandEditorWindowViewModel((HangmanGameCommandModel)existingCommand);
                    break;
                case GameCommandTypeEnum.Heist:
                    this.editorDetailsControl = new HeistGameCommandEditorDetailsControl();
                    this.viewModel = new HeistGameCommandEditorWindowViewModel((HeistGameCommandModel)existingCommand);
                    break;
                case GameCommandTypeEnum.Hitman:
                    this.editorDetailsControl = new HitmanGameCommandEditorDetailsControl();
                    this.viewModel = new HitmanGameCommandEditorWindowViewModel((HitmanGameCommandModel)existingCommand);
                    break;
                case GameCommandTypeEnum.HotPotato:
                    this.editorDetailsControl = new HotPotatoGameCommandEditorDetailsControl();
                    this.viewModel = new HotPotatoGameCommandEditorWindowViewModel((HotPotatoGameCommandModel)existingCommand);
                    break;
                case GameCommandTypeEnum.LockBox:
                    this.editorDetailsControl = new LockBoxGameCommandEditorDetailsControl();
                    this.viewModel = new LockBoxGameCommandEditorWindowViewModel((LockBoxGameCommandModel)existingCommand);
                    break;
                case GameCommandTypeEnum.Roulette:
                    this.editorDetailsControl = new RouletteGameCommandEditorDetailsControl();
                    this.viewModel = new RouletteGameCommandEditorWindowViewModel((RouletteGameCommandModel)existingCommand);
                    break;
                case GameCommandTypeEnum.RussianRoulette:
                    this.editorDetailsControl = new RussianRouletteGameCommandEditorDetailsControl();
                    this.viewModel = new RussianRouletteGameCommandEditorWindowViewModel((RussianRouletteGameCommandModel)existingCommand);
                    break;
                case GameCommandTypeEnum.SlotMachine:
                    this.editorDetailsControl = new SlotMachineGameCommandEditorDetailsControl();
                    this.viewModel = new SlotMachineGameCommandEditorWindowViewModel((SlotMachineGameCommandModel)existingCommand);
                    break;
                case GameCommandTypeEnum.Spin:
                    this.editorDetailsControl = new SpinGameCommandEditorDetailsControl();
                    this.viewModel = new SpinGameCommandEditorWindowViewModel((SpinGameCommandModel)existingCommand);
                    break;
                case GameCommandTypeEnum.Steal:
                    this.editorDetailsControl = new StealGameCommandEditorDetailsControl();
                    this.viewModel = new StealGameCommandEditorWindowViewModel((StealGameCommandModel)existingCommand);
                    break;
                case GameCommandTypeEnum.TreasureDefense:
                    this.editorDetailsControl = new TreasureDefenseGameCommandEditorDetailsControl();
                    this.viewModel = new TreasureDefenseGameCommandEditorWindowViewModel((TreasureDefenseGameCommandModel)existingCommand);
                    break;
                case GameCommandTypeEnum.Trivia:
                    this.editorDetailsControl = new TriviaGameCommandEditorDetailsControl();
                    this.viewModel = new TriviaGameCommandEditorWindowViewModel((TriviaGameCommandModel)existingCommand);
                    break;
                case GameCommandTypeEnum.Volcano:
                    this.editorDetailsControl = new VolcanoGameCommandEditorDetailsControl();
                    this.viewModel = new VolcanoGameCommandEditorWindowViewModel((VolcanoGameCommandModel)existingCommand);
                    break;
                case GameCommandTypeEnum.WordScramble:
                    this.editorDetailsControl = new WordScrambleGameCommandEditorDetailsControl();
                    this.viewModel = new WordScrambleGameCommandEditorWindowViewModel((WordScrambleGameCommandModel)existingCommand);
                    break;
            }

            this.DataContext = this.ViewModel = this.viewModel;
            this.ViewModel.StartLoadingOperationOccurred += (sender, eventArgs) => { this.StartLoadingOperation(); };
            this.ViewModel.EndLoadingOperationOccurred += (sender, eventArgs) => { this.EndLoadingOperation(); };
        }

        public GameCommandEditorWindow(GameCommandTypeEnum gameType, CurrencyModel currency)
            : this()
        {
            switch (gameType)
            {
                case GameCommandTypeEnum.Bet:
                    this.editorDetailsControl = new BetGameCommandEditorDetailsControl();
                    this.viewModel = new BetGameCommandEditorWindowViewModel(currency);
                    break;
                case GameCommandTypeEnum.Bid:
                    this.editorDetailsControl = new BidGameCommandEditorDetailsControl();
                    this.viewModel = new BidGameCommandEditorWindowViewModel(currency);
                    break;
                case GameCommandTypeEnum.CoinPusher:
                    this.editorDetailsControl = new CoinPusherGameCommandEditorDetailsControl();
                    this.viewModel = new CoinPusherGameCommandEditorWindowViewModel(currency);
                    break;
                case GameCommandTypeEnum.Duel:
                    this.editorDetailsControl = new DuelGameCommandEditorDetailsControl();
                    this.viewModel = new DuelGameCommandEditorWindowViewModel(currency);
                    break;
                case GameCommandTypeEnum.Hangman:
                    this.editorDetailsControl = new HangmanGameCommandEditorDetailsControl();
                    this.viewModel = new HangmanGameCommandEditorWindowViewModel(currency);
                    break;
                case GameCommandTypeEnum.Heist:
                    this.editorDetailsControl = new HeistGameCommandEditorDetailsControl();
                    this.viewModel = new HeistGameCommandEditorWindowViewModel(currency);
                    break;
                case GameCommandTypeEnum.Hitman:
                    this.editorDetailsControl = new HitmanGameCommandEditorDetailsControl();
                    this.viewModel = new HitmanGameCommandEditorWindowViewModel(currency);
                    break;
                case GameCommandTypeEnum.HotPotato:
                    this.editorDetailsControl = new HotPotatoGameCommandEditorDetailsControl();
                    this.viewModel = new HotPotatoGameCommandEditorWindowViewModel(currency);
                    break;
                case GameCommandTypeEnum.LockBox:
                    this.editorDetailsControl = new LockBoxGameCommandEditorDetailsControl();
                    this.viewModel = new LockBoxGameCommandEditorWindowViewModel(currency);
                    break;
                case GameCommandTypeEnum.Roulette:
                    this.editorDetailsControl = new RouletteGameCommandEditorDetailsControl();
                    this.viewModel = new RouletteGameCommandEditorWindowViewModel(currency);
                    break;
                case GameCommandTypeEnum.RussianRoulette:
                    this.editorDetailsControl = new RussianRouletteGameCommandEditorDetailsControl();
                    this.viewModel = new RussianRouletteGameCommandEditorWindowViewModel(currency);
                    break;
                case GameCommandTypeEnum.SlotMachine:
                    this.editorDetailsControl = new SlotMachineGameCommandEditorDetailsControl();
                    this.viewModel = new SlotMachineGameCommandEditorWindowViewModel(currency);
                    break;
                case GameCommandTypeEnum.Spin:
                    this.editorDetailsControl = new SpinGameCommandEditorDetailsControl();
                    this.viewModel = new SpinGameCommandEditorWindowViewModel(currency);
                    break;
                case GameCommandTypeEnum.Steal:
                    this.editorDetailsControl = new StealGameCommandEditorDetailsControl();
                    this.viewModel = new StealGameCommandEditorWindowViewModel(currency);
                    break;
                case GameCommandTypeEnum.TreasureDefense:
                    this.editorDetailsControl = new TreasureDefenseGameCommandEditorDetailsControl();
                    this.viewModel = new TreasureDefenseGameCommandEditorWindowViewModel(currency);
                    break;
                case GameCommandTypeEnum.Trivia:
                    this.editorDetailsControl = new TriviaGameCommandEditorDetailsControl();
                    this.viewModel = new TriviaGameCommandEditorWindowViewModel(currency);
                    break;
                case GameCommandTypeEnum.Volcano:
                    this.editorDetailsControl = new VolcanoGameCommandEditorDetailsControl();
                    this.viewModel = new VolcanoGameCommandEditorWindowViewModel(currency);
                    break;
                case GameCommandTypeEnum.WordScramble:
                    this.editorDetailsControl = new WordScrambleGameCommandEditorDetailsControl();
                    this.viewModel = new WordScrambleGameCommandEditorWindowViewModel(currency);
                    break;
            }

            this.DataContext = this.ViewModel = this.viewModel;
            this.ViewModel.StartLoadingOperationOccurred += (sender, eventArgs) => { this.StartLoadingOperation(); };
            this.ViewModel.EndLoadingOperationOccurred += (sender, eventArgs) => { this.EndLoadingOperation(); };
        }

        private GameCommandEditorWindow()
        {
            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            if (this.viewModel != null)
            {
                this.viewModel.CommandSaved += ViewModel_CommandSaved;
                await this.viewModel.OnOpen();

                this.DetailsContentControl.Content = this.editorDetailsControl;
            }
            await base.OnLoaded();
        }

        private void ViewModel_CommandSaved(object sender, CommandModelBase command)
        {
            this.CommandSaved(this, command);
            this.Close();
        }
    }
}