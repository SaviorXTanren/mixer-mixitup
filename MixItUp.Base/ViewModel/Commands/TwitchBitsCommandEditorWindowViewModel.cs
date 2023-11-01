using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Commands
{
    public class TwitchBitsCommandEditorWindowViewModel : CommandEditorWindowViewModelBase
    {
        public bool IsRange
        {
            get { return this.isRange; }
            set
            {
                this.isRange = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.IsSingle));
            }
        }
        private bool isRange;

        public bool IsSingle { get { return !this.IsRange; } }

        public int StartingAmount
        {
            get { return this.startingAmount; }
            set
            {
                this.startingAmount = value;
                this.NotifyPropertyChanged();
            }
        }
        private int startingAmount;

        public int EndingAmount
        {
            get { return this.endingAmount; }
            set
            {
                this.endingAmount = value;
                this.NotifyPropertyChanged();
            }
        }
        private int endingAmount;

        public TwitchBitsCommandEditorWindowViewModel(TwitchBitsCommandModel existingCommand)
            : base(existingCommand)
        {
            this.StartingAmount = existingCommand.StartingAmount;
            this.EndingAmount = existingCommand.EndingAmount;
            this.IsRange = existingCommand.IsRange;
        }

        public TwitchBitsCommandEditorWindowViewModel() : base(CommandTypeEnum.TwitchBits) { }

        public override Task<Result> Validate()
        {
            if (string.IsNullOrWhiteSpace(this.Name))
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.ACommandNameMustBeSpecified));
            }

            if (this.StartingAmount <= 0)
            {
                return Task.FromResult(new Result(Resources.ValidAmountGreaterThan0MustBeSpecified));
            }

            if (this.IsRange)
            {
                if (this.EndingAmount <= 0)
                {
                    return Task.FromResult(new Result(Resources.ValidAmountGreaterThan0MustBeSpecified));
                }

                if (this.EndingAmount < this.StartingAmount)
                {
                    return Task.FromResult(new Result(Resources.EndingAmountMustBeGreaterThanStartingAmount));
                }
            }

            foreach (TwitchBitsCommandModel command in ServiceManager.Get<CommandService>().TwitchBitsCommands)
            {
                if ((this.IsRange && command.IsRange && command.StartingAmount == this.StartingAmount && command.EndingAmount == this.EndingAmount) ||
                    (!this.IsRange && !command.IsRange && command.StartingAmount == this.StartingAmount))
                {
                    if (this.existingCommand == null || this.existingCommand.ID != command.ID)
                    {
                        return Task.FromResult(new Result(Resources.TwitchBitsCommandsAlreadyExistsDuplicateAmount));
                    }
                }
            }

            return Task.FromResult(new Result());
        }

        public override Dictionary<string, string> GetTestSpecialIdentifiers() { return TwitchBitsCommandModel.GetBitsTestSpecialIdentifiers(); }

        public override Task<CommandModelBase> CreateNewCommand()
        {
            if (this.IsRange)
            {
                return Task.FromResult<CommandModelBase>(new TwitchBitsCommandModel(this.Name, this.StartingAmount, this.EndingAmount));
            }
            else
            {
                return Task.FromResult<CommandModelBase>(new TwitchBitsCommandModel(this.Name, this.StartingAmount, this.StartingAmount));
            }
        }

        public override async Task UpdateExistingCommand(CommandModelBase command)
        {
            await base.UpdateExistingCommand(command);
            ((TwitchBitsCommandModel)command).StartingAmount = this.StartingAmount;
            ((TwitchBitsCommandModel)command).EndingAmount = (this.IsRange) ? this.EndingAmount : this.StartingAmount;
        }

        public override Task SaveCommandToSettings(CommandModelBase command)
        {
            ServiceManager.Get<CommandService>().TwitchBitsCommands.Remove((TwitchBitsCommandModel)this.existingCommand);
            ServiceManager.Get<CommandService>().TwitchBitsCommands.Add((TwitchBitsCommandModel)command);
            return Task.CompletedTask;
        }
    }
}
