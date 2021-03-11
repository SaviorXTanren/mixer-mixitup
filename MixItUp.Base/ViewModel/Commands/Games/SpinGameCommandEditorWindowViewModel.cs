using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Commands.Games;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Games
{
    public class SpinGameCommandEditorWindowViewModel : GameCommandEditorWindowViewModelBase
    {
        public SpinGameCommandEditorWindowViewModel(SpinGameCommandModel command)
            : base(command)
        {
            this.Outcomes.AddRange(command.Outcomes.Select(o => new GameOutcomeViewModel(o)));
        }

        public SpinGameCommandEditorWindowViewModel(CurrencyModel currency)
            : base(currency)
        {
            this.Name = MixItUp.Base.Resources.Spin;
            this.Triggers = MixItUp.Base.Resources.Spin.Replace(" ", string.Empty).ToLower();

            this.Outcomes.Add(new GameOutcomeViewModel(MixItUp.Base.Resources.Win, 50, 200, this.CreateBasicChatCommand(string.Format(MixItUp.Base.Resources.GameCommandSpinWinExample, this.PrimaryCurrencyName))));
            this.Outcomes.Add(new GameOutcomeViewModel(MixItUp.Base.Resources.Lose, 50, 0, this.CreateBasicChatCommand(MixItUp.Base.Resources.GameCommandSpinLoseExample)));
        }

        public override Task<CommandModelBase> CreateNewCommand()
        {
            return Task.FromResult<CommandModelBase>(new SpinGameCommandModel(this.Name, this.GetChatTriggers(), this.Outcomes.Select(o => o.GetModel())));
        }

        public override async Task UpdateExistingCommand(CommandModelBase command)
        {
            await base.UpdateExistingCommand(command);
            SpinGameCommandModel gCommand = (SpinGameCommandModel)command;
            gCommand.Outcomes = new List<GameOutcomeModel>(this.Outcomes.Select(o => o.GetModel()));
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
