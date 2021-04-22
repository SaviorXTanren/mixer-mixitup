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
    public class SlotMachineGameOutcomeViewModel : GameOutcomeViewModel
    {
        public string Symbol1
        {
            get { return this.symbol1; }
            set
            {
                this.symbol1 = value;
                this.NotifyPropertyChanged();
            }
        }
        private string symbol1;

        public string Symbol2
        {
            get { return this.symbol2; }
            set
            {
                this.symbol2 = value;
                this.NotifyPropertyChanged();
            }
        }
        private string symbol2;

        public string Symbol3
        {
            get { return this.symbol3; }
            set
            {
                this.symbol3 = value;
                this.NotifyPropertyChanged();
            }
        }
        private string symbol3;

        public bool AnyOrder
        { 
            get { return this.anyOrder; }
            set
            {
                this.anyOrder = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool anyOrder;

        public SlotMachineGameOutcomeViewModel(CustomCommandModel command) : this(0, command, string.Empty, string.Empty, string.Empty) { }

        public SlotMachineGameOutcomeViewModel(double payout, CustomCommandModel command, string symbol1, string symbol2, string symbol3, bool anyOrder = false)
            : base(MixItUp.Base.Resources.Outcome, 0, payout, command)
        {
            this.Symbol1 = symbol1;
            this.Symbol2 = symbol2;
            this.Symbol3 = symbol3;
            this.AnyOrder = anyOrder;
        }

        public SlotMachineGameOutcomeViewModel(SlotMachineGameOutcomeModel outcome)
            : base(outcome)
        {
            this.Symbol1 = outcome.Symbols[0];
            this.Symbol2 = outcome.Symbols[1];
            this.Symbol3 = outcome.Symbols[2];
            this.AnyOrder = outcome.AnyOrder;
        }

        public override Result Validate()
        {
            Result result = base.Validate();
            if (!result.Success)
            {
                return result;
            }

            if (string.IsNullOrEmpty(this.Symbol1) || string.IsNullOrEmpty(this.Symbol2) || string.IsNullOrEmpty(this.Symbol3))
            {
                return new Result(MixItUp.Base.Resources.GameCommandSlotMachineOutcomeMissingSymbol);
            }

            return new Result();
        }

        public new SlotMachineGameOutcomeModel GetModel()
        {
            return new SlotMachineGameOutcomeModel(this.Name, this.RoleProbabilityPayouts.ToDictionary(rpp => rpp.Role, rpp => rpp.GetModel()), this.Command, new List<string>() { this.Symbol1, this.Symbol2, this.Symbol3 }, this.AnyOrder);
        }
    }

    public class SlotMachineGameCommandEditorWindowViewModel : GameCommandEditorWindowViewModelBase
    {
        public string Symbols
        {
            get { return this.symbols; }
            set
            {
                this.symbols = value;
                this.NotifyPropertyChanged();
            }
        }
        private string symbols;

        public CustomCommandModel FailureCommand
        {
            get { return this.failureCommand; }
            set
            {
                this.failureCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel failureCommand;

        public SlotMachineGameCommandEditorWindowViewModel(SlotMachineGameCommandModel command)
            : base(command)
        {
            this.Symbols = string.Join(" ", command.Symbols);
            this.FailureCommand = command.FailureCommand;
            this.Outcomes.AddRange(command.Outcomes.Select(o => new SlotMachineGameOutcomeViewModel(o)));

            this.SetUICommands();
        }

        public SlotMachineGameCommandEditorWindowViewModel(CurrencyModel currency)
            : base(currency)
        {
            this.Name = MixItUp.Base.Resources.SlotMachine;
            this.Triggers = MixItUp.Base.Resources.GameCommandSlotMachineDefaultChatTrigger;

            this.Symbols = "X O $";
            this.FailureCommand = this.CreateBasicChatCommand(MixItUp.Base.Resources.GameCommandSlotMachineLoseExample);
            this.Outcomes.Add(new SlotMachineGameOutcomeViewModel(500, this.CreateBasicChatCommand(string.Format(MixItUp.Base.Resources.GameCommandSlotMachineWinExample, this.PrimaryCurrencyName)), "O", "O", "O"));
            this.Outcomes.Add(new SlotMachineGameOutcomeViewModel(200, this.CreateBasicChatCommand(string.Format(MixItUp.Base.Resources.GameCommandSlotMachineWinExample, this.PrimaryCurrencyName)), "$", "O", "$"));
            this.Outcomes.Add(new SlotMachineGameOutcomeViewModel(150, this.CreateBasicChatCommand(string.Format(MixItUp.Base.Resources.GameCommandSlotMachineWinExample, this.PrimaryCurrencyName)), "X", "$", "O", anyOrder: true));

            this.SetUICommands();
        }

        public IEnumerable<string> SymbolsList { get { return (!string.IsNullOrEmpty(this.Symbols)) ? this.Symbols.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>(); } }

        public override Task<CommandModelBase> CreateNewCommand()
        {
            return Task.FromResult<CommandModelBase>(new SlotMachineGameCommandModel(this.Name, this.GetChatTriggers(), this.SymbolsList, this.FailureCommand, this.Outcomes.Select(o => ((SlotMachineGameOutcomeViewModel)o).GetModel())));
        }

        public override async Task UpdateExistingCommand(CommandModelBase command)
        {
            await base.UpdateExistingCommand(command);
            SlotMachineGameCommandModel gCommand = (SlotMachineGameCommandModel)command;
            gCommand.Symbols = new List<string>(this.SymbolsList);
            gCommand.FailureCommand = this.FailureCommand;
            gCommand.Outcomes = new List<SlotMachineGameOutcomeModel>(this.Outcomes.Select(o => ((SlotMachineGameOutcomeViewModel)o).GetModel()));
        }

        public override async Task<Result> Validate()
        {
            Result result = await base.Validate();
            if (!result.Success)
            {
                return result;
            }

            IEnumerable<string> symbolsList = this.SymbolsList;
            if (symbolsList.Count() < 2)
            {
                return new Result(MixItUp.Base.Resources.GameCommandSlotMachineAtLeast2Symbols);
            }

            if (symbolsList.GroupBy(s => s).Any(g => g.Count() > 1))
            {
                return new Result(MixItUp.Base.Resources.GameCommandSlotMachineAllSymbolsMustBeUnique);
            }

            HashSet<string> symbols = new HashSet<string>(symbolsList);
            if (this.Outcomes.Count() == 0)
            {
                return new Result(MixItUp.Base.Resources.GameCommandAtLeast1Outcome);
            }

            foreach (SlotMachineGameOutcomeViewModel outcome in this.Outcomes)
            {
                if (!symbols.Contains(outcome.Symbol1) || !symbols.Contains(outcome.Symbol2) || !symbols.Contains(outcome.Symbol3))
                {
                    return new Result(MixItUp.Base.Resources.GameCommandSlotMachineOutcomeSymbolNotInAllSymbols);
                }
            }

            return new Result();
        }

        private void SetUICommands()
        {
            this.AddOutcomeCommand = this.CreateCommand(() =>
            {
                this.Outcomes.Add(new SlotMachineGameOutcomeViewModel(this.CreateBasicChatCommand(string.Format(MixItUp.Base.Resources.GameCommandSlotMachineWinExample, this.PrimaryCurrencyName))));
            });
        }
    }
}