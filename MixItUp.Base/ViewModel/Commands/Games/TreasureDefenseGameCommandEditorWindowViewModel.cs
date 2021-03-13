using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Commands.Games;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Util;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Games
{
    public class TreasureDefenseGameCommandEditorWindowViewModel : GameCommandEditorWindowViewModelBase
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

        public int KingTimeLimit
        {
            get { return this.kingTimeLimit; }
            set
            {
                this.kingTimeLimit = value;
                this.NotifyPropertyChanged();
            }
        }
        private int kingTimeLimit;

        public int ThiefPlayerPercentage
        {
            get { return this.thiefPlayerPercentage; }
            set
            {
                this.thiefPlayerPercentage = value;
                this.NotifyPropertyChanged();
            }
        }
        private int thiefPlayerPercentage;

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

        public CustomCommandModel KnightUserCommand
        {
            get { return this.knightUserCommand; }
            set
            {
                this.knightUserCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel knightUserCommand;

        public CustomCommandModel ThiefUserCommand
        {
            get { return this.thiefUserCommand; }
            set
            {
                this.thiefUserCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel thiefUserCommand;

        public CustomCommandModel KingUserCommand
        {
            get { return this.kingUserCommand; }
            set
            {
                this.kingUserCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel kingUserCommand;

        public CustomCommandModel KnightSelectedCommand
        {
            get { return this.knightSelectedCommand; }
            set
            {
                this.knightSelectedCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel knightSelectedCommand;

        public CustomCommandModel ThiefSelectedCommand
        {
            get { return this.thiefSelectedCommand; }
            set
            {
                this.thiefSelectedCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel thiefSelectedCommand;

        public TreasureDefenseGameCommandEditorWindowViewModel(TreasureDefenseGameCommandModel command)
            : base(command)
        {
            this.MinimumParticipants = command.MinimumParticipants;
            this.TimeLimit = command.TimeLimit;
            this.KingTimeLimit = command.KingTimeLimit;
            this.ThiefPlayerPercentage = command.ThiefPlayerPercentage;
            this.StartedCommand = command.StartedCommand;
            this.UserJoinCommand = command.UserJoinCommand;
            this.NotEnoughPlayersCommand = command.NotEnoughPlayersCommand;
            this.KnightUserCommand = command.KnightUserCommand;
            this.ThiefUserCommand = command.ThiefUserCommand;
            this.KingUserCommand = command.KingUserCommand;
            this.KnightSelectedCommand = command.KnightSelectedCommand;
            this.ThiefSelectedCommand = command.ThiefSelectedCommand;
        }

        public TreasureDefenseGameCommandEditorWindowViewModel(CurrencyModel currency)
            : base(currency)
        {
            this.Name = MixItUp.Base.Resources.TreasureDefense;
            this.Triggers = MixItUp.Base.Resources.GameCommandTreasureDefenseDefaultChatTrigger;

            this.MinimumParticipants = 2;
            this.TimeLimit = 60;
            this.KingTimeLimit = 300;
            this.ThiefPlayerPercentage = 50;
            this.StartedCommand = this.CreateBasicChatCommand(string.Format(MixItUp.Base.Resources.GameCommandTreasureDefenseStartedExample, this.PrimaryCurrencyName));
            this.UserJoinCommand = this.CreateBasicCommand();
            this.NotEnoughPlayersCommand = this.CreateBasicChatCommand(MixItUp.Base.Resources.GameCommandNotEnoughPlayersExample);
            this.KnightUserCommand = this.CreateBasicChatCommand(MixItUp.Base.Resources.GameCommandTreasureDefenseKnightUserExample, whisper: true);
            this.ThiefUserCommand = this.CreateBasicChatCommand(MixItUp.Base.Resources.GameCommandTreasureDefenseThiefUserExample, whisper: true);
            this.KingUserCommand = this.CreateBasicChatCommand(MixItUp.Base.Resources.GameCommandTreasureDefenseKingUserExample);
            this.KnightSelectedCommand = this.CreateBasicChatCommand(string.Format(MixItUp.Base.Resources.GameCommandTreasureDefenseKnightSelectedExample, this.PrimaryCurrencyName));
            this.ThiefSelectedCommand = this.CreateBasicChatCommand(string.Format(MixItUp.Base.Resources.GameCommandTreasureDefenseThiefSelectedExample, this.PrimaryCurrencyName));
        }

        public override Task<CommandModelBase> CreateNewCommand()
        {
            return Task.FromResult<CommandModelBase>(new TreasureDefenseGameCommandModel(this.Name, this.GetChatTriggers(), this.MinimumParticipants, this.TimeLimit, this.KingTimeLimit, this.ThiefPlayerPercentage, this.StartedCommand,
                this.UserJoinCommand, this.NotEnoughPlayersCommand, this.KnightUserCommand, this.ThiefUserCommand, this.KingUserCommand, this.KnightSelectedCommand, this.ThiefSelectedCommand));
        }

        public override async Task UpdateExistingCommand(CommandModelBase command)
        {
            await base.UpdateExistingCommand(command);
            TreasureDefenseGameCommandModel gCommand = (TreasureDefenseGameCommandModel)command;
            gCommand.MinimumParticipants = this.MinimumParticipants;
            gCommand.TimeLimit = this.TimeLimit;
            gCommand.KingTimeLimit = this.KingTimeLimit;
            gCommand.ThiefPlayerPercentage = this.ThiefPlayerPercentage;
            gCommand.StartedCommand = this.StartedCommand;
            gCommand.UserJoinCommand = this.UserJoinCommand;
            gCommand.NotEnoughPlayersCommand = this.NotEnoughPlayersCommand;
            gCommand.KnightUserCommand = this.KnightUserCommand;
            gCommand.ThiefUserCommand = this.ThiefUserCommand;
            gCommand.KingUserCommand = this.KingUserCommand;
            gCommand.KnightSelectedCommand = this.KnightSelectedCommand;
            gCommand.ThiefSelectedCommand = this.ThiefSelectedCommand;
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

            if (this.TimeLimit <= 0 || this.KingTimeLimit <= 0)
            {
                return new Result(MixItUp.Base.Resources.GameCommandTimeLimitMustBePositive);
            }

            if (this.ThiefPlayerPercentage <= 0)
            {
                return new Result(MixItUp.Base.Resources.GameCommandTreasureDefenseThiefPlayerPercentageMustBeGreaterThan0);
            }

            return new Result();
        }
    }
}