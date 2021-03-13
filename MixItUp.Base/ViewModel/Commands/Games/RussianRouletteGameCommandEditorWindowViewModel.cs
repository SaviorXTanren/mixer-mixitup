using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Commands.Games;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Util;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Games
{
    public class RussianRouletteGameCommandEditorWindowViewModel : GameCommandEditorWindowViewModelBase
    {
        public int MinimumParticipants
        {
            get { return this.minimumParticipants; }
            set
            {
                this.minimumParticipants = value;
                this.NotifyPropertyChanged();
            }
        }
        private int minimumParticipants;

        public int TimeLimit
        {
            get { return this.timeLimit; }
            set
            {
                this.timeLimit = value;
                this.NotifyPropertyChanged();
            }
        }
        private int timeLimit;

        public int MaxWinners
        {
            get { return this.maxWinners; }
            set
            {
                this.maxWinners = value;
                this.NotifyPropertyChanged();
            }
        }
        private int maxWinners;

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

        public CustomCommandModel UserJoinCommand
        {
            get { return this.userJoinCommand; }
            set
            {
                this.userJoinCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel userJoinCommand;

        public CustomCommandModel NotEnoughPlayersCommand
        {
            get { return this.notEnoughPlayersCommand; }
            set
            {
                this.notEnoughPlayersCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel notEnoughPlayersCommand;

        public CustomCommandModel UserSuccessCommand
        {
            get { return this.userSuccessCommand; }
            set
            {
                this.userSuccessCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel userSuccessCommand;

        public CustomCommandModel UserFailureCommand
        {
            get { return this.userFailureCommand; }
            set
            {
                this.userFailureCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel userFailureCommand;

        public CustomCommandModel GameCompleteCommand
        {
            get { return this.gameCompleteCommand; }
            set
            {
                this.gameCompleteCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel gameCompleteCommand;

        public RussianRouletteGameCommandEditorWindowViewModel(RussianRouletteGameCommandModel command)
            : base(command)
        {
            this.MinimumParticipants = command.MinimumParticipants;
            this.TimeLimit = command.TimeLimit;
            this.MaxWinners = command.MaxWinners;
            this.StartedCommand = command.StartedCommand;
            this.UserJoinCommand = command.UserJoinCommand;
            this.NotEnoughPlayersCommand = command.NotEnoughPlayersCommand;
            this.UserSuccessCommand = command.UserSuccessCommand;
            this.UserFailureCommand = command.UserFailureCommand;
            this.GameCompleteCommand = command.GameCompleteCommand;
        }

        public RussianRouletteGameCommandEditorWindowViewModel(CurrencyModel currency)
            : base(currency)
        {
            this.Name = MixItUp.Base.Resources.RussianRoulette;
            this.Triggers = MixItUp.Base.Resources.GameCommandRussianRouletteDefaultChatTrigger;

            this.MinimumParticipants = 2;
            this.TimeLimit = 60;
            this.MaxWinners = 1;
            this.StartedCommand = this.CreateBasicChatCommand(string.Format(MixItUp.Base.Resources.GameCommandRussianRouletteStartedExample, this.PrimaryCurrencyName));
            this.UserJoinCommand = this.CreateBasicCommand();
            this.NotEnoughPlayersCommand = this.CreateBasicChatCommand(MixItUp.Base.Resources.GameCommandNotEnoughPlayersExample);
            this.UserSuccessCommand = this.CreateBasicCommand();
            this.UserFailureCommand = this.CreateBasicCommand();
            this.GameCompleteCommand = this.CreateBasicChatCommand(string.Format(MixItUp.Base.Resources.GameCommandRussianRouletteGameCompleteExample, this.PrimaryCurrencyName));
        }

        public override Task<CommandModelBase> CreateNewCommand()
        {
            return Task.FromResult<CommandModelBase>(new RussianRouletteGameCommandModel(this.Name, this.GetChatTriggers(), this.MinimumParticipants, this.TimeLimit, this.MaxWinners, this.StartedCommand,
                this.UserJoinCommand, this.NotEnoughPlayersCommand, this.UserSuccessCommand, this.UserFailureCommand, this.GameCompleteCommand));
        }

        public override async Task UpdateExistingCommand(CommandModelBase command)
        {
            await base.UpdateExistingCommand(command);
            RussianRouletteGameCommandModel gCommand = (RussianRouletteGameCommandModel)command;
            gCommand.MinimumParticipants = this.MinimumParticipants;
            gCommand.TimeLimit = this.TimeLimit;
            gCommand.MaxWinners = this.MaxWinners;
            gCommand.StartedCommand = this.StartedCommand;
            gCommand.UserJoinCommand = this.UserJoinCommand;
            gCommand.NotEnoughPlayersCommand = this.NotEnoughPlayersCommand;
            gCommand.UserSuccessCommand = this.UserSuccessCommand;
            gCommand.UserFailureCommand = this.UserFailureCommand;
            gCommand.GameCompleteCommand = this.GameCompleteCommand;
        }

        public override async Task<Result> Validate()
        {
            Result result = await base.Validate();
            if (!result.Success)
            {
                return result;
            }

            if (this.MinimumParticipants < 1)
            {
                return new Result(MixItUp.Base.Resources.GameCommandMinimumParticipantsMustBeGreaterThan0);
            }

            if (this.TimeLimit <= 0)
            {
                return new Result(MixItUp.Base.Resources.GameCommandTimeLimitMustBePositive);
            }

            if (this.MaxWinners <= 0)
            {
                return new Result(MixItUp.Base.Resources.GameCommandRussianRouletteMaxWinnersMustBeGreaterThan0);
            }

            return new Result();
        }
    }
}