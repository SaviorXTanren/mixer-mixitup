using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Commands.Games;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Util;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Games
{
    public class SpinGameCommandEditorWindowViewModel : GameCommandEditorWindowViewModelBase
    {
        public ObservableCollection<GameOutcomeViewModel> Outcomes { get; set; } = new ObservableCollection<GameOutcomeViewModel>();

        public ICommand AddOutcomeCommand { get; set; }
        public ICommand DeleteOutcomeCommand { get; set; }

        private SpinGameCommandModel existingCommand;

        public SpinGameCommandEditorWindowViewModel(CurrencyModel currency)
            : this()
        {
            this.Outcomes.Add(new GameOutcomeViewModel(MixItUp.Base.Resources.Win, 50, 200, this.CreateBasicChatCommand(string.Format(MixItUp.Base.Resources.GameCommandSpinWinExample, currency.Name))));
            this.Outcomes.Add(new GameOutcomeViewModel(MixItUp.Base.Resources.Lose, 50, 0, this.CreateBasicChatCommand(MixItUp.Base.Resources.GameCommandSpinLoseExample)));
        }

        public SpinGameCommandEditorWindowViewModel(SpinGameCommandModel command)
            : this()
        {
            this.existingCommand = command;
            foreach (GameOutcomeModel outcome in this.existingCommand.Outcomes)
            {
                this.Outcomes.Add(new GameOutcomeViewModel(outcome));
            }
        }

        private SpinGameCommandEditorWindowViewModel()
        {
            this.AddOutcomeCommand = this.CreateCommand((parameter) =>
            {
                this.Outcomes.Add(new GameOutcomeViewModel());
                return Task.FromResult(0);
            });

            this.DeleteOutcomeCommand = this.CreateCommand((parameter) =>
            {
                this.Outcomes.Remove((GameOutcomeViewModel)parameter);
                return Task.FromResult(0);
            });
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
