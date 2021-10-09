using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Commands.Games;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.Requirements;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Games
{
    public class BidGameCommandEditorWindowViewModel : GameCommandEditorWindowViewModelBase
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

        public CustomCommandModel NewTopBidderCommand
        {
            get { return this.newTopBidderCommand; }
            set
            {
                this.newTopBidderCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel newTopBidderCommand;

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

        public BidGameCommandEditorWindowViewModel(BidGameCommandModel command)
            : base(command)
        {
            this.SelectedStarterRole = command.StarterUserRole;
            this.TimeLimit = command.TimeLimit;
            this.StartedCommand = command.StartedCommand;
            this.NewTopBidderCommand = command.NewTopBidderCommand;
            this.NotEnoughPlayersCommand = command.NotEnoughPlayersCommand;
            this.GameCompleteCommand = command.GameCompleteCommand;
        }

        public BidGameCommandEditorWindowViewModel(CurrencyModel currency)
            : base(currency)
        {
            this.Name = MixItUp.Base.Resources.Bid;
            this.Triggers = MixItUp.Base.Resources.Bid.Replace(" ", string.Empty).ToLower();

            this.SelectedStarterRole = UserRoleEnum.Moderator;
            this.TimeLimit = 60;
            this.StartedCommand = this.CreateBasicChatCommand(string.Format(MixItUp.Base.Resources.GameCommandBidStartedExample, this.PrimaryCurrencyName));
            this.NewTopBidderCommand = this.CreateBasicChatCommand(string.Format(MixItUp.Base.Resources.GameCommandBidNewTopBidderExample, this.PrimaryCurrencyName));
            this.NotEnoughPlayersCommand = this.CreateBasicChatCommand(MixItUp.Base.Resources.GameCommandNotEnoughPlayersExample);
            this.GameCompleteCommand = this.CreateBasicChatCommand(string.Format(MixItUp.Base.Resources.GameCommandBidGameCompleteExample, this.PrimaryCurrencyName));

            if (this.Requirements.Currency.Items.Count > 0)
            {
                this.Requirements.Currency.Items.FirstOrDefault().SelectedRequirementType = CurrencyRequirementTypeEnum.MinimumOnly;
            }
        }

        public override bool RequirePrimaryCurrency { get { return true; } }

        public override Task<CommandModelBase> CreateNewCommand()
        {
            return Task.FromResult<CommandModelBase>(new BidGameCommandModel(this.Name, this.GetChatTriggers(), this.SelectedStarterRole, this.TimeLimit, this.StartedCommand, this.NewTopBidderCommand,
                this.NotEnoughPlayersCommand, this.GameCompleteCommand));
        }

        public override async Task UpdateExistingCommand(CommandModelBase command)
        {
            await base.UpdateExistingCommand(command);
            BidGameCommandModel gCommand = (BidGameCommandModel)command;
            gCommand.StarterUserRole = this.SelectedStarterRole;
            gCommand.TimeLimit = this.timeLimit;
            gCommand.StartedCommand = this.StartedCommand;
            gCommand.NewTopBidderCommand = this.NewTopBidderCommand;
            gCommand.NotEnoughPlayersCommand = this.NotEnoughPlayersCommand;
            gCommand.GameCompleteCommand = this.GameCompleteCommand;
        }

        public override async Task<Result> Validate()
        {
            Result result = await base.Validate();
            if (!result.Success)
            {
                return result;
            }

            if (this.TimeLimit < 0)
            {
                return new Result(MixItUp.Base.Resources.GameCommandTimeLimitMustBePositive);
            }

            if (this.Requirements.Currency.Items.Count > 0)
            {
                if (this.Requirements.Currency.Items.First().SelectedRequirementType != CurrencyRequirementTypeEnum.MinimumOnly)
                {
                    return new Result(MixItUp.Base.Resources.GameCommandBidPrimaryCurrencyMustBeMinimumOnly);
                }
            }
            else
            {
                return new Result(MixItUp.Base.Resources.GameCommandRequiresAtLeast1CurrencyRequirement);
            }

            return new Result();
        }
    }
}