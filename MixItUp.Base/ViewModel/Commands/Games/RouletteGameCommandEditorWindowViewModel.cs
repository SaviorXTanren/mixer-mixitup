using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Commands.Games;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Games
{
    public class RouletteGameCommandEditorWindowViewModel : GameCommandEditorWindowViewModelBase
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

        public GameOutcomeViewModel UserSuccessOutcome
        {
            get { return this.userSuccessOutcome; }
            set
            {
                this.userSuccessOutcome = value;
                this.NotifyPropertyChanged();
            }
        }
        private GameOutcomeViewModel userSuccessOutcome;

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

        public RouletteGameCommandEditorWindowViewModel(RouletteGameCommandModel command)
            : base(command)
        {
            this.MinimumParticipants = command.MinimumParticipants;
            this.TimeLimit = command.TimeLimit;
            this.SelectedBetType = command.BetType;
            if (this.SelectedBetType == RouletteGameCommandBetType.NumberRange)
            {
                this.BetOptionsNumberRangeLow = int.Parse(command.BetOptions.ElementAt(0));
                this.BetOptionsNumberRangeHigh = int.Parse(command.BetOptions.ElementAt(1));
            }
            else if (this.SelectedBetType == RouletteGameCommandBetType.Custom)
            {
                this.BetOptionsCustom = string.Join(" ", command.BetOptions);
            }
            this.StartedCommand = command.StartedCommand;
            this.UserJoinCommand = command.UserJoinCommand;
            this.NotEnoughPlayersCommand = command.NotEnoughPlayersCommand;
            this.UserSuccessOutcome = new GameOutcomeViewModel(command.UserSuccessOutcome);
            this.UserFailureCommand = command.UserFailureCommand;
            this.GameCompleteCommand = command.GameCompleteCommand;
        }

        public RouletteGameCommandEditorWindowViewModel(CurrencyModel currency)
            : base(currency)
        {
            this.Name = MixItUp.Base.Resources.Roulette;
            this.Triggers = MixItUp.Base.Resources.Roulette.Replace(" ", string.Empty).ToLower();

            this.MinimumParticipants = 2;
            this.TimeLimit = 60;
            this.SelectedBetType = RouletteGameCommandBetType.NumberRange;
            this.BetOptionsNumberRangeLow = 1;
            this.BetOptionsNumberRangeHigh = 9;
            this.StartedCommand = this.CreateBasicChatCommand(string.Format(MixItUp.Base.Resources.GameCommandRouletteStartedExample, this.PrimaryCurrencyName));
            this.UserJoinCommand = this.CreateBasicCommand();
            this.NotEnoughPlayersCommand = this.CreateBasicChatCommand(MixItUp.Base.Resources.GameCommandNotEnoughPlayersExample);
            this.UserSuccessOutcome = new GameOutcomeViewModel(string.Empty, 0, 200);
            this.UserFailureCommand = this.CreateBasicCommand();
            this.GameCompleteCommand = this.CreateBasicChatCommand(string.Format(MixItUp.Base.Resources.GameCommandRouletteGameCompleteExample, this.PrimaryCurrencyName));
        }

        public IEnumerable<string> BetOptionsCustomList { get { return (!string.IsNullOrEmpty(this.BetOptionsCustom)) ? this.BetOptionsCustom.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>(); } }

        public override Task<CommandModelBase> CreateNewCommand()
        {
            return Task.FromResult<CommandModelBase>(new RouletteGameCommandModel(this.Name, this.GetChatTriggers(), this.MinimumParticipants, this.TimeLimit, this.SelectedBetType, this.GetBetOptions(), this.StartedCommand,
                this.UserJoinCommand, this.NotEnoughPlayersCommand, this.UserSuccessOutcome.GetModel(), this.UserFailureCommand, this.GameCompleteCommand));
        }

        public override async Task UpdateExistingCommand(CommandModelBase command)
        {
            await base.UpdateExistingCommand(command);
            RouletteGameCommandModel gCommand = (RouletteGameCommandModel)command;
            gCommand.MinimumParticipants = this.MinimumParticipants;
            gCommand.TimeLimit = this.TimeLimit;
            gCommand.BetType = this.SelectedBetType;
            gCommand.BetOptions = this.GetBetOptions();
            gCommand.StartedCommand = this.StartedCommand;
            gCommand.UserJoinCommand = this.UserJoinCommand;
            gCommand.NotEnoughPlayersCommand = this.NotEnoughPlayersCommand;
            gCommand.UserSuccessOutcome = this.UserSuccessOutcome.GetModel();
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

            if (this.SelectedBetType == RouletteGameCommandBetType.NumberRange)
            {
                if (this.BetOptionsNumberRangeLow < 0 || this.BetOptionsNumberRangeHigh < 0 || this.BetOptionsNumberRangeHigh < this.BetOptionsNumberRangeLow)
                {
                    return new Result(MixItUp.Base.Resources.GameCommandRouletteBetOptionsNumberRangeInvalid);
                }
            }
            else if (this.SelectedBetType == RouletteGameCommandBetType.Custom)
            {
                IEnumerable<string> symbolsList = this.BetOptionsCustomList;
                if (symbolsList.Count() < 2)
                {
                    return new Result(MixItUp.Base.Resources.GameCommandRouletteBetOptionsCustomMustBe2OrMore);
                }

                if (symbolsList.GroupBy(s => s).Any(g => g.Count() > 1))
                {
                    return new Result(MixItUp.Base.Resources.GameCommandRouletteBetOptionsCustomMustBeUnique);
                }
            }
            return new Result();
        }

        private HashSet<string> GetBetOptions()
        {
            HashSet<string> betOptions = new HashSet<string>();
            if (this.SelectedBetType == RouletteGameCommandBetType.NumberRange)
            {
                betOptions.Add(this.BetOptionsNumberRangeLow.ToString());
                betOptions.Add(this.BetOptionsNumberRangeHigh.ToString());
            }
            else if (this.SelectedBetType == RouletteGameCommandBetType.Custom)
            {
                betOptions = new HashSet<string>(this.BetOptionsCustomList);
            }
            return betOptions;
        }
    }
}