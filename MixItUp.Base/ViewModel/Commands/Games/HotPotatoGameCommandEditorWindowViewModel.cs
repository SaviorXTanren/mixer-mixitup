using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Commands.Games;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Util;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Games
{
    public class HotPotatoGameCommandEditorWindowViewModel : GameCommandEditorWindowViewModelBase
    {
        public int LowerTimeLimit
        {
            get { return this.lowerTimeLimit; }
            set
            {
                this.lowerTimeLimit = value;
                this.NotifyPropertyChanged();
            }
        }
        private int lowerTimeLimit;

        public int UpperTimeLimit
        {
            get { return this.upperTimeLimit; }
            set
            {
                this.upperTimeLimit = value;
                this.NotifyPropertyChanged();
            }
        }
        private int upperTimeLimit;

        public bool UserSelectionTargeted
        {
            get { return this.userSelectionTargeted; }
            set
            {
                this.userSelectionTargeted = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool userSelectionTargeted;

        public bool UserSelectionRandom
        {
            get { return this.userSelectionRandom; }
            set
            {
                this.userSelectionRandom = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool userSelectionRandom;

        public CustomCommandModel StartedCommand
        {
            get { return this.startedCommand; }
            set
            {
                this.startedCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel startedCommand;

        public CustomCommandModel TossPotatoCommand
        {
            get { return this.tossPotatoCommand; }
            set
            {
                this.tossPotatoCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel tossPotatoCommand;

        public CustomCommandModel PotatoExplodeCommand
        {
            get { return this.potatoExplodeCommand; }
            set
            {
                this.potatoExplodeCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel potatoExplodeCommand;

        public HotPotatoGameCommandEditorWindowViewModel(HotPotatoGameCommandModel command)
            : base(command)
        {
            this.LowerTimeLimit = command.LowerTimeLimit;
            this.UpperTimeLimit = command.UpperTimeLimit;
            this.UserSelectionTargeted = command.SelectionType.HasFlag(GamePlayerSelectionType.Targeted);
            this.UserSelectionRandom = command.SelectionType.HasFlag(GamePlayerSelectionType.Random);
            this.StartedCommand = command.StartedCommand;
            this.TossPotatoCommand = command.TossPotatoCommand;
            this.PotatoExplodeCommand = command.PotatoExplodeCommand;
        }

        public HotPotatoGameCommandEditorWindowViewModel(CurrencyModel currency)
            : base(currency)
        {
            this.LowerTimeLimit = 30;
            this.UpperTimeLimit = 60;
            this.UserSelectionTargeted = true;
            this.UserSelectionRandom = true;
            this.StartedCommand = this.CreateBasicChatCommand(MixItUp.Base.Resources.GameCommandHotPotatoStartedExample);
            this.TossPotatoCommand = this.CreateBasicChatCommand(MixItUp.Base.Resources.GameCommandHotPotatoPassedExample);
            this.PotatoExplodeCommand = this.CreateBasicChatCurrencyCommand(string.Format(MixItUp.Base.Resources.GameCommandHotPotatoExplodedExample, 100, currency.Name), currency, "100");
        }

        public override Task<CommandModelBase> GetCommand()
        {
#pragma warning disable CS0612 // Type or member is obsolete
            GamePlayerSelectionType selectionType = GamePlayerSelectionType.None;
#pragma warning restore CS0612 // Type or member is obsolete
            if (this.UserSelectionTargeted) { selectionType |= GamePlayerSelectionType.Targeted; }
            if (this.UserSelectionRandom) { selectionType |= GamePlayerSelectionType.Random; }

            return Task.FromResult<CommandModelBase>(new HotPotatoGameCommandModel(this.Name, this.GetChatTriggers(), this.LowerTimeLimit, this.UpperTimeLimit, selectionType, this.StartedCommand, this.TossPotatoCommand, this.PotatoExplodeCommand));
        }

        public override async Task<Result> Validate()
        {
            Result result = await base.Validate();
            if (!result.Success)
            {
                return result;
            }

            if (this.LowerTimeLimit < 0 || this.UpperTimeLimit < 0 || this.UpperTimeLimit < this.LowerTimeLimit)
            {
                return new Result(MixItUp.Base.Resources.GameCommandTimeLimitMustBePositive);
            }

            if (!this.UserSelectionTargeted && !this.UserSelectionRandom)
            {
                return new Result(MixItUp.Base.Resources.GameCommandOneUserSelectionTypeMustBeSelected);
            }

            return new Result();
        }
    }
}