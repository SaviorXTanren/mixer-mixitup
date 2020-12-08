using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Commands.Games;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Util;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Games
{
    public class SpinGameCommandEditorWindowViewModel : GameCommandEditorWindowViewModelBase
    {
        private SpinGameCommandModel existingCommand;

        public SpinGameCommandEditorWindowViewModel(SpinGameCommandModel command)
            : base(command)
        {
            this.existingCommand = command;
            foreach (GameOutcomeModel outcome in this.existingCommand.Outcomes)
            {
                this.Outcomes.Add(new GameOutcomeViewModel(outcome));
            }
        }

        public SpinGameCommandEditorWindowViewModel(CurrencyModel currency)
            : base(currency)
        {
            this.Outcomes.Add(new GameOutcomeViewModel(MixItUp.Base.Resources.Win, 50, 200, this.CreateBasicChatCommand(string.Format(MixItUp.Base.Resources.GameCommandSpinWinExample, currency.Name))));
            this.Outcomes.Add(new GameOutcomeViewModel(MixItUp.Base.Resources.Lose, 50, 0, this.CreateBasicChatCommand(MixItUp.Base.Resources.GameCommandSpinLoseExample)));
        }

        public override Task<CommandModelBase> GetCommand()
        {
            return Task.FromResult<CommandModelBase>(new SpinGameCommandModel(this.Name, this.GetChatTriggers(), this.Outcomes.Select(o => o.GetModel())));
        }

        public override async Task<Result> Validate()
        {
            Result result = await base.Validate();
            if (!result.Success)
            {
                return result;
            }

            result = this.ValidateOutcomes(this.Outcomes);
            if (!result.Success)
            {
                return result;
            }

            return new Result();
        }
    }
}
