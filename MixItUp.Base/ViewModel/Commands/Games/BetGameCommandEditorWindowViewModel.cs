using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Commands.Games;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Games
{
    public class BetGameCommandEditorWindowViewModel : GameCommandEditorWindowViewModelBase
    {
        public IEnumerable<UserRoleEnum> StarterRoles { get { return UserRoles.All; } }

        public UserRoleEnum SelectedStarterRole
        {
            get { return this.selectedStarterRole; }
            set
            {
                this.selectedStarterRole = value;
                this.NotifyPropertyChanged();
            }
        }
        private UserRoleEnum selectedStarterRole;

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

        public IEnumerable<RouletteGameCommandBetType> BetTypes { get { return EnumHelper.GetEnumList<RouletteGameCommandBetType>(); } }

        public RouletteGameCommandBetType SelectedBetType
        {
            get { return this.selectedBetType; }
            set
            {
                this.selectedBetType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("IsBetTypeNumberRange");
                this.NotifyPropertyChanged("IsBetTypeCustom");
            }
        }
        private RouletteGameCommandBetType selectedBetType;

        public bool IsBetTypeNumberRange { get { return this.SelectedBetType == RouletteGameCommandBetType.NumberRange; } }

        public int BetOptionsNumberRangeLow
        {
            get { return this.betOptionsNumberRangeLow; }
            set
            {
                this.betOptionsNumberRangeLow = value;
                this.NotifyPropertyChanged();
            }
        }
        private int betOptionsNumberRangeLow;

        public int BetOptionsNumberRangeHigh
        {
            get { return this.betOptionsBumberRangeHigh; }
            set
            {
                this.betOptionsBumberRangeHigh = value;
                this.NotifyPropertyChanged();
            }
        }
        private int betOptionsBumberRangeHigh;

        public bool IsBetTypeCustom { get { return this.SelectedBetType == RouletteGameCommandBetType.Custom; } }

        public string BetOptionsCustom
        {
            get { return this.betOptionsCustom; }
            set
            {
                this.betOptionsCustom = value;
                this.NotifyPropertyChanged();
            }
        }
        private string betOptionsCustom;

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

        public CustomCommandModel BetsClosedCommand
        {
            get { return this.betsClosedCommand; }
            set
            {
                this.betsClosedCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel betsClosedCommand;

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

        public BetGameCommandEditorWindowViewModel(BetGameCommandModel command)
            : base(command)
        {
            this.SelectedStarterRole = command.StarterUserRole;
            this.MinimumParticipants = command.MinimumParticipants;
            this.TimeLimit = command.TimeLimit;
            this.StartedCommand = command.StartedCommand;
            this.UserJoinCommand = command.UserJoinCommand;
            this.NotEnoughPlayersCommand = command.NotEnoughPlayersCommand;
            this.BetsClosedCommand = command.BetsClosedCommand;
            this.GameCompleteCommand = command.GameCompleteCommand;

            this.Outcomes.AddRange(command.BetOptions.Select(o => new GameOutcomeViewModel(o)));
        }

        public BetGameCommandEditorWindowViewModel(CurrencyModel currency)
            : base(currency)
        {
            this.Name = MixItUp.Base.Resources.Bet;
            this.Triggers = MixItUp.Base.Resources.Bet.Replace(" ", string.Empty).ToLower();

            this.SelectedStarterRole = UserRoleEnum.Moderator;
            this.MinimumParticipants = 2;
            this.TimeLimit = 60;
            this.StartedCommand = this.CreateBasicChatCommand(string.Format(MixItUp.Base.Resources.GameCommandBetStartedExample, this.PrimaryCurrencyName));
            this.UserJoinCommand = this.CreateBasicCommand();
            this.NotEnoughPlayersCommand = this.CreateBasicChatCommand(MixItUp.Base.Resources.GameCommandNotEnoughPlayersExample);
            this.BetsClosedCommand = this.CreateBasicChatCommand(MixItUp.Base.Resources.GameCommandBetBetsClosedExample);
            this.GameCompleteCommand = this.CreateBasicChatCommand(MixItUp.Base.Resources.GameCommandBetGameCompleteExample);
        }

        public override Task<CommandModelBase> CreateNewCommand()
        {
            return Task.FromResult<CommandModelBase>(new BetGameCommandModel(this.Name, this.GetChatTriggers(), this.SelectedStarterRole, this.MinimumParticipants, this.TimeLimit, this.Outcomes.Select(o => o.GetModel()),
                this.StartedCommand, this.UserJoinCommand, this.NotEnoughPlayersCommand, this.BetsClosedCommand, this.GameCompleteCommand));
        }

        public override async Task UpdateExistingCommand(CommandModelBase command)
        {
            await base.UpdateExistingCommand(command);
            BetGameCommandModel gCommand = (BetGameCommandModel)command;
            gCommand.StarterUserRole = this.SelectedStarterRole;
            gCommand.MinimumParticipants = this.MinimumParticipants;
            gCommand.TimeLimit = this.timeLimit;
            gCommand.BetOptions = new List<GameOutcomeModel>(this.Outcomes.Select(o => o.GetModel()));
            gCommand.StartedCommand = this.StartedCommand;
            gCommand.UserJoinCommand = this.UserJoinCommand;
            gCommand.NotEnoughPlayersCommand = this.NotEnoughPlayersCommand;
            gCommand.BetsClosedCommand = this.BetsClosedCommand;
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

            if (this.Outcomes.Count == 0)
            {
                return new Result(MixItUp.Base.Resources.GameCommandAtLeast1Outcome);
            }

            return new Result();
        }
    }
}